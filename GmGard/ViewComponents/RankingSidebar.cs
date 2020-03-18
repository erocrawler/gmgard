using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Entity;
using Microsoft.Extensions.Options;

namespace GmGard.ViewComponents
{
    public class RankingSidebar : ViewComponent
    {
        private readonly IVisitCounter _visitCounter;
        private readonly IMemoryCache _cache;
        private readonly BlogContext _db;
        private readonly BlogUtil _blogUtil;
        private readonly TimeSpan _cacheInterval;

        public RankingSidebar(BlogContext db, BlogUtil blogUtil, IMemoryCache cache, IVisitCounter visitCounter, IOptions<AppSettingsModel> appsetting)
        {
            _db = db;
            _blogUtil = blogUtil;
            _cache = cache;
            _visitCounter = visitCounter;
            _cacheInterval = TimeSpan.FromMinutes(appsetting.Value.UpdateInterval > 0 ? appsetting.Value.UpdateInterval : 10);
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var latestRanking = await _cache.GetOrCreateAsync("~RankSidebar", async e =>
            {
                e.AbsoluteExpirationRelativeToNow = _cacheInterval;
                var latestData = await _db.HistoryRankings.AsNoTracking().Where(g => g.RankType != HistoryRanking.Type.RankDaily).GroupBy(g => g.RankType).SelectMany(g => g.OrderByDescending(d => d.RankDate).Take(5))
                    .Select(h => new { HistoryRanking = h, Deleted = !_db.Blogs.Any(b => b.BlogID == h.BlogID) })
                    .ToListAsync();
                var name2nick = _blogUtil.GetNickNames(latestData.Select(r => r.HistoryRanking.Author));
                return latestData.Where(a => !a.Deleted).Select(h => new RankingDisplay
                {
                    Author = name2nick[h.HistoryRanking.Author],
                    BlogDate = h.HistoryRanking.BlogDate,
                    BlogID = h.HistoryRanking.BlogID,
                    BlogThumb = h.HistoryRanking.BlogThumb,
                    BlogTitle = h.HistoryRanking.BlogTitle,
                    BlogVisit = h.HistoryRanking.BlogVisit,
                    BlogUrl = Url.Action("Details", "Blog", new { id = h.HistoryRanking.BlogID }),
                    RankDate = h.HistoryRanking.RankDate,
                    Rating = h.HistoryRanking.Rating,
                    RankType = h.HistoryRanking.RankType,
                    PostCount = h.HistoryRanking.PostCount
                });
            });
            var ids = latestRanking.Select(c => c.BlogID);
            _visitCounter.PrepareBlogVisits(ids);
            var ratingUtil = ViewContext.HttpContext.RequestServices.GetService(typeof(RatingUtil)) as RatingUtil;
            ratingUtil.PrepareRatings(ids);
            return View(latestRanking);
        }
    }
}
