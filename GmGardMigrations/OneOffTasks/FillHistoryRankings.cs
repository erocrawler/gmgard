using GmGard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class FillHistoryRankings
    {

        static string firstImgPath(Blog blog)
        {
            if (blog == null || blog.ImagePath == null)
                return null;
            string path = blog.ImagePath;
            if (blog.IsLocalImg)
            {
                int s = path.IndexOf(';');
                if (s > 0)
                {
                    path = path.Substring(0, s);
                }
                path = path.Replace("/upload/", "/thumbs/");
            }
            return path;
        }

        class Ranking
        {
            public int BlogID { get; set; }
            public string BlogTitle { get; set; }
            public string BlogThumb { get; set; }
            public long BlogVisit { get; set; }
            public int PostCount { get; set; }
            public int Rating { get; set; }
            public string Author { get; set; }
            public DateTime BlogDate { get; set; }
        }

        class RankingList
        {
            public IEnumerable<Ranking> r1m { get; set; }
            public IEnumerable<Ranking> r24h { get; set; }
        }

        const int BATCH_SIZE = 1000;

        public static void Run()
        {
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            using (var db = blogContextFactory.Create())
            {
                var joins = db.HistoryRankings.Join(db.Blogs, r => r.BlogID, b => b.BlogID, (r, b) => new { rank = r, blog = b, pc = db.Posts.Count(p => p.ItemId == b.BlogID && p.IdType == ItemType.Blog) });
                int count = joins.Count();
                Console.WriteLine("Total ranking count: {0}", count);
                for (int i = 0; i < count; i+=BATCH_SIZE)
                {
                    var items = joins.OrderBy(b => b.rank.RankDate).Skip(i).Take(BATCH_SIZE).ToList();
                    foreach (var item in items)
                    {
                        item.rank.Author = item.blog.Author;
                        item.rank.BlogDate = item.blog.BlogDate;
                        item.rank.BlogThumb = firstImgPath(item.blog);
                        item.rank.BlogTitle = item.blog.BlogTitle;
                        item.rank.BlogVisit = item.blog.BlogVisit;
                        item.rank.PostCount = item.pc;
                    }
                    db.SaveChanges();
                    Console.WriteLine($"Done {i+BATCH_SIZE}");
                }
            }

            using (var db = blogContextFactory.Create())
            {
                var rankdata = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../App_Data/ranking.js"));
                var rankings = JsonConvert.DeserializeObject<RankingList>(rankdata);
                db.HistoryRankings.AddRange(rankings.r1m.Select(r => new HistoryRanking
                {
                    Author = r.Author,
                    BlogDate = r.BlogDate,
                    BlogID = r.BlogID,
                    BlogThumb = r.BlogThumb,
                    BlogTitle = r.BlogTitle,
                    BlogVisit = r.BlogVisit,
                    PostCount = r.PostCount,
                    RankDate = DateTime.Today,
                    RankType = HistoryRanking.Type.RankMonthly,
                    Rating = r.Rating
                }));
                db.HistoryRankings.AddRange(rankings.r24h.Select(r => new HistoryRanking
                {
                    Author = r.Author,
                    BlogDate = r.BlogDate,
                    BlogID = r.BlogID,
                    BlogThumb = r.BlogThumb,
                    BlogTitle = r.BlogTitle,
                    BlogVisit = r.BlogVisit,
                    PostCount = r.PostCount,
                    RankDate = DateTime.Today,
                    RankType = HistoryRanking.Type.Rank24h,
                    Rating = r.Rating
                }));

                rankdata = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../App_Data/monthly_ranking.js"));
                var AllRankings = JsonConvert.DeserializeObject<Dictionary<DateTime, IEnumerable<Ranking>>>(rankdata) ?? new Dictionary<DateTime, IEnumerable<Ranking>>();
                db.HistoryRankings.AddRange(AllRankings.SelectMany(kvp => kvp.Value.Select(r => new HistoryRanking
                {
                    Author = r.Author,
                    BlogDate = r.BlogDate,
                    BlogID = r.BlogID,
                    BlogThumb = r.BlogThumb,
                    BlogTitle = r.BlogTitle,
                    BlogVisit = r.BlogVisit,
                    PostCount = r.PostCount,
                    RankDate = kvp.Key,
                    RankType = HistoryRanking.Type.RankMonthly,
                    Rating = r.Rating
                })));
                
                for (int year = 2014; year <= 2018; year++)
                {
                    var firstDay = new DateTime(year, 1, 1);
                    var daySinceMonday = DayOfWeek.Monday - firstDay.DayOfWeek ;
                    if (daySinceMonday < 0)
                        daySinceMonday += 7;
                    var firstMonday = daySinceMonday == 0 ? firstDay : firstDay.AddDays(daySinceMonday);
                    for (int week = 1; week <= 52; week++)
                    {
                        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"../../../../App_Data/ranking{year}{week}.js");
                        if (!File.Exists(path))
                        {
                            continue;
                        }
                        rankdata = File.ReadAllText(path);
                        var ranking = JsonConvert.DeserializeObject<IEnumerable<Ranking>>(rankdata);
                        db.HistoryRankings.AddRange(ranking.Select(r => new HistoryRanking
                        {
                            Author = r.Author,
                            BlogDate = r.BlogDate,
                            BlogID = r.BlogID,
                            BlogThumb = r.BlogThumb,
                            BlogTitle = r.BlogTitle,
                            BlogVisit = r.BlogVisit,
                            PostCount = r.PostCount,
                            RankDate = firstMonday.AddDays(6 + (week - 1) * 7),
                            RankType = HistoryRanking.Type.RankWeekly,
                            Rating = r.Rating
                        }));
                    }
                }
                db.SaveChanges();
            }
        }

        public static void FillMonth(int year, int month)
        {
            DateTime firstday = new DateTime(year, month, 1);
            DateTime lastDay = new DateTime(firstday.Year, firstday.Month, DateTime.DaysInMonth(firstday.Year, firstday.Month));
            var factory = new BlogContextFactory();
            using (var db = factory.Create())
            {
                var oldRankings = db.HistoryRankings.Where(h => h.RankType == HistoryRanking.Type.RankMonthly && DbFunctions.DiffMonths(lastDay, h.RankDate) == 0).ToList();
                db.HistoryRankings.RemoveRange(oldRankings);
                var rankings = db.BlogRatings.Where(r => DbFunctions.DiffMonths(firstday, r.ratetime) == 0).GroupBy(r => r.BlogID)
                    .Select(g => new { blogId = g.Key, rating = g.Sum(r => r.value) })
                    .Join(
                        db.Blogs.Where(b => b.isApproved == true && !(new[] { 11, 12 }).Contains(b.CategoryID)),
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
                    .Take(5)
                    .ToList().Select(r => new HistoryRanking
                    {
                        Author = r.blog.Author,
                        BlogDate = r.blog.BlogDate,
                        BlogID = r.blog.BlogID,
                        BlogThumb = firstImgPath(r.blog),
                        BlogTitle = r.blog.BlogTitle,
                        BlogVisit = r.blog.BlogVisit,
                        PostCount = r.postCount,
                        Rating = r.rating,
                        RankType = HistoryRanking.Type.RankMonthly,
                        RankDate = lastDay,
                    });
                db.HistoryRankings.AddRange(rankings);
                db.SaveChanges();
            }
        }
    }
}
