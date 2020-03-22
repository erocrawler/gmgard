using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using GmGard.ViewComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class ReplyController : Controller
    {
        private BlogContext _db;
        private UsersContext _udb;
        private IMemoryCache _cache;
        private AdminUtil _adminUtil;
        private BlogUtil _blogUtil;
        private ExpUtil _expUtil;
        private MessageUtil _msgUtil;
        private RatingUtil _ratingUtil;
        private IWebHostEnvironment _env;
        private AppSettingsModel _appSettings;
        private ICompositeViewEngine _viewEngine;
        private IActionContextAccessor _actionAccessor;
        private readonly HtmlSanitizerService _sanitizerService;

        public ReplyController(
            IOptions<AppSettingsModel> appSettings,
            BlogContext db,
            UsersContext udb,
            AdminUtil adminUtil,
            BlogUtil blogUtil,
            ExpUtil expUtil,
            MessageUtil msgUtil,
            RatingUtil ratingUtil,
            IWebHostEnvironment env,
            IMemoryCache cache,
            ICompositeViewEngine viewEngine,
            IActionContextAccessor actionAccessor,
            HtmlSanitizerService sanitizerService)
        {
            _appSettings = appSettings.Value;
            _db = db;
            _udb = udb;
            _adminUtil = adminUtil;
            _blogUtil = blogUtil;
            _cache = cache;
            _env = env;
            _expUtil = expUtil;
            _ratingUtil = ratingUtil;
            _msgUtil = msgUtil;
            _viewEngine = viewEngine;
            _actionAccessor = actionAccessor;
            _sanitizerService = sanitizerService;
        }

        private int replypagesize => _appSettings.ReplyPageSize;

        public static event EventHandler<PostEventArgs> OnAddPost;

        public static event EventHandler<PostEventArgs> OnDeletePost;

        public static event EventHandler<ReplyEventArgs> OnAddReply;

        public static event EventHandler<RatePostEventArgs> OnRatePost;

        private void TriggerAddPost(Post p)
        {
            OnAddPost?.Invoke(this, new PostEventArgs(p));
        }

        private void TriggerDeletePost(Post p)
        {
            OnDeletePost?.Invoke(this, new PostEventArgs(p));
        }

        private void TriggerAddReply(Reply r)
        {
            OnAddReply?.Invoke(this, new ReplyEventArgs(r));
        }

        private void TriggerRatePost(PostRating r)
        {
            OnRatePost?.Invoke(this, new RatePostEventArgs(r));
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddReplyWithRate(int itemid, string addreplycontent, string Captcha, string Prefix, int? rating)
        {
            Post post;
            try
            {
                CheckPost(Captcha, Prefix, addreplycontent);
                UsersRating userrate = null;
                if (rating != null && RatingUtil.RatingValue.ContainsKey(rating.Value))
                {
                    userrate = _ratingUtil.GetUsersRating(itemid);
                    if (userrate.Rating != null && (userrate.HasPost || userrate.Rating.value != RatingUtil.RatingValue[rating.Value]))
                    {
                        throw new BlogException(userrate.RateWithAccount ? "您已经评过分了" : "今天已经评过分了");
                    }
                }
                post = _blogUtil.AddBlogPost(itemid, User.Identity.Name, _sanitizerService.Sanitize(addreplycontent));
                HttpContext.Session.SetDateTime("LastPostTime", DateTime.Now);
                if (userrate != null)
                {
                    if (userrate.Rating == null)
                    {
                        var Rate = _ratingUtil.AddBlogRating(itemid, rating.Value, userrate.credential, post.PostId);
                        if (Rate != null)
                        {
                            _ratingUtil.getRating(itemid, false);
                        }
                    }
                    else
                    {
                        userrate.Rating.PostId = post.PostId;
                        _db.SaveChanges();
                    }
                }
                TriggerAddPost(post);
            }
            catch (BlogException e)
            {
                return Json(new { err = e.Message });
            }
            string expmsg = HttpContext.Items["QuestMsg"] as string;
            return Json(new { id = post.PostId, expmsg });
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddReply(int itemid, string addreplycontent, string Captcha, string Prefix, ItemType idtype)
        {
            Post post;
            try
            {
                addreplycontent = _sanitizerService.Sanitize(addreplycontent);
                CheckPost(Captcha, Prefix, addreplycontent);
                switch (idtype)
                {
                    case ItemType.Topic:
                        post = _blogUtil.AddTopicPost(itemid, User.Identity.Name, addreplycontent);
                        break;
                    case ItemType.Blog:
                        post = _blogUtil.AddBlogPost(itemid, User.Identity.Name, addreplycontent);
                        break;
                    default:
                        throw new BlogException("无效参数");
                }
                TriggerAddPost(post);
                HttpContext.Session.SetDateTime("LastPostTime", DateTime.Now);
            }
            catch (BlogException e)
            {
                return Json(new { err = e.Message });
            }
            string expmsg = HttpContext.Items["QuestMsg"] as string;
            return Json(new { id = post.PostId, expmsg });
        }

        [HttpPost]
        [Authorize]
        public async Task<JsonResult> AddPostReply(int postid, string addreplycontent)
        {
            DateTime? lastpost = HttpContext.Session.GetDateTime("LastPostTime");
            if (lastpost.HasValue)
            {
                var diff = DateTime.Now - lastpost.Value;
                if (diff.TotalSeconds < 30 && _expUtil.getUserLvl(User.Identity.Name) < 10)
                {
                    return Json(new { errmsg = "回复CD为30秒，请等" + (30 - diff.Seconds) + "秒后再试" });
                }
            }
            if (string.IsNullOrWhiteSpace(BlogHelper.removeAllTags(addreplycontent)))
            {
                return Json(new { errmsg = "回复不能为空" });
            }
            var reply = _blogUtil.AddPostReply(postid, User.Identity.Name, _sanitizerService.Sanitize(addreplycontent));
            TriggerAddReply(reply);
            string expmsg = HttpContext.Items["QuestMsg"] as string;
            HttpContext.Session.SetDateTime("LastPostTime", DateTime.Now);
            return Json(new { id = reply.ReplyId, view = await RenderPartail("Components/ReplyView/ReplyPartial", reply), expmsg });
        }

        [HttpPost]
        public JsonResult DeleteReply(int replyid)
        {
            int id = replyid;
            var reply = _db.Replies.Find(id);
            if (reply == null)
                return Json(new { errmsg = "无效的id" });
            if (reply.Author != User.Identity.Name && !User.IsInRole("Administrator") && !User.IsInRole("Moderator"))
            {
                return Json(new { errmsg = "你tmd没权限删除" });
            }
            if (User.Identity.Name != reply.Author)
            {
                _adminUtil.log(User.Identity.Name, "deletereply", _blogUtil.GetPostLink(reply.post));
            }
            _db.Replies.Remove(reply);
            _db.SaveChanges();
            return Json(true);
        }

        public ActionResult ShowReply(int itemid, int pagenum = 1, ItemType idtype = ItemType.Blog, bool hottest = true)
        {
            int id = itemid;
            return ViewComponent(nameof(ReplyView), new { itemid = id, pagenum = pagenum, idtype = idtype, hottest = hottest });
        }

        public ActionResult ShowUserReply(int itemid, string name, int pagenum = 1, ItemType idtype = ItemType.Blog, bool hottest = false)
        {
            int id = itemid;
            return ViewComponent(nameof(ReplyView), new { itemid = id, pagenum = pagenum, idtype = idtype, name = name, hottest = hottest });
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditReply(int PostId, string content)
        {
            int id = PostId;
            var post = _db.Posts.Find(id);
            if (post == null)
                return NotFound();
            if (post.Author != User.Identity.Name && !User.IsInRole("Administrator") && !User.IsInRole("Moderator"))
            {
                return Json(new { errmsg = "你tmd没权限更改" });
            }
            else if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { errmsg = "回复不能为空" });
            }
            if (User.Identity.Name != post.Author)
            {
                _adminUtil.log(User.Identity.Name, "editreply", _blogUtil.GetPostLink(post));
            }
            var mention = new MentionHandler(_udb);
            post.Content = mention.ParseMentions(_sanitizerService.Sanitize(content));
            if (mention.HasMentions())
            {
                mention.SendMentionMsg(_msgUtil, User.Identity.Name, _blogUtil.GetPostTitle(post), _blogUtil.GetPostLink(post));
            }
            _db.SaveChanges();
            return Json(true);
        }

        [HttpPost]
        public ActionResult DeletePost(int PostId)
        {
            int id = PostId;
            var post = _db.Posts.Find(id);
            if (post == null)
                return NotFound();
            if (post.Author != User.Identity.Name && !User.IsInRole("Administrator") && !User.IsInRole("Moderator"))
            {
                return Json(new { errmsg = "你tmd没权限删除" });
            }
            if (User.Identity.Name != post.Author)
            {
                _adminUtil.log(User.Identity.Name, "deletepost", _blogUtil.GetPostLink(post));
            }
            TriggerDeletePost(post);
            _db.Posts.Remove(post);
            _db.SaveChanges();
            return Json(true);
        }

        public ActionResult FindReply(int itemid, ItemType itemtype, int id)
        {
            int replyid = id;
            var reply = _db.Replies.Find(replyid);
            if (reply != null)
            {
                return FindPost(itemid, itemtype, reply.PostId);
            }
            return new EmptyResult();
        }

        public ActionResult FindPost(int itemid, ItemType itemtype, int id)
        {
            int postid = id;
            int iid = itemid;
            var post = _db.Posts.FirstOrDefault(p => p.PostId == postid && itemtype == p.IdType && iid == p.ItemId);
            if (post != null)
            {
                var tmp = _db.Posts.Where(p => p.PostDate > post.PostDate && p.ItemId == post.ItemId && p.IdType == post.IdType);
                int postindex = tmp.Count();
                int pagenum = postindex / replypagesize + 1;
                return ShowReply(itemid, pagenum, itemtype, false);
            }
            return new EmptyResult();
        }

        [Authorize]
        public async Task<ActionResult> RatePost(int postid, int value)
        {
            if (Math.Abs(value) != 1)
            {
                return BadRequest();
            }
            var p = await _db.Posts.Include("Ratings").SingleOrDefaultAsync(pp => pp.PostId == postid);
            if (p == null)
            {
                return NotFound();
            }
            var rating = p.Ratings.SingleOrDefault(r => r.Rater.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (rating == null)
            {
                rating = new PostRating { PostId = postid, Rater = User.Identity.Name, RatingID = Guid.NewGuid() };
                p.Ratings.Add(rating);
            }
            if (rating.Value != value)
            {
                rating.Value = value;
                await _db.SaveChangesAsync();
                TriggerRatePost(rating);
            }
            return Json(new { value = p.Ratings.Sum(r => r.Value) });
        }

        protected async Task<string> RenderPartail(string viewName, object model)
        {
            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = _viewEngine.FindView(_actionAccessor.ActionContext, viewName, false);
                ViewContext viewContext = new ViewContext(_actionAccessor.ActionContext, viewResult.View, ViewData, TempData, sw, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext);

                return sw.ToString();
            }
        }

        private void CheckPost(string Captcha, string Prefix, string Content)
        {
            if (_blogUtil.CheckCaptchaError(Captcha, Prefix))
            {
                throw new BlogException("验证码计算错误，请重试。");
            }
            DateTime? lastpost = HttpContext.Session.GetDateTime("LastPostTime");
            if (lastpost.HasValue)
            {
                var diff = DateTime.Now - lastpost.Value;
                if (diff.TotalSeconds < 30 && _expUtil.getUserLvl(User.Identity.Name) < 10)
                {
                    throw new BlogException("回复CD为30秒，请等" + (30 - diff.Seconds) + "秒后再试");
                }
            }
            if (string.IsNullOrWhiteSpace(BlogHelper.removeAllTags(Content)))
            {
                throw new BlogException("回复不能为空或纯表情");
            }
        }
    }
}