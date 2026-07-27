using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using System.Threading;

namespace GmGardMigrations.OneOffTasks
{
    class FillElasticSearch
    {
        class BlogProvider
        {
            private readonly BlogContext db;
            private readonly IEnumerable<int> categoryIds;
            public BlogProvider(BlogContext blogContext, IEnumerable<int> catIds)
            {
                db = blogContext;
                categoryIds = catIds;
            }

            private IQueryable<Blog> Blogs
            {
                get
                {
                    var blogs = db.Blogs.Where(b => b.BlogID > LAST_BLOG_ID);
                    if (categoryIds != null) return blogs.Where(b => categoryIds.Contains(b.CategoryID));
                    return blogs;
                }
            }

            public int Count() => Blogs.Where(b => b.BlogID > LAST_BLOG_ID).Count();

            public IEnumerable<ElasticCommon.BlogIndexed> GetBlogs(int skip, int size)
            {
                var blogs = Blogs.Where(b => b.BlogID > LAST_BLOG_ID).OrderBy(b => b.BlogID).Skip(skip).Take(size)
                    .Select(b => new
                    {
                        b.BlogID, b.BlogTitle, b.Content, b.BlogDate, b.CategoryID,
                        b.Author, b.isHarmony, b.isApproved, b.BlogVisit, b.Rating,
                        b.ImagePath, b.IsLocalImg,
                        PostCount = db.Posts.Count(p => p.IdType == ItemType.Blog && p.ItemId == b.BlogID),
                    }).ToList();
                var blogIds = blogs.Select(b => b.BlogID).ToList();
                var tags = db.TagsInBlogs
                    .Where(tib => blogIds.Contains(tib.BlogID))
                    .Select(tib => new { tib.BlogID, tib.tag.TagName })
                    .ToList().GroupBy(tib => tib.BlogID)
                    .ToDictionary(g => g.Key, g => g.Select(t => t.TagName).ToList());
                return blogs.Select(b => new ElasticCommon.BlogIndexed
                {
                    Id = b.BlogID, Title = b.BlogTitle, Content = b.Content,
                    Tags = tags.TryGetValue(b.BlogID, out var t) ? t : Enumerable.Empty<string>(),
                    CategoryId = b.CategoryID, CreateDate = b.BlogDate, Author = b.Author,
                    IsHarmony = b.isHarmony, IsApproved = b.isApproved, BlogVisit = b.BlogVisit,
                    PostCount = b.PostCount, Rating = b.Rating ?? 0, ImagePath = b.ImagePath, IsLocalImg = b.IsLocalImg,
                });
            }
        }

        const int LAST_BLOG_ID = 0;
        const int BATCH_SIZE = 5000;

        private static void UpdateBlogs(BlogProvider blogProvider, ElasticsearchClient client)
        {
            var totalBlogs = blogProvider.Count();
            Console.WriteLine($"total blogs: {totalBlogs}");
            int lastBlogId = LAST_BLOG_ID;
            for (int i = 0; i < totalBlogs; i += BATCH_SIZE)
            {
                var blogs = blogProvider.GetBlogs(i, BATCH_SIZE).ToList();
                Console.WriteLine($"Send Items for {i} to {i + BATCH_SIZE - 1} count={blogs.Count}");
                var bulkResp = client.Bulk(b => b.Index("blogs").IndexMany(blogs, (op, doc) => op.Id(doc.Id)));
                if (!bulkResp.IsValidResponse) Console.WriteLine($"Bulk failed: {bulkResp.DebugInformation}");
                else if (bulkResp.Errors) foreach (var item in bulkResp.ItemsWithErrors) Console.WriteLine($"  ERROR id={item.Id} reason={item.Error?.Reason} caused by {item.Error?.CausedBy?.Reason}");
                else Console.WriteLine($"  Batch {i / BATCH_SIZE + 1} indexed ok {blogs.Count}");
                if (blogs.Count > 0) lastBlogId = blogs.Last().Id;
                if (blogs.Count < BATCH_SIZE) break;
                Thread.Sleep(200);
            }
            client.Indices.Refresh("blogs");
            Console.WriteLine($"last blogs: {lastBlogId}");
            Console.ReadLine();
        }

        public static void Run(string endpoint, string username, string password, bool create = false, IEnumerable<int> categoryIds = null)
        {
            var client = ElasticCommon.CreateClient(endpoint, username, password);
            if (create)
            {
                var resp = client.Indices.Create("blogs", c => c
                    .Settings(s => s
                        .MaxNgramDiff(30)
                        .MaxResultWindow(100000)
                        .MaxRescoreWindow(100000)
                        .Analysis(a => a
                            .Analyzers(an => an.Custom("ngram_lc", cc => cc.Tokenizer("ngram_tokenizer").Filter(new[] { "lowercase" })))
                            .Tokenizers(t => t.NGram("ngram_tokenizer", ng => ng.MinGram(1).MaxGram(30).TokenChars(new[] { TokenChar.Letter, TokenChar.Digit })))
                            .Normalizers(n => n.Custom("lowercase", cn => cn.Filter(new[] { "lowercase" })))
                        )
                    )
                    .Mappings(m => m.Properties<ElasticCommon.BlogIndexed>(p => p
                        .Text(t => t.Title, d => { d.Analyzer("ngram_lc"); })
                        .Text(t => t.Content, d => { d.Analyzer("ngram_lc"); })
                        .Keyword(k => k.Author, d => d.Normalizer("lowercase"))
                        .Keyword(k => k.Tags)
                        .IntegerNumber(n => n.CategoryId)
                        .Date(d => d.CreateDate)
                        .Boolean(b => b.IsHarmony)
                        .Boolean(b => b.IsApproved)
                        .LongNumber(n => n.BlogVisit)
                        .IntegerNumber(n => n.PostCount)
                        .IntegerNumber(n => n.Rating)
                        .Keyword(k => k.ImagePath, d => d.Index(false))
                        .Boolean(b => b.IsLocalImg, d => d.Index(false))
                    ))
                );
                if (!resp.IsValidResponse)
                {
                    Console.WriteLine("error creating index:");
                    Console.WriteLine(resp.DebugInformation);
                    return;
                }
                Console.WriteLine("Index created.");
            }
            var factory = new BlogContextFactory();
            using var db = factory.CreateDbContext(null);
            UpdateBlogs(new BlogProvider(db, categoryIds), client);
        }
    }
}
