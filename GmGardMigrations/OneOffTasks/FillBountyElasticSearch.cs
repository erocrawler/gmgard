using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GmGard.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Microsoft.EntityFrameworkCore;

namespace GmGardMigrations.OneOffTasks
{
    // One-off to populate bounties index: title, content, answer content
    // Creates index with ngram_lc analyzer (lowercase) same as blogs for case-insensitive search
    class FillBountyElasticSearch
    {
        static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            var plain = Regex.Replace(html, "<.*?>", " ");
            return System.Net.WebUtility.HtmlDecode(plain);
        }

        public class BountyIndexedDoc
        {
            public int Id { get; set; }
            public string Title { get; set; } = "";
            public string Content { get; set; } = "";
            public string AnswerContent { get; set; } = "";
            public string Author { get; set; } = "";
            public DateTime CreateDate { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsAccepted { get; set; }
            public int Prize { get; set; }
            public int AnswerCount { get; set; }
        }

        public static void CreateIndex(ElasticsearchClient client, bool deleteIfExists = true)
        {
            if (deleteIfExists)
            {
                var exists = client.Indices.Exists("bounties");
                if (exists.Exists)
                {
                    Console.WriteLine("Deleting existing bounties index...");
                    client.Indices.Delete("bounties");
                }
            }

            // Hybrid: primary standard_lc for relevance, ngram_lc subfield for substring fallback
            var resp = client.Indices.Create("bounties", c => c
                .Settings(s => s
                    .MaxNgramDiff(20)
                    .MaxResultWindow(100000)
                    .MaxRescoreWindow(100000)
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("ngram_lc", cc => cc.Tokenizer("ngram_tokenizer").Filter(new[] { "lowercase", "asciifolding" }))
                            .Custom("standard_lc", cc => cc.Tokenizer("standard").Filter(new[] { "lowercase", "asciifolding" }))
                        )
                        .Tokenizers(t => t.NGram("ngram_tokenizer", ng => ng.MinGram(2).MaxGram(15).TokenChars(new[] { TokenChar.Letter, TokenChar.Digit })))
                        .Normalizers(n => n.Custom("lowercase", cn => cn.Filter(new[] { "lowercase", "asciifolding" })))
                    )
                )
                .Mappings(m => m.Properties<BountyIndexedDoc>(p => p
                    .Text(t => t.Title, d => d
                        .Analyzer("standard_lc")
                        .SearchAnalyzer("standard_lc")
                        .Fields(f => f
                            .Text("ngram", fd => fd.Analyzer("ngram_lc").SearchAnalyzer("standard_lc"))
                        )
                    )
                    .Text(t => t.Content, d => d
                        .Analyzer("standard_lc")
                        .SearchAnalyzer("standard_lc")
                        .Fields(f => f
                            .Text("ngram", fd => fd.Analyzer("ngram_lc").SearchAnalyzer("standard_lc"))
                        )
                    )
                    .Text(t => t.AnswerContent, d => d
                        .Analyzer("standard_lc")
                        .SearchAnalyzer("standard_lc")
                        .Fields(f => f
                            .Text("ngram", fd => fd.Analyzer("ngram_lc").SearchAnalyzer("standard_lc"))
                        )
                    )
                    .Keyword(k => k.Author, d => d.Normalizer("lowercase"))
                    .Date(d => d.CreateDate)
                    .Boolean(b => b.IsDeleted)
                    .Boolean(b => b.IsAccepted)
                    .IntegerNumber(n => n.Prize)
                    .IntegerNumber(n => n.AnswerCount)
                ))
            );

            if (!resp.IsValidResponse)
            {
                Console.WriteLine("error creating bounties index:");
                Console.WriteLine(resp.DebugInformation);
                throw new Exception($"Failed to create bounties index: {resp.DebugInformation}");
            }
            Console.WriteLine("bounties index created with ngram_lc analyzer (case-insensitive).");
        }

        public static void Fill(ElasticsearchClient client, BlogContext db, int batch = 200, bool create = false)
        {
            if (create)
            {
                CreateIndex(client, deleteIfExists: true);
            }
            else
            {
                // ensure exists, if not create
                var exists = client.Indices.Exists("bounties");
                if (!exists.Exists)
                {
                    CreateIndex(client, deleteIfExists: false);
                }
            }

            var total = db.Bounties.Count(b => !b.IsDeleted);
            Console.WriteLine($"Bounties to index: {total}");
            int skip = 0;
            while (true)
            {
                var bountyPage = db.Bounties.Where(b => !b.IsDeleted).OrderBy(b => b.BountyId).Skip(skip).Take(batch)
                    .Select(b => new { b.BountyId, b.Title, b.Content, b.Author, b.CreateDate, b.IsDeleted, b.IsAccepted, b.Prize }).ToList();
                if (!bountyPage.Any()) break;
                var ids = bountyPage.Select(b => b.BountyId).ToList();
                var answersLookup = db.Answers.Where(a => ids.Contains(a.BountyId)).Select(a => new { a.BountyId, a.Content }).ToList()
                    .GroupBy(a => a.BountyId).ToDictionary(g => g.Key, g => g.Select(x => x.Content).ToList());

                var docs = bountyPage.Select(b =>
                {
                    answersLookup.TryGetValue(b.BountyId, out var ans);
                    var answerCombined = ans != null ? string.Join("\n", ans.Where(s => !string.IsNullOrWhiteSpace(s)).Select(StripHtml)) : "";
                    return new BountyIndexedDoc
                    {
                        Id = b.BountyId,
                        Title = StripHtml(b.Title ?? ""),
                        Content = StripHtml(b.Content ?? ""),
                        AnswerContent = answerCombined,
                        Author = b.Author ?? "",
                        CreateDate = b.CreateDate,
                        IsDeleted = b.IsDeleted,
                        IsAccepted = b.IsAccepted,
                        Prize = b.Prize,
                        AnswerCount = ans?.Count ?? 0
                    };
                }).ToList();

                var bulkResp = client.Bulk(b => b.Index("bounties").IndexMany(docs, (op, doc) => op.Id(doc.Id)));
                if (!bulkResp.IsValidResponse)
                {
                    Console.WriteLine($"Bulk failed: {bulkResp.DebugInformation}");
                }
                else if (bulkResp.Errors)
                {
                    foreach (var item in bulkResp.ItemsWithErrors) Console.WriteLine($"  ERROR id={item.Id} reason={item.Error?.Reason}");
                }
                else
                {
                    Console.WriteLine($"Indexed {skip}-{skip + docs.Count - 1} ({docs.Count})");
                }
                skip += batch;
                if (bountyPage.Count < batch) break;
            }
            client.Indices.Refresh("bounties");
            Console.WriteLine("Refresh done.");
        }

        // Entry for manual execution
        public static void Run(string endpoint, string username, string password, bool recreate = false)
        {
            var client = ElasticCommon.CreateClient(endpoint, username, password);
            var factory = new BlogContextFactory();
            using var db = factory.CreateDbContext(null);
            Fill(client, db, batch: 200, create: recreate);
        }
    }
}
