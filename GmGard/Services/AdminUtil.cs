using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class AdminUtil
    {
        private BackgroundTaskQueue _taskQueue;

        public AdminUtil(BackgroundTaskQueue taskQueue)
        {
            _taskQueue = taskQueue;
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
            _taskQueue.QueueBackgroundWorkItem(Job.CreateJob(log));
        }

        public void ArchiveAudit(Blog b, string auditor, BlogAudit.Action action, bool amend, string reason = "")
        {
            _taskQueue.QueueBackgroundWorkItem(Job.CreateJob(new ArchiveAuditArgs { Action = action, Auditor = auditor, BlogId = b.BlogID, IsAmend = amend, Reason = reason }));
        }
    }
}