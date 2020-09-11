using GmGard.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class GachaBonusService
    {
        private readonly UsersContext db_;
        private readonly HttpContext context_;
        private readonly ExpUtil expUtil_;

        public GachaBonusService(UsersContext db, IHttpContextAccessor accessor, ExpUtil expUtil)
        {
            db_ = db;
            context_ = accessor.HttpContext;
            expUtil_ = expUtil;
        }

        public async Task<List<string>> HandleForItemsAsync(IEnumerable<GachaItem> items)
        {
            var user = await db_.Users.Include("quest").SingleOrDefaultAsync(u => u.UserName == context_.User.Identity.Name);
            if (user.quest == null)
            {
                user.quest = new UserQuest { UserId = user.Id };
            }
            var quest = user.quest;
            var itemCounts = items.GroupBy(i => i.Id).ToDictionary(g => g.Key, g => new { Item = g.FirstOrDefault(), Count = g.Count() });
            var itemIds = itemCounts.Keys;
            var allUserRareItems = await GetUserMissionItemsAsync(user);

            var bonus = new GachaBonus();
            bonus.AddExp(items.Count());
            var result = new List<string>();
            foreach (var item in itemCounts.Values)
            {
                var userItem = allUserRareItems.FirstOrDefault(i => i.ItemName == item.Item.Name);
                var conds = userItem == null ? Enumerable.Empty<IGachaTitleCondition>() : titleConditions.Where(c => c.Related(userItem)).ToList();
                if (conds.Count() > 0)
                {
                    int minBonusTimes = -1;
                    foreach (var cond in conds)
                    {
                        if (cond.Satisfy(allUserRareItems))
                        {
                            if (!quest.HasTitle(cond.Title))
                            {
                                quest.AddTitle(cond.Title);
                                result.Add(string.Format("获得称号【{0}】", UserQuest.ProfessionName(cond.Title)));
                            }

                            int bonusTimes = item.Count;
                            int threshold = cond.GetThresholdForItem(item.Item.Name);
                            if (userItem.ItemCount - item.Count < threshold)
                            {
                                bonusTimes = userItem.ItemCount - threshold;
                            }
                            if (minBonusTimes < 0 || bonusTimes < minBonusTimes)
                            {
                                minBonusTimes = bonusTimes;
                            }
                        }
                    }
                    if (item.Item.Rarity > 1 && item.Item.Rarity < 5 && conds.All(c => c.Satisfy(allUserRareItems)))
                    {
                        bonus.IncreaseMultiple((item.Item.Rarity - 1) * 4, minBonusTimes); // r2: 4, r3: 8, r4: 12
                    }
                }
                else if (item.Item.Rarity > 1 && item.Item.Rarity < 5)
                {
                    bonus.IncreaseMultiple((item.Item.Rarity - 1) * 4, item.Count); // r2: 4, r3: 8, r4: 12
                }
                if (item.Item.Rarity >= 5)
                {
                    for (int i = 0; i < item.Count; ++i)
                    {
                        bonus.AddPointRandom(30, 50);
                    }
                }
            }
            bonus.ApplyBonus(user, expUtil_);
            await db_.SaveChangesAsync();
            result.Add(bonus.GetResult());
            return result;
        }

        public async Task<List<string>> GetProgressForUserAsync()
        {
            var user = await db_.Users.Include("quest").SingleOrDefaultAsync(u => u.UserName == context_.User.Identity.Name);
            if (user.quest == null)
            {
                return new List<string>();
            }
            var userItems = await GetUserMissionItemsAsync(user);
            var result = new List<string>();
            foreach (var cond in titleConditions)
            {
                string progress = cond.GetProgressString(userItems);
                if (!string.IsNullOrEmpty(progress))
                {
                    result.Add(progress);
                }
            }
            return result;
        }

        private async Task<List<UserItemCount>> GetUserMissionItemsAsync(UserProfile user)
        {
            var allitems = await db_.UserGachas.Include(g => g.Item)
                .Where(u => user.quest.UserId == u.UserID && u.Item.HasMission)
                .ToListAsync();
            return allitems.GroupBy(u => u.Item.Name).Select(g => new UserItemCount
            {
                ItemName = g.Key,
                ItemId = g.FirstOrDefault().ItemId,
                ItemCount = g.Count(),
                ItemPool = g.GroupBy(gg => gg.PoolName).ToDictionary(d => d.Key, d => d.Count()),
            }).ToList();
        }

        protected class GachaBonus
        {
            private Random random = new Random();
            private int Points = 0;
            private int Exp = 0;

            public void Increase(int count)
            {
                if (random.Next() % 2 == 0)
                {
                    Points += count;
                }
                else
                {
                    Exp += count;
                }
            }

            public void AddExp(int count)
            {
                Exp += count;
            }

            public void AddPointRandom(int min, int max)
            {
                Points += random.Next(min, max);
            }

            public void IncreaseMultiple(int count, int times)
            {
                for (int i = 0; i < times; ++i)
                {
                    Increase(count);
                }
            }

            public string GetResult()
            {
                StringBuilder sb = new StringBuilder();
                if (Points > 0)
                {
                    sb.AppendFormat("棒棒糖+{0} ", Points);
                }
                if (Exp > 0)
                {
                    sb.AppendFormat("绅士度+{0} ", Exp);
                }
                return sb.ToString();
            }

            public void ApplyBonus(UserProfile u, ExpUtil expUtil)
            {
                expUtil.AddPoint(u, Points);
                expUtil.addExp(u, Exp);
            }
        }

        protected class UserItemCount
        {
            public string ItemName { get; set; }
            public int ItemId { get; set; }
            public int ItemCount { get; set; }
            public IDictionary<GachaPool.PoolName, int> ItemPool { get; set; }
        }

        protected interface IGachaTitleCondition
        {
            bool Related(UserItemCount userItemCount);
            bool Satisfy(IEnumerable<UserItemCount> userItems);
            UserQuest.UserProfession Title { get; }
            int GetThresholdForItem(string Name);
            string GetProgressString(IEnumerable<UserItemCount> userItems);
        }

        protected class SingleGachaTitleCondition : IGachaTitleCondition
        {
            public SingleGachaTitleCondition(string name, int count, UserQuest.UserProfession profession)
            {
                Name = name;
                Count = count;
                Title = profession;
            }

            string Name { get; set; }
            public int Count { get; set; }
            public UserQuest.UserProfession Title { get; set; }

            public bool Related(UserItemCount userItemCount) => this.Name == userItemCount.ItemName;

            public bool Satisfy(IEnumerable<UserItemCount> userItems)
            {
                var item = userItems.FirstOrDefault(uic => uic.ItemName == Name);
                return (item != null && item.ItemCount >= Count);
            }
            
            public int GetThresholdForItem(string Name)
            {
                if (Name != this.Name)
                {
                    throw new ArgumentException($"Incorrect name ${Name}, expected ${this.Name}", "Name");
                }
                return Count;
            }

            public string GetProgressString(IEnumerable<UserItemCount> userItems)
            {
                var item = userItems.FirstOrDefault(uic => uic.ItemName == Name);
                if (item == null)
                {
                    return "";
                }
                int progress = item.ItemCount;
                if (progress > Count)
                {
                    progress = Count;
                }
                return $"称号【{UserQuest.ProfessionName(Title)}】： {Name} {progress}/{Count}";
            }
        }

        protected class AnyOfGachaTitleCondition : IGachaTitleCondition
        {
            public AnyOfGachaTitleCondition(string[] name, UserQuest.UserProfession profession)
            {
                Name = name;
                Count = 1;
                Title = profession;
            }

            string[] Name { get; set; }
            public int Count { get; set; }
            public UserQuest.UserProfession Title { get; set; }

            public bool Related(UserItemCount userItemCount) => Name.Contains(userItemCount.ItemName);

            public bool Satisfy(IEnumerable<UserItemCount> userItems)
            {
                var item = userItems.Where(uic => this.Name.Contains(uic.ItemName)).Sum(uic => uic.ItemCount);
                return item >= Count;
            }
            
            public int GetThresholdForItem(string Name)
            {
                if (!this.Name.Contains(Name))
                {
                    throw new ArgumentException($"Incorrect name ${Name}, expected ${this.Name}", "Name");
                }
                return Count;
            }

            public string GetProgressString(IEnumerable<UserItemCount> userItems)
            {
                var progress = userItems.Where(uic => this.Name.Contains(uic.ItemName)).Sum(uic => uic.ItemCount);
                if (progress == 0)
                {
                    return "";
                }
                if (progress > Count)
                {
                    progress = Count;
                }
                return ($"称号【{UserQuest.ProfessionName(Title)}】： {progress}/{Count}");
            }
        }

        protected class CombinedGachaTitleCondition : IGachaTitleCondition
        {
            public CombinedGachaTitleCondition(IDictionary<string, int> nameCountMap, UserQuest.UserProfession profession)
            {
                NameCountMap = nameCountMap;
                Title = profession;
                Hidden = false;
            }

            public CombinedGachaTitleCondition(IDictionary<string, int> nameCountMap, UserQuest.UserProfession profession, bool hidden)
            {
                NameCountMap = nameCountMap;
                Title = profession;
                Hidden = hidden;
            }

            private IDictionary<string, int> NameCountMap { get; set; }
            public UserQuest.UserProfession Title { get; private set; }
            /// <summary>
            /// Only show progress when satisfy.
            /// </summary>
            private bool Hidden { get; set; }

            public bool Related(UserItemCount userItemCount) => NameCountMap.ContainsKey(userItemCount.ItemName);

            public bool Satisfy(IEnumerable<UserItemCount> userItems)
            {
                foreach (var cond in NameCountMap)
                {
                    var item = userItems.FirstOrDefault(uic => uic.ItemName == cond.Key);
                    if (item == null || item.ItemCount < cond.Value)
                    {
                        return false;
                    }
                }
                return true;
            }
            
            public int GetThresholdForItem(string Name)
            {
                if (!NameCountMap.ContainsKey(Name))
                {
                    throw new ArgumentException($"Incorrect name ${Name}", "Name");
                }
                return NameCountMap[Name];
            }

            public string GetProgressString(IEnumerable<UserItemCount> userItems)
            {
                if (Hidden && !Satisfy(userItems))
                {
                    return "";
                }
                if (userItems.All(uic => !Related(uic))) 
                {
                    return "";
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("称号【{0}】：", UserQuest.ProfessionName(Title));
                foreach (var cond in NameCountMap)
                {
                    var item = userItems.FirstOrDefault(uic => uic.ItemName == cond.Key);
                    if (item != null)
                    {
                        int progress = item.ItemCount;
                        int threshold = cond.Value;
                        if (progress > threshold)
                        {
                            progress = threshold;
                        }
                        sb.AppendFormat($"{cond.Key} {progress}/{threshold}；");
                    }
                    else
                    {
                        sb.AppendFormat($"？？？ 0/{cond.Value}；");
                    }
                }
                return sb.ToString();
            }
        }

        protected class PoolCountTitleCondition : IGachaTitleCondition
        {
            public PoolCountTitleCondition(GachaPool.PoolName poolName, int count, UserQuest.UserProfession profession)
            {
                Count = count;
                Title = profession;
                PoolName = poolName;
            }

            public UserQuest.UserProfession Title { get; set; }
            public int Count { get; set; }
            public GachaPool.PoolName PoolName { get; set; }

            public string GetProgressString(IEnumerable<UserItemCount> userItems)
            {
                var progress = userItems.Where(i => i.ItemPool.ContainsKey(PoolName)).Sum(i => i.ItemPool[PoolName]);
                if (progress == 0)
                {
                    return "";
                }
                if (progress > Count)
                {
                    progress = Count;
                }
                var tmp = (Count > 1) ? "抽取卡池次数" : "";
                return ($"称号【{UserQuest.ProfessionName(Title)}】:{tmp} {progress}/{Count}");
            }

            public int GetThresholdForItem(string Name)
            {
                return 0;
            }

            public bool Related(UserItemCount uic)
            {
                return uic.ItemPool.ContainsKey(PoolName);
            }

            public bool Satisfy(IEnumerable<UserItemCount> userItems)
            {
                var progress = userItems.Where(i => i.ItemPool.ContainsKey(PoolName)).Sum(i => i.ItemPool[PoolName]);
                return progress >= Count;
            }
        }

        protected static readonly List<IGachaTitleCondition> titleConditions = new List<IGachaTitleCondition>()
        {
            new SingleGachaTitleCondition("佐菲", 10, UserQuest.UserProfession.沙福林),
            new SingleGachaTitleCondition("光美", 10, UserQuest.UserProfession.大友),
            new SingleGachaTitleCondition("大流士", 10, UserQuest.UserProfession.非洲人),
            new SingleGachaTitleCondition("千代田", 10, UserQuest.UserProfession.lowb),
            new SingleGachaTitleCondition("田中", 10, UserQuest.UserProfession.亲妈卫星),
            new SingleGachaTitleCondition("那珂", 10, UserQuest.UserProfession.非洲提督),
            new SingleGachaTitleCondition("绿帽子", 10, UserQuest.UserProfession.原谅她),
            new SingleGachaTitleCondition("露娜", 10, UserQuest.UserProfession.唱歌好听),
            new SingleGachaTitleCondition("大和", 5, UserQuest.UserProfession.欧洲提督),
            new SingleGachaTitleCondition("大米", 5, UserQuest.UserProfession.伟大的哈里发),
            new SingleGachaTitleCondition("斑比酱", 5, UserQuest.UserProfession.斑比酱的眷属),
            new SingleGachaTitleCondition("春日野穹", 3, UserQuest.UserProfession.德国骨科),
            new SingleGachaTitleCondition("欧洲人", 3, UserQuest.UserProfession.欧洲人),
            new SingleGachaTitleCondition("王之力", 5, UserQuest.UserProfession.王的力量),
            new SingleGachaTitleCondition("蓝酱", 5, UserQuest.UserProfession.站长的老婆),
            new SingleGachaTitleCondition("诗音", 3, UserQuest.UserProfession.eden),
            new SingleGachaTitleCondition("黑圣杯", 5, UserQuest.UserProfession.此世所有之恶),
            new SingleGachaTitleCondition("野兽先辈", 5, UserQuest.UserProfession.恶臭难闻),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "博丽灵梦", 5 }, { "博丽神社", 5 } }, UserQuest.UserProfession.乐园的巫女),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "雾雨魔理沙", 5 }, { "迷你八卦炉", 5 } }, UserQuest.UserProfession.偷心的魔法使),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "十六夜咲夜", 5 }, { "咲夜的世界", 5 } }, UserQuest.UserProfession.完美潇洒的从者),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "蕾米莉亚", 5 }, { "威严", 5 } }, UserQuest.UserProfession.永远鲜红的幼月),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "东风谷早苗", 5 }, { "守矢神社", 5 } }, UserQuest.UserProfession.风之祭祀),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "爱丽丝威震天", 5 }, { "帕秋莉香草", 5 } }, UserQuest.UserProfession.魔理沙的后宫),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "爱丽丝·玛格特罗依德", 5 }, { "上海人形", 5 } }, UserQuest.UserProfession.七色的人偶使),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "帕秋莉·诺蕾姬", 5 }, { "大图书馆", 5 } }, UserQuest.UserProfession.七曜的大法师),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "芙兰朵露", 5 }, { "蓝蓝路", 5 } }, UserQuest.UserProfession.恶魔之妹),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "魂魄妖梦", 5 }, { "麻薯", 5 } }, UserQuest.UserProfession.半灵庭师),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "西行寺幽幽子", 5 }, { "西行妖", 5 } }, UserQuest.UserProfession.幽冥公主),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "八云紫", 5 }, { "萝莉紫", 5 } }, UserQuest.UserProfession.十七岁的妖怪贤者),
            new AnyOfGachaTitleCondition(new[]{"惠1", "惠2" }, UserQuest.UserProfession.理想中的女主角),
            new AnyOfGachaTitleCondition(new[]{ "路人组合1", "路人组合2" } , UserQuest.UserProfession.未战先败),
            new SingleGachaTitleCondition("出海", 1, UserQuest.UserProfession.欧派不是力量),
            new AnyOfGachaTitleCondition(new[]{ "学姐1", "学姐2" }, UserQuest.UserProfession.教科书般的黑丝),
            new AnyOfGachaTitleCondition(new[]{ "英梨梨1", "英梨梨2" } , UserQuest.UserProfession.教科书般的傲娇),
            new SingleGachaTitleCondition("美智留", 1, UserQuest.UserProfession.最远的最近),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {
                { "惠1", 1 }, { "惠2", 1 },
                { "路人组合1", 1 }, { "路人组合2", 1 },
                { "出海", 1 }, { "美智留", 1 },
                { "学姐1", 1 }, { "学姐2", 1 },
                { "英梨梨1", 1 }, { "英梨梨2", 1 },
            }, UserQuest.UserProfession.路人女主的养成方法),
            new SingleGachaTitleCondition("伦也", 5, UserQuest.UserProfession.BlessingSoftware),
            new PoolCountTitleCondition(GachaPool.PoolName.April2018, 1, UserQuest.UserProfession.神经病凡是女人),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "晓", 3 }, { "响", 3 } }, UserQuest.UserProfession.晓之地平线),
            new SingleGachaTitleCondition("橘爱丽丝", 2, UserQuest.UserProfession.请叫我橘),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "朝潮", 1 }, { "朝潮猫", 3 } }, UserQuest.UserProfession.那是什么暗号么),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "埃尔德里奇", 1 }, { "小贝法", 1 } }, UserQuest.UserProfession.彩虹Project),
            new CombinedGachaTitleCondition(new Dictionary<string, int> { { "万圣智乃", 1 }, { "普通智乃", 1 } }, UserQuest.UserProfession.没有点兔看我要死啦),
            new SingleGachaTitleCondition("舆水幸子", 2, UserQuest.UserProfession.这都怪我太可爱了),
            new SingleGachaTitleCondition("阿比酱", 2, UserQuest.UserProfession.窥探深渊之人),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {
                { "朝潮", 1 }, { "阿比酱", 1 },
                { "埃尔德里奇", 1 }, { "橘爱丽丝", 1 },
                { "万圣智乃", 1 }, { "舆水幸子", 1 },
                { "晓", 1 }, { "小贝法", 1 },
                { "普通智乃", 1 },
            }, UserQuest.UserProfession.虹色萝莉薄),
            new PoolCountTitleCondition(GachaPool.PoolName.April2019, 1, UserQuest.UserProfession.绅士之魂),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "SP", 1 }, { "棒棒糖2", 1 }}, UserQuest.UserProfession.资本的力量),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "关西录音笔", 1 }, { "窃窃私语", 1 }, { "记录的相机", 1 }, { "记录的笔记本", 1 }}, UserQuest.UserProfession.真相的探求者),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "Rivers77", 1 }, { "彎枝理樹", 1 }, { "男子高中生的蜘蛛日常", 1 }, { "风雷", 1 }, { "黑犬狂魔", 1 }}, UserQuest.UserProfession.群众里面有坏人),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "echo", 1 }, { "IS", 1 }, { "lzone", 1 }, { "小豆梓", 1 }, { "悠久", 1 }, { "杉崎key", 1 } }, UserQuest.UserProfession.权限亦或苦力),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "人间入间", 1 }, { "傲沉", 1 }}, UserQuest.UserProfession.站长在看着你),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {
                { "SP", 1 }, { "棒棒糖2", 1 },
                { "关西录音笔", 1 }, { "窃窃私语", 1 }, { "记录的相机", 1 }, { "记录的笔记本", 1 },
                { "Rivers77", 1 }, { "彎枝理樹", 1 }, { "男子高中生的蜘蛛日常", 1 }, { "风雷", 1 }, { "黑犬狂魔", 1 },
                { "echo", 1 }, { "IS", 1 }, { "lzone", 1 }, { "小豆梓", 1 }, { "悠久", 1 }, { "杉崎key", 1 },
                { "人间入间", 1 }, { "傲沉", 1 }
            }, UserQuest.UserProfession.愚人节快乐, true),
            new PoolCountTitleCondition(GachaPool.PoolName.August2020, 1, UserQuest.UserProfession.迷影重重),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "人间入间2", 1 }, { "L娘", 1 }}, UserQuest.UserProfession.永恒的恋人),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "前站长", 3 }, { "响学研究协会长", 3 }, { "L娘", 3 }}, UserQuest.UserProfession.好朋友),
            new CombinedGachaTitleCondition(new Dictionary<string, int> {{ "Duo", 1 }, { "F酱", 1 }, { "人间入间2", 1 }, { "杉崎key2", 1 }, { "林檎", 1 }, { "漫游火焰", 1 }, { "真诗君", 1 }, { "里奥", 1 }, { "杂乱无序", 1 } }, UserQuest.UserProfession.高级搜集者),
            new SingleGachaTitleCondition("抽卡姬", 1, UserQuest.UserProfession.克苏鲁机械),
            new SingleGachaTitleCondition("黑市商人无末", 1, UserQuest.UserProfession.至暗の交易),
        };
    }
}
