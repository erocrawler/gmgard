using GmGard.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using static GmGard.Services.SendNoticeArgs;

namespace GmGard.Services
{
    public class JobTaskRunner
    {
        private BlogContext _blogContext;
        private UsersContext _usersContext;
        private ILogger<JobTaskRunner> _logger;
        private IServiceProvider _serviceProvider;

        public JobTaskRunner(ILoggerFactory loggerFactory, BlogContext blogContext, UsersContext usersContext, IServiceProvider serviceProvider)
        {
            _blogContext = blogContext;
            _usersContext = usersContext;
            _logger = loggerFactory.CreateLogger<JobTaskRunner>();
            _serviceProvider = serviceProvider;
        }

        public Task RunJob(Job job)
        {
            switch (job.JobType)
            {
                case JobType.AdminLog:
                    {
                        return AdminLog(job.AdminLogArgs);
                    }
                case JobType.ArchiveAudit:
                    {
                        return ArchiveAudit(job.ArchiveAuditArgs);
                    }
                case JobType.SendNotice:
                    {
                        return SendNoticeMessage(job.SendNoticeArgs);
                    }
                case JobType.AddOrUpdateBlog:
                    {
                        return TryUpdateES(u => u.AddOrUpdateBlogAsync(job.AddOrUpdateBlogArgs));
                    }
                case JobType.UpdateBlogRate:
                    {
                        return TryUpdateES(u => u.UpdateBlogRateAsync(job.UpdateBlogRateArgs));
                    }
                case JobType.UpdateBlogTag:
                    {
                        return TryUpdateES(u => u.UpdateBlogTagAsync(job.UpdateBlogTagArgs));
                    }
                case JobType.UpdatePostCount:
                    {
                        return TryUpdateES(u => u.UpdatePostCountAsync(job.UpdatePostCountArgs));
                    }
                case JobType.RemoveBlog:
                    {
                        return TryUpdateES(u => u.RemoveBlogAsync(job.RemoveBlogArgs));
                    }
                default:
                    _logger.LogWarning("Unrecognized job: {0}", job.JobType);
                    break;
            }
            return Task.CompletedTask;
        }

        private Task TryUpdateES(Func<ElasticSearchUpdateService, Task> action)
        {
            var updater = _serviceProvider.GetService<ElasticSearchUpdateService>();
            if (updater != null)
            {
                return action(updater);
            }
            return Task.CompletedTask;
        }

        private async Task AddMessage(Message args)
        {
            args.MsgDate = DateTime.Now;
            _usersContext.Messages.Add(args);
            await _usersContext.SaveChangesAsync();
        }

        private async Task AdminLog(AdminLog log)
        {
            _usersContext.AdminLogs.Add(log);
            await _usersContext.SaveChangesAsync();
        }

        private async Task ArchiveAudit(ArchiveAuditArgs args)
        {
            try
            {
            var db = _blogContext;
            var udb = _usersContext;
                var blogDecisions = db.BlogAudits.Where(ba => ba.BlogID == args.BlogId && (ba.AuditAction == BlogAudit.Action.Approve || ba.AuditAction == BlogAudit.Action.Deny));
                List<BlogAudit> latestAudits;
                int version = 1;
                if (args.IsAmend)
                {
                    var latestDecision = blogDecisions.OrderByDescending(ba => ba.BlogVersion).FirstOrDefault();
                    if (latestDecision != null)
                    {
                        db.BlogAudits.Remove(latestDecision);
                        db.SaveChanges();
                        latestAudits = db.BlogAudits.Where(bb => bb.BlogID == args.BlogId && bb.BlogVersion == latestDecision.BlogVersion).ToList();
                    }
                    else
                    {
                        latestAudits = db.BlogAudits.Where(bb => bb.BlogID == args.BlogId && bb.BlogVersion == 1).ToList();
                    }
                }
                else
                {
                    version = blogDecisions.DefaultIfEmpty().Max(bm => bm == null ? 0 : bm.BlogVersion) + 1;
                    latestAudits = db.BlogAudits.Where(bb => bb.BlogID == args.BlogId && bb.BlogVersion >= version).ToList();
                }

                if (latestAudits.Count > 0)
                {
                    version = latestAudits[0].BlogVersion;
                }
                // It is possible the auditor also voted. In that case update the vote as decision, don't calculate the auditor's statistics.
                var blogAudit = latestAudits.SingleOrDefault(la => la.BlogID == args.BlogId && la.Auditor == args.Auditor && la.BlogVersion == version)
                    ?? db.BlogAudits.Add(new BlogAudit { Auditor = args.Auditor, BlogID = args.BlogId, BlogVersion = version });
                blogAudit.AuditAction = args.Action;
                blogAudit.AuditDate = DateTime.Now;
                blogAudit.Reason = args.Reason;
                var currentStat = latestAudits.Where(ba => ba.AuditAction == BlogAudit.Action.VoteApprove || ba.AuditAction == BlogAudit.Action.VoteDeny)
                    .ToDictionary(la => la.Auditor.ToLower(), la => (la.AuditAction == BlogAudit.Action.VoteApprove && args.Action == BlogAudit.Action.Approve)
                                                        || (la.AuditAction == BlogAudit.Action.VoteDeny && args.Action == BlogAudit.Action.Deny));
                var latestAuditors = currentStat.Keys;
                if (latestAuditors.Count > 0)
                {
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
                    var usersToUpdate = stats.Keys.Concat(currentStat.Keys);
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
                        update.Auditor.CorrectCount = correctcount + (currentStat[update.UserName.ToLower()] ? 1 : 0);
                    }
                }
                await Task.WhenAll(udb.SaveChangesAsync(), db.SaveChangesAsync());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error running ArchiveAudit");
            }
        }

