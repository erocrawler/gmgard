using GmGard.Models;
using System;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using static GmGard.Services.SendNoticeArgs;

namespace GmGard.Services
{
    public class MessageUtil : UtilityService
    {
        BackgroundTaskQueue _taskQueue;

        public MessageUtil(BlogContext db, UsersContext udb, IMemoryCache cache, BackgroundTaskQueue taskQueue) : base(db, udb, cache)
        {
            _taskQueue = taskQueue;
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
            _cache.Remove("unreadmsg" + m.Recipient);
            _udb.SaveChanges();
        }

        public void SendExpChangeNotice(string noticeuser, string actor, string reason)
            => SendNoticeMsg(noticeuser, NoticeType.ExpChange, actor, reason);
        public void SendEmailUpdateNotice(string noticeuser, string actor, string reason)
            => SendNoticeMsg(noticeuser, NoticeType.UpdateEmail, actor, reason);

        public void SendNoticeMsg(string noticeuser, NoticeType type, string actor, string content = null, string url = null)
        {
            if (noticeuser == actor)
            {
                return;
            }
            _taskQueue.QueueBackgroundWorkItem(Job.CreateJob(new SendNoticeArgs { Actor = actor, Content = content, NoticeUser = noticeuser, Type = type, Url = url }));
            _cache.Remove("unreadmsg" + noticeuser);
        }

        public string GetUnreadMsg(string name, bool fromcache = true)
        {
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

        internal void SendBountyAcceptedNotice(string noticeUser, string actor, string bountyTitle, string url)
            => SendNoticeMsg(noticeUser, NoticeType.BountyAccepted, actor, bountyTitle, url);
        internal void SendBountyHelpfulNotice(string noticeUser, string actor, string bountyTitle, string url)
            => SendNoticeMsg(noticeUser, NoticeType.BountyAccepted, actor, bountyTitle, url); // same type, title distinguishes points
        internal void SendBountyExpiredNotice(string noticeUser, string bountyTitle, string url)
            => SendNoticeMsg(noticeUser, NoticeType.BountyExpired, "system", bountyTitle, url);
        internal void SendBountyAutoAcceptedNotice(string noticeUser, string actor, string bountyTitle, string url, bool isBest)
            => SendNoticeMsg(noticeUser, isBest ? NoticeType.BountyAutoAccepted : NoticeType.BountyAccepted, actor ?? "system", bountyTitle, url);
    }
}