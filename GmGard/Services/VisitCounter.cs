using FluentScheduler;
using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GmGard.Services
{
    /// <summary>
    /// Singleton class
    /// </summary>
    public class VisitCounter : IVisitCounter
    {
        private ConcurrentDictionary<int, AtomicLong> BlogVisits;
        private ConcurrentDictionary<int, byte> DirtyBlogs;
        private ConcurrentDictionary<int, AtomicLong> TopicVisits;
        private ConcurrentDictionary<int, byte> DirtyTopics;
        private IServiceScopeFactory _scopeFactory;

        private BlogContext GetDB(IServiceScope scope) => scope.ServiceProvider.GetService<BlogContext>();

        public VisitCounter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            BlogVisits = new ConcurrentDictionary<int, AtomicLong>();
            DirtyBlogs = new ConcurrentDictionary<int, byte>();
            TopicVisits = new ConcurrentDictionary<int, AtomicLong>();
            DirtyTopics = new ConcurrentDictionary<int, byte>();
            JobManager.AddJob(SaveVisits, schedule => schedule.ToRunEvery(15).Minutes());
        }

        public void SaveVisits()
        {
            SaveBlogVisit();
            SaveTopicVisit();
        }

        protected void SaveBlogVisit()
        {
            var IdPairs = new Dictionary<int, long>();
            foreach (var dirty in DirtyBlogs)
            {
                int id = dirty.Key;
                if (BlogVisits.TryGetValue(id, out AtomicLong l) && !IdPairs.ContainsKey(id))
                {
                    IdPairs.Add(id, l.Value);
                }
            }
            if (IdPairs.Count > 0)
            {
                DirtyBlogs.Clear();
                var ValueString = IdPairs.Select(v => string.Format("({0},{1})", v.Key, v.Value));
                using (var scope = _scopeFactory.CreateScope())
                {
                    GetDB(scope).Database.ExecuteSqlCommand(
                        @"UPDATE b SET BlogVisit = t.BlogVisit
                    FROM Blogs b
                    JOIN (
	                    VALUES " + string.Join(",", ValueString) +
                        ") t (id, BlogVisit) ON b.BlogID = t.id"
                    );
                    var updater = scope.ServiceProvider.GetService<ElasticSearchUpdateService>();
                    if (updater != null)
                    {
                        updater.UpdateViewCount(IdPairs);
                    }
                }
            }
        }

        protected void SaveTopicVisit()
        {
            var IdPairs = new Dictionary<int, long>();
            foreach (var dirty in DirtyTopics)
            {
                int id = dirty.Key;
                if (TopicVisits.TryGetValue(id, out AtomicLong l) && !IdPairs.ContainsKey(id))
                {
                    IdPairs.Add(id, l.Value);
                }
            }
            if (IdPairs.Count > 0)
            {
                DirtyTopics.Clear();
                var ValueString = IdPairs.Select(v => string.Format("({0},{1})", v.Key, v.Value));
                using (var scope = _scopeFactory.CreateScope())
                {
                    GetDB(scope).Database.ExecuteSqlCommand(
                        @"UPDATE b SET TopicVisit = t.TopicVisit
                        FROM Topics b
                        JOIN (
	                        VALUES " + string.Join(",", ValueString) +
                        ") t (id, TopicVisit) ON b.TopicID = t.id"
                    );
                }
            }
        }

        public long GetBlogVisit(int id, bool increment = false)
        {
            var visit = BlogVisits.GetOrAdd(id, i =>
            {
                long v = 0;
                using (var scope = _scopeFactory.CreateScope())
                {
                    v = GetDB(scope).Blogs.AsNoTracking().Where(b => b.BlogID == id).Select(b => b.BlogVisit).SingleOrDefault();
                }
                return new AtomicLong(v);
            });
            if (increment)
            {
                visit.Increment();
                if (!DirtyBlogs.ContainsKey(id))
                {
                    DirtyBlogs.TryAdd(id, 0);
                }
            }
            return visit.Value;
        }

        public void PrepareBlogVisits(IEnumerable<int> ids)
        {
            var uncached = ids.Where(i => !BlogVisits.ContainsKey(i));
            if (uncached.Count() > 0)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var visits = GetDB(scope).Blogs.AsNoTracking().Where(b => uncached.Contains(b.BlogID)).ToDictionary(b => b.BlogID, b => b.BlogVisit);
                    foreach (var v in visits)
                    {
                        BlogVisits.TryAdd(v.Key, new AtomicLong(v.Value));
                    }
                }
            }
        }

        public long GetTopicVisit(int id, bool increment = false)
        {
            var visit = TopicVisits.GetOrAdd(id, i =>
            {
                long v = 0;
                using (var scope = _scopeFactory.CreateScope())
                {
                    v = GetDB(scope).Topics.AsNoTracking().Where(b => b.TopicID == id).Select(b => b.TopicVisit).SingleOrDefault();
                }
                return new AtomicLong(v);
            });
            if (increment)
            {
                visit.Increment();
                if (!DirtyTopics.ContainsKey(id))
                {
                    DirtyTopics.TryAdd(id, 0);
                }
            }
            return visit.Value;
        }
    }
}