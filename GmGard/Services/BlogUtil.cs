using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GmGard.Models;
using Microsoft.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Html;

namespace GmGard.Services
{
    public class BlogUtil : ContextlessBlogUtil
    {
        private MessageUtil _msgUtil;
        private AdminUtil _adminUtil;
        private IHttpContextAccessor _contextAccessor;
        private IUrlHelper _urlHelper;
        private UploadUtil _uploadUtil;
        private ExpUtil _expUtil;
        private IVisitCounter _visitCounter;

        private HttpContext HttpContext => _contextAccessor.HttpContext;

        public BlogUtil(
            BlogContext db, 
            UsersContext udb, 
            IMemoryCache cache, 
            IHttpContextAccessor contextAccessor, 
            IUrlHelper urlHelper, 
            MessageUtil msgUtil, 
            AdminUtil adminUtil, 
            UploadUtil uploadUtil, 
            ExpUtil expUtil,
            IVisitCounter visitCounter,
            INickNameProvider nicknameProvider) : base(db, udb, cache, visitCounter, nicknameProvider)
        {
            _msgUtil = msgUtil;
            _adminUtil = adminUtil;
            _contextAccessor = contextAccessor;
            _urlHelper = urlHelper;
            _uploadUtil = uploadUtil;
            _expUtil = expUtil;
            _visitCounter = visitCounter;
        }

        public Blog AddBlog(string title, string content, int blogCategory, string ImagePath, string author, bool approve, bool isLocalimg, IEnumerable<BlogLink> links)
        {
            var newBlog = new Blog
            {
                BlogTitle = title,
                Content = content,
                BlogDate = DateTime.Now,
                ImagePath = ImagePath,
                CategoryID = blogCategory,
                Author = author,
                isApproved = approve ? true : (bool?)null,
                IsLocalImg = isLocalimg,
                Links = Newtonsoft.Json.JsonConvert.SerializeObject(links),
                Rating = 0,
                isHarmony = false
            };

            var mention = new MentionHandler(_udb);
            newBlog.Content = mention.ParseMentions(newBlog.Content);
            _db.Blogs.Add(newBlog);
            _db.SaveChanges();
            mention.SendMentionMsg(_msgUtil, author, title, _urlHelper.Action("Details", "Blog", new { id = newBlog.BlogID }));

            return newBlog;
        }

        public Reply AddPostReply(int id, string author, string content)
        {
            var r = new Reply
            {
                Author = author,
                Content = content,
                PostId = id,
                ReplyDate = DateTime.Now
            };
            Post p = _db.Posts.Find(id);
            if (p != null)
            {
                bool isTopic = p.IdType == ItemType.Topic;
                string noticeuser = p.Author;
                bool isUserName = true;
                Regex re = new Regex(@"回复(?:&nbsp;|\s*)<span>(.+)</span>");
                var match = re.Match(content);
                if (match.Length > 1)
                {
                    noticeuser = System.Net.WebUtility.HtmlDecode(match.Groups[1].Value.Trim());
                    // Check format of "[Title] NickName"
                    var idx = noticeuser.IndexOf("] ");
                    if (idx >= 0)
                    {
                        noticeuser = noticeuser.Substring(idx + 2);
                    }
                    isUserName = false;
                }
                var user = _udb.Users.Include("option").AsNoTracking().SingleOrDefault(u => (isUserName ? u.UserName : u.NickName) == noticeuser);
                if (user != null)
                {
                    UserOption o = user.option;
                    if (o == null || o.sendNoticeForNewPostReply == true)
                    {
                        _msgUtil.SendNewReplyNotice(user.UserName, author, GetPostTitle(p), GetPostLink(p));
                    }
                }
                var mention = new MentionHandler(_udb);
                r.Content = mention.ParseMentions(r.Content);
                if (mention.HasMentions())
                {
                    mention.SendMentionMsg(_msgUtil, r.Author, GetPostTitle(p), GetPostLink(p));
                }
            }
            p.Replies.Add(r);
            _db.SaveChanges();
            return r;
        }

        public Post AddBlogPost(int blogid, string author, string content)
        {
            return AddPost(blogid, ItemType.Blog, author, content);
        }

        public Post AddTopicPost(int topicid, string author, string content)
        {
            return AddPost(topicid, ItemType.Topic, author, content);
        }

