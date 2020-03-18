using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class AdminUtil
    {
        private BackgroundJobService _jobService;

        public AdminUtil(BackgroundJobService jobService)
        {
            _jobService = jobService;
        }

        public void log(string actor, string action, string target, string reason = null)
        {
            if (reason != null)
            {
                var pos = reason.IndexOf("原因：");
                if (pos >= 0)
                {
                    reason = reason.Substring(pos);
                }
                if (reason.Length > 100)
                {
                    reason = reason.Substring(0, 100);
                }
            }
            if (target.Length > 100)
            {
                target = target.Substring(0, 100);
            }
            var log = new AdminLog { Action = action, Actor = actor, Target = target, Reason = reason, LogTime = DateTime.Now };
            _jobService.RunJob(JobRunner.Job.CreateJob(log));
        }

        public void ArchiveAudit(Blog b, string auditor, BlogAudit.Action action, bool amend, string reason = "")
        {
            _jobService.RunJob(JobRunner.Job.CreateJob(new JobRunner.ArchiveAuditArgs { Action = action, Auditor = auditor, BlogId = b.BlogID, IsAmend = amend, Reason = reason }));
        }
    }
}