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
        
        const int LAST_BLOG_ID = 100000;
        const int BATCH_SIZE = 10000;

        public static void Run(string endpoint, string username, string password, bool create = false)
        {
            var settings = new ConnectionSettings(new Uri(endpoint)).DefaultIndex("blogs").BasicAuthentication(username, password);
            var client = new ElasticClient(settings);
            if (create)
            {
                var resp = client.Indices.Create("blogs", cid => cid
                    .Map<BlogIndexed>(m => m.AutoMap()
                        .Properties(p => p.Keyword(kp => kp.Name(b => b.Author).Normalizer("lowercase")))
                        .Properties(p => p.Text(tp => tp.Name(b => b.Title).Fields(f => f.Text(tf => tf.Analyzer("ngram_lc").Name("ngram_lc")))))
                        .Properties(p => p.Keyword(tp => tp.Name(b => b.Tags).Fields(f => f.Text(tf => tf.Analyzer("ngram_lc").Name("ngram_lc"))))))
                    .Settings(i => i.Setting("max_ngram_diff", 30).Setting("max_result_window", 100000).Analysis(a => 
                        a.Analyzers(ana => ana.Custom("ngram_lc", c => c.Filters("lowercase").Tokenizer("ngram_tokenizer")))
                        .Tokenizers(t => t.NGram("ngram_tokenizer", n => n.MaxGram(30).MinGram(1).TokenChars(TokenChar.Letter, TokenChar.Digit)))
                        .Normalizers(n => n.Custom("lowercase", cn => cn.Filters("lowercase") )))));
                if (!resp.IsValid)
                {
                    Console.WriteLine("error creating index");
                    return;
                }
            }
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            using (var db = blogContextFactory.Create())
            {
                var totalBlogs = db.Blogs.Where(b => b.BlogID > 0).Count();
                Console.WriteLine($"total blogs: {totalBlogs}");
                int lastBlogId = LAST_BLOG_ID;
                for (int i = 0; i < totalBlogs; i += BATCH_SIZE)
                {
                    var blogs = db.Blogs.Where(b => b.BlogID > LAST_BLOG_ID).OrderBy(b => b.BlogID).Skip(i).Take(BATCH_SIZE)
                        .GroupJoin(db.Posts.Where(p => p.IdType == GmGard.Models.ItemType.Blog), b => b.BlogID, p => p.PostId, (b, p) => new { blog = b, post = p.Count() })
                        .GroupJoin(db.TagsInBlogs.DefaultIfEmpty(),
                            b => b.blog.BlogID,
                            tib => tib.BlogID,
                            (b, tib) => new
                            {
                                b.blog,
                                tag = tib.Select(t => t.tag),
                                b.post,
                            }).ToList();
                    Console.WriteLine($"Send Items for {i} to {i + BATCH_SIZE - 1}");
                    var bulk = client.BulkAll(blogs.Select(b => new BlogIndexed
                    {
                        Id = b.blog.BlogID,
                        Title = b.blog.BlogTitle,
                        Content = b.blog.Content,
                        Tags = b.tag.Select(t => t.TagName),
                        CreateDate = b.blog.BlogDate,
                        CategoryId = b.blog.CategoryID,
                        Author = b.blog.Author,
                        IsHarmony = b.blog.isHarmony,
                        IsApproved = b.blog.isApproved,
                        BlogVisit = b.blog.BlogVisit,
                        PostCount = b.post,
                        Rating = b.blog.Rating ?? 0,
                        ImagePath = b.blog.ImagePath,
                        IsLocalImg = b.blog.IsLocalImg,
                    }), s => s
                        // in case of 429 response, how long we should wait before retrying
                        .BackOffTime(TimeSpan.FromSeconds(5))
                        // in case of 429 response, how many times to retry before failing
                        .BackOffRetries(5)
                        .Index<BlogIndexed>());
                    var waitHandle = new ManualResetEvent(false);
                    var bulkAllObserver = new BulkAllObserver(
                        onNext: bulkAllResponse =>
                        {
                            // do something after each bulk request
                            Console.WriteLine($"Done page {bulkAllResponse.Page} with retry {bulkAllResponse.Retries}");
                        },
                        onError: exception =>
                        {
                            waitHandle.Set();
                            throw exception;
                        },
                        onCompleted: () =>
                        {
                            // do something when all bulk operations complete
                            waitHandle.Set();
                        });
                    bulk.Subscribe(bulkAllObserver);
                    waitHandle.WaitOne();
                    if (blogs.Count > 0)
                    {
                        lastBlogId = blogs.Last().blog.BlogID;
                    }
                    if (blogs.Count < BATCH_SIZE)
                    {
                        break;
                    }
                }
                client.Indices.Refresh(Indices.Index("blogs"));
                Console.WriteLine($"last blogs: {lastBlogId}");
                Console.ReadLine();
            }
        }
    }
}
