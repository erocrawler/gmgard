using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GmGard.Models;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;

namespace GmGardMigrations.OneOffTasks
{
    // One-off to populate bounties index: title, content, answer content
    class FillBountyElasticSearch
    {
        static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return "";
            var plain = Regex.Replace(html, "<.*?>", " ");
            return System.Net.WebUtility.HtmlDecode(plain);
        }

        public static void Fill(ElasticsearchClient client, BlogContext db, int batch = 200)
        {
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
                    return new
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

                var resp = client.IndexMany(docs, "bounties");
                if (!resp.IsValidResponse)
                {
                    Console.WriteLine($"Bulk failed: {resp.DebugInformation}");
                }
                else
                {
                    Console.WriteLine($"Indexed {skip}-{skip + docs.Count}");
                }
                skip += batch;
                if (bountyPage.Count < batch) break;
            }
            client.Indices.Refresh("bounties");
        }
    }
}
