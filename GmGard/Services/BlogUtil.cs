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
    public class BlogUtil : UtilityService
    {
        private MessageUtil _msgUtil;
        private AdminUtil _adminUtil;
        private IHttpContextAccessor _contextAccessor;
        private IUrlHelper _urlHelper;
        private UploadUtil _uploadUtil;
        private ExpUtil _expUtil;
        private ImageUtil _imgUtil;
        private RatingUtil _rateUtil;
        private INickNameProvider _nicknameProvider;
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
            ImageUtil imgUtil, 
            RatingUtil rateUtil,
            IVisitCounter visitCounter,
            INickNameProvider nicknameProvider) : base(db, udb, cache)
        {
            _msgUtil = msgUtil;
            _adminUtil = adminUtil;
            _contextAccessor = contextAccessor;
            _urlHelper = urlHelper;
            _uploadUtil = uploadUtil;
            _expUtil = expUtil;
            _imgUtil = imgUtil;
            _rateUtil = rateUtil;
            _nicknameProvider = nicknameProvider;
            _visitCounter = visitCounter;
        }

        public Blog AddBlog(string title, string content, int blogCategory, string ImagePath, string author, bool approve, bool isLocalimg, IEnumerable<BlogLink> links)
        {
            var newBlog = new Blog();
            newBlog.BlogTitle = title;
            newBlog.Content = content;
            newBlog.BlogDate = DateTime.Now;
            newBlog.ImagePath = ImagePath;
            newBlog.CategoryID = blogCategory;
            newBlog.Author = author;
            newBlog.isApproved = approve ? true : (bool?)null;
            newBlog.IsLocalImg = isLocalimg;
            newBlog.Links = Newtonsoft.Json.JsonConvert.SerializeObject(links);
            newBlog.Rating = 0;
            newBlog.isHarmony = false;

            var mention = new MentionHandler(_udb);
            newBlog.Content = mention.ParseMentions(newBlog.Content);
            _db.Blogs.Add(newBlog);
            _db.SaveChanges();
            mention.SendMentionMsg(_msgUtil, author, title, "/gm" + newBlog.BlogID);

            return newBlog;
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

        public string GetPostLink(Post p)
        {
            return GetPostUrl(p.IdType, p.ItemId, "#listpost" + p.PostId);
        }

        public string GetPostTitle(Post p)
        {
            switch (p.IdType)
            {
                case ItemType.Blog:
                    switch (p.ItemId)
                    {
                        case 0:
                            return "意见建议";

                        case -1:
                            return "问题汇报";

                        case -2:
                            return "历史排行";

                        default:
                            return _db.Blogs.Single(b => b.BlogID == p.ItemId).BlogTitle;
                    }
                case ItemType.Topic:
                    return _db.Topics.Single(t => t.TopicID == p.ItemId).TopicTitle;
                    //case ItemType.Bounty:
                    //    return db.Bounty.Find(ItemId).BountyTitle;
                    //case ItemType.Answer:
                    //    // TODO: find bounty id here
                    //    return db.Bounty.Find(ItemId).BountyTitle;
            }
            throw new InvalidEnumArgumentException("IdType", (int)p.IdType, p.IdType.GetType());
        }

        public string GetReplyLink(UserReply reply)
        {
            return GetPostUrl(reply.IdType, reply.ItemID, (reply.IsPost ? "#listpost" : "#reply") + reply.ReplyID);
        }

        public T GetUserOption<T>(string user, Func<UserOption, T> option)
        {
            user = user.ToLower();
            UserOption useroption = _cache.Get<UserOption>("useroption" + user);
            if (useroption == null)
            {
                useroption = _udb.UserOptions.AsNoTracking().SingleOrDefault(u => u.user.UserName == user) ?? new UserOption();
                _cache.Set("useroption" + user, useroption);
            }
            return option(useroption);
        }

        public void CacheUserOption(UserOption currentoption, string user)
        {
            user = user.ToLower();
            _cache.Set("useroption" + user, currentoption);
        }

        public string GetUserDesc(string username)
        {
            username = username.ToLower();
            string desc = _cache.Get<string>("desc" + username);
            if (desc == null)
            {
                var p = _udb.Users.FirstOrDefault(u => u.UserName == username);
                desc = p.UserComment ?? string.Empty;
                _cache.Set("desc" + username, desc);
            }
            return desc;
        }

        public int GetFavCount(string username)
        {
            username = username.ToLower();
            var count = _cache.Get<int?>("favcount" + username);
            if (count == null)
            {
                count = _db.Favorites.Count(f => f.Username == username);
                _cache.Set("favcount" + username, count);
            }
            return count.Value;
        }

        public string GetNickName(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return string.Empty;
            }
            return _nicknameProvider.GetNickName(user);
        }

        public IDictionary<string, string> GetNickNames(IEnumerable<string> users)
        {
            return _nicknameProvider.GetNickNames(users);
        }

        public int GetPostCount(Blog blog)
        {
            return GetBlogPostCount(blog.BlogID);
        }

        public int GetBlogPostCount(int blogid)
        {
            var postCountCache = _cache.Get<ConcurrentDictionary<int, int>>(CacheService.PostCountCacheKey) ?? new ConcurrentDictionary<int, int>();
            int count = postCountCache.GetOrAdd(blogid, id => _db.Posts.Count(p => p.ItemId == id && p.IdType == ItemType.Blog));
            _cache.Set(CacheService.PostCountCacheKey, postCountCache, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
            return count;
        }

        public void PreparePostCount(IEnumerable<int> blogs)
        {
            var ids = new HashSet<int>(blogs);
            var result = new Dictionary<int, int>(ids.Count());
            var postCountCache = _cache.Get<ConcurrentDictionary<int, int>>(CacheService.PostCountCacheKey) ?? new ConcurrentDictionary<int, int>();
            var uncached = ids.Where(i => !postCountCache.ContainsKey(i));
            if (uncached.Count() > 0)
            {
                var postCounts = _db.Posts.Where(u => uncached.Contains(u.ItemId) && u.IdType == ItemType.Blog)
                    .GroupBy(p => p.ItemId).ToDictionary(u => u.Key, u => u.Count());
                foreach (var id in uncached)
                {
                    int count = 0;
                    postCounts.TryGetValue(id, out count);
                    postCountCache.TryAdd(id, count);
                }
            }
            _cache.Set(CacheService.PostCountCacheKey, postCountCache, new MemoryCacheEntryOptions { Priority = CacheItemPriority.High });
        }

        public void PrepareListCache(IEnumerable<Blog> list)
        {
            if (list == null || list.Count() == 0)
            {
                return;
            }
            var blogIds = list.Select(b => b.BlogID);
            var visitCounter = _contextAccessor.HttpContext.RequestServices.GetService(typeof(IVisitCounter)) as IVisitCounter;
            visitCounter.PrepareBlogVisits(blogIds);
            PreparePostCount(blogIds);
            _rateUtil.PrepareRatings(blogIds);
            GetNickNames(list.Select(b => b.Author));
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

        public bool CheckAdmin(bool includeWriter = false)
        {
            return HttpContext.User.Identity.IsAuthenticated
                && ((HttpContext.User.IsInRole("Administrator")
                || HttpContext.User.IsInRole("Moderator"))
                || (includeWriter && HttpContext.User.IsInRole("Writers")));
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

        public int GetUnapproveCount()
        {
            int? c = _cache.Get<int?>("UnapproveCount");
            if (c == null)
            {
                c = _db.Blogs.Count(b => b.isApproved == null && b.BlogID > 0);
                _cache.Set("UnapproveCount", c, TimeSpan.FromMinutes(10));
            }
            return c.Value;
        }

        public string GetNewSuggestionCount()
        {
            int c1, c2, c3;
            string c = _cache.Get<string>("NewSuggestionCount");
            if (c == null)
            {
                // 0 = Suggestions, -1 = Report
                var specialBlogs = _db.Posts.Where(p => (p.ItemId == 0 || p.ItemId == -1) && p.IdType == ItemType.Blog && DbFunctions.DiffDays(p.PostDate, DateTime.Now) < 1);
                c1 = specialBlogs.Count(p => p.ItemId == 0);
                c2 = specialBlogs.Count(p => p.ItemId == -1);
                c3 = _udb.Messages.Count(p => p.Recipient == "admin" && DbFunctions.DiffDays(p.MsgDate, DateTime.Now) < 1);
                c = string.Format("{0}/{1}/{2}", c1, c2, c3);
                _cache.Set<string>("NewSuggestionCount", c, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) });
            }
            return c;
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
                        IsFavorite = _db.Favorites.Count(f => f.Username == HttpContext.User.Identity.Name && f.BlogID == id) > 0
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
                        Option = b.option
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
            blogDetailDisplay.Option.NoRate = (blog.isApproved != true && !blogDetailDisplay.Option.NoApprove) || blogDetailDisplay.Option.NoRate;
        }

        public HtmlString GetFirstLine(Blog b, bool RemoveTags = false)
        {
            var cached = _cache.GetOrCreate(CacheService.GetBlogFirstLineKey(b.BlogID), e => BlogHelper.getFirstLine(b.Content, 100));
            if (RemoveTags)
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(cached.Value);
                return new HtmlString(doc.DocumentNode.InnerText);
            }
            return cached;
        }
    }
}