        private Post AddPost(int itemid, ItemType idtype, string author, string content)
        {
            var p = new Post
            {
                Author = author,
                Content = content,
                PostDate = DateTime.Now,
                IdType = idtype,
                ItemId = itemid
            };

            var mention = new MentionHandler(_udb);
            string ItemAuthor = null, ItemTitle = null;
            if (itemid > 0)
            {
                p.Content = mention.ParseMentions(p.Content);
            }
            _db.Posts.Add(p);
            _db.SaveChanges();

            switch (idtype)
            {
                case ItemType.Blog:
                    var b = _db.Blogs.Find(itemid);
                    ItemAuthor = b.Author;
                    ItemTitle = b.BlogTitle;
                    break;
                case ItemType.Topic:
                    var t = _db.Topics.Find(itemid);
                    ItemAuthor = t.Author;
                    ItemTitle = t.TopicTitle;
                    break;
                case ItemType.Bounty:
                    break;
                case ItemType.Answer:
                    break;
            }
            string postlink = GetPostLink(p);
            if (ItemAuthor != null && GetUserOption(ItemAuthor, o => o.sendNoticeForNewReply))
            {
                _msgUtil.SendNewPostNotice(ItemAuthor, author, ItemTitle, postlink);
            }
            mention.SendMentionMsg(_msgUtil, author, ItemTitle, postlink);
            return p;
        }

        public string GetPostLink(Post p)
        {
            return GetPostUrl(p.IdType, p.ItemId, "#listpost" + p.PostId);
        }

        public string GetReplyLink(UserReply reply)
        {
            return GetPostUrl(reply.IdType, reply.ItemID, (reply.IsPost ? "#listpost" : "#reply") + reply.ReplyID);
        }

