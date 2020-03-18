using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;

namespace GmGard.Controllers
{
    [Authorize(Policy = "Harmony"), ResponseCache(CacheProfileName = "Never")]
    public class RankingController : Controller
    {
        public readonly DateTime minDate = new DateTime(2014, 1, 1);

        private BlogContext _db;
        private IMemoryCache _cache;
        private BlogUtil _blogUtil;

        public RankingController(
            BlogContext db,
            BlogUtil blogUtil,
            IMemoryCache cache)
        {
            _db = db;
            _blogUtil = blogUtil;
            _cache = cache;
        }

        public ViewResult History()
        {
            var today = DateTime.Today;
            var model = GetMonthRanking(today.Year, today.Month);
            return View(model);
        }

        [HttpPost]
        public JsonResult History(int year, int month)
        {
            return Json(GetMonthRanking(year, month));
        }

        protected MonthRanking GetMonthRanking(int year, int month)
        {
            var thisMonth = new DateTime(year, month, 1);
            if (thisMonth < minDate || thisMonth > DateTime.Today)
            {
                return null;
            }
            MonthRanking model;
            if (thisMonth.Month == DateTime.Today.Month && thisMonth.Year == DateTime.Today.Year)
            {
                model = _cache.Get<MonthRanking>("~ThisMonthRanking");
                if (model == null)
                {
                    model = GetMonthRankingInner(year, month);
                    _cache.Set("~ThisMonthRanking", model, new TimeSpan(0, 10, 0));
                }
            }
            else
            {
                var cache = _cache.Get<ConcurrentDictionary<DateTime, MonthRanking>>("~MonthRanking") ?? new ConcurrentDictionary<DateTime, MonthRanking>();
                model = cache.GetOrAdd(thisMonth, (s) =>
                {
                    _cache.Set("~MonthRanking", cache);
                    return GetMonthRankingInner(year, month);
                });
            }
            return model;
        }

        protected MonthRanking GetMonthRankingInner(int year, int month)
        {
            var thisMonth = new DateTime(year, month, 1);
            var monthEndDate = thisMonth.AddMonths(1).AddDays(-1);
            var model = new MonthRanking();
            var currentMonthData = _db.HistoryRankings.Where(h => DbFunctions.DiffMonths(h.RankDate, thisMonth) == 0 && DbFunctions.DiffYears(h.RankDate, thisMonth) == 0)
                .Select(h => new { HistoryRanking = h, PostCount = _db.Posts.Count(p => p.ItemId == h.BlogID && p.IdType == ItemType.Blog), Deleted = !_db.Blogs.Any(b => b.BlogID == h.BlogID) })
                .ToList();
            var name2nick = _blogUtil.GetNickNames(currentMonthData.Select(r => r.HistoryRanking.Author));
            var currentMonthRankingData = currentMonthData.Select(h => new RankingDisplay
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
                PostCount = h.PostCount,
                RankType = h.HistoryRanking.RankType,
                Deleted = h.Deleted,
            });

            var dailyRanking = currentMonthRankingData.Where(d => d.RankType == HistoryRanking.Type.RankDaily).ToList();
            if (year == DateTime.Today.Year && month == DateTime.Today.Month)
            {
                var dayFromMonday = DateTime.Now.DayOfWeek - DayOfWeek.Monday;
                if (dayFromMonday < 0)
                    dayFromMonday += 7;
                monthEndDate = DateTime.Today.AddDays(-dayFromMonday);
                // If don't have today's ranking, read from 24h
                if (!dailyRanking.Any(h => h.RankDate.Date == DateTime.Today))
                {
                    dailyRanking.AddRange(currentMonthRankingData.Where(d => d.RankType == HistoryRanking.Type.Rank24h));
                }
            }
            model.MonthlyRankings = currentMonthRankingData.Where(d => d.RankType == HistoryRanking.Type.RankMonthly).ToList();
            model.DailyRankings = dailyRanking;
            model.WeeklyRankings = currentMonthRankingData.Where(d => d.RankType == HistoryRanking.Type.RankWeekly && d.RankDate < monthEndDate).ToList();
            return model;
        }
    }
}