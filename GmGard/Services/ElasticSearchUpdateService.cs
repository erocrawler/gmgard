using GmGard.Controllers;
using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Elastic.Clients.Elasticsearch;
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
        private readonly ElasticsearchClient _client;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackgroundTaskQueue _taskQueue;
        private readonly ILogger<ElasticSearchUpdateService> _logger;

        public ElasticSearchUpdateService(IServiceScopeFactory scopeFactory, ElasticsearchClient elasticClient, ILoggerFactory loggerFactory, BackgroundTaskQueue taskQueue)
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
            // Bounty ES hooks
            Controllers.App.BountyController.OnCreateBounty += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.AddOrUpdateBounty(e));
            Controllers.App.BountyController.OnAnswerBounty += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.AddOrUpdateBountyById(e.BountyId));
            Controllers.App.BountyController.OnDeleteBounty += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.RemoveBounty(e));
            Controllers.App.BountyController.OnAcceptBounty += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.AddOrUpdateBountyById(e.BountyId));
            Controllers.App.BountyController.OnCloseBounty += (s, e) => _taskQueue.QueueBackgroundWorkItem(Job.AddOrUpdateBountyById(e.BountyId));
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
                var result = await _client.UpdateAsync<BlogIndexed, object>("blogs", e.Model.BlogID, u => u.Doc(new { rating }).Refresh(Refresh.True));
                if (!result.IsValidResponse)
                {
                    _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason);
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
            var result = await _client.UpdateAsync<BlogIndexed, object>("blogs", e.Blog.BlogID, u => u.Doc(new { tags = e.Model.Select(t => t.TagName), isHarmony = e.Blog.isHarmony }).Refresh(Refresh.True));
            if (!result.IsValidResponse)
            {
                _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason);
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
                    var postCount = util.GetBlogPostCount(p.Model.ItemId);
                    var result = await _client.UpdateAsync<BlogIndexed, object>("blogs", p.Model.ItemId, u => u.Doc(new { postCount }).Refresh(Refresh.True));
                    if (!result.IsValidResponse)
                    {
                        _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason);
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
            if (visits == null || !visits.Any()) return;

            var bulkResponse = _client.Bulk(b =>
            {
                b.Index("blogs").Refresh(Refresh.True);
                foreach (var kvp in visits)
                {
                    b.Update<BlogIndexed, object>(ud =>
                    {
                        ud.Id(kvp.Key);
                        ud.Doc(new { blogVisit = kvp.Value });
                    });
                }
            });

            if (!bulkResponse.IsValidResponse)
            {
                _logger.LogError(bulkResponse.DebugInformation ?? bulkResponse.ElasticsearchServerError?.Error?.Reason);
            }
            else if (bulkResponse.Errors)
            {
                foreach (var item in bulkResponse.ItemsWithErrors)
                {
                    _logger.LogError($"Bulk update error id={item.Id} reason={item.Error?.Reason} caused by {item.Error?.CausedBy?.Reason}");
                }
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
                var doc = BlogIndexed.FromBlogTag(b.Model, tags.Select(t => t.TagName), util.GetPostCount(b.Model));
                var result = await _client.IndexAsync(doc, i => i.Index("blogs").Id(doc.Id).Refresh(Refresh.True));
                if (!result.IsValidResponse)
                {
                    _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason);
                }
            }
        }

        public async Task RemoveBlogAsync(BlogEventArgs b)
        {
            var r = await _client.DeleteAsync(b.Model.BlogID, d => d.Index("blogs").Refresh(Refresh.True));
            if (!r.IsValidResponse)
            {
                _logger.LogError(r.DebugInformation ?? r.ElasticsearchServerError?.Error?.Reason);
            }
        }

        // Bounty indexing
        public async Task AddOrUpdateBountyAsync(BountyEventArgs b)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetService<BlogContext>();
                Bounty bounty;
                List<Answer> answers;
                if (b.Bounty != null)
                {
                    bounty = b.Bounty;
                    answers = await db.Answers.Where(a => a.BountyId == bounty.BountyId).ToListAsync();
                }
                else
                {
                    bounty = await db.Bounties.FirstOrDefaultAsync(x => x.BountyId == b.BountyId);
                    if (bounty == null) return;
                    answers = await db.Answers.Where(a => a.BountyId == bounty.BountyId).ToListAsync();
                }
                var doc = BountyIndexed.FromBountyWithAnswers(bounty, answers);
                var result = await _client.IndexAsync(doc, i => i.Index("bounties").Id(doc.Id).Refresh(Refresh.True));
                if (!result.IsValidResponse)
                    _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason);
            }
            catch (Exception ex) { _logger.LogError(ex, "AddOrUpdateBountyAsync failed"); }
        }

        public async Task AddOrUpdateBountyByIdAsync(int bountyId)
        {
            await AddOrUpdateBountyAsync(new BountyEventArgs { BountyId = bountyId });
        }

        public async Task RemoveBountyAsync(BountyEventArgs b)
        {
            try
            {
                var r = await _client.DeleteAsync(b.BountyId, d => d.Index("bounties").Refresh(Refresh.True));
                if (!r.IsValidResponse)
                    _logger.LogError(r.DebugInformation ?? r.ElasticsearchServerError?.Error?.Reason);
            }
            catch (Exception ex) { _logger.LogError(ex, "RemoveBountyAsync failed"); }
        }
    }

    public class BountyEventArgs : EventArgs
    {
        public int BountyId { get; set; }
        public Bounty? Bounty { get; set; }
    }
}
