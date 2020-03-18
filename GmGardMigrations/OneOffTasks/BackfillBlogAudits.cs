using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class BackfillBlogAudits
    {
        public static void Run()
        {
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            UsersContextFactory usersContextFactory = new UsersContextFactory();
            using (var udb = usersContextFactory.Create())
            using (var db = blogContextFactory.Create())
            {
                var start = new DateTime(2017, 11, 24);
                var audits = db.BlogAudits.Where(b => b.AuditDate > start).GroupBy(b => b.BlogID);
                var finalizedAudits = audits.Where(g => g.Any(b => b.AuditAction == BlogAudit.Action.Approve || b.AuditAction == BlogAudit.Action.Deny));
                var latestAuditors = finalizedAudits.SelectMany(g => g.Select(b => b.Auditor));
                var stats = db.BlogAudits.Where(ba => ba.AuditAction == BlogAudit.Action.Approve || ba.AuditAction == BlogAudit.Action.Deny)
                                            .GroupJoin(db.BlogAudits.Where(ba => latestAuditors.Contains(ba.Auditor)
                                                    && (ba.AuditAction == BlogAudit.Action.VoteApprove || ba.AuditAction == BlogAudit.Action.VoteDeny)),
                                                ba => new { ba.BlogID, ba.BlogVersion }, la => new { la.BlogID, la.BlogVersion }, (ba, la) => new { Decision = ba, Votes = la })
                                            .SelectMany(d => d.Votes, (d, v) => new
                                            {
                                                Auditor = v.Auditor,
                                                Correct = (v.AuditAction == BlogAudit.Action.VoteApprove && d.Decision.AuditAction == BlogAudit.Action.Approve)
                                                       || (v.AuditAction == BlogAudit.Action.VoteDeny && d.Decision.AuditAction == BlogAudit.Action.Deny)
                                            }).GroupBy(v => v.Auditor).ToDictionary(g => g.Key.ToLower(), g => new { CorrectCount = g.Count(d => d.Correct), Total = g.Count() });
                var usersToUpdate = stats.Keys;
                var updates = udb.Auditors.Where(a => usersToUpdate.Contains(a.User.UserName)).Select(a => new { a.User.UserName, Auditor = a });
                foreach (var update in updates)
                {
                    int total = 0, correctcount = 0;
                    if (stats.ContainsKey(update.UserName.ToLower()))
                    {
                        var stat = stats[update.UserName.ToLower()];
                        total = stat.Total;
                        correctcount = stat.CorrectCount;
                    }

                    update.Auditor.AuditCount = total + 1;
                    update.Auditor.CorrectCount = correctcount;
                }
                udb.SaveChanges();
            }
        }
    }
}
