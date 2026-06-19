using Elasticsearch.Net;
using GmGard.Models;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GmGardMigrations.OneOffTasks
{
    class FindMissingFromElasticSearch
    {
        const int SCROLL_SIZE = 5000;
        const int REINDEX_BATCH_SIZE = 500;

        public static void Run(string endpoint, string username, string password, string author = null, bool reindex = false)
        {
            var factory = new BlogContextFactory();
            using var db = factory.CreateDbContext(Array.Empty<string>());

            // 1. Fetch all blog IDs from DB (or filtered by author)
            Console.WriteLine(author != null ? $"Fetching DB blog IDs for author '{author}'..." : "Fetching all DB blog IDs...");
            var dbQuery = db.Blogs.Where(b => b.BlogID > 0).Select(b => new { b.BlogID, b.isApproved, b.BlogDate, b.CategoryID, b.Author });
            if (author != null)
                dbQuery = dbQuery.Where(b => b.Author == author);
            var dbBlogs = dbQuery.OrderBy(b => b.BlogID).ToList();
            Console.WriteLine($"DB total: {dbBlogs.Count}");

            var breakdown = dbBlogs.GroupBy(b => b.isApproved).Select(g => new { Approved = g.Key, Count = g.Count() });
            foreach (var g in breakdown)
                Console.WriteLine($"  isApproved={g.Approved?.ToString() ?? "null"}: {g.Count}");

            // 2. Fetch all indexed IDs from ES using scroll
            Console.WriteLine("Fetching ES indexed IDs (scroll)...");
            var settings = new ConnectionSettings(new Uri(endpoint))
                .DefaultIndex("blogs")
                .BasicAuthentication(username, password)
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            var client = new ElasticClient(settings);

            var esIds = new HashSet<int>();
            var firstSearch = client.Search<BlogIdOnly>(s => {
                var q = s.Source(false).Size(SCROLL_SIZE).Scroll("2m");
                if (author != null)
                    q = q.Query(qq => qq.Term("author", author));
                else
                    q = q.Query(qq => qq.MatchAll());
                return q;
            });

            if (!firstSearch.IsValid)
            {
                Console.WriteLine($"ES search failed: {firstSearch.ServerError}");
                return;
            }

            var scrollId = firstSearch.ScrollId;
            foreach (var hit in firstSearch.Hits)
                if (int.TryParse(hit.Id, out int id)) esIds.Add(id);

            Console.Write($"  Scrolled {esIds.Count}...");
            while (true)
            {
                var scrollResp = client.Scroll<BlogIdOnly>("2m", scrollId);
                if (!scrollResp.IsValid || scrollResp.Hits.Count == 0) break;
                scrollId = scrollResp.ScrollId;
                foreach (var hit in scrollResp.Hits)
                    if (int.TryParse(hit.Id, out int id)) esIds.Add(id);
                Console.Write($"\r  Scrolled {esIds.Count}...");
            }
            Console.WriteLine();
            if (scrollId != null)
                client.ClearScroll(c => c.ScrollId(scrollId));
            Console.WriteLine($"ES total indexed: {esIds.Count}");

            // 3. Find DB IDs missing from ES
            var dbIds = dbBlogs.Select(b => b.BlogID).ToHashSet();
            var missingInEs = dbBlogs.Where(b => !esIds.Contains(b.BlogID)).ToList();
            var extraInEs = esIds.Where(id => !dbIds.Contains(id)).ToList();

            Console.WriteLine($"\nBlogs in DB but NOT in ES: {missingInEs.Count}");
            if (missingInEs.Count > 0)
            {
                foreach (var g in missingInEs.GroupBy(b => b.isApproved))
                    Console.WriteLine($"  isApproved={g.Key?.ToString() ?? "null"}: {g.Count()}");

                Console.WriteLine("  By category (top 20):");
                foreach (var g in missingInEs.GroupBy(b => b.CategoryID).OrderByDescending(g => g.Count()).Take(20))
                    Console.WriteLine($"    CategoryID={g.Key}: {g.Count()}");

                Console.WriteLine("  By author (top 20):");
                foreach (var g in missingInEs.GroupBy(b => b.Author).OrderByDescending(g => g.Count()).Take(20))
                    Console.WriteLine($"    {g.Key}: {g.Count()}");

                var oldest = missingInEs.OrderBy(b => b.BlogDate).First();
                var newest = missingInEs.OrderByDescending(b => b.BlogDate).First();
                Console.WriteLine($"  Oldest missing: BlogID={oldest.BlogID}, Date={oldest.BlogDate:yyyy-MM-dd}");
                Console.WriteLine($"  Newest missing: BlogID={newest.BlogID}, Date={newest.BlogDate:yyyy-MM-dd}");
            }

            Console.WriteLine($"\nBlogs in ES but NOT in DB (stale): {extraInEs.Count}");

            // 4. Optionally reindex missing blogs in batches
            if (reindex && missingInEs.Count > 0)
            {
                var missingIds = missingInEs.Select(b => b.BlogID).ToList();
                Console.WriteLine($"\nReindexing {missingIds.Count} missing blogs in batches of {REINDEX_BATCH_SIZE}...");

                for (int i = 0; i < missingIds.Count; i += REINDEX_BATCH_SIZE)
                {
                    var batchIds = missingIds.Skip(i).Take(REINDEX_BATCH_SIZE).ToList();
                    Console.WriteLine($"  Batch {i / REINDEX_BATCH_SIZE + 1}: BlogIDs {batchIds.First()}..{batchIds.Last()}");

                    var blogs = db.Blogs
                        .Where(b => batchIds.Contains(b.BlogID))
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
                        .ToList()
                        .GroupBy(tib => tib.BlogID)
                        .ToDictionary(g => g.Key, g => g.Select(t => t.TagName).ToList());

                    var docs = blogs.Select(b => new BlogIdOnly
                    {
                        Id = b.BlogID,
                        Title = b.BlogTitle ?? "",
                        Content = b.Content != null && b.Content.Length > 100_000 ? b.Content.Substring(0, 100_000) : (b.Content ?? ""),
                        Tags = tags.TryGetValue(b.BlogID, out var t) ? t : Enumerable.Empty<string>(),
                        CreateDate = b.BlogDate,
                        CategoryId = b.CategoryID,
                        Author = b.Author ?? "",
                        IsHarmony = b.isHarmony,
                        IsApproved = b.isApproved,
                        BlogVisit = b.BlogVisit,
                        PostCount = b.PostCount,
                        Rating = b.Rating ?? 0,
                        ImagePath = b.ImagePath ?? "",
                        IsLocalImg = b.IsLocalImg,
                    }).ToList();

                    // Index one-by-one to surface per-document errors
                    var bulkRequest = new BulkDescriptor();
                    foreach (var doc in docs)
                        bulkRequest.Index<BlogIdOnly>(op => op.Document(doc).Id(doc.Id));

                    var bulkResp = client.Bulk(bulkRequest);
                    if (!bulkResp.IsValid)
                    {
                        Console.WriteLine($"    Request error: {bulkResp.ServerError?.Error?.Reason ?? bulkResp.OriginalException?.Message}");
                    }
                    if (bulkResp.Errors)
                    {
                        foreach (var item in bulkResp.ItemsWithErrors)
                        {
                            var reason = item.Error?.Reason;
                            var causedBy = item.Error?.CausedBy?.Reason;
                            Console.WriteLine($"    ERROR BlogID={item.Id}: {reason} | caused by: {causedBy}");
                        }
                    }
                    Console.WriteLine($"    Indexed {bulkResp.Items.Count - bulkResp.ItemsWithErrors.Count()}/{bulkResp.Items.Count}, errors: {bulkResp.ItemsWithErrors.Count()}");
                }

                client.Indices.Refresh(Indices.Index("blogs"));
                Console.WriteLine("Reindex complete.");
            }
        }

        private class BlogIdOnly
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
    }
}
