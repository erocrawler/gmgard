using GmGard.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GmGard.Services
{
    /// <summary>
    /// Daily scheduler to auto-close expired bounties with auto-accept.
    /// If OP didn't act, system picks best to prevent OP getting answer for free + refund exploit.
    /// Logic:
    /// - No answers: refund total (Prize+Deposit+Helpful) to OP, CloseDate=ExpiresAt, IsAccepted=false
    /// - Has answers: auto-accept: pick best by strategy, deposit refund OP, prize->best, helpful split rest among other answers, remainder->OP
    ///   IsAccepted=true, CloseDate=ExpiresAt, AcceptedAnswerId=best
    /// </summary>
    public class BountyExpireService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BountyExpireService> _logger;

        public BountyExpireService(IServiceScopeFactory scopeFactory, ILogger<BountyExpireService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BountyExpireService started");
            try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch { }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = await ExpireBountiesAsync();
                    if (result.ClosedNoAnswer > 0 || result.AutoAccepted > 0)
                        _logger.LogInformation("Bounty expire: closedNoAnswer={NoAnswer} autoAccepted={Accepted} failed={Failed}", result.ClosedNoAnswer, result.AutoAccepted, result.Failed);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in BountyExpireService");
                }
                try { await Task.Delay(TimeSpan.FromHours(24), stoppingToken); } catch (TaskCanceledException) { break; }
            }
            _logger.LogInformation("BountyExpireService stopping");
        }

        public class ExpireResult
        {
            public int ClosedNoAnswer;
            public int AutoAccepted;
            public int Failed;
        }

        public async Task<ExpireResult> ExpireBountiesAsync()
        {
            var result = new ExpireResult();
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BlogContext>();
            var udb = scope.ServiceProvider.GetRequiredService<UsersContext>();
            var expUtil = scope.ServiceProvider.GetRequiredService<ExpUtil>();
            var cfg = scope.ServiceProvider.GetServices<Microsoft.Extensions.Options.IOptions<GmGard.Models.App.BountyConfig>>().FirstOrDefault()?.Value ?? new GmGard.Models.App.BountyConfig();

            var now = DateTime.UtcNow;
            var expired = await db.Bounties
                .Where(b => !b.IsAccepted && !b.IsDeleted && b.ExpiresAt != null && b.ExpiresAt < now && b.CloseDate == null)
                .Include(b => b.Answers)
                .ToListAsync();

            if (!expired.Any()) return result;

            foreach (var bounty in expired)
            {
                // For each bounty, use separate transaction scope to avoid one failure blocking all
                using var blogTx = await db.Database.BeginTransactionAsync();
                using var userTx = await udb.Database.BeginTransactionAsync();
                try
                {
                    var answers = bounty.Answers?.OrderBy(a => a.CreateDate).ToList() ?? new System.Collections.Generic.List<Answer>();
                    if (answers.Count == 0)
                    {
                        // No answers: refund total to OP
                        var op = await udb.Users.SingleOrDefaultAsync(u => u.UserName == bounty.Author);
                        if (op != null)
                        {
                            int total = bounty.Prize + bounty.Deposit + bounty.HelpfulReward;
                            expUtil.AddPoint(op, total);
                        }
                        bounty.CloseDate = bounty.ExpiresAt ?? now;
                        // IsAccepted remains false = expired without answer
                        await db.SaveChangesAsync();
                        await udb.SaveChangesAsync();
                        await userTx.CommitAsync();
                        await blogTx.CommitAsync();
                        result.ClosedNoAnswer++;

                        // Notifications: expiration to OP + ES cleanup
                        try
                        {
                            var scope2 = _scopeFactory.CreateScope();
                            var msgUtil = scope2.ServiceProvider.GetRequiredService<MessageUtil>();
                            var url = $"/app/bounty/{bounty.BountyId}";
                            msgUtil.SendBountyExpiredNotice(bounty.Author, bounty.Title ?? $"悬赏#{bounty.BountyId}", url);
                            var taskQueue = scope2.ServiceProvider.GetService<BackgroundTaskQueue>();
                            taskQueue?.QueueBackgroundWorkItem(Job.RemoveBounty(new BountyEventArgs { BountyId = bounty.BountyId }));
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "Failed to send bounty expired notice for {Id}", bounty.BountyId); }
                    }
                    else
                    {
                        if (!cfg.AutoAcceptOnExpire)
                        {
                            // Legacy close-only mode (should not happen if cfg true)
                            bounty.CloseDate = bounty.ExpiresAt ?? now;
                            await db.SaveChangesAsync();
                            await blogTx.CommitAsync();
                            result.ClosedNoAnswer++;

                            try
                            {
                                var scope2 = _scopeFactory.CreateScope();
                                var msgUtil = scope2.ServiceProvider.GetRequiredService<MessageUtil>();
                                var url = $"/app/bounty/{bounty.BountyId}";
                                msgUtil.SendBountyExpiredNotice(bounty.Author, bounty.Title ?? $"悬赏#{bounty.BountyId}", url);
                                var taskQueue = scope2.ServiceProvider.GetService<BackgroundTaskQueue>();
                                taskQueue?.QueueBackgroundWorkItem(Job.AddOrUpdateBountyById(bounty.BountyId));
                            }
                            catch (Exception ex) { _logger.LogWarning(ex, "Failed to send bounty expired notice for {Id}", bounty.BountyId); }
                            continue;
                        }

                        // Auto-accept: pick best
                        Answer best = null;
                        if (cfg.AutoPickStrategy == "MostDiscussion")
                        {
                            // count Posts discussing each answer (IdType=Answer)
                            var answerIds = answers.Select(a => a.AnswerId).ToList();
                            var postCounts = await db.Posts.Where(p => p.IdType == ItemType.Answer && answerIds.Contains(p.ItemId))
                                .GroupBy(p => p.ItemId)
                                .Select(g => new { AnswerId = g.Key, Cnt = g.Count() })
                                .ToDictionaryAsync(x => x.AnswerId, x => x.Cnt);
                            best = answers.OrderByDescending(a => postCounts.TryGetValue(a.AnswerId, out var c) ? c : 0).ThenBy(a => a.CreateDate).First();
                        }
                        else // Earliest
                        {
                            best = answers.First();
                        }

                        var helpfulOthers = answers.Where(a => a.AnswerId != best.AnswerId).ToList();

                        // Mark bounty
                        bounty.AcceptedAnswerId = best.AnswerId;
                        bounty.IsAccepted = true;
                        bounty.CloseDate = bounty.ExpiresAt ?? now;
                        best.IsHelpful = false;
                        foreach (var h in helpfulOthers) h.IsHelpful = true; // all rest get helpful if reward pool exists, matches requirement of rewarding genuine answers

                        // Distribution: on auto-expire with answers, deposit is LOST (not returned, not given to best)
                        // - Prize -> best
                        // - Helpful pool split among other answers, remainder -> best (not OP)
                        // - Deposit burned (system forfeiture for abandonment)
                        var bestUser = await udb.Users.SingleOrDefaultAsync(u => u.UserName == best.Author);
                        if (bestUser != null) expUtil.AddPoint(bestUser, bounty.Prize);

                        if (helpfulOthers.Count > 0 && bounty.HelpfulReward > 0)
                        {
                            int per = bounty.HelpfulReward / helpfulOthers.Count;
                            int remainder = bounty.HelpfulReward % helpfulOthers.Count;
                            foreach (var ha in helpfulOthers)
                            {
                                var hu = await udb.Users.SingleOrDefaultAsync(u => u.UserName == ha.Author);
                                if (hu != null) expUtil.AddPoint(hu, per);
                            }
                            if (remainder > 0 && bestUser != null) expUtil.AddPoint(bestUser, remainder);
                        }
                        else if (helpfulOthers.Count == 0 && bounty.HelpfulReward > 0)
                        {
                            // No other answers: helpful pool also goes to best (OP abandoned, so no refund)
                            if (bestUser != null) expUtil.AddPoint(bestUser, bounty.HelpfulReward);
                        }

                        await db.SaveChangesAsync();
                        await udb.SaveChangesAsync();
                        await userTx.CommitAsync();
                        await blogTx.CommitAsync();
                        result.AutoAccepted++;
                        _logger.LogInformation("Auto-accepted expired bounty {BountyId} best={BestId} prize={Prize} helpfulCount={HCount}", bounty.BountyId, best.AnswerId, bounty.Prize, helpfulOthers.Count);

                        // Notifications: OP auto-accepted + best + helpful
                        try
                        {
                            var scope2 = _scopeFactory.CreateScope();
                            var msgUtil = scope2.ServiceProvider.GetRequiredService<MessageUtil>();
                            var taskQueue = scope2.ServiceProvider.GetService<BackgroundTaskQueue>();
                            var baseUrl = $"/app/bounty/{bounty.BountyId}";
                            // OP
                            msgUtil.SendBountyAutoAcceptedNotice(bounty.Author, best.Author, bounty.Title ?? $"悬赏#{bounty.BountyId}", baseUrl, true);
                            // Best
                            var bestUrl = baseUrl + $"#postcontent{best.AnswerId}";
                            msgUtil.SendBountyAcceptedNotice(best.Author, "system", bounty.Title ?? $"悬赏#{bounty.BountyId}", bestUrl);
                            // Helpful others
                            foreach (var ha in helpfulOthers)
                            {
                                var hUrl = baseUrl + $"#postcontent{ha.AnswerId}";
                                msgUtil.SendBountyHelpfulNotice(ha.Author, "system", bounty.Title ?? $"悬赏#{bounty.BountyId}", hUrl);
                            }
                            taskQueue?.QueueBackgroundWorkItem(Job.AddOrUpdateBountyById(bounty.BountyId));
                        }
                        catch (Exception ex) { _logger.LogWarning(ex, "Failed to send bounty auto-accept notices for {Id}", bounty.BountyId); }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to expire bounty {BountyId}", bounty.BountyId);
                    try { await userTx.RollbackAsync(); } catch { }
                    try { await blogTx.RollbackAsync(); } catch { }
                    result.Failed++;
                }
            }

            return result;
        }
    }
}
