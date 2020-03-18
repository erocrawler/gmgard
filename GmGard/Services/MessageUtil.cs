using GmGard.Models;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using static GmGard.JobRunner.SendNoticeArgs;

namespace GmGard.Services
{
    public class MessageUtil : UtilityService
    {
        BackgroundJobService _jobService;

        public MessageUtil(BlogContext db, UsersContext udb, IMemoryCache cache, BackgroundJobService jobService) : base(db, udb, cache)
        {
            _jobService = jobService;
        }

        public void AddMsg(string author, string recipient, string title, string content, bool senderdel = false)
        {
            Message m = new Message
            {
                Sender = author,
                Recipient = recipient,
                MsgTitle = title,
                MsgContent = content,
                MsgDate = DateTime.Now
            };
            if (senderdel)
            {
                m.IsSenderDelete = true;
            }
            _udb.Messages.Add(m);
            _cache.Remove("unreadmsg" + m.Recipient.ToLower());
            _udb.SaveChanges();
        }

        public void SendExpChangeNotice(string noticeuser, string actor, string reason)
            => SendNoticeMsg(noticeuser, NoticeType.ExpChange, actor, reason);

        public void SendNoticeMsg(string noticeuser, NoticeType type, string actor, string content = null, string url = null)
        {
            if (noticeuser == actor)
            {
                return;
            }
            _jobService.RunJob(JobRunner.Job.CreateJob(new JobRunner.SendNoticeArgs { Actor = actor, Content = content, NoticeUser = noticeuser, Type = type, Url = url }));
            _cache.Remove("unreadmsg" + noticeuser.ToLower());
        }

        public string GetUnreadMsg(string name, bool fromcache = true)
        {
            name = name.ToLower();
            int? count = _cache.Get<int?>("unreadmsg" + name);
            if (!count.HasValue || !fromcache)
            {
                count = _udb.Messages.Count(m => m.Recipient == name && !m.IsRead && !m.IsRecipientDelete);
                _cache.Set<int?>("unreadmsg" + name, count);
            }
            if (count > 0)
            {
                return count.ToString();
            }
            return string.Empty;
        }

        internal void SendNewPostNotice(string noticeUser, string actor, string blogTitle, string url)
            => SendNoticeMsg(noticeUser, NoticeType.NewPost, actor, blogTitle, url);

        internal void SendMentionNotice(string noticeUser, string sender, string title, string url)
            => SendNoticeMsg(noticeUser, NoticeType.Mention, sender, title, url);

        internal void SendNewReplyNotice(string noticeUser, string actor, string postTitle, string postLink)
            => SendNoticeMsg(noticeUser, NoticeType.NewReply, actor, postTitle, postLink);

        internal void SendRankNotice(string author, string content)
            => SendNoticeMsg(author, NoticeType.RankReward, "admin", content);

        internal void SendUnapproveNotice(string author, string actor, string msgContent, string url)
            => SendNoticeMsg(author, NoticeType.Unapprove, actor, msgContent, url);

        internal void SendDeleteBlogNotice(string author, string actor, string msgContent, string blogTitle)
            => SendNoticeMsg(author, NoticeType.DeleteBlog, actor, msgContent, blogTitle);
    }
}