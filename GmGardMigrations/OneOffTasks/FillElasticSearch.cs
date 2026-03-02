using Elasticsearch.Net;
using GmGard.Models;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class FillElasticSearch
    {
        public class BlogIndexed
        {
            public int Id { get; set; }
            [Text]
            public string Title { get; set; }
            [Text]
            public string Content { get; set; }
            [Keyword]
            public IEnumerable<string> Tags { get; set; }
            public int CategoryId { get; set; }
            public DateTime CreateDate { get; set; }
            [Keyword]
            public string Author { get; set; }
            public bool IsHarmony { get; set; }
            public bool? IsApproved { get; set; }
            public long BlogVisit { get; set; }
            public int PostCount { get; set; }
            public int Rating { get; set; }
            [Keyword(Index = false)]
            public string ImagePath { get; set; }
            [Boolean(Index = false)]
            public bool IsLocalImg { get; set; }
        }

        class BlogProvider
        {
            private readonly BlogContext db;
            private readonly IEnumerable<int> categoryIds;
            public BlogProvider(BlogContext blogContext, IEnumerable<int> catIds)
            {
                db = blogContext;
                categoryIds = catIds;
            }

            private IQueryable<GmGard.Models.Blog> Blogs {
                get
                {
                    var blogs = db.Blogs.Where(b => b.BlogID > LAST_BLOG_ID);
                    if (categoryIds != null)
                    {
                        return blogs.Where(b => categoryIds.Contains(b.CategoryID));
                    }
                    return blogs;
                }
            }

            public int Count()
            {
                return Blogs.Where(b => b.BlogID > LAST_BLOG_ID).Count();
            }

            public IEnumerable<BlogIndexed> GetBlogs(int skip, int size)
            {
                var blogs = Blogs.Where(b => b.BlogID > LAST_BLOG_ID).OrderBy(b => b.BlogID).Skip(skip).Take(size)
                    .Select(b => new
                    {
                        b.BlogID,
                        b.BlogTitle,
                        b.Content,
                        b.BlogDate,
                        b.CategoryID,
                        b.Author,
                        b.isHarmony,
                        b.isApproved,
                        b.BlogVisit,
                        b.Rating,
                        b.ImagePath,
                        b.IsLocalImg,
                        PostCount = db.Posts.Count(p => p.IdType == GmGard.Models.ItemType.Blog && p.ItemId == b.BlogID),
                    }).ToList();
                var blogIds = blogs.Select(b => b.BlogID).ToList();
                var tags = db.TagsInBlogs
                    .Where(tib => blogIds.Contains(tib.BlogID))
                    .Select(tib => new { tib.BlogID, tib.tag.TagName })
                    .ToList()
                    .GroupBy(tib => tib.BlogID)
                    .ToDictionary(g => g.Key, g => g.Select(t => t.TagName).ToList());
                return blogs.Select(b => new BlogIndexed
                {
                    Id = b.BlogID,
                    Title = b.BlogTitle,
                    Content = b.Content,
                    Tags = tags.TryGetValue(b.BlogID, out var t) ? t : Enumerable.Empty<string>(),
                    CreateDate = b.BlogDate,
                    CategoryId = b.CategoryID,
                    Author = b.Author,
                    IsHarmony = b.isHarmony,
                    IsApproved = b.isApproved,
                    BlogVisit = b.BlogVisit,
                    PostCount = b.PostCount,
                    Rating = b.Rating ?? 0,
                    ImagePath = b.ImagePath,
                    IsLocalImg = b.IsLocalImg,
                });
            }
        }

        const int LAST_BLOG_ID = 0;
        const int BATCH_SIZE = 5000;
        

        private static void UpdateBlogs(BlogProvider blogProvider, ElasticClient client)
        {
            var totalBlogs = blogProvider.Count();
            Console.WriteLine($"total blogs: {totalBlogs}");
            int lastBlogId = LAST_BLOG_ID;
            for (int i = 0; i < totalBlogs; i += BATCH_SIZE)
            {
                var blogs = blogProvider.GetBlogs(i, BATCH_SIZE);
                Console.WriteLine($"Send Items for {i} to {i + BATCH_SIZE - 1}");
                var bulk = client.BulkAll(blogs, s => s
                    // in case of 429 response, how long we should wait before retrying
                    .BackOffTime(TimeSpan.FromSeconds(5))
                    // in case of 429 response, how many times to retry before failing
                    .BackOffRetries(5)
                    .Index<BlogIndexed>());
                var waitHandle = new ManualResetEvent(false);
                var bulkAllObserver = new BulkAllObserver(
                    onNext: bulkAllResponse =>
                    {
                        Console.WriteLine($"Done page {bulkAllResponse.Page} with retry {bulkAllResponse.Retries}");
                    },
                    onError: exception =>
                    {
                        waitHandle.Set();
                        throw exception;
                    },
                    onCompleted: () =>
                    {
                        waitHandle.Set();
                    });
                bulk.Subscribe(bulkAllObserver);
                waitHandle.WaitOne();
                if (blogs.Count() > 0)
                {
                    lastBlogId = blogs.Last().Id;
                }
                if (blogs.Count() < BATCH_SIZE)
                {
                    break;
                }
            }
            client.Indices.Refresh(Indices.Index("blogs"));
            Console.WriteLine($"last blogs: {lastBlogId}");
            Console.ReadLine();
        }

        public static void Run(string endpoint, string username, string password, bool create = false, IEnumerable<int> categoryIds = null)
        {
            var settings = new ConnectionSettings(new Uri(endpoint)).DefaultIndex("blogs").BasicAuthentication(username, password)
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            var client = new ElasticClient(settings);
            if (create)
            {
                var resp = client.Indices.Create("blogs", cid => cid
                    .Map<BlogIndexed>(m => m.AutoMap()
                        .Properties(p => p.Keyword(kp => kp.Name(b => b.Author).Normalizer("lowercase")))
                        .Properties(p => p.Text(tp => tp.Name(b => b.Title).Fields(f => f.Text(tf => tf.Analyzer("ngram_lc").Name("ngram_lc")))))
                        .Properties(p => p.Keyword(tp => tp.Name(b => b.Tags).Fields(f => f.Text(tf => tf.Analyzer("ngram_lc").Name("ngram_lc"))))))
                    .Settings(i =>
                        i.Setting("max_ngram_diff", 30)
                        .Setting("max_result_window", 100000)
                        .Setting("max_rescore_window", 100000)
                        .Analysis(a =>
                            a.Analyzers(ana =>
                                ana.Custom("ngram_lc", c => c.Filters("lowercase").Tokenizer("ngram_tokenizer")))
                            .Tokenizers(t => 
                                t.NGram("ngram_tokenizer", n => n.MaxGram(30).MinGram(1).TokenChars(TokenChar.Letter, TokenChar.Digit)))
                            .Normalizers(n => n.Custom("lowercase", cn => cn.Filters("lowercase") )))));
                if (!resp.IsValid)
                {
                    Console.WriteLine("error creating index:");
                    Console.WriteLine(resp.DebugInformation);
                    return;
                }
            }
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            using var db = blogContextFactory.CreateDbContext(null);
            UpdateBlogs(new BlogProvider(db, categoryIds), client);
        }
        }
    }