        public async Task DeleteBlogAsync(int id, string reason = null)
        {
            Blog b = await _db.Blogs.FindAsync(id);
            if (b == null)
                return;
            _db.TagsInBlogs.RemoveRange(_db.TagsInBlogs.Where(tib => tib.BlogID == b.BlogID));
            var posts = await _db.Posts.Where(p => p.IdType == ItemType.Blog && p.ItemId == b.BlogID).ToListAsync();
            if (posts.Count != 0)
            {
                _db.Posts.RemoveRange(posts);
            }
            var auditTypes = await _db.BlogAudits.Where(ba => ba.BlogID == id).Select(ba => ba.AuditAction).Distinct().ToListAsync();
            if (auditTypes.Any(ba => ba == BlogAudit.Action.Approve || ba == BlogAudit.Action.Deny) && auditTypes.Any(ba => ba == BlogAudit.Action.VoteApprove || ba == BlogAudit.Action.VoteDeny))
            {
                    // If vote accuracy was affected by this blog, move all audits to BlogID = 0 to preserve them.
                    await _db.Database.ExecuteSqlCommandAsync(@"WITH maxv AS (SELECT max(BlogVersion) AS v FROM BlogAudits WHERE BlogID = 0)
                        UPDATE BlogAudits SET BlogVersion = BlogVersion + ISNULL(maxv.v, 0), BlogID = 0 FROM BlogAudits, maxv WHERE BlogID = @id", new SqlParameter("@id", id));
            }
            try
            {
                if (b.IsLocalImg)
                {
                    IEnumerable<string> name = b.ImagePath.Split(';');
                    await _uploadUtil.DeleteFilesAsync(name.Concat(new[] { name.First().Replace("/upload/", "/thumbs/") }));
                }
            }
            finally
            {
                _db.Blogs.Remove(b);
                await _db.SaveChangesAsync();
            }
            _adminUtil.log(HttpContext.User.Identity.Name, "deleteblog", b.BlogID + ": " + b.BlogTitle, reason);
        }

        public bool CheckAdmin(bool includeWriter = false, bool includeAdManager = false)
        {
            return HttpContext.User.Identity.IsAuthenticated
                && ((HttpContext.User.IsInRole("Administrator")
                || HttpContext.User.IsInRole("Moderator"))
                || (includeWriter && HttpContext.User.IsInRole("Writers"))
                || (includeAdManager && HttpContext.User.IsInRole("AdManager")));
        }

        public int getCaptchaId()
        {
            int? capthcaid = HttpContext.Session.GetInt32("captchaid");
            int prefix = 0;
            if (capthcaid.HasValue)
            {
                prefix = capthcaid.Value;
                HttpContext.Session.SetInt32("captchaid", prefix == 5 ? 1 : (capthcaid.Value + 1));
            }
            return prefix;
        }

        public bool CheckCaptchaError(string CaptchaAttempt, string Prefix)
        {
            byte[] data;
            if (HttpContext.Session.TryGetValue("skipcaptcha", out data))
            {
                return false;
            }
            int? captchaAnswer = HttpContext.Session.GetInt32("Captcha" + Prefix);
            if (CheckAdmin(true) || (HttpContext.User.Identity.IsAuthenticated && _expUtil.getUserLvl(HttpContext.User.Identity.Name) >= 4))
            {
                return false;
            }
            else if (captchaAnswer == null || CaptchaAttempt != captchaAnswer.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetPostUrl(ItemType IdType, int ItemId, string HashTag = "")
        {
            switch (IdType)
            {
                case ItemType.Blog:
                    switch (ItemId)
                    {
                        case 0:
                            return _urlHelper.Action("Suggestions", "Home") + HashTag;

                        case -1:
                            return _urlHelper.Action("Suggestions", "Home", new { pos = "Report" }) + HashTag;

                        case -2:
                            return _urlHelper.Action("History", "Ranking") + HashTag;

                        default:
                            return _urlHelper.Action("Details", "Blog", new { id = ItemId, area = "" }) + HashTag;
                    }

                case ItemType.Topic:
                    return _urlHelper.Action("Details", "Topic", new { id = ItemId }) + HashTag;

                case ItemType.Bounty:
                    return _urlHelper.Action("Details", "Bounty", new { id = ItemId }) + HashTag;

                case ItemType.Answer:
                    // TODO: find bounty id here
                    return _urlHelper.Action("Details", "Bounty", new { id = ItemId }) + HashTag;
                default:
                    break;
            }
            throw new InvalidEnumArgumentException("IdType", (int)IdType, IdType.GetType());
        }

        public bool HasModified(DateTime modificationDate)
        {
            string headerValue = HttpContext.Request.Headers[HeaderNames.IfModifiedSince];
            if (headerValue == null)
                return true;

            var modifiedSince = DateTime.Parse(headerValue).ToLocalTime();
            return modificationDate - modifiedSince >= TimeSpan.FromSeconds(2);
        }

        public BlogDetailDisplay GetDetailDisplay(int id)
        {
            if (id <= 0)
            {
                return null;
            }
            BlogDetailDisplay bd;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var query = _db.Blogs.AsNoTracking().Where(b => b.BlogID == id)
                    .GroupJoin(_db.TagsInBlogs.DefaultIfEmpty(), b => b.BlogID, tib => tib.BlogID, (b, tib) => new BlogDetailDisplay
                    {
                        blog = b,
                        tag = tib.Select(t => t.tag),
                        Option = b.option,
                        IsFavorite = _db.Favorites.Count(f => f.Username == HttpContext.User.Identity.Name && f.BlogID == id) > 0,
                        Category = b.Category
                    });
                bd = query.SingleOrDefault();
            }
            else
            {
                var query = _db.Blogs.AsNoTracking().Where(b => b.BlogID == id)
                    .GroupJoin(_db.TagsInBlogs.DefaultIfEmpty(), b => b.BlogID, tib => tib.BlogID, (b, tib) => new BlogDetailDisplay
                    {
                        blog = b,
                        tag = tib.Select(t => t.tag),
                        Option = b.option,
                        Category = b.Category
                    });
                bd = query.SingleOrDefault();
            }
            if (bd == null)
            {
                return null;
            }
            ProcessBlogDetails(bd);
            return bd;
        }

        public void ProcessBlogDetails(BlogDetailDisplay blogDetailDisplay)
        {
            blogDetailDisplay.Option = blogDetailDisplay.Option ?? new BlogOption();
            blogDetailDisplay.IsAuthor = string.Equals(HttpContext.User.Identity.Name, blogDetailDisplay.blog.Author, StringComparison.OrdinalIgnoreCase);
            if (blogDetailDisplay.Option.NoComment)
            {
                blogDetailDisplay.Option.NoComment = !(blogDetailDisplay.IsAuthor || CheckAdmin());
            }
            blogDetailDisplay.AuthorDesc = string.IsNullOrEmpty(blogDetailDisplay.Option.LockDesc) 
                ? GetUserDesc(blogDetailDisplay.blog.Author) 
                : blogDetailDisplay.Option.LockDesc;
            var blog = blogDetailDisplay.blog;
            blog.BlogVisit = _visitCounter.GetBlogVisit(blog.BlogID, true);
            blogDetailDisplay.Option.NoRate = (blog.isApproved != true && !blogDetailDisplay.Option.NoApprove) || blogDetailDisplay.Option.NoRate || blogDetailDisplay.Category.DisableRating;
        }
    }
}