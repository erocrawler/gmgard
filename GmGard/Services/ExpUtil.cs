using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Http;
using GmGard.Extensions;
using System.Threading.Tasks;
using System.Data.Entity;

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
                    _currentUser = _udb.Users.AsNoTracking().Include("quest").SingleOrDefault(u => u.UserName == HttpContext.User.Identity.Name);
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
                var username = user.UserName.ToLower();
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
                _cache.Set(ExpTableKey, table);
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
            username = username.ToLower();
            int? lvl = _cache.Get<int?>(LevelCacheKey + username);
            if (lvl.HasValue)
                return lvl.Value;
            lvl = _udb.Users.AsNoTracking().SingleOrDefault(u => u.UserName == username)?.Level;
            _cache.Set(LevelCacheKey + username, lvl);
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
            username = username.ToLower();
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
            _cache.Set(LevelCacheKey + username, level);
            _cache.Set(ExpCacheKey + username, new Tuple<int, int>(expCurrent, expNext));
        }

        public int getUserPoints(string username)
        {
            username = username.ToLower();
            int? points = _cache.Get<int?>(PointsCacheKey + username);
            if (points == null)
            {
                var user = _udb.Users.AsNoTracking().SingleOrDefault(u => u.UserName == username);
                if (user == null)
                    return 0;

                points = user.Points;
                _cache.Set(PointsCacheKey + username, points);
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
            var username = user.UserName.ToLower();
            _cache.Set(LevelCacheKey + username, user.Level);
            _cache.Set(ExpCacheKey + username, new Tuple<int, int>(expCur, expNext));

            //Also add point
            user.Points += expCount;
            _cache.Set<int?>(PointsCacheKey + username, user.Points);
            return;
        }

        public void AddPoint(UserProfile user, int point)
        {
            user.Points += point;
            _cache.Set<int?>(PointsCacheKey + user.UserName.ToLower(), user.Points);
        }

        public async Task addPointAsync(string username, int pCount)
        {
            var user = await _udb.Users.SingleAsync(u => u.UserName == username);
            AddPoint(user, pCount);
            await _udb.SaveChangesAsync();
        }

        public async Task<Tuple<bool, int>> trySpendPointAsync(string username, int pCount)
        {
            username = username.ToLower();
            var user = await _udb.Users.SingleAsync(u => u.UserName == username);
            int remain = user.Points;
            if (user.Points >= pCount)
            {
                await addPointAsync(username, -pCount);
                remain -= pCount;
            }
            else
            {
                return new Tuple<bool, int>(false, remain);
            }
            return new Tuple<bool, int>(true, remain);
        }

        public bool HasSigned(out int ConsecutiveSign)
        {
            ConsecutiveSign = 0;
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }
            string username = HttpContext.User.Identity.Name.ToLower();
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
            string username = HttpContext.User.Identity.Name.ToLower();
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
            string username = HttpContext.User.Identity.Name.ToLower();
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
            string username = HttpContext.User.Identity.Name.ToLower();
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
            string username = HttpContext.User.Identity.Name.ToLower();
            var result = _cache.Get<int?>(WeekBloggedCacheKey + username);
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

        public bool HasRatedPost()
        {
            string username = HttpContext.User.Identity.Name.ToLower();
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
            return p + " / " + total;
        }

        public bool setRateDateAddExp(string username)
        {
            bool isNewRate = true;
            var profile = _udb.Users.Include("quest").SingleOrDefault(u => u.UserName == username);
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
            if (isNewRate)
            {
                addExp(profile, RateExp);
                _udb.SaveChanges();
            }
            return isNewRate;
        }

        public bool SetRatePostDateAddExp(string username)
        {
            bool isNewRate = true;
            var profile = _udb.Users.Include("quest").SingleOrDefault(u => u.UserName == username);
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
            var profile = _udb.Users.SingleOrDefault(u => u.UserName == username);
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
            if (isNewPost)
            {
                addExp(profile, PostExp);
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
                var content = string.Format("恭喜！您的投稿 <a href='/gm{3}'>{4}</a> 今天在{0}上位居第{1}，特此奖励{2}点绅士度和棒棒糖！", type.Substring(0, 2), i + 1, reward[i], rank.BlogID, rank.BlogTitle);
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