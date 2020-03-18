using static GmGard.Services.CacheService;
using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Data.Entity;

namespace GmGard.ViewComponents
{
    public class Advertisement : ViewComponent
    {
        public const int MinAds = 12;
        public const int AdsPerPage = 4;
        private readonly BlogContext _db;
        private readonly IMemoryCache _cache;

        public Advertisement(BlogContext context, IMemoryCache cache)
        {
            _db = context;
            _cache = cache;
        }

        public IViewComponentResult Invoke(AdvertisementType AdType, string pos)
        {
            switch (AdType)
            {
                case AdvertisementType.Carousel:
                case AdvertisementType.CarouselBanner:
                    return Carousel();
                case AdvertisementType.Sidebar:
                    return Sidebar(pos);
                case AdvertisementType.Banner:
                    return Banner();
                case AdvertisementType.SidebarBanner1:
                case AdvertisementType.SidebarBanner2:
                    return SidebarBanner(AdType);
                default:
                    throw new ArgumentException("Unknown Ad type", "AdType");
            }
        }

        public IViewComponentResult Carousel()
        {
            List<Advertisment> ads = null;
            List<Advertisment> query = _cache.Get<List<Advertisment>>(CarouselAdKey);
            if (query == null)
            {
                query = _db.Advertisments.Where(a => a.AdType == AdvertisementType.Carousel || a.AdType == AdvertisementType.CarouselBanner).ToList();
                _cache.Set(CarouselAdKey, query);
            }
            if (query.Count() > 0)
            {
                Random random = new Random();
                var itemads = query.Where(a => a.AdType == AdvertisementType.Carousel);
                var fixedads = itemads.Where(a => a.AdOrder != null)
                    .GroupBy(a => a.AdOrder)
                    .Select(g => g.OrderBy(_ => random.Next()).First())
                    .OrderBy(a => a.AdOrder)
                    .ToList();
                var orderads = itemads.Where(a => a.AdOrder == null).OrderByDescending(a => a.ClickCount);
                ads = orderads.Take(AdsPerPage).ToList();
                var restcount = MinAds - fixedads.Count - ads.Count;
                if (restcount < 0)
                {
                    restcount = restcount % AdsPerPage + AdsPerPage;
                }
                var restads = orderads.Skip(AdsPerPage).OrderBy(_ => random.Next()).Take(restcount);
                ads.InsertRange(0, fixedads);
                ads.AddRange(restads);

                var bannerads = query.Where(a => a.AdType == AdvertisementType.CarouselBanner);
                ads.AddRange(bannerads.GroupBy(a => a.AdOrder).Select(g => g.OrderBy(_ => random.Next()).First()));
            }
            return View("Carousel", ads);
        }

        public IViewComponentResult Sidebar(string pos)
        {
            List<Advertisment> query = _cache.Get<List<Advertisment>>(SidebarAdKey);
            if (query == null)
            {
                query = _db.Advertisments.Where(a => a.AdType == AdvertisementType.Sidebar).ToList();
                _cache.Set(SidebarAdKey, query);
            }
            var ads = query.GroupBy(a => a.AdOrder).Select(g => g.OrderBy(_ => Guid.NewGuid()).First()).OrderBy(a => a.AdOrder);
            if (pos == "bottom")
            {
                ads = ads.OrderByDescending(a => a.AdOrder);
            }
            return View("Sidebar", ads.ToList());
        }

        public IViewComponentResult Banner()
        {
            List<Advertisment> query = _cache.Get<List<Advertisment>>(BannerAdKey);
            if (query == null)
            {
                query = _db.Advertisments.Where(a => a.AdType == AdvertisementType.Banner).ToList();
                _cache.Set(BannerAdKey, query);
            }
            var ads = query.GroupBy(a => a.AdOrder).Select(g => g.OrderBy(_ => Guid.NewGuid()).First()).ToList();
            return View("Banner", ads);
        }

        public IViewComponentResult SidebarBanner(AdvertisementType adType)
        {
            List<Advertisment> query = _cache.Get<List<Advertisment>>(SidebarBannerAdKey);
            if (query == null)
            {
                query = _db.Advertisments.Where(a => a.AdType == AdvertisementType.SidebarBanner1 || a.AdType == AdvertisementType.SidebarBanner2).ToList();
                _cache.Set(SidebarBannerAdKey, query);
            }
            var ads = query.Where(a => a.AdType == adType).GroupBy(a => a.AdOrder).OrderBy(_ => Guid.NewGuid()).FirstOrDefault()?.ToList();
            return View("SidebarBanner", ads);
        }
    }
}
