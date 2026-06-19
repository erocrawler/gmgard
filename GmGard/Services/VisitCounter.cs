using FluentScheduler;
using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
        private readonly Schedule _saveSchedule;
        private IServiceScopeFactory _scopeFactory;

        private BlogContext GetDB(IServiceScope scope) => scope.ServiceProvider.GetService<BlogContext>();

        public VisitCounter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            BlogVisits = new ConcurrentDictionary<int, AtomicLong>();
            DirtyBlogs = new ConcurrentDictionary<int, byte>();
            TopicVisits = new ConcurrentDictionary<int, AtomicLong>();
            DirtyTopics = new ConcurrentDictionary<int, byte>();
            _saveSchedule = new Schedule(() => SaveVisits().Wait(), run => run.Every(15).Minutes());
            _saveSchedule.Start();
        }

        public async Task SaveVisits()
        {
            await SaveBlogVisit();
            await SaveTopicVisit();
        }

        protected async Task SaveBlogVisit()
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = GetDB(scope);
                    var blogIds = IdPairs.Keys.ToList();
                    var blogsToUpdate = await db.Blogs.Where(b => blogIds.Contains(b.BlogID)).ToListAsync();
                    foreach (var blog in blogsToUpdate)
                    {
                        if (IdPairs.TryGetValue(blog.BlogID, out long newVisit))
                        {
                            blog.BlogVisit = newVisit;
                        }
                    }
                    await db.SaveChangesAsync();
                    var updater = scope.ServiceProvider.GetService<ElasticSearchUpdateService>();
                    if (updater != null)
                    {
                        updater.UpdateViewCount(IdPairs);
                    }
                }
            }
        }

        protected async Task SaveTopicVisit()
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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = GetDB(scope);
                    var topicIds = IdPairs.Keys.ToList();
                    var topicsToUpdate = await db.Topics.Where(t => topicIds.Contains(t.TopicID)).ToListAsync();
                    foreach (var topic in topicsToUpdate)
                    {
                        if (IdPairs.TryGetValue(topic.TopicID, out long newVisit))
                        {
                            topic.TopicVisit = newVisit;
                        }
                    }
                    await db.SaveChangesAsync();
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