using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

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

            Console.WriteLine(author != null ? $"Fetching DB blog IDs for author '{author}'..." : "Fetching all DB blog IDs...");
            var dbQuery = db.Blogs.Where(b => b.BlogID > 0).Select(b => new { b.BlogID, b.isApproved, b.BlogDate, b.CategoryID, b.Author });
            if (author != null) dbQuery = dbQuery.Where(b => b.Author == author);
            var dbBlogs = dbQuery.OrderBy(b => b.BlogID).ToList();
            Console.WriteLine($"DB total: {dbBlogs.Count}");
            foreach (var g in dbBlogs.GroupBy(b => b.isApproved).Select(g => new { Approved = g.Key, Count = g.Count() }))
                Console.WriteLine($"  isApproved={g.Approved?.ToString() ?? "null"}: {g.Count}");

            Console.WriteLine("Fetching ES indexed IDs (search_after)...");
            var client = ElasticCommon.CreateClient(endpoint, username, password);
            var esIds = new HashSet<int>();

            string pitId = null;
            try
            {
                var pitResp = client.OpenPointInTime("blogs", p => p.KeepAlive("2m"));
                if (pitResp.IsValidResponse) pitId = pitResp.Id;
            }
            catch (Exception ex) { Console.WriteLine($"PIT open failed, fallback to sort: {ex.Message}"); }

            IReadOnlyCollection<FieldValue> searchAfter = null;
            int scrolled = 0;
            while (true)
            {
                var searchResp = client.Search<ElasticCommon.BlogIndexed>(s =>
                {
                    s.Size(SCROLL_SIZE).Source(false).Sort(srt => srt.Field("_id", d => d.Order(SortOrder.Asc)));
                    if (!string.IsNullOrEmpty(pitId)) s.Pit(p => p.Id(pitId).KeepAlive("2m"));
                    else s.Indices("blogs");
                    if (searchAfter != null) s.SearchAfter(searchAfter.ToList());
                    if (author != null) s.Query(q => q.Term(t => t.Field(new Field("author.keyword")).Value(author)));
                    else s.Query(q => q.MatchAll());
                });

                if (!searchResp.IsValidResponse)
                {
                    Console.WriteLine($"ES search failed: {searchResp.DebugInformation}");
                    break;
                }
                if (searchResp.Hits.Count == 0) break;
                foreach (var hit in searchResp.Hits) if (int.TryParse(hit.Id, out int id)) esIds.Add(id);
                scrolled += searchResp.Hits.Count;
                Console.Write($"\r  Scrolled {esIds.Count} (batch {searchResp.Hits.Count})...");
                searchAfter = searchResp.Hits.LastOrDefault()?.Sort;
                if (searchResp.Hits.Count < SCROLL_SIZE) break;
            }
            if (pitId != null) try { client.ClosePointInTime(c => c.Id(pitId)); } catch { }

            Console.WriteLine();
            Console.WriteLine($"ES total indexed: {esIds.Count}");

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

            if (reindex && missingInEs.Count > 0)
            {
                var missingIds = missingInEs.Select(b => b.BlogID).ToList();
                Console.WriteLine($"\nReindexing {missingIds.Count} missing blogs in batches of {REINDEX_BATCH_SIZE}...");
                for (int i = 0; i < missingIds.Count; i += REINDEX_BATCH_SIZE)
                {
                    var batchIds = missingIds.Skip(i).Take(REINDEX_BATCH_SIZE).ToList();
                    Console.WriteLine($"  Batch {i / REINDEX_BATCH_SIZE + 1}: BlogIDs {batchIds.First()}..{batchIds.Last()}");
                    var blogs = db.Blogs.Where(b => batchIds.Contains(b.BlogID)).Select(b => new
                    {
                        b.BlogID, b.BlogTitle, b.Content, b.BlogDate, b.CategoryID,
                        b.Author, b.isHarmony, b.isApproved, b.BlogVisit, b.Rating, b.ImagePath, b.IsLocalImg,
                        PostCount = db.Posts.Count(p => p.IdType == ItemType.Blog && p.ItemId == b.BlogID),
                    }).ToList();
                    var blogIdsList = blogs.Select(b => b.BlogID).ToList();
                    var tags = db.TagsInBlogs.Where(tib => blogIdsList.Contains(tib.BlogID)).Select(tib => new { tib.BlogID, tib.tag.TagName }).ToList().GroupBy(tib => tib.BlogID).ToDictionary(g => g.Key, g => g.Select(t => t.TagName).ToList());
                    var docs = blogs.Select(b => new ElasticCommon.BlogIndexed
                    {
                        Id = b.BlogID, Title = b.BlogTitle ?? "",
                        Content = b.Content != null && b.Content.Length > 100_000 ? b.Content.Substring(0, 100_000) : (b.Content ?? ""),
                        Tags = tags.TryGetValue(b.BlogID, out var tt) ? tt : Enumerable.Empty<string>(),
                        CreateDate = b.BlogDate, CategoryId = b.CategoryID, Author = b.Author ?? "", IsHarmony = b.isHarmony, IsApproved = b.isApproved,
                        BlogVisit = b.BlogVisit, PostCount = b.PostCount, Rating = b.Rating ?? 0, ImagePath = b.ImagePath ?? "", IsLocalImg = b.IsLocalImg,
                    }).ToList();
                    var bulkResp = client.Bulk(b => b.Index("blogs").IndexMany(docs, (op, doc) => op.Id(doc.Id)));
                    if (!bulkResp.IsValidResponse) Console.WriteLine($"    Request error: {bulkResp.DebugInformation}");
                    if (bulkResp.Errors) foreach (var item in bulkResp.ItemsWithErrors) Console.WriteLine($"    ERROR BlogID={item.Id}: {item.Error?.Reason} | caused by: {item.Error?.CausedBy?.Reason}");
                    Console.WriteLine($"    Indexed {bulkResp.Items.Count - bulkResp.ItemsWithErrors.Count()}/{bulkResp.Items.Count}, errors: {bulkResp.ItemsWithErrors.Count()}");
                }
                client.Indices.Refresh("blogs");
                Console.WriteLine("Reindex complete.");
            }
        }
    }
}
