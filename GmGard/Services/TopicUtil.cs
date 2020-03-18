using GmGard.Models;
using System.Linq;
using System.Web;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace GmGard.Services
{
    public class TopicUtil : UtilityService
    {
        public TopicUtil(BlogContext db, UsersContext udb, IMemoryCache cache) : base(db, udb, cache)
        {
        }

        public int getBlogCount(int id)
        {
            return _db.BlogsInTopics.Count(t => t.TopicID == id);
        }

        public string getBlogTitle(int id)
        {
            Blog b = _db.Blogs.Find(id);
            if (b == null || id <= 0)
                return null;
            else
                return b.BlogTitle;
        }

        public int getPostCount(Topic t)
        {
            var postCountCache = _cache.Get<ConcurrentDictionary<int, int>>(CacheService.PostCountCacheKey + "T") ?? new ConcurrentDictionary<int, int>();
            int count = postCountCache.GetOrAdd(t.TopicID, i => _db.Posts.Count(p => p.ItemId == i && p.IdType == ItemType.Topic));
            _cache.Set(CacheService.PostCountCacheKey + "T", postCountCache, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
            return count;
        }
    }
}