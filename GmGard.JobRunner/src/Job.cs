using GmGard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.JobRunner
{
    [Serializable]
    public class Job
    {
        [JsonProperty]
        internal JobType JobType { get; set; }
        [JsonProperty]
        internal string JobPayload { get; set; }

        public static Job CreateJob(AddExpPtsArgs args) 
            => new Job { JobType = JobType.AddMessage, JobPayload = JsonConvert.SerializeObject(args) };

        public static Job CreateJob(ArchiveAuditArgs args)
            => new Job { JobType = JobType.ArchiveAudit, JobPayload = JsonConvert.SerializeObject(args) };

        public static Job CreateJob(AdminLog args)
            => new Job { JobType = JobType.AdminLog, JobPayload = JsonConvert.SerializeObject(args) };

        public static Job CreateJob(SendNoticeArgs sendNoticeArgs)
            => new Job { JobType = JobType.SendNotice, JobPayload = JsonConvert.SerializeObject(sendNoticeArgs) };

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    enum JobType
    {
        Unknown = 0,
        AdminLog,
        AddMessage,
        ArchiveAudit,
        SendNotice,
    }

    public class AddExpPtsArgs
    {
        public string UserName { get; set; }
        public int Exp { get; set; }
        public int Pts { get; set; }
    }

    public class ArchiveAuditArgs
    {
        public int BlogId { get; set; }
        public string Auditor { get; set; }
        public BlogAudit.Action Action { get; set; }
        public bool IsAmend { get; set; }
        public string Reason { get; set; }
    }

    public class SendNoticeArgs
    {
        public enum NoticeType
        {
            Default = 0,
            NewPost,
            NewReply,
            Unapprove,
            DeletePost,
            DeleteReply,
            DeleteBlog,
            ExpChange,
            Mention,
            RankReward
        }
        public string NoticeUser { get; set; }
        public NoticeType Type { get; set; }
        public string Actor { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }
}
