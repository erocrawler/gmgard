using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GmGard.Models;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Html;

namespace GmGard.Services
{
    public class ContextlessBlogUtil : UtilityService
    {
        private INickNameProvider _nicknameProvider;
        private IVisitCounter _visitCounter;

        public ContextlessBlogUtil(
            BlogContext db, 
            UsersContext udb, 
            IMemoryCache cache, 
            IVisitCounter visitCounter,
            INickNameProvider nicknameProvider) : base(db, udb, cache)
        {
            _nicknameProvider = nicknameProvider;
            _visitCounter = visitCounter;
        }

        public string GetPostTitle(Post p)
        {
            switch (p.IdType)
            {
                case ItemType.Blog:
                    switch (p.ItemId)
                    {
                        case 0:
                            return "意见建议";

                        case -1:
                            return "问题汇报";

                        case -2:
                            return "历史排行";

                        default:
                            return _db.Blogs.Single(b => b.BlogID == p.ItemId).BlogTitle;
                    }
                case ItemType.Topic:
                    return _db.Topics.Single(t => t.TopicID == p.ItemId).TopicTitle;
                    //case ItemType.Bounty:
                    //    return db.Bounty.Find(ItemId).BountyTitle;
                    //case ItemType.Answer:
                    //    // TODO: find bounty id here
                    //    return db.Bounty.Find(ItemId).BountyTitle;
            }
            throw new InvalidEnumArgumentException("IdType", (int)p.IdType, p.IdType.GetType());
        }

        public T GetUserOption<T>(string user, Func<UserOption, T> option)
        {
            user = user.ToLower();
            UserOption useroption = _cache.Get<UserOption>("useroption" + user);
            if (useroption == null)
            {
                useroption = _udb.UserOptions.AsNoTracking().SingleOrDefault(u => u.user.UserName == user) ?? new UserOption();
                _cache.Set("useroption" + user, useroption);
            }
            return option(useroption);
        }

        public void CacheUserOption(UserOption currentoption, string user)
        {
            user = user.ToLower();
            _cache.Set("useroption" + user, currentoption);
        }

        public string GetUserDesc(string username)
        {
            username = username.ToLower();
            string desc = _cache.Get<string>("desc" + username);
            if (desc == null)
            {
                var p = _udb.Users.FirstOrDefault(u => u.UserName == username);
                desc = p.UserComment ?? string.Empty;
                _cache.Set("desc" + username, desc);
            }
            return desc;
        }

        public int GetFavCount(string username)
        {
            username = username.ToLower();
            var count = _cache.Get<int?>("favcount" + username);
            if (count == null)
            {
                count = _db.Favorites.Count(f => f.Username == username);
                _cache.Set("favcount" + username, count);
            }
            return count.Value;
        }

        public string GetNickName(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return string.Empty;
            }
            return _nicknameProvider.GetNickName(user);
        }

        public IDictionary<string, string> GetNickNames(IEnumerable<string> users)
        {
            return _nicknameProvider.GetNickNames(users);
        }

        public int GetPostCount(Blog blog)
        {
            return GetBlogPostCount(blog.BlogID);
        }

        public int GetBlogPostCount(int blogid)
        {
            var postCountCache = _cache.Get<ConcurrentDictionary<int, int>>(CacheService.PostCountCacheKey) ?? new ConcurrentDictionary<int, int>();
            int count = postCountCache.GetOrAdd(blogid, id => _db.Posts.Count(p => p.ItemId == id && p.IdType == ItemType.Blog));
            _cache.Set(CacheService.PostCountCacheKey, postCountCache, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
            return count;
        }

        public void PreparePostCount(IEnumerable<int> blogs)
        {
            var ids = new HashSet<int>(blogs);
            var result = new Dictionary<int, int>(ids.Count());
            var postCountCache = _cache.Get<ConcurrentDictionary<int, int>>(CacheService.PostCountCacheKey) ?? new ConcurrentDictionary<int, int>();
            var uncached = ids.Where(i => !postCountCache.ContainsKey(i));
            if (uncached.Count() > 0)
            {
                var postCounts = _db.Posts.Where(u => uncached.Contains(u.ItemId) && u.IdType == ItemType.Blog)
                    .GroupBy(p => p.ItemId).ToDictionary(u => u.Key, u => u.Count());
                foreach (var id in uncached)
                {
                    int count = 0;
                    postCounts.TryGetValue(id, out count);
                    postCountCache.TryAdd(id, count);
                }
            }
            _cache.Set(CacheService.PostCountCacheKey, postCountCache, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
        }

        public void PrepareListCache(IEnumerable<Blog> list)
        {
            if (list == null || list.Count() == 0)
            {
                return;
            }
            var blogIds = list.Select(b => b.BlogID);
            _visitCounter.PrepareBlogVisits(blogIds);
            PreparePostCount(blogIds);
            RatingUtil.PrepareRatings(_cache, _db, blogIds);
            GetNickNames(list.Select(b => b.Author));
        }

        public int GetUnapproveCount()
        {
            int? c = _cache.Get<int?>("UnapproveCount");
            if (c == null)
            {
                c = _db.Blogs.Count(b => b.isApproved == null && b.BlogID > 0);
                _cache.Set("UnapproveCount", c, TimeSpan.FromMinutes(10));
            }
            return c.Value;
        }

        public string GetNewSuggestionCount()
        {
            int c1, c2, c3;
            string c = _cache.Get<string>("NewSuggestionCount");
            if (c == null)
            {
                // 0 = Suggestions, -1 = Report
                var specialBlogs = _db.Posts.Where(p => (p.ItemId == 0 || p.ItemId == -1) && p.IdType == ItemType.Blog && DbFunctions.DiffDays(p.PostDate, DateTime.Now) < 1);
                c1 = specialBlogs.Count(p => p.ItemId == 0);
                c2 = specialBlogs.Count(p => p.ItemId == -1);
                c3 = _udb.Messages.Count(p => p.Recipient == "admin" && DbFunctions.DiffDays(p.MsgDate, DateTime.Now) < 1);
                c = string.Format("{0}/{1}/{2}", c1, c2, c3);
                _cache.Set<string>("NewSuggestionCount", c, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return c;
        }

        public HtmlString GetFirstLine(Blog b, bool RemoveTags = false)
        {
            var cached = _cache.GetOrCreate(CacheService.GetBlogFirstLineKey(b.BlogID), e => BlogHelper.getFirstLine(b.Content, 100));
            if (RemoveTags)
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(cached.Value);
                return new HtmlString(doc.DocumentNode.InnerText);
            }
            return cached;
        }
    }
}