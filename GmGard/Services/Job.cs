using GmGard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    [Serializable]
    public class Job
    {
        internal JobType JobType { get; set; }
        internal ArchiveAuditArgs ArchiveAuditArgs { get; set; }
        internal AdminLog AdminLogArgs { get; set; }
        internal SendNoticeArgs SendNoticeArgs { get; set; }
        internal BlogEventArgs AddOrUpdateBlogArgs { get; set; }
        internal RateEventArgs UpdateBlogRateArgs { get; set; }
        internal TagEventArgs UpdateBlogTagArgs { get; set; }
        internal PostEventArgs UpdatePostCountArgs { get; set; }
        internal BlogEventArgs RemoveBlogArgs { get; set; }

        public static Job CreateJob(ArchiveAuditArgs args)
            => new Job { JobType = JobType.ArchiveAudit, ArchiveAuditArgs = args };

        public static Job CreateJob(AdminLog args)
            => new Job { JobType = JobType.AdminLog, AdminLogArgs = args };

        public static Job CreateJob(SendNoticeArgs sendNoticeArgs)
            => new Job { JobType = JobType.SendNotice, SendNoticeArgs = sendNoticeArgs };
        public static Job AddOrUpdateBlog(BlogEventArgs addOrUpdateBlogArgs)
            => new Job { JobType = JobType.AddOrUpdateBlog, AddOrUpdateBlogArgs = addOrUpdateBlogArgs };
        public static Job UpdateBlogRate(RateEventArgs updateBlogRateArgs)
            => new Job { JobType = JobType.UpdateBlogRate, UpdateBlogRateArgs = updateBlogRateArgs };
        public static Job UpdateBlogTag(TagEventArgs updateBlogTagArgs)
            => new Job { JobType = JobType.UpdateBlogTag, UpdateBlogTagArgs = updateBlogTagArgs };
        public static Job UpdatePostCount(PostEventArgs args)
            => new Job { JobType = JobType.UpdatePostCount, UpdatePostCountArgs = args };
        public static Job RemoveBlog(BlogEventArgs args)
            => new Job { JobType = JobType.RemoveBlog, RemoveBlogArgs = args };

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
        AddOrUpdateBlog,
        UpdateBlogRate,
        UpdateBlogTag,
        UpdatePostCount,
        RemoveBlog,
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
            RankReward,
            UpdateEmail,
        }
        public string NoticeUser { get; set; }
        public NoticeType Type { get; set; }
        public string Actor { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
    }
}
