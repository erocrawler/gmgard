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
        private readonly ILogger _logger;

        public ElasticSearchUpdateService(IServiceScopeFactory scopeFactory, ElasticClient elasticClient, ILoggerFactory loggerFactory)
        {
            _client = elasticClient;
            _scopeFactory = scopeFactory;
            _logger = loggerFactory.CreateLogger<ElasticSearchUpdateService>();
            if (_client != null)
            {
                RegisterUpdateEvents();
            }
        }

        public void RegisterUpdateEvents()
        {
            ReplyController.OnAddPost += async (s, e) => await UpdatePostCountAsync(s, e);
            Controllers.App.ReplyController.OnAddPost += async (s, e) => await UpdatePostCountAsync(s, e);
            ReplyController.OnDeletePost += async (s, e) => await UpdatePostCountAsync(s, e);
            Controllers.App.ReplyController.OnDeletePost += async (s, e) => await UpdatePostCountAsync(s, e);
            AuditController.OnApproveBlog += async (s, e) => await AddOrUpdateBlogAsync(s, e);
            AuditController.OnDenyBlog += async (s, e) => await AddOrUpdateBlogAsync(s, e);
            BlogController.OnNewBlog += async (s, e) => await AddOrUpdateBlogAsync(s, e);
            BlogController.OnDeleteBlog += async (s, e) => 
            {
                if (e.Deleted)
                {
                    await RemoveBlogAsync(s, e);
                }
                else
                {
                    await AddOrUpdateBlogAsync(s, e);
                }
            };
            BlogController.OnEditBlog += async (s, e) => await AddOrUpdateBlogAsync(s, e);
            RatingUtil.OnRateBlog += async (s, e) => await UpdateBlogRateAsync(s, e);
        }

        private async Task UpdateBlogRateAsync(object s, RateEventArgs e)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var util = scope.ServiceProvider.GetService<RatingUtil>();
                var rating = util.GetRating(e.Model.BlogID).Total;
                var result = await _client.UpdateAsync<BlogIndexed, object>(DocumentPath<BlogIndexed>.Id(e.Model.BlogID), ud => ud.Doc(new { rating }));
                if (!result.IsValid)
                {
                    _logger.LogError(result.DebugInformation);
                    var db = scope.ServiceProvider.GetService<BlogContext>();
                    var blog = await db.Blogs.FindAsync(e.Model.BlogID);
                    if (blog != null)
                    {
                        await AddOrUpdateBlogAsync(s, new BlogEventArgs(blog));
                    }
                }
            }
        }

        private async Task UpdateBlogTagAsync(object s, TagEventArgs e)
        {
            var result = await _client.UpdateAsync<BlogIndexed, object>(DocumentPath<BlogIndexed>.Id(e.Blog.BlogID), ud => ud.Doc(new { tags = e.Model, e.Blog.isHarmony }));
            if (!result.IsValid)
            {
                _logger.LogError(result.DebugInformation);
                await AddOrUpdateBlogAsync(s, new BlogEventArgs(e.Blog, e.Model));
            }
        }

        public async Task UpdatePostCountAsync(object sender, PostEventArgs p)
        {
            if (p.Model.IdType == ItemType.Blog)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var util = scope.ServiceProvider.GetService<BlogUtil>();
                    var result = await _client.UpdateAsync<BlogIndexed, object>(DocumentPath<BlogIndexed>.Id(p.Model.ItemId), ud => ud.Doc(new { PostCount = util.GetBlogPostCount(p.Model.ItemId) }));
                    if (!result.IsValid)
                    {
                        _logger.LogError(result.DebugInformation);
                        var db = scope.ServiceProvider.GetService<BlogContext>();
                        var blog = await db.Blogs.FindAsync(p.Model.ItemId);
                        if (blog != null)
                        {
                            await AddOrUpdateBlogAsync(sender, new BlogEventArgs(blog));
                        }
                    }
                }
            }
        }

        public void UpdateViewCount(IDictionary<int, long> visits)
        {
            var result = _client.Bulk(bd => visits.Aggregate(bd, (b, kvp) => b.Update<BlogIndexed, object>(bud => bud.Id(kvp.Key).Doc(new { BlogVisit = kvp.Value }))));
            if (!result.IsValid)
            {
                _logger.LogError(result.DebugInformation);
            }
        }

        public async Task AddOrUpdateBlogAsync(object sender, BlogEventArgs b)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var util = scope.ServiceProvider.GetService<BlogUtil>();
                var tags = b.Tags;
                if (tags == null)
                {
                    var tagUtil = scope.ServiceProvider.GetService<TagUtil>();
                    tags = await tagUtil.GetTagsInBlogAsync(b.Model.BlogID);
                }
                var result = await _client.IndexAsync(BlogIndexed.FromBlogTag(b.Model, tags.Select(t => t.TagName), util.GetPostCount(b.Model)), i => i);
                if (!result.IsValid)
                {
                    _logger.LogError(result.DebugInformation);
                }
            }
        }

        public Task RemoveBlogAsync(object sender, BlogEventArgs b)
        {
            return _client.DeleteAsync(new DocumentPath<BlogIndexed>(b.Model.BlogID)).ContinueWith(r =>
            {
                if (!r.Result.IsValid)
                {
                    _logger.LogError(r.Result.DebugInformation);
                }
            });
        }
    }
}
