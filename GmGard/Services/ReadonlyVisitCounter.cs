using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class ReadonlyVisitCounter : IVisitCounter
    {
        private ConcurrentDictionary<int, AtomicLong> BlogVisits;
        private ConcurrentDictionary<int, AtomicLong> TopicVisits;
        private IServiceScopeFactory _scopeFactory;

        private BlogContext GetDB(IServiceScope scope) => scope.ServiceProvider.GetService<BlogContext>();

        public ReadonlyVisitCounter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            BlogVisits = new ConcurrentDictionary<int, AtomicLong>();
            TopicVisits = new ConcurrentDictionary<int, AtomicLong>();
        }

        public void SaveVisits()
        {
            SaveBlogVisit();
            SaveTopicVisit();
        }

        protected void SaveBlogVisit()
        {
        }

        protected void SaveTopicVisit()
        {
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
            }
            return visit.Value;
        }
    }
}
