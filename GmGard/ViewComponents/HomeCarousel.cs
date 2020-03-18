using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.ViewComponents
{
    public class HomeCarousel : ViewComponent
    {
        private readonly BlogContext _db;
        private readonly DataSettingsModel _dataSettings;
        private readonly IMemoryCache _cache;
        private readonly int _topicCarouselSize;

        public HomeCarousel(BlogContext db, IOptionsSnapshot<DataSettingsModel> dataSettings, IOptions<AppSettingsModel> appSettings, IMemoryCache cache)
        {
            _db = db;
            _dataSettings = dataSettings.Value;
            _cache = cache;
            _topicCarouselSize = appSettings.Value.TopicCarouselSize;
        }

        public IViewComponentResult Invoke()
        {
            List<CarouselDisplay> model = _cache.Get<List<CarouselDisplay>>(CacheService.HomePageCarouselCacheKey);
            if (model == null)
            {
                model = new List<CarouselDisplay>();
                var bannerBlogId = _dataSettings.BannerBlogIdList();
                if (bannerBlogId != null && bannerBlogId.Count > 0)
                {
                    var blogs = _db.Blogs.Where(b => bannerBlogId.Contains(b.BlogID)).ToList();
                    model.AddRange(blogs.OrderBy(b => bannerBlogId.IndexOf(b.BlogID)).Select(b => CarouselDisplay.FromBlog(Url, b)));
                }
                var query = _db.Topics.Where(t => t.BannerPath != null);
                var bannerTopicId = _dataSettings.BannerTopicIdList();
                if (bannerTopicId != null && bannerTopicId.Count > 0)
                {
                    query = query.OrderByDescending(t => bannerTopicId.Contains(t.TopicID)).ThenByDescending(t => t.UpdateDate);
                }
                else
                {
                    query = query.OrderByDescending(t => t.UpdateDate);
                }
                var topics = query.Take(_topicCarouselSize).ToList();
                model.AddRange(topics.Select(tt => CarouselDisplay.FromTopic(Url, tt)));
                _cache.Set(CacheService.HomePageCarouselCacheKey, model);
            }
            return View(model);
        }
    }
}