        private async Task SendNoticeMessage(SendNoticeArgs args)
        {
            switch (args.Type)
            {
                case NoticeType.NewPost:
                case NoticeType.NewReply:
                    string notice;
                    if (args.Type == NoticeType.NewPost)
                    {
                        notice = string.Format("<a href='/User/{0}'>{0}</a> 回复了您的投稿：<a href='{2}'>{1}</a>", args.Actor, args.Content, args.Url);
                    }
                    else
                    {
                        notice = string.Format("<a href='/User/{0}'>{0}</a> 在<a href='{2}'>{1}</a>中回复了您的评论", args.Actor, args.Content, args.Url);
                    }
                    int num = 0;
                    var db = _usersContext;
                    var msg = db.Messages
                        .Where(m => m.MsgTitle.StartsWith("新回复通知") && m.Recipient == args.NoticeUser && m.Sender == "admin")
                        .OrderByDescending(m => m.MsgDate)
                        .FirstOrDefault();
                    if (msg != null && msg.IsRead == false)
                    {
                        var value = Regex.Match(msg.MsgTitle, @"\d+").Value;
                        if (string.IsNullOrEmpty(value) || !int.TryParse(value, out num))
                        {
                            num = 1;
                        }
                        msg.MsgTitle = string.Format("新回复通知 ({0})", num + 1);
                        msg.MsgDate = DateTime.Now;
                        msg.MsgContent = notice + '\n' + msg.MsgContent;
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        await AddMessage(new Message
                        {
                            Sender = "admin",
                            Recipient = args.NoticeUser,
                            MsgTitle = "新回复通知",
                            MsgContent = notice,
                            IsSenderDelete = true
                        });
                    }
                    return;
                case NoticeType.DeletePost:
                    await AddMessage(new Message
                    {
                        Sender = args.Actor,
                        Recipient = args.NoticeUser,
                        MsgTitle = "删除通知",
                        MsgContent = string.Format("您在投稿<a href='{1}'>{0}</a>中的评论已被管理员删除", args.Content, args.Url)
                    });
                    return;

                case NoticeType.DeleteReply:
                    await AddMessage(new Message
                    {
                        Sender = args.Actor,
                        Recipient = args.NoticeUser,
                        MsgTitle = "删除通知",
                        MsgContent = string.Format("您在投稿<a href='{1}'>{0}</a>中的回复已被管理员删除", args.Content, args.Url)
                    });
                    return;

                case NoticeType.DeleteBlog:
                    await AddMessage(new Message
                    {
                        Sender = args.Actor,
                        Recipient = args.NoticeUser,
                        MsgTitle = "删除通知",
                        MsgContent = string.Format("您的投稿 {1} 已被管理员删除<br>原因：<br>{0}", args.Content, args.Url)
                    });
                    return;

                case NoticeType.Unapprove:
                    await AddMessage(new Message
                    {
                        Sender = args.Actor,
                        Recipient = args.NoticeUser,
                        MsgTitle = "审核通知",
                        MsgContent = string.Format("您的投稿<a href='{1}'>{1}</a>已被管理员标为审核不通过<br>原因：<br>{0}", args.Content, args.Url)
                    });
                    return;

                case NoticeType.ExpChange:
                    await AddMessage(new Message
                    {
                        Sender = args.Actor,
                        Recipient = args.NoticeUser,
                        MsgTitle = "绅士度/棒棒糖调整通知",
                        MsgContent = string.Format("您的绅士度/棒棒糖获得了如下更改：{0}<br>如有疑问，请联系管理员。", args.Content)
                    });
                    return;

                case NoticeType.Mention:
                    await AddMessage(new Message
                    {
                        Sender = "admin",
                        Recipient = args.NoticeUser,
                        MsgTitle = "提及通知",
                        MsgContent = string.Format("<a href='/User/{0}'>{0}</a> 在 <a href='{2}'>{1}</a>中提到了您。", args.Actor, args.Content, args.Url),
                        IsSenderDelete = true
                    });
                    return;

                case NoticeType.RankReward:
                    await AddMessage(new Message
                    {
                        Sender = "admin",
                        Recipient = args.NoticeUser,
                        MsgTitle = "奖励通知",
                        MsgContent = args.Content,
                        IsSenderDelete = true
                    });
                    return;
                case NoticeType.UpdateEmail:
                    await AddMessage(new Message
                    {
                        Sender = "admin",
                        Recipient = args.NoticeUser,
                        MsgTitle = "邮箱更改通知",
                        MsgContent = string.Format("您的邮箱已被更改<br>{0}<br>如有疑问，请联系管理员。", args.Content),
                        IsSenderDelete = true
                    });
                    return;

                default:
                    return;
            }
        }
    }
}
