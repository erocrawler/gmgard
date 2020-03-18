using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [Authorize, ResponseCache(CacheProfileName = "Never")]
    public class AuditController : Controller
    {
        private BlogContext _db;
        private UsersContext _udb;
        private AdminUtil _adminUtil;
        private MessageUtil _msgUtil;
        private AppSettingsModel _appSettings;
        private DataSettingsModel _dataSettings;
        private UserManager<UserProfile> _userManager;
        private IMemoryCache _cache;
        private BlogUtil _blogUtil;

        public AuditController(
            IOptions<AppSettingsModel> appSettings,
            IOptionsSnapshot<DataSettingsModel> dataSettings,
            BlogContext db,
            UsersContext udb,
            AdminUtil adminUtil,
            BlogUtil blogUtil,
            MessageUtil msgUtil,
            UserManager<UserProfile> userManager,
            IMemoryCache cache)
        {
            _db = db;
            _udb = udb;
            _adminUtil = adminUtil;
            _blogUtil = blogUtil;
            _msgUtil = msgUtil;
            _appSettings = appSettings.Value;
            _dataSettings = dataSettings.Value;
            _userManager = userManager;
            _cache = cache;
        }

        private int PageSize => _appSettings.AuditPageSize;
        private int HistoryPageSize => _appSettings.AuditPageSize * 2;

        public static event EventHandler<BlogEventArgs> OnApproveBlog;
        public static event EventHandler<BlogEventArgs> OnDenyBlog;

        private void TriggerApproveBlog(Blog b)
        {
            OnApproveBlog?.Invoke(this, new BlogEventArgs(b));
        }

        private void TriggerDenyBlog(Blog b)
        {
            OnDenyBlog?.Invoke(this, new BlogEventArgs(b));
        }

        private void ApproveBlog(Blog b, string auditor, bool updatetime)
        {
            // if the blog is already disapproved, approving it consider an amend.
            bool amend = b.isApproved == false;
            if (b.isApproved != true)
            {
                b.isApproved = true;
                TriggerApproveBlog(b);
                if (updatetime)
                {
                    b.BlogDate = DateTime.Now;
                }
                _adminUtil.ArchiveAudit(b, auditor, BlogAudit.Action.Approve, amend);
                _adminUtil.log(auditor, "approve", b.BlogID.ToString());
            }
        }

        private void DenyBlog(Blog b, string auditor, bool sendmsg, string reason = "")
        {
            bool amend = b.isApproved == true;
            if (b.isApproved != false)
            {
                b.isApproved = false;
                TriggerDenyBlog(b);
                var message = new StringBuilder(reason);
                if (message.Length == 0)
                {
                    message.AppendFormat("您的投稿 {0} 未通过审核。", WebUtility.HtmlEncode(b.BlogTitle));
                }
                message.AppendFormat(" <br>投稿地址：<br><a href='{0}'>{0}</a>", Url.Action("Details", "Blog", new { id = b.BlogID }));
                _adminUtil.ArchiveAudit(b, auditor, BlogAudit.Action.Deny, amend, reason);
                _adminUtil.log(auditor, "unapprove", b.BlogID.ToString(), reason);
                if (sendmsg)
                    _msgUtil.AddMsg(auditor, b.Author, "审核通知", message.ToString(), auditor == "admin");
            }
        }

        private bool HasPassedExam() => _udb.AuditExamSubmissions.Any(a => a.User.UserName == User.Identity.Name && a.IsSubmitted && a.HasPassed);

        private bool CanJoinAuditor(int UserLevel) {
            if (!HasPassedExam())
            {
                return false;
            }
            var setting = _dataSettings;
            return (setting.JoinAuditorLevel > 0 && UserLevel >= setting.JoinAuditorLevel) || _blogUtil.CheckAdmin(true);
        }

        private IQueryable<BlogAudit> LatestAudits(BlogContext db) =>
            db.BlogAudits.GroupBy(b => b.BlogID).SelectMany(bg => bg.Where(ba => ba.BlogVersion >
                bg.Where(bm => bm.AuditAction == BlogAudit.Action.Approve || bm.AuditAction == BlogAudit.Action.Deny)
                    .DefaultIfEmpty().Max(bm => bm == null ? 0 : bm.BlogVersion)));

        public async Task<ActionResult> Index(int page = 1)
        {
            var user = _udb.Users.Include(u => u.auditor).SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (!await _userManager.IsInRoleAsync(user, "Auditor"))
            {
                return RedirectToAction("Join");
            }
            ViewBag.Auditor = user.auditor ?? new Auditor();
            var query = _db.Blogs.Where(b => b.isApproved == null && b.BlogID > 0)
                .GroupJoin(LatestAudits(_db).Where(ba => ba.Auditor == User.Identity.Name), b => b.BlogID, ba => ba.BlogID, (b, ba) => new { blog = b, audits = ba })
                .SelectMany(a => a.audits.DefaultIfEmpty(), (a, ba) => new AuditModel { blog = a.blog, audit = ba })
                .OrderBy(b => b.blog.BlogDate);

            Func<object, ActionResult> Result = View;
            if (Request.IsAjaxRequest())
            {
                Result = PartialView;
            }
            return Result(query.ToPagedList(page, PageSize));
        }

        [Authorize(Roles = "Administrator, Moderator, Auditor"), ValidateAntiForgeryToken]
        public async Task<JsonResult> Vote(int id, BlogAudit.Action auditAction, string reason)
        {
            if (!new[] { BlogAudit.Action.VoteApprove, BlogAudit.Action.VoteDeny, BlogAudit.Action.None }.Contains(auditAction))
            {
                return Json(new { success = false });
            }
            var blog = _db.Blogs.Include("blogAudits").SingleOrDefault(b => b.BlogID == id);
            var user = _udb.Users.Include("auditor").Where(u => u.UserName == User.Identity.Name).Single();

            if (blog == null || blog.isApproved != null)
            {
                return Json(new { success = false, blogGone = true });
            }
            if (user.auditor == null)
            {
                user.auditor = new Auditor();
                await _udb.SaveChangesAsync();
            }
            var version = blog.blogAudits.Where(ba => ba.AuditAction == BlogAudit.Action.Approve || ba.AuditAction == BlogAudit.Action.Deny)
                .DefaultIfEmpty().Max(bm => bm == null ? 0 : bm.BlogVersion) + 1;
            var audit = _db.BlogAudits.Find(id, User.Identity.Name, version)
                ?? _db.BlogAudits.Add(new BlogAudit { Auditor = User.Identity.Name, BlogID = id, BlogVersion = version });
            if (audit.AuditAction == auditAction)
            {
                return Json(new { success = true });
            }
            audit.AuditAction = auditAction;
            audit.AuditDate = DateTime.Now;
            audit.Reason = reason;
            await _db.SaveChangesAsync();
            // Calculate votes
            var setting = _dataSettings;
            var latestAudits = blog.blogAudits.Where(b => b.BlogVersion == version).ToList();
            var latestAuditAuthors = latestAudits.Select(ba => ba.Auditor);
            var auditors = _udb.Auditors.Include(a => a.User).Where(a => latestAuditAuthors.Contains(a.User.UserName)).ToDictionary(a => a.User.UserName.ToLower());
            bool needRefresh = false;
            float denyScore = latestAudits.Where(b => b.AuditAction == BlogAudit.Action.VoteDeny).Aggregate(0F, (score, ba) => score + auditors[ba.Auditor.ToLower()].Accuracy);
            if (denyScore >= setting.BlogDenyThreshold && setting.BlogDenyThreshold > 0)
            {
                DenyBlog(blog, "admin", true, latestAudits.Aggregate(new StringBuilder("投稿未通过审核。审核组给出的原因：<br>"),
                    (sb, ba) => sb.AppendFormat("{0}：{1}<br>", ba.Auditor, string.IsNullOrEmpty(ba.Reason) ? "不通过" : ba.Reason)).ToString());
                needRefresh = true;
            }
            else
            {
                float approveScore = latestAudits.Where(b => b.AuditAction == BlogAudit.Action.VoteApprove).Aggregate(0F, (score, ba) => score + auditors[ba.Auditor.ToLower()].Accuracy);
                if (approveScore >= setting.BlogApproveThreshold && setting.BlogApproveThreshold > 0)
                {
                    ApproveBlog(blog, "admin", blog.Rating == 0);
                    needRefresh = true;
                }
            }
            await _db.SaveChangesAsync();

            return Json(new { success = true, needRefresh = needRefresh });
        }

        [Authorize(Roles = "Administrator, Moderator, Auditor")]
        public ActionResult Stats(int? id, int? version)
        {
            if (id == null)
            {
                return NotFound();
            }
            var blog = _db.Blogs.Include("blogAudits").SingleOrDefault(b => b.BlogID == id.Value);
            var user = _udb.Users.Include("auditor").Where(u => u.UserName == User.Identity.Name).Single();

            if (blog == null)
            {
                return PartialView("StatsPartial");
            }
            List<BlogAudit> audits;
            if (version.HasValue)
            {
                audits = blog.blogAudits.Where(ba => ba.AuditAction != BlogAudit.Action.None && ba.BlogVersion == version).ToList();
            }
            else
            {
                audits = blog.blogAudits.Where(ba => (ba.AuditAction != BlogAudit.Action.None)
                    && (blog.isApproved != null ? ba.BlogVersion + 1 : ba.BlogVersion) > blog.blogAudits
                        .Where(bm => bm.AuditAction == BlogAudit.Action.Approve || bm.AuditAction == BlogAudit.Action.Deny)
                        .DefaultIfEmpty().Max(bm => bm == null ? 0 : bm.BlogVersion)).ToList();
            }
            return PartialView("StatsPartial", audits);
        }

        [Authorize(Roles = "Administrator, Moderator, Auditor")]
        public ActionResult History(string name, int page = 1)
        {
            ViewBag.Auditor = _udb.Auditors.SingleOrDefault(u => u.User.UserName == User.Identity.Name) ?? new Auditor();
            var query = _db.BlogAudits.Include("blog").Where(ba => ba.BlogID > 0 && ba.Auditor == name
                                        && (ba.AuditAction == BlogAudit.Action.VoteApprove || ba.AuditAction == BlogAudit.Action.VoteDeny))
                                .GroupJoin(_db.BlogAudits.Where(ba => ba.AuditAction == BlogAudit.Action.Approve || ba.AuditAction == BlogAudit.Action.Deny).DefaultIfEmpty(),
                                    la => new { la.BlogID, la.BlogVersion }, ba => new { ba.BlogID, ba.BlogVersion }, (la, ba) => new { Decision = ba, Vote = la })
                                .Select(a => new VoteResult
                                {
                                    Audit = a.Vote,
                                    Correct = a.Decision.Count() == 0 ? (bool?)null
                                            : (a.Vote.AuditAction == BlogAudit.Action.VoteApprove && a.Decision.FirstOrDefault().AuditAction == BlogAudit.Action.Approve
                                           || a.Vote.AuditAction == BlogAudit.Action.VoteDeny && a.Decision.FirstOrDefault().AuditAction == BlogAudit.Action.Deny)
                                }).OrderByDescending(v => v.Audit.AuditDate);
            Func<object, ActionResult> Result = View;
            if (Request.IsAjaxRequest())
            {
                Result = PartialView;
            }
            return Result(query.ToPagedList(page, HistoryPageSize));
        }

        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult AdminHistory(int id = 0, int page = 1)
        {
            var query = _db.BlogAudits.Where(b => b.BlogID > 0 && b.AuditAction != BlogAudit.Action.None);
            if (id > 0)
            {
                ViewBag.BlogID = id;
                query = query.Where(b => b.BlogID == id);
            }
            query = query.OrderByDescending(v => v.AuditDate);
            Func<object, ActionResult> Result = View;
            if (Request.IsAjaxRequest())
            {
                Result = PartialView;
            }
            return Result(query.ToPagedList(page, HistoryPageSize));
        }

        [HttpGet]
        public ActionResult Join()
        {
            var user = _udb.Users.Single(u => u.UserName == User.Identity.Name);
            var hasPassed = HasPassedExam();
            ViewBag.HasPassed = hasPassed;
            ViewBag.CanJoin = hasPassed && CanJoinAuditor(user.Level);
            return View();
        }

        [HttpPost, ActionName("Join"), ValidateAntiForgeryToken]
        public async Task<ActionResult> DoJoin([FromServices] SignInManager<UserProfile> signInManager)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (CanJoinAuditor(user.Level))
            {
                if (!await _userManager.IsInRoleAsync(user, "Auditor"))
                {
                    var result = await _userManager.AddToRoleAsync(user, "Auditor");
                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", string.Join(";", result.Errors.Select(e => e.Description)));
                        return View();
                    }
                    else if (user.auditor == null)
                    {
                        user.auditor = new Auditor();
                        await _userManager.UpdateAsync(user);
                    }
                    await signInManager.RefreshSignInAsync(user);
                }
                return RedirectToAction("Index");
            }

            return View();
        }

        [Authorize(Roles = "Administrator, Moderator")]
        public ActionResult Admin(int page = 1, bool showunapprove = false)
        {
            IQueryable<Blog> query;
            if (showunapprove)
                query = _db.Blogs.Where(b => b.isApproved == false && b.BlogID > 0).OrderByDescending(b => b.BlogDate);
            else
            {
                query = _db.Blogs.Where(b => b.isApproved == null && b.BlogID > 0).OrderBy(b => b.BlogDate);
            }
            ViewBag.showunapprove = showunapprove;
            Func<object, ActionResult> Result = View;
            if (Request.IsAjaxRequest())
            {
                Result = PartialView;
            }
            else
            {
                ViewBag.CurrentVoteCount = LatestAudits(_db).Count(ba => query.Any(b => b.BlogID == ba.BlogID));
                ViewBag.TotalVoteCount = _db.BlogAudits.Where(ba => ba.AuditAction == BlogAudit.Action.VoteApprove || ba.AuditAction == BlogAudit.Action.VoteDeny).Count();
                ViewBag.AverageAccuracy = _udb.Auditors.DefaultIfEmpty().Average(a => a == null ? 0 : (a.AuditCount > 0 ? a.CorrectCount / (float)a.AuditCount : 0));
            }
            return Result(query.ToPagedList(page, PageSize));
        }

        [HttpPost, Authorize(Roles = "Administrator, Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UnApprove(int blogid, int page = 1, bool sendmsg = false, bool delete = false, string MsgContent = "")
        {
            Blog b = _db.Blogs.Find(blogid);
            if (b != null)
            {
                MsgContent = WebUtility.HtmlEncode(MsgContent);
                if (delete)
                {
                    _adminUtil.ArchiveAudit(b, User.Identity.Name, BlogAudit.Action.Deny, b.isApproved == true, MsgContent);
                    await _blogUtil.DeleteBlogAsync(b.BlogID, MsgContent);
                    if (sendmsg)
                        _msgUtil.AddMsg(User.Identity.Name, b.Author, "删除通知", MsgContent);
                }
                else
                {
                    DenyBlog(b, User.Identity.Name, sendmsg, MsgContent);
                    _db.SaveChanges();
                }
                return new EmptyResult();
            }
            return NotFound();
        }

        [HttpPost, Authorize(Roles = "Administrator, Moderator")]
        [ValidateAntiForgeryToken]
        public ActionResult Pass(int blogid, bool updatetime = true, int page = 1)
        {
            Blog b = _db.Blogs.Find(blogid);
            if (b != null)
            {
                ApproveBlog(b, User.Identity.Name, updatetime);
                _db.SaveChanges();
                string referrer = Request.Headers[HeaderNames.Referer];
                var showunapprove = referrer.IndexOf("showunapprove=true", StringComparison.InvariantCultureIgnoreCase) >= 0;
                return new EmptyResult();
            }
            return NotFound();
        }

        [HttpPost, Authorize(Roles = "Administrator, Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> MultiAudit(string blogids, string action, bool sendmsg = true, int page = 1)
        {
            List<int> ids = BlogHelper.ParseIntListFromString(blogids, new List<int>());
            Action<Blog> func;
            List<Task> tasks = new List<Task>();
            switch (action)
            {
                case "multipass":
                    func = (b) => ApproveBlog(b, User.Identity.Name, b.Rating == 0);
                    break;

                case "multifail":
                    func = (b) => DenyBlog(b, User.Identity.Name, sendmsg);
                    break;

                case "multidel":
                    func = (b) =>
                    {
                        tasks.Add(_blogUtil.DeleteBlogAsync(b.BlogID));
                        var MsgContent = "您的投稿 " + WebUtility.HtmlEncode(b.BlogTitle) + " 未通过审核。";
                        _adminUtil.log(User.Identity.Name, "unapprove", b.BlogID.ToString());
                        if (sendmsg)
                            _msgUtil.AddMsg(User.Identity.Name, b.Author, "删除通知", MsgContent);
                    };
                    break;

                default:
                    return NotFound();
            }

            var blogs = _db.Blogs.Where(b => ids.Contains(b.BlogID)).ToList();
            blogs.ForEach(func);
            _db.SaveChanges();
            await Task.WhenAll(tasks);
            return new EmptyResult();
        }
    }
}