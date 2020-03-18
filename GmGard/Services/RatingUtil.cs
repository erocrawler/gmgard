using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Data.Entity;
using System.Collections.Concurrent;

namespace GmGard.Services
{
    public class RatingUtil : UtilityService
    {
        private AppSettingsModel _appSettings;
        private IHttpContextAccessor _contextAccessor;

        private HttpContext Context => _contextAccessor.HttpContext;
        private bool RateWithAccount => "A".Equals(_appSettings.RateCredentialType);

        private void TriggerRateBlog(BlogRating r) => OnRateBlog?.Invoke(this, new RateEventArgs(r, Context));
        public static event EventHandler<RateEventArgs> OnRateBlog;

        public RatingUtil(BlogContext db, UsersContext udb, IMemoryCache cache, IOptions<AppSettingsModel> settings, IHttpContextAccessor contextAccessor) : base(db, udb, cache)
        {
            _appSettings = settings.Value;
            _contextAccessor = contextAccessor;
        }

        public static readonly Dictionary<int, int> RatingValue = new Dictionary<int, int>()
        {
            {1, -1}, {2, 0}, {3, 1}, {4, 3}, {5, 5}
        };

        public BlogRating AddBlogRating(int blogid, int value, string credential, int? postid = null)
        {
            if (!RatingValue.ContainsKey(value))
                return null;
            value = RatingValue[value];
            var rate = new BlogRating();
            rate.value = value;
            rate.ratetime = DateTime.Now;
            rate.credential = credential;
            rate.BlogID = blogid;
            rate.RatingID = Guid.NewGuid();
            rate.PostId = postid;
            _db.BlogRatings.Add(rate);
            _db.SaveChanges();
            TriggerRateBlog(rate);
            return rate;
        }

        public string getRating(string id, bool fromcache = true)
        {
            return getRating(int.Parse(id), fromcache);
        }

        public string getRating(int id, bool fromcache = true)
        {
            return GetRating(id, fromcache).ToString();
        }

        public BlogRatingDisplay GetRating(string id, bool fromcache = true)
        {
            return GetRating(int.Parse(id), fromcache);
        }

        public BlogRatingDisplay GetRating(int id, bool fromcache = true)
        {
            var ratings = _cache.Get<ConcurrentDictionary<int, BlogRatingDisplay>>("Rating~") ?? new ConcurrentDictionary<int, BlogRatingDisplay>();
            if (!ratings.TryGetValue(id, out BlogRatingDisplay rating) || fromcache == false)
            {
                var rates = _db.BlogRatings.Where(r => r.BlogID == id);
                rating = new BlogRatingDisplay
                {
                    BlogId = id,
                    CountByRating = rates.GroupBy(r => r.value).ToDictionary(v => v.Key, v => v.Count()),
                };
                ratings.AddOrUpdate(id, rating, (i, old) => rating);
                _cache.Set("Rating~", ratings, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
            }
            return rating;
        }

        public void PrepareRatings(IEnumerable<int> blogs)
        {
            var ids = new HashSet<int>(blogs);
            var result = new Dictionary<int, int>(ids.Count());
            var ratingCache = _cache.Get<ConcurrentDictionary<int, BlogRatingDisplay>>("Rating~") ?? new ConcurrentDictionary<int, BlogRatingDisplay>();
            var uncached = ids.Where(i => !ratingCache.ContainsKey(i));
            if (uncached.Count() > 0)
            {
                var rates = _db.BlogRatings.Where(r => uncached.Contains(r.BlogID))
                    .Select(r => new { r.BlogID, r.value })
                    .GroupBy(r => r.BlogID)
                    .ToList()
                    .Select(r => new BlogRatingDisplay {
                        BlogId = r.Key,
                        CountByRating = r.GroupBy(br => br.value).ToDictionary(v => v.Key, v => v.Count())
                    })
                    .ToDictionary(r => r.BlogId);
                foreach (var id in uncached)
                {
                    BlogRatingDisplay br;
                    if (!rates.TryGetValue(id, out br))
                    {
                        br = new BlogRatingDisplay { BlogId = id };
                    }
                    ratingCache.TryAdd(id, br);
                }
            }
            _cache.Set("Rating~", ratingCache, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
        }

        public UsersRating GetUsersRating(int blogid)
        {
            var credential = RateWithAccount ? Context.User.Identity.Name : ExpUtil.GetIPAddress(Context);
            BlogRating rating = null;
            if (RateWithAccount)
            {
                rating = _db.BlogRatings.FirstOrDefault(r => r.BlogID == blogid && r.credential == credential);
            }
            else
            {
                rating = _db.BlogRatings.FirstOrDefault(r => r.BlogID == blogid && r.credential == credential && DbFunctions.DiffDays(r.ratetime, DateTime.Today) == 0);
            }
            var self = new UsersRating
            {
                BlogID = blogid,
                credential = credential,
                RateWithAccount = RateWithAccount
            };
            if (rating != null)
            {
                self.Rating = rating;
                self.HasPost = rating.PostId.HasValue;
            }
            return self;
        }

        public Dictionary<int, int> GetUsersRatingValues(IEnumerable<int> blogids)
        {
            var credential = RateWithAccount ? Context.User.Identity.Name : ExpUtil.GetIPAddress(Context);
            IQueryable<BlogRating> ratings = null;
            if (RateWithAccount)
            {
                ratings = _db.BlogRatings.Where(r => blogids.Contains(r.BlogID) && r.credential == credential);
            }
            else
            {
                ratings = _db.BlogRatings.Where(r => blogids.Contains(r.BlogID) && r.credential == credential && DbFunctions.DiffDays(r.ratetime, DateTime.Today) == 0);
            }
            return ratings.ToDictionary(v => v.BlogID, v => v.value);
        }

        public string TryRateBlog(int id, int rating)
        {
            string credential = ExpUtil.GetIPAddress(Context);
            bool rated = false;
            if (RateWithAccount)
            {
                if (!Context.User.Identity.IsAuthenticated)
                {
                    return "login";
                }
                credential = Context.User.Identity.Name;
                rated = _db.BlogRatings.Any(r => r.BlogID == id && r.credential == credential);
            }
            else
            {
                rated = _db.BlogRatings.Any(r => r.BlogID == id && r.credential == credential && DbFunctions.DiffDays(r.ratetime, DateTime.Today) < 1);
            }
            if (rated)
            {
                return "rated" + (RateWithAccount ? "" : "_today");
            }
            var Rate = AddBlogRating(id, rating, credential);
            if (Rate == null)
            {
                return "error";
            }
            TriggerRateBlog(Rate);
            return "ok";
        }
    }
}
