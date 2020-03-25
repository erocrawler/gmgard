using GmGard.Controllers;
using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static GmGard.Services.ElasticSearchProvider;

namespace GmGard.Services
{
    public class ElasticSearchUpdateService
    {
        private readonly ElasticClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackgroundTaskQueue _taskQueue;
        private readonly ILogger<ElasticSearchUpdateService> _logger;

        public ElasticSearchUpdateService(IServiceScopeFactory scopeFactory, ElasticClient elasticClient, ILoggerFactory loggerFactory, BackgroundTaskQueue taskQueue)
        {
            _client = elasticClient;
            _scopeFactory = scopeFactory;
            _logger = loggerFactory.CreateLogger<ElasticSearchUpdateService>();
            _taskQueue = taskQueue;
            if (_client != null)
            {
                RegisterUpdateEvents();
            }
        }

        public void RegisterUpdateEvents()
        {
            EventHandler<BlogEventArgs> addOrUpdate = (s, e) => 
                _taskQueue.QueueBackgroundWorkItem(Job.AddOrUpdateBlog(e));
            ReplyController.OnAddPost += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.UpdatePostCount(e));
            Controllers.App.ReplyController.OnAddPost += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.UpdatePostCount(e));
            ReplyController.OnDeletePost += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.UpdatePostCount(e));
            Controllers.App.ReplyController.OnDeletePost += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.UpdatePostCount(e));
            AuditController.OnApproveBlog += addOrUpdate;
            AuditController.OnDenyBlog += addOrUpdate;
            BlogController.OnNewBlog += addOrUpdate;
            BlogController.OnDeleteBlog += (s, e) => 
            {
                if (e.Deleted)
                {
                    _taskQueue.QueueBackgroundWorkItem(Job.RemoveBlog(e));
                }
                else
                {
                    _taskQueue.QueueBackgroundWorkItem(Job.AddOrUpdateBlog(e));
                }
            };
            BlogController.OnEditBlog += addOrUpdate;
            BlogController.OnEditTags += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.UpdateBlogTag(e));
            RatingUtil.OnRateBlog += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.UpdateBlogRate(e));
        }

        public async Task UpdateBlogRateAsync(RateEventArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var util = scope.ServiceProvider.GetService<RatingUtil>();
                var rating = util.GetRating(e.Model.BlogID).Total;
                var result = await _client.UpdateAsync<BlogIndexed, object>(DocumentPath<BlogIndexed>.Id(e.Model.BlogID), ud => ud.Doc(new { rating }).Refresh(Elasticsearch.Net.Refresh.True));
                if (!result.IsValid)
                {
                    _logger.LogError(result.DebugInformation);
                    var db = scope.ServiceProvider.GetService<BlogContext>();
                    var blog = await db.Blogs.FindAsync(e.Model.BlogID);
                    if (blog != null)
                    {
                        await AddOrUpdateBlogAsync(new BlogEventArgs(blog));
                    }
                }
            }
        }

        public async Task UpdateBlogTagAsync(TagEventArgs e)
        {
            var result = await _client.UpdateAsync<BlogIndexed, object>(DocumentPath<BlogIndexed>.Id(e.Blog.BlogID), ud => ud.Doc(new { tags = e.Model, e.Blog.isHarmony }).Refresh(Elasticsearch.Net.Refresh.True));
            if (!result.IsValid)
            {
                _logger.LogError(result.DebugInformation);
                await AddOrUpdateBlogAsync(new BlogEventArgs(e.Blog, e.Model));
            }
        }

        public async Task UpdatePostCountAsync(PostEventArgs p)
        {
            if (p.Model.IdType == ItemType.Blog)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var util = scope.ServiceProvider.GetService<ContextlessBlogUtil>();
                    var result = await _client.UpdateAsync<BlogIndexed, object>(DocumentPath<BlogIndexed>.Id(p.Model.ItemId), ud => ud.Doc(new { PostCount = util.GetBlogPostCount(p.Model.ItemId) }).Refresh(Elasticsearch.Net.Refresh.True));
                    if (!result.IsValid)
                    {
                        _logger.LogError(result.DebugInformation);
                        var db = scope.ServiceProvider.GetService<BlogContext>();
                        var blog = await db.Blogs.FindAsync(p.Model.ItemId);
                        if (blog != null)
                        {
                            await AddOrUpdateBlogAsync(new BlogEventArgs(blog));
                        }
                    }
                }
            }
        }

        public void UpdateViewCount(IDictionary<int, long> visits)
        {
            var result = _client.Bulk(bd => visits.Aggregate(bd, (b, kvp) => b.Update<BlogIndexed, object>(bud => bud.Id(kvp.Key).Doc(new { BlogVisit = kvp.Value })), i => i.Refresh(Elasticsearch.Net.Refresh.True)));
            if (!result.IsValid)
            {
                _logger.LogError(result.DebugInformation);
            }
        }

        public async Task AddOrUpdateBlogAsync(BlogEventArgs b)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var util = scope.ServiceProvider.GetService<ContextlessBlogUtil>();
                var tags = b.Tags;
                if (tags == null)
                {
                    var tagUtil = scope.ServiceProvider.GetService<TagUtil>();
                    tags = await tagUtil.GetTagsInBlogAsync(b.Model.BlogID);
                }
                var result = await _client.IndexAsync(BlogIndexed.FromBlogTag(b.Model, tags.Select(t => t.TagName), util.GetPostCount(b.Model)), i => i.Refresh(Elasticsearch.Net.Refresh.True));
                if (!result.IsValid)
                {
                    _logger.LogError(result.DebugInformation);
                }
            }
        }

        public Task RemoveBlogAsync(BlogEventArgs b)
        {
            return _client.DeleteAsync(new DocumentPath<BlogIndexed>(b.Model.BlogID), i => i.Refresh(Elasticsearch.Net.Refresh.True)).ContinueWith(r =>
            {
                if (!r.Result.IsValid)
                {
                    _logger.LogError(r.Result.DebugInformation);
                }
            });
        }
    }
}
