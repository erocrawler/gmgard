using FluentScheduler;
using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using Terradue.ServiceModel.Syndication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace GmGard.Services
{
    public class SchedulerService : Registry
    {
        public SchedulerService(IServiceScopeFactory scopeFactory, IHostingEnvironment env, IMemoryCache cache, IOptions<AppSettingsModel> appsetting, ILoggerFactory logger)
        {
            _scopeFactory = scopeFactory;
            _env = env;
            _cache = cache;
            _appSettings = appsetting.Value;
            _logger = logger.CreateLogger<SchedulerService>();
            
            InitSchedule();

            Controllers.AdminController.OnSettingsChanged += OnAppSettingsChanged;
            JobManager.JobException += OnJobException;
        }

        private void OnJobException(JobExceptionInfo obj)
        {
            _logger.LogError(new EventId(2), obj.Exception, "Job {0}: {1}", obj.Name, obj.Exception.Message);
        }

        private void OnAppSettingsChanged(object sender, SettingsEventArgs e)
        {
            if (e.AppModel != null)
            {
                JobManager.RemoveJob(RankingTask);
                _appSettings = e.AppModel;
                RankingScheduler();
            }
        }

        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMemoryCache _cache;
        private IHostingEnvironment _env;
        private AppSettingsModel _appSettings;

        private int UpdateInterval => _appSettings.UpdateInterval;
        private List<int> NoRankCategories => _appSettings.NoRankCategories;
        private List<int> DailyReward => _appSettings.DailyReward;
        private List<int> WeeklyReward => _appSettings.WeeklyReward;
        private List<int> MonthlyReward => _appSettings.MonthlyReward;
        private int RankSize => _appSettings.RankingSize;
        private string filepath => Path.Combine(_env.WebRootPath, "rss.xml");

        private const string RankingTask = "RankingTask";

        private void InitSchedule()
        {
            RankingScheduler();
            Schedule(new Action(RssTask)).ToRunNow().AndEvery(1).Hours();
            Schedule(new Action(DailyRewardTask)).ToRunEvery(1).Days().At(20, 00);
            Schedule(new Action(WeeklyRewardTask)).ToRunEvery(0).Weeks().On(DayOfWeek.Monday).At(0, 1);
            Schedule(new Action(MonthlyRewardTask)).ToRunEvery(1).Months().On(1).At(0, 1);
        }

        private void RankingScheduler()
        {
            Action timertask = new Action(() =>
            {
                _logger.LogInformation("Starting Ranking Scheduler!");
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
                    var sincemonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    if (DateTime.Today.Day != 1) // Show last month on day 1
                    {
                        var ranking1month = GetRankingSinceDate(db, sincemonth, DateTime.Today, HistoryRanking.Type.RankMonthly);
                        db.HistoryRankings.RemoveRange(db.HistoryRankings.Where(h => h.RankType == HistoryRanking.Type.RankMonthly && DbFunctions.DiffMonths(h.RankDate, sincemonth) == 0));
                        db.HistoryRankings.AddRange(ranking1month);
                    }

                    var rankings24h = db.Blogs.Where(b => !NoRankCategories.Contains(b.CategoryID) && DbFunctions.DiffMinutes(b.BlogDate, DateTime.Now) < 1440 && b.isApproved == true)
                        .OrderByDescending(r => r.Rating)
                        .ThenByDescending(r => r.BlogDate)
                        .Take(RankSize)
                        .Select(b => new
                        {
                            blog = b,
                            rating = b.Rating ?? 0,
                            postCount = db.Posts.Count(p => p.IdType == ItemType.Blog && p.ItemId == b.BlogID)
                        }).ToList()
                        .Select(r => new HistoryRanking
                        {
                            Rating = r.rating,
                            PostCount = r.postCount,
                            Author = r.blog.Author,
                            BlogVisit = r.blog.BlogVisit,
                            BlogTitle = r.blog.BlogTitle,
                            BlogThumb = BlogHelper.firstImgPath(r.blog, true),
                            BlogID = r.blog.BlogID,
                            BlogDate = r.blog.BlogDate,
                            RankDate = DateTime.Now,
                            RankType = HistoryRanking.Type.Rank24h
                        });
                    db.HistoryRankings.RemoveRange(db.HistoryRankings.Where(h => h.RankType == HistoryRanking.Type.Rank24h));
                    db.HistoryRankings.AddRange(rankings24h);

                    var dayFromMonday = DateTime.Now.DayOfWeek - DayOfWeek.Monday;
                    if (dayFromMonday < 0)
                        dayFromMonday += 7;
                    var firstday = DateTime.Today.AddDays(-dayFromMonday);
                    // remove this week's data.
                    db.HistoryRankings.RemoveRange(db.HistoryRankings.Where(h => h.RankType == HistoryRanking.Type.RankWeekly && firstday <= h.RankDate));
                    if (dayFromMonday == 0) //if today is monday reuse last week data.
                    {
                        var lastWeek = GetRankingSinceDate(db, firstday.AddDays(-7), DateTime.Today, HistoryRanking.Type.RankWeekly);
                        db.HistoryRankings.AddRange(lastWeek);
                    }
                    else
                    {
                        var rankings7d = GetRankingSinceDate(db, firstday, DateTime.Today, HistoryRanking.Type.RankWeekly);
                        db.HistoryRankings.AddRange(rankings7d);
                    }
                    db.SaveChanges();
                }
            });
            Schedule(timertask).WithName(RankingTask).ToRunEvery(UpdateInterval).Minutes();
        }

        private IEnumerable<HistoryRanking> GetRankingSinceDate(BlogContext db, DateTime since, DateTime rankDate, HistoryRanking.Type type)
        {
            return db.BlogRatings.Where(r => DbFunctions.DiffDays(since, r.ratetime) >= 0).GroupBy(r => r.BlogID)
                .Select(g => new { blogId = g.Key, rating = g.Sum(r => r.value) })
                .Join(
                    db.Blogs.Where(b => b.isApproved == true && !NoRankCategories.Contains(b.CategoryID)),
                    a => a.blogId,
                    b => b.BlogID,
                    (a, b) =>
                        new
                        {
                            blog = b,
                            a.rating,
                            postCount = db.Posts.Count(p => p.IdType == ItemType.Blog && p.ItemId == b.BlogID)
                        }
                ).OrderByDescending(r => r.rating)
                .ThenByDescending(r => r.blog.BlogDate)
                .Take(RankSize)
                .ToList().Select(r => new HistoryRanking
                {
                    Author = r.blog.Author,
                    BlogDate = r.blog.BlogDate,
                    BlogID = r.blog.BlogID,
                    BlogThumb = BlogHelper.firstImgPath(r.blog, true),
                    BlogTitle = r.blog.BlogTitle,
                    BlogVisit = r.blog.BlogVisit,
                    PostCount = r.postCount,
                    Rating = r.rating,
                    RankType = type,
                    RankDate = rankDate,
                });
        }

        private readonly Uri link = new Uri("https://gmgard.com");
        private readonly Uri rsslink = new Uri("https://gmgard.com/Rss");
        private readonly Uri imgurl = new Uri("https://gmgard.com/Images/sinsi.png");

        private class CDataSyndicationContent : TextSyndicationContent
        {
            public CDataSyndicationContent(TextSyndicationContent content)
                : base(content)
            { }

            public CDataSyndicationContent(string content)
                : base(content)
            { }

            protected override void WriteContentsTo(System.Xml.XmlWriter writer)
            {
                writer.WriteCData(Text);
            }
        }

        private void RssTask()
        {
            var feed = new SyndicationFeed("绅士之庭订阅源", "应广大群众的呼声，Gmgard.com的订阅源轰然面世。如遇任何问题请联系站长", rsslink);
            //feed.Language = "zh-CN";
            feed.Authors.Add(new SyndicationPerson("admin@gmgard.com"));
            feed.LastUpdatedTime = DateTimeOffset.Now;
            feed.Links.Add(new SyndicationLink(link, "self", "gmgard.com", null, 0));
            feed.ImageUrl = imgurl;
            var items = new List<SyndicationItem>();
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
                var blogs = db.Blogs.Where(b => b.isApproved == true).OrderByDescending(b => b.BlogDate).Take(20);
                var categoryUtil = scope.ServiceProvider.GetRequiredService<CategoryUtil>();
                foreach (var blog in blogs)
                {
                    var item = new SyndicationItem();
                    foreach (var CategoryName in categoryUtil.GetFullCategoryNames(blog.CategoryID))
                    {
                        item.Categories.Add(new SyndicationCategory(CategoryName));
                    }
                    var path = BlogHelper.firstImgPath(blog, true);
                    item.Summary = new CDataSyndicationContent(string.Format("<img src='{0}'></img><br/><p>{1}</p>", path, BlogHelper.getFirstLine(blog.Content, 200, true)));
                    item.Authors.Add(new SyndicationPerson(blog.Author));
                    item.PublishDate = blog.BlogDate;
                    item.LastUpdatedTime = blog.BlogDate;
                    item.Title = new TextSyndicationContent(blog.BlogTitle);
                    item.Id = "https://gmgard.com/gm" + blog.BlogID;
                    item.AddPermalink(new Uri("https://gmgard.com/gm" + blog.BlogID));
                    items.Add(item);
                }
                feed.Items = items;
            }

            using (var writer = System.Xml.XmlWriter.Create(filepath))
            {
                feed.SaveAsAtom10(writer);
            }
        }

        private void DailyRewardTask()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
                var rankings = db.Blogs.Where(b => DbFunctions.DiffMinutes(b.BlogDate, DateTime.Now) < 1440 && b.isApproved == true && !NoRankCategories.Contains(b.CategoryID))
                        .Join(db.BlogRatings.GroupBy(r => r.BlogID).Select(g => new { blogId = g.Key, rating = g.Sum(r => r.value) })
                            , b => b.BlogID, r => r.blogId, (b, r) => new
                            {
                                blog = b,
                                Rating = r.rating,
                                PostCount = db.Posts.Count(p => p.IdType == ItemType.Blog && p.ItemId == b.BlogID)
                            })
                        .OrderByDescending(r => r.Rating)
                        .ThenByDescending(r => r.blog.BlogDate)
                        .Take(RankSize)
                        .ToArray()
                        .Select(rank => new HistoryRanking
                        {
                            BlogID = rank.blog.BlogID,
                            RankDate = DateTime.Now.Date,
                            Rating = rank.Rating,
                            Author = rank.blog.Author,
                            BlogDate = rank.blog.BlogDate,
                            BlogThumb = BlogHelper.firstImgPath(rank.blog, true),
                            BlogTitle = rank.blog.BlogTitle,
                            BlogVisit = rank.blog.BlogVisit,
                            PostCount = rank.PostCount,
                            RankType = HistoryRanking.Type.RankDaily,
                        });

                db.HistoryRankings.AddRange(rankings);
                db.SaveChanges();
                ExpUtil expUtil = scope.ServiceProvider.GetService<ExpUtil>();
                expUtil.addRankExp(rankings, DailyReward, "日榜奖励");
            }
        }

        private void WeeklyRewardTask()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
                // today should be first day of new week
                var firstday = DateTime.Today.AddDays(-7);
                var rankings = GetRankingSinceDate(db, firstday, firstday.AddDays(6), HistoryRanking.Type.RankWeekly);
                db.HistoryRankings.RemoveRange(db.HistoryRankings.Where(h => h.RankType == HistoryRanking.Type.RankWeekly && firstday <= h.RankDate));
                db.HistoryRankings.AddRange(rankings);
                db.SaveChanges();
                ExpUtil expUtil = scope.ServiceProvider.GetService<ExpUtil>();
                expUtil.addRankExp(rankings, WeeklyReward, "周榜奖励");
            }
        }

        private void MonthlyRewardTask()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
                // today should be first day of new month
                DateTime firstday = DateTime.Today.AddMonths(-1);
                DateTime lastDay = new DateTime(firstday.Year, firstday.Month, DateTime.DaysInMonth(firstday.Year, firstday.Month));
                var rankings = GetRankingSinceDate(db, firstday, lastDay, HistoryRanking.Type.RankMonthly);
                db.HistoryRankings.RemoveRange(db.HistoryRankings.Where(h => h.RankType == HistoryRanking.Type.RankMonthly && DbFunctions.DiffMonths(h.RankDate, firstday) == 0));
                db.HistoryRankings.AddRange(rankings);
                db.SaveChanges();
                // Remove any key at rankdate as we just updated.
                IDictionary<DateTime, MonthRanking> cache = _cache.Get<System.Collections.Concurrent.ConcurrentDictionary<DateTime, MonthRanking>>("~MonthRanking");
                if (cache != null && cache.ContainsKey(firstday))
                {
                    cache.Remove(firstday);
                }
                ExpUtil expUtil = scope.ServiceProvider.GetService<ExpUtil>();
                expUtil.addRankExp(rankings, MonthlyReward, "月榜奖励");
            }
        }
    }
}
