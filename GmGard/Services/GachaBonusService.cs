using GmGard.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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
        private readonly TitleService titleService_;
        private readonly IMemoryCache cache_;
        private readonly ILogger logger_;

        public GachaBonusService(UsersContext db, IHttpContextAccessor accessor, ExpUtil expUtil, TitleService titleService, IMemoryCache memoryCache, ILoggerFactory logger)
        {
            db_ = db;
            context_ = accessor.HttpContext;
            expUtil_ = expUtil;
            titleService_ = titleService;
            cache_ = memoryCache;
            logger_ = logger.CreateLogger(nameof(GachaBonusService));
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
                var conds = userItem == null ? Enumerable.Empty<IGachaTitleCondition>() : TitleConditions.Where(c => c.Related(userItem)).ToList();
                if (conds.Count() > 0)
                {
                    int minBonusTimes = -1;
                    foreach (var cond in conds)
                    {
                        if (cond.Satisfy(allUserRareItems))
                        {
                            int? titleId = titleService_.GetTitleId(cond.Title);
                            if (titleId.HasValue && !quest.HasTitle(titleId.Value))
                            {
                                quest.AddTitle(titleId.Value);
                                result.Add(string.Format("获得称号【{0}】", cond.Title));
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
            foreach (var cond in TitleConditions)
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
            string Title { get; }
            int GetThresholdForItem(string Name);
            string GetProgressString(IEnumerable<UserItemCount> userItems);
        }

        protected class SingleGachaTitleCondition : IGachaTitleCondition
        {
            public SingleGachaTitleCondition(string name, int count, string title)
            {
                Name = name;
                Count = count;
                Title = title;
            }

            string Name { get; set; }
            public int Count { get; set; }
            public string Title { get; set; }

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
                return $"称号【{Title}】： {Name} {progress}/{Count}";
            }
        }

        protected class AnyOfGachaTitleCondition : IGachaTitleCondition
        {
            public AnyOfGachaTitleCondition(IEnumerable<string> name, string title)
            {
                Name = name;
                Count = 1;
                Title = title;
            }

            IEnumerable<string> Name { get; set; }
            public int Count { get; set; }
            public string Title { get; set; }

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
                return ($"称号【{Title}】： {progress}/{Count}");
            }
        }

        protected class CombinedGachaTitleCondition : IGachaTitleCondition
        {
            public CombinedGachaTitleCondition(IDictionary<string, int> nameCountMap, string profession)
            {
                NameCountMap = nameCountMap;
                Title = profession;
                Hidden = false;
            }

            public CombinedGachaTitleCondition(IDictionary<string, int> nameCountMap, string profession, bool hidden)
            {
                NameCountMap = nameCountMap;
                Title = profession;
                Hidden = hidden;
            }

            private IDictionary<string, int> NameCountMap { get; set; }
            public string Title { get; private set; }
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
                sb.AppendFormat("称号【{0}】：", Title);
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
            public PoolCountTitleCondition(GachaPool.PoolName poolName, int count, string title)
            {
                Count = count;
                Title = title;
                PoolName = poolName;
            }

            public string Title { get; set; }
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
                return ($"称号【{Title}】:{tmp} {progress}/{Count}");
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

        protected class SingleGachaTitleConditionRequirement
        {
            public string ItemName { get; set; }
            public int Count { get; set; }
        }

        protected class PoolCountTitleConditionRequirement
        {
            public GachaPool.PoolName PoolName { get; set; }
            public int Count { get; set; }
        }

        protected class CombinedTitleConditionRequirement
        {
            public IDictionary<string, int> NameCountMap { get; set; }
            public bool Hidden { get; set; }
        }

        protected IEnumerable<IGachaTitleCondition> TitleConditions {
            get
            {
                return cache_.GetOrCreate("~GachaTitleConditions", entry =>
                {
                    var configs = db_.GachaTitleConditionConfigs.Include(x => x.TitleConfig).ToList();
                    List<IGachaTitleCondition> titleConditions = new();
                    foreach (var config in configs)
                    {
                        switch (config.ConditionType)
                        {
                            case "Single":
                                var req = Newtonsoft.Json.JsonConvert.DeserializeObject<SingleGachaTitleConditionRequirement>(config.ConditionRequirements);
                                titleConditions.Add(new SingleGachaTitleCondition(req.ItemName, req.Count, config.TitleConfig.TitleName));
                                break;
                            case "Combined":
                                var reqs = Newtonsoft.Json.JsonConvert.DeserializeObject<CombinedTitleConditionRequirement>(config.ConditionRequirements);
                                titleConditions.Add(new CombinedGachaTitleCondition(reqs.NameCountMap, config.TitleConfig.TitleName, reqs.Hidden));
                                break;
                            case "PoolCount":
                                var req2 = Newtonsoft.Json.JsonConvert.DeserializeObject<PoolCountTitleConditionRequirement>(config.ConditionRequirements);
                                titleConditions.Add(new PoolCountTitleCondition(req2.PoolName, req2.Count, config.TitleConfig.TitleName));
                                break;
                            case "AnyOf":
                                var req3 = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<string>>(config.ConditionRequirements);
                                titleConditions.Add(new AnyOfGachaTitleCondition(req3, config.TitleConfig.TitleName));
                                break;
                            default:
                                logger_.LogError("Ignoring Unknown condition type {ConditionType}", config.ConditionType);
                                break;
                        }
                    }
                    return titleConditions;
                });
            }
        }
    }
}
