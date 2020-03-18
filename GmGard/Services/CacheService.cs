using GmGard.Controllers;
using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;

namespace GmGard.Services
{
    public class CacheService
    {
        public CacheService(IMemoryCache cache)
        {
            _cache = cache;
            RegisterCacheEvents();
        }

        private readonly IMemoryCache _cache;
        
        public const string HomePageCacheKey = "~HPList";
        public const string BlogListCacheKey = "~BGList";
        public const string HomePageCarouselCacheKey = "~HPCarousel";
        public const string PostCountCacheKey = "~postcount";
        public const string HanGroupListCacheKey = "~HGList";
        public const string HanGroupsCacheKey = "~HGroups";
        public const string CarouselAdKey = "~CarouselAd";
        public const string SidebarAdKey = "~SidebarAd";
        public const string BannerAdKey = "~BannerAd";
        public const string SidebarBannerAdKey = "~SbBannerAd";

        public static string GetHanGroupCacheKey(string GroupName) => $"~HG~{GroupName}";

        public static string GetHomePageListKey(int PageNumber, bool IsHarmony, bool hideHarmony, string categoryIds)
        {
            return PageNumber.ToString() + (IsHarmony ? 'T' : (hideHarmony ? 'H' : 'F')).ToString() + categoryIds;
        }

        public static string GetAvatarCacheKey(string Username) => $"~Avatar{Username.ToLower()}";

        public static string GetIsFavCacheKey(int Id, string Username) => $"~fav~{Id}~{Username.ToLower()}";

        public static string GetBlogFirstLineKey(int id) => $"~BFL~{id}";

        public void RegisterCacheEvents()
        {
            // We don't need to clear homepage cache on rate blog because rating is stored in different cache and will be get/set individually.
            //BlogController.OnRateBlog += ClearHPCache;
            //ReplyController.OnRateBlog += ClearHPCache;
            ReplyController.OnAddPost += ClearPostCount;
            Controllers.App.ReplyController.OnAddPost += ClearPostCount;
            ReplyController.OnDeletePost += ClearPostCount;
            Controllers.App.ReplyController.OnDeletePost += ClearPostCount;
            AuditController.OnApproveBlog += ClearHPCache;
            BlogController.OnNewBlog += ClearHPCache;
            BlogController.OnDeleteBlog += ClearHPCache;
            BlogController.OnEditBlog += ClearHPCache;
            BlogController.OnEditBlog += ClearBlogFirstLineCache;
            AdminController.OnSettingsChanged += ClearHPCache;
            AdminController.OnSettingsChanged += ClearCarouselCache;
            TopicController.OnNewTopic += ClearCarouselCache;
            TopicController.OnEditTopic += ClearCarouselCache;
        }

        private void ClearBlogFirstLineCache(object sender, BlogEventArgs e)
        {
            _cache.Remove(GetBlogFirstLineKey(e.Model.BlogID));
        }

        public void ClearPostCount(object sender, PostEventArgs p)
        {
            var context = sender as Controller;
            if (p.Model != null)
            {
                switch (p.Model.IdType)
                {
                    case ItemType.Blog:
                        var cache = _cache.Get<ConcurrentDictionary<int, int>>(PostCountCacheKey);
                        if (cache != null)
                        {
                            int notused;
                            cache.TryRemove(p.Model.ItemId, out notused);
                        }
                        break;
                    case ItemType.Topic:
                        var tcache = _cache.Get<ConcurrentDictionary<int, int>>(PostCountCacheKey + "T");
                        if (tcache != null)
                        {
                            int notused;
                            tcache.TryRemove(p.Model.ItemId, out notused);
                        }
                        break;
                    default:
                        break;
                }
                
            }
        }

        public void ClearHPCache(object sender, System.EventArgs o)
        {
            var context = sender as Controller;
            _cache.Remove(HomePageCacheKey);
            _cache.Remove(BlogListCacheKey);
            _cache.Remove("UnapproveCount");
        }

        public void ClearCarouselCache(object sender, System.EventArgs o)
        {
            var context = sender as Controller;
            _cache.Remove(HomePageCarouselCacheKey);
        }
    }
}