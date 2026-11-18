using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using GmGard.Extensions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GmGard.Services
{
    public class ExpUtil : UtilityService
    {
        private IHttpContextAccessor _contextAccessor;
        private MessageUtil _msgUtil;
        private AdminUtil _adminUtil;
        private UserProfile _currentUser;

        private UserProfile CurrentUser
        {
            get {
                if (_currentUser == null && HttpContext.User.Identity.IsAuthenticated)
                {
                    _currentUser = _udb.Users.AsNoTracking().Include(u => u.quest).SingleOrDefault(u => u.UserName == HttpContext.User.Identity.Name);
                }
                return _currentUser;
            }
        }

        private HttpContext HttpContext => _contextAccessor.HttpContext;

        private const int RateExp = 2;

        private const int PostExp = 3;

        static public DateTime FirstSignHistoryDate => new DateTime(2019, 1, 25);

        public const string ExpTableKey = "~EXPTABLE";

        public const string LevelCacheKey = "~lvl";

        public const string ExpCacheKey = "~exp";

        public const string PointsCacheKey = "~pts";

        public const string SignCacheKey = "~sign";

        public const string HasRatedCacheKey = "~hrt";

        public const string HasRatedPostCacheKey = "~hrp";

        public const string HasPostedCacheKey = "~hpt";

        public const string HasBloggedCacheKey = "~hbg";

        public const string WeekBloggedCacheKey = "~wbg";

        public const string WeekAllDailyCacheKey = "~wad";

        public ExpUtil(BlogContext db, UsersContext udb, IMemoryCache cache, IHttpContextAccessor contextAccessor, MessageUtil msgUtil, AdminUtil adminUtil) : base(db, udb, cache)
        {
            _contextAccessor = contextAccessor;
            _adminUtil = adminUtil;
            _msgUtil = msgUtil;
        }

        public int calculateUserLevel(int userid)
        {
            int lvl = 0;
            var user = _udb.Users.Find(userid);
            var exp = _udb.ExpTable.SingleOrDefault(e => e.ExperienceStart <= user.Experience && e.ExperienceEnd >= user.Experience);
            if (exp != null)
            {
                lvl = exp.Level;
                var username = user.UserName;
                _cache.Remove(LevelCacheKey + username);
                _cache.Remove(ExpCacheKey + username);
            }

            return lvl;
        }

        public IEnumerable<ExperienceTable> getExpTable()
        {
            var table = _cache.Get<IEnumerable<ExperienceTable>>(ExpTableKey);
            if (table == null)
            {
                table = _udb.ExpTable.ToList();
                _cache.Set(ExpTableKey, table, new MemoryCacheEntryOptions 
                { 
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                    Priority = CacheItemPriority.High
                });
            }
            return table;
        }

        public string getLevelTitle(int level)
        {
            
            var title = getExpTable().SingleOrDefault(t => t.Level == level)?.Title;
            return title;
        }

        public int getUserLvl(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return -1;
            }
            int? lvl = _cache.Get<int?>(LevelCacheKey + username);
            if (lvl.HasValue)
                return lvl.Value;
            lvl = _udb.Users.AsNoTracking().SingleOrDefault(u => u.UserName == username)?.Level;
            _cache.Set(LevelCacheKey + username, lvl, new MemoryCacheEntryOptions 
            { 
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            });
            return lvl.HasValue ? lvl.Value : -1;
        }

        public void getUserExp(string username, out int expCurrent, out int expNext, out int lvl)
        {
            expCurrent = 0;
            expNext = 0;
            lvl = -1;
            if (string.IsNullOrEmpty(username))
            {
                return;
            }
            int? level = _cache.Get<int?>(LevelCacheKey + username);
            Tuple<int, int> exptuple = _cache.Get<Tuple<int, int>>(ExpCacheKey + username);
            if (level.HasValue && exptuple != null)
            {
                expCurrent = exptuple.Item1;
                expNext = exptuple.Item2;
                lvl = level.Value;
                return;
            }
            var user = _udb.Users.SingleOrDefault(u => u.UserName == username);
            if (user == null)
            {
                return;
            }
            var exp = getExpTable().SingleOrDefault(t => t.Level == user.Level);
            if (exp != null && exp.ExperienceEnd > 0)
            {
                expCurrent = user.Experience - exp.ExperienceStart;
                expNext = exp.ExperienceEnd - exp.ExperienceStart + 1;
            }
            lvl = user.Level;
            level = lvl;
            _cache.Set(LevelCacheKey + username, level, new MemoryCacheEntryOptions 
            { 
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            });
            _cache.Set(ExpCacheKey + username, new Tuple<int, int>(expCurrent, expNext), new MemoryCacheEntryOptions 
            { 
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            });
        }

        public int getUserPoints(string username)
        {
            int? points = _cache.Get<int?>(PointsCacheKey + username);
            if (points == null)
            {
                var user = _udb.Users.AsNoTracking().SingleOrDefault(u => u.UserName == username);
                if (user == null)
                    return 0;

                points = user.Points;
                _cache.Set(PointsCacheKey + username, points, new MemoryCacheEntryOptions 
                { 
                    SlidingExpiration = TimeSpan.FromMinutes(30),
                    Priority = CacheItemPriority.Normal
                });
            }
            return points.Value;
        }

        public void addExp(string username, int expCount)
        {
            var user = _udb.Users.Single(u => u.UserName == username);
            addExp(user, expCount);
            _udb.SaveChanges();
        }

        public void addExp(UserProfile user, int expCount)
        {
            int expCur = 0, expNext = 0;
            user.Experience += expCount;
            if (user.Level < 90)
            {
                var exp = getExpTable().SingleOrDefault(e => e.ExperienceEnd >= user.Experience && e.ExperienceStart <= user.Experience);
                if (exp != null)
                {
                    user.Level = exp.Level;
                    expCur = user.Experience - exp.ExperienceStart;
                    expNext = exp.ExperienceEnd - exp.ExperienceStart + 1;
                }
            }
            var username = user.UserName;
            _cache.Set(LevelCacheKey + username, user.Level, new MemoryCacheEntryOptions 
            { 
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            });
            _cache.Set(ExpCacheKey + username, new Tuple<int, int>(expCur, expNext), new MemoryCacheEntryOptions 
            { 
                SlidingExpiration = TimeSpan.FromMinutes(30),
                Priority = CacheItemPriority.Normal
            });

            //Also add point (atomic UPDATE; see TryChangePoints)
            if (expCount != 0)
            {
                TryChangePoints(user, expCount, allowOverdraft: true);
            }
            return;
        }

        /// <summary>
        /// 原子地变更积分:数据库里一条 UPDATE ... SET Points = Points + @delta,不再"读-改-写",
        /// 并发请求不会互相覆盖(双花、丢更新)。
        /// allowOverdraft=false(默认)时额外要求 Points + delta >= 0:余额不足返回 false,且不扣。
        /// 扣费的调用方必须检查返回值。调用方已开事务时,UPDATE 在该事务内执行(回滚时一并回滚)。
        /// 成功后会把 user.Points 同步为数据库里的新值,并让 EF 不再把旧值写回。
        /// </summary>
        public bool TryChangePoints(UserProfile user, int delta, bool allowOverdraft = false)
        {
            if (user.Id == 0)
            {
                // 还没入库的新实体:保持原行为,由调用方 SaveChanges 时一并写入
                user.Points += delta;
                return true;
            }
            int id = user.Id;
            var q = _udb.Users.Where(u => u.Id == id);
            if (delta < 0 && !allowOverdraft)
            {
                q = q.Where(u => u.Points + delta >= 0);
            }
            int rows = q.ExecuteUpdate(s => s.SetProperty(u => u.Points, u => u.Points + delta));
            if (rows == 0)
            {
                return false;
            }
            int newPoints = _udb.Users.AsNoTracking().Where(u => u.Id == id).Select(u => u.Points).Single();
            user.Points = newPoints;
            var entry = _udb.Entry(user);
            if (entry.State != EntityState.Detached)
            {
                var prop = entry.Property(u => u.Points);
                prop.OriginalValue = newPoints;
                prop.IsModified = false;
            }
            _cache.Remove(PointsCacheKey + user.UserName);
            return true;
        }

        private const int PostCooldownSeconds = 30;
        private static readonly object PostCooldownLock = new object();

        /// <summary>
        /// 发言冷却(评论、回复):30 秒,等级 10 以上不受限。按用户名记录在内存缓存里,不依赖 Session
        /// (原来的 Session 冷却不带 Cookie 就能绕过,新版接口则完全没有冷却)。
        /// "检查并占位"在同一个锁里完成,并发请求不会一起通过。
        /// 返回 0 表示可以发言(同时记下本次时间);大于 0 表示还需要等待的秒数。
        /// 调用方应在内容校验通过、真正写入之前调用。
        /// </summary>
        public int TryBeginPost(string username)
        {
            if (getUserLvl(username) >= 10)
            {
                return 0;
            }
            string key = "~lastpost" + username;
            lock (PostCooldownLock)
            {
                var last = _cache.Get<DateTime?>(key);
                if (last.HasValue)
                {
                    double remain = PostCooldownSeconds - (DateTime.Now - last.Value).TotalSeconds;
                    if (remain > 0)
                    {
                        return (int)Math.Ceiling(remain);
                    }
                }
                _cache.Set<DateTime?>(key, DateTime.Now, TimeSpan.FromMinutes(2));
                return 0;
            }
        }

        public void AddPoint(UserProfile user, int point)
        {
            TryChangePoints(user, point, allowOverdraft: true);
        }

        public async Task addPointAsync(string username, int pCount)
        {
            var user = await _udb.Users.SingleAsync(u => u.UserName == username);
            TryChangePoints(user, pCount, allowOverdraft: true);
        }

        public async Task<Tuple<bool, int>> trySpendPointAsync(string username, int amount)
        {
            var user = await _udb.Users.SingleAsync(u => u.UserName == username);
            if (!TryChangePoints(user, -amount))
            {
                int remain = await _udb.Users.AsNoTracking().Where(u => u.Id == user.Id).Select(u => u.Points).SingleAsync();
                return new Tuple<bool, int>(false, remain);
            }
            return new Tuple<bool, int>(true, user.Points);
        }

        public bool HasSigned(out int ConsecutiveSign)
        {
            ConsecutiveSign = 0;
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }
            string username = HttpContext.User.Identity.Name;
            var signday = _cache.Get<int?>(SignCacheKey + username);
            if (signday.HasValue)
            {
                ConsecutiveSign = signday.Value;
                return ConsecutiveSign > 0;
            }
            var profile = CurrentUser;
            if (profile == null)
            {
                return false;
            }
            TimeSpan diffday = DateTime.Today - profile.LastSignDate.Date;
            if (diffday.Days >= 1)
            {
                signday = -diffday.Days;
            }
            else
            {
                signday = profile.ConsecutiveSign;
            }
            ConsecutiveSign = signday.Value;
            _cache.Set(SignCacheKey + username, signday, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return ConsecutiveSign > 0;
        }

        public bool HasRated()
        {
            string username = HttpContext.User.Identity.Name;
            bool? result = _cache.Get<bool?>(HasRatedCacheKey + username);
            if (result.HasValue)
                return result.Value;
            var quest = CurrentUser?.quest;
            if (quest == null || (quest.LastRateDate == null || quest.LastRateDate.Value.Date < DateTime.Today))
            {
                result = false;
            }
            else
                result = true;
            _cache.Set(HasRatedCacheKey + username, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return result.Value;
        }

        public bool HasPosted()
        {
            string username = HttpContext.User.Identity.Name;
            bool? result = _cache.Get<bool?>(HasPostedCacheKey + username);
            if (result.HasValue)
                return result.Value;
            var quest = CurrentUser?.quest;
            if (quest == null || (quest.LastPostDate == null || quest.LastPostDate.Value.Date < DateTime.Today))
            {
                result = false;
            }
            else
                result = true;
            _cache.Set(HasPostedCacheKey + username, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return result.Value;
        }

        public int HasBlogged()
        {
            string username = HttpContext.User.Identity.Name;
            var result = _cache.Get<int?>(HasBloggedCacheKey + username);
            if (result.HasValue)
                return result.Value;
            var quest = CurrentUser?.quest;
            if (quest == null || quest.LastBlogDate == null || quest.LastBlogDate.Value.Date < DateTime.Today)
                result = 0;
            else
                result = quest.DayBlogCount;
            _cache.Set(HasBloggedCacheKey + username, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return result.Value;
        }

        public int WeekBlogged()
        {
            string username = HttpContext.User.Identity.Name;
            int? result = _cache.Get<int?>(WeekBloggedCacheKey + username);
            if (result.HasValue)
                return result.Value;
            var quest = CurrentUser?.quest;
            if (quest == null || quest.LastBlogDate == null || !quest.LastBlogDate.Value.isSameWeek(DateTime.Today))
                result = 0;
            else
                result = quest.WeekBlogCount;
            _cache.Set(WeekBloggedCacheKey + username, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return result.Value;
        }

        /// <summary>
        /// Number of days in the current week on which the user completed all required daily quests
        /// (sign-in + comment + rating). Returns 0 once the stored week has rolled over.
        /// </summary>
        public int WeekAllDaily()
        {
            string username = HttpContext.User.Identity.Name;
            int? result = _cache.Get<int?>(WeekAllDailyCacheKey + username);
            if (result.HasValue)
                return result.Value;
            var quest = CurrentUser?.quest;
            if (quest == null || quest.LastAllDailyDate == null || !quest.LastAllDailyDate.Value.isSameWeek(DateTime.Today))
                result = 0;
            else
                result = quest.WeekAllDailyCount;
            _cache.Set(WeekAllDailyCacheKey + username, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return result.Value;
        }

        /// <summary>
        /// Records that the user finished today's required daily quests (sign-in + comment + rating),
        /// advancing the weekly all-daily quest. Awards <see cref="QuestService.weekAllDailyTicket"/>
        /// 补签券 once all <see cref="QuestService.weekAllDailyCount"/> days of the week are complete.
        /// </summary>
        /// <returns>True if the quest state changed and the caller must persist it.</returns>
        public bool CheckAllDailyQuest(UserProfile profile)
        {
            var quest = profile?.quest;
            if (quest == null)
            {
                return false;
            }
            bool signedToday = profile.LastSignDate.Date == DateTime.Today;
            bool postedToday = quest.LastPostDate != null && quest.LastPostDate.Value.Date == DateTime.Today;
            bool ratedToday = quest.LastRateDate != null && quest.LastRateDate.Value.Date == DateTime.Today;
            if (!signedToday || !postedToday || !ratedToday)
            {
                return false;
            }
            var prevDay = quest.LastAllDailyDate;
            if (prevDay.HasValue && prevDay.Value.Date == DateTime.Today)
            {
                return false; // already counted today
            }
            quest.LastAllDailyDate = DateTime.Today;
            if (prevDay.HasValue && prevDay.Value.isSameWeek(DateTime.Today))
            {
                quest.WeekAllDailyCount++;
            }
            else
            {
                quest.WeekAllDailyCount = 1;
            }
            if (quest.WeekAllDailyCount == QuestService.weekAllDailyCount)
            {
                quest.PunchInTicket += QuestService.weekAllDailyTicket;
            }
            else if (quest.WeekAllDailyCount > QuestService.weekAllDailyCount)
            {
                quest.WeekAllDailyCount = QuestService.weekAllDailyCount;
            }
            _cache.Remove(WeekAllDailyCacheKey + profile.UserName);
            return true;
        }

        public bool HasRatedPost()
        {
            string username = HttpContext.User.Identity.Name;
            bool? result = _cache.Get<bool?>(HasRatedPostCacheKey + username);
            if (result.HasValue)
                return result.Value;
            var quest = CurrentUser?.quest;
            if (quest == null || (quest.LastRatePostDate == null || quest.LastRatePostDate.Value.Date < DateTime.Today))
            {
                result = false;
            }
            else
                result = true;
            _cache.Set(HasRatedPostCacheKey + username, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) });
            return result.Value;
        }

        public string GetQuestProgress()
        {
            int p = 0;
            int total = 5;
            if (HasSigned(out int i))
            {
                p++;
            }
            if (HasPosted())
            {
                p++;
            }
            if (HasRated())
            {
                p++;
            }
            if (HasRatedPost())
            {
                p++;
            }
            int blogcount = HasBlogged();
            int weekcount = WeekBlogged();
            int alldailycount = WeekAllDaily();
            if (blogcount > 0)
            {
                p++;
                total++;
                if (blogcount >= 5)
                    p++;
            }
            if (weekcount > 0)
            {
                total++;
                if (weekcount >= 15)
                    p++;
            }
            if (alldailycount > 0)
            {
                total++;
                if (alldailycount >= QuestService.weekAllDailyCount)
                    p++;
            }
            return p + " / " + total;
        }

        public bool setRateDateAddExp(string username)
        {
            bool isNewRate = true;
            var profile = _udb.Users.Include(u => u.quest).SingleOrDefault(u => u.UserName == username);
            if (profile == null)
            {
                return false;
            }
            if (profile.quest == null)
            {
                profile.quest = new UserQuest { UserId = profile.Id };
            }
            if (profile.quest.LastRateDate != null)
            {
                TimeSpan diffday = DateTime.Today - profile.quest.LastRateDate.Value.Date;
                if (diffday.Days == 0)
                {
                    isNewRate = false;
                }
            }
            profile.quest.LastRateDate = DateTime.Today;
            bool dailyQuestChanged = CheckAllDailyQuest(profile);
            if (isNewRate)
            {
                addExp(profile, RateExp);
            }
            if (isNewRate || dailyQuestChanged)
            {
                _udb.SaveChanges();
            }
            return isNewRate;
        }

        public bool SetRatePostDateAddExp(string username)
        {
            bool isNewRate = true;
            var profile = _udb.Users.Include(u => u.quest).SingleOrDefault(u => u.UserName == username);
            if (profile == null)
            {
                return false;
            }
            if (profile.quest == null)
            {
                profile.quest = new UserQuest { UserId = profile.Id };
            }
            if (profile.quest.LastRatePostDate != null)
            {
                TimeSpan diffday = DateTime.Today - profile.quest.LastRatePostDate.Value.Date;
                if (diffday.Days == 0)
                {
                    isNewRate = false;
                }
            }
            profile.quest.LastRatePostDate = DateTime.Today;
            if (isNewRate)
            {
                addExp(profile, RateExp);
                _udb.SaveChanges();
            }
            return isNewRate;
        }

        public bool setPostDateAddExp(string username)
        {
            bool isNewPost = true;
            var profile = _udb.Users.Include(u => u.quest).SingleOrDefault(u => u.UserName == username);
            if (profile == null)
            {
                return false;
            }
            if (profile.quest == null)
            {
                profile.quest = new UserQuest { UserId = profile.Id };
            }
            if (profile.quest.LastPostDate != null)
            {
                TimeSpan diffday = DateTime.Today - profile.quest.LastPostDate.Value.Date;
                if (diffday.Days == 0)
                {
                    isNewPost = false;
                }
            }
            profile.quest.LastPostDate = DateTime.Today;
            bool dailyQuestChanged = CheckAllDailyQuest(profile);
            if (isNewPost)
            {
                addExp(profile, PostExp);
            }
            if (isNewPost || dailyQuestChanged)
            {
                _udb.SaveChanges();
            }
            return isNewPost;
        }

        public void addRankExp(IEnumerable<HistoryRanking> blogs, List<int> reward, string type)
        {
            for (int i = 0; i < blogs.Count() && i < reward.Count; i++)
            {
                var rank = blogs.ElementAt(i);
                addExp(rank.Author, reward[i]);
                var content = string.Format("恭喜！您的投稿 <a href='/gm{3}'>{4}</a> 今天在{0}上位屋的第{1}名，特此奖励{2}点经验度和棒棒糖！", type[..2], i + 1, reward[i], rank.BlogID, rank.BlogTitle);
                _msgUtil.SendRankNotice(rank.Author, content);
                _adminUtil.log("admin", type + "#" + (i + 1) + "(" + reward[i] + ")", rank.Author + "@gm" + rank.BlogID);
            }
        }

        public static string GetIPAddress(HttpContext context)
        {
            string ipAddress = context.Request.Headers["X-Forwarded-For"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Connection.RemoteIpAddress.ToString();
        }
    }
}