using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [Authorize(Policy = "Harmony"), ResponseCache(CacheProfileName = "Never")]
    public class TopicController : Controller
    {
        private BlogContext _db;
        private UsersContext _udb;
        private AdminUtil _adminUtil;
        private CategoryUtil _catUtil;
        private MessageUtil _msgUtil;
        private TagUtil _tagUtil;
        private AppSettingsModel _appSettings;
        private DataSettingsModel _dataSettings;
        private UserManager<UserProfile> _userManager;
        private IMemoryCache _cache;
        private BlogUtil _blogUtil;
        private UploadUtil _uploadUtil;
        private RatingUtil _ratingUtil;
        private IVisitCounter _visitCounter;
        private CacheService _cacheService;

        public TopicController(
            IOptions<AppSettingsModel> appSettings,
            IOptionsSnapshot<DataSettingsModel> dataSettings,
            BlogContext db,
            UsersContext udb,
            AdminUtil adminUtil,
            BlogUtil blogUtil,
            CategoryUtil catUtil,
            MessageUtil msgUtil,
            TagUtil tagUtil,
            UserManager<UserProfile> userManager,
            UploadUtil uploadUtil,
            RatingUtil ratingUtil,
            IVisitCounter visitCounter,
            CacheService cacheService,
            IMemoryCache cache)
        {
            _db = db;
            _udb = udb;
            _adminUtil = adminUtil;
            _catUtil = catUtil;
            _blogUtil = blogUtil;
            _msgUtil = msgUtil;
            _appSettings = appSettings.Value;
            _dataSettings = dataSettings.Value;
            _userManager = userManager;
            _cache = cache;
            _uploadUtil = uploadUtil;
            _tagUtil = tagUtil;
            _ratingUtil = ratingUtil;
            _visitCounter = visitCounter;
            _cacheService = cacheService;
        }

        private int listpagesize => _appSettings.ListPageSize;
        private int replypagesize => _appSettings.ReplyPageSize;

        public static event EventHandler<TopicEventArgs> OnDeleteTopic;

        public static event EventHandler<TopicEventArgs> OnNewTopic;

        public static event EventHandler<TopicEventArgs> OnEditTopic;

        private void TriggerNewTopic(Topic t)
        {
            OnNewTopic?.Invoke(this, new TopicEventArgs(t));
        }

        private void TriggerEditTopic(Topic t)
        {
            OnEditTopic?.Invoke(this, new TopicEventArgs(t));
        }

        private void TriggerDeleteTopic(Topic t)
        {
            OnDeleteTopic?.Invoke(this, new TopicEventArgs(t));
        }

        public ActionResult List(int? id, int page = 1, string sort = "")
        {
            var topics = _db.Topics.Include(t => t.tag);
            if (id.HasValue && id.Value > 0)
            {
                if (_catUtil.GetCategoryList().Find(l => l.CategoryID == id.Value) == null)
                    return NotFound();
                topics = topics.Where(b => b.CategoryID == id);
            }
            SearchModel model = new SearchModel { Sort = sort };
            ViewBag.SearchModel = model;
            topics = BlogHelper.getSortedQuery(_db, topics, sort);

            if (Request.IsAjaxRequest())
            {
                return PartialView(topics.ToPagedList(page, listpagesize));
            }
            return View(topics.ToPagedList(page, listpagesize));
        }

        //
        // GET: /Topic/Details/5

        public ActionResult Details(int id = 0)
        {
            Topic topic = _db.Topics.Find(id);
            if (topic == null)
            {
                return NotFound();
            }
            var model = new TopicDisplay();
            model.topic = topic;
            model.blogs = _db.BlogsInTopics.Where(t => t.TopicID == topic.TopicID).OrderBy(t => t.BlogOrder).Select(t => t.blog).ToList();
            topic.TopicVisit = _visitCounter.GetTopicVisit(topic.TopicID, true);
            string referrer = Request.Headers[HeaderNames.Referer];
            if (referrer != null && (referrer.IndexOf("Create", StringComparison.OrdinalIgnoreCase) > 0 || referrer.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) > 0))
            {
                Response.Headers.Add("X-XSS-Protection", "0");
            }
            return View(model);
        }

        //
        // GET: /Topic/Create
        [Authorize(Roles = "Writers,Administrator,Moderator")]
        public ActionResult Create()
        {
            ViewBag.CategoryID = _catUtil.GetCategoryDropdown();
            return View();
        }

        //
        // POST: /Topic/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Writers,Administrator,Moderator")]
        public async Task<ActionResult> Create(TopicEdit topic, [FromServices] HtmlSanitizerService sanitizerService)
        {
            ViewBag.CategoryID = _catUtil.GetCategoryDropdown();
            topic.LoadBlog(_db);
            if (ModelState.IsValid)
            {
                Topic ntopic = new Topic();
                if (!_blogUtil.CheckAdmin())
                {
                    topic.Content = sanitizerService.Sanitize(topic.Content);
                }
                int i = 0;
                foreach (var bid in topic.BlogIDs.Distinct())
                {
                    var b = topic.Blogs.SingleOrDefault(bb => bb.BlogID == bid);
                    if (b == null)
                    {
                        ModelState.AddModelError("", "未找到ID编号为" + bid + "的资源");
                        return View(topic);
                    }
                    var blogintopic = new BlogsInTopic { blog = b, topic = ntopic, BlogOrder = i++ };
                    _db.BlogsInTopics.Add(blogintopic);
                }

                ntopic.Author = User.Identity.Name;
                ntopic.CategoryID = topic.CategoryID;
                ntopic.Content = topic.Content;
                ntopic.CreateDate = DateTime.Now;
                ntopic.UpdateDate = DateTime.Now;
                ntopic.TopicVisit = 0;
                ntopic.TopicTitle = topic.TopicTitle;
                
                if (topic.TopicImage != null)
                {   //ValidateFileAttribute里已经检查过了
                    ntopic.isLocalImg = true;
                    var imglist = await _uploadUtil.SaveImagesAsync(new IFormFile[] { topic.TopicImage });
                    if (imglist.Count < 1)
                    {
                        ModelState.AddModelError("", "图片服务器上传出错，请稍后再试。如多次出错，请汇报给管理员。");
                        return View(topic);
                    }
                    ntopic.ImagePath = imglist[0];
                }
                else
                {
                    string imgname = BlogHelper.getFirstImg(ntopic.Content);
                    if (imgname == null || imgname.Length < 5)
                    {
                        ModelState.AddModelError("", "请添加预览图！（上传或在文中外链图片）");
                        return View(topic);
                    }
                    ntopic.ImagePath = imgname;
                }
                if (topic.TopicBanner != null)
                {
                    var imglist = await _uploadUtil.SaveImagesAsync(new IFormFile[] { topic.TopicBanner }, false);
                    if (imglist.Count < 1)
                    {
                        ModelState.AddModelError("", "图片服务器上传出错，请稍后再试。如多次出错，请汇报给管理员。");
                        return View(topic);
                    }
                    ntopic.BannerPath = imglist[0];
                }

                var tag = _db.Tags.SingleOrDefault(t => t.TagName == topic.TagName);
                if (tag == null)
                {
                    tag = new Tag { TagName = topic.TagName };
                }
                ntopic.tag = tag;
                var mention = new MentionHandler(_udb);
                ntopic.Content = mention.ParseMentions(BlogHelper.RemoveComments(ntopic.Content));
                _db.Topics.Add(ntopic);
                _db.SaveChanges();
                mention.SendMentionMsg(_msgUtil, ntopic.Author, ntopic.TopicTitle, Url.Action("Details", new { id = ntopic.TopicID }));
                TriggerNewTopic(ntopic);
                return RedirectToAction("Details", new { id = ntopic.TopicID });
            }
            return View(topic);
        }

        //
        // GET: /Topic/Edit/5
        [Authorize(Roles = "Writers,Administrator,Moderator")]
        public ActionResult Edit(int id = 0)
        {
            Topic topic = _db.Topics.Find(id);
            if (topic == null)
            {
                return NotFound();
            }
            ViewBag.id = id;
            ViewBag.CategoryID = _catUtil.GetCategoryDropdown();
            return View(new TopicEdit(_db, topic));
        }

        //
        // POST: /Topic/Edit/5

        [HttpPost]
        [Authorize(Roles = "Writers,Administrator,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, TopicEdit etopic, [FromServices] HtmlSanitizerService sanitizerService)
        {
            etopic.LoadBlog(_db);
            ViewBag.CategoryID = _catUtil.GetCategoryDropdown();
            int ret = TagUtil.CheckBlogTag(etopic.TagName, 1);
            if (ret != 0)
            {
                ModelState.AddModelError("", ret > 0 ? "专题标签只能有1个" : "标签不得超过20个字符");
            }
            else if (!_blogUtil.CheckAdmin())
            {
                etopic.Content = sanitizerService.Sanitize(etopic.Content);
            }
            else if (ModelState.IsValid)
            {
                var topic = _db.Topics.Find(id);
                bool uploadsaved = false;
                bool bannersaved = false;
                var blogcurrent = _db.BlogsInTopics.Where(bi => bi.TopicID == id).ToList();
                foreach (var blog in blogcurrent)
                {
                    _db.BlogsInTopics.Remove(blog);
                }
                int i = 0;
                foreach (var bid in etopic.BlogIDs.Distinct())
                {
                    var b = etopic.Blogs.SingleOrDefault(bb => bb.BlogID == bid);
                    if (b == null)
                    {
                        ModelState.AddModelError("", "未找到ID编号为" + bid + "的资源");
                        return View(topic);
                    }
                    var blogintopic = new BlogsInTopic { blog = b, topic = topic, BlogOrder = i++ };
                    _db.BlogsInTopics.Add(blogintopic);
                }

                if (topic.tag.TagName != etopic.TagName)
                {
                    var tag = _db.Tags.SingleOrDefault(t => t.TagName == etopic.TagName);
                    if (tag == null)
                    {
                        tag = new Tag { TagName = etopic.TagName };
                    }
                    topic.tag = tag;
                }
                try
                {
                    var originalImage = topic.ImagePath;
                    var originalBanner = topic.BannerPath;
                    bool shouldDeleteOldImage = false;
                    bool shouldDeleteOldBanner = string.IsNullOrWhiteSpace(etopic.BannerPath);
                    
                    if (etopic.TopicImage != null)
                    {
                        shouldDeleteOldImage = topic.isLocalImg;
                        topic.isLocalImg = true;
                        var imglist = await _uploadUtil.SaveImagesAsync(new IFormFile[] { etopic.TopicImage }, true);
                        if (imglist.Count < 1)
                        {
                            ModelState.AddModelError("", "保存图片时发生异常。请尝试转换图片格式后再次上传。如多次出错，请汇报给管理员。");
                            return View(etopic);
                        }
                        topic.ImagePath = imglist[0];
                        uploadsaved = true;
                    }
                    else if (!topic.isLocalImg || (topic.isLocalImg && !etopic.IsLocalImg))
                    {
                        string imgname = BlogHelper.getFirstImg(etopic.Content);
                        if (imgname == null || imgname.Length < 5)
                        {
                            ModelState.AddModelError("", "请添加预览图！（上传或在文中外链图片）");
                            return View(etopic);
                        }
                        shouldDeleteOldImage = !etopic.IsLocalImg;
                        topic.isLocalImg = false;
                        topic.ImagePath = imgname;
                    }
                    if (etopic.TopicBanner != null)
                    {
                        var imglist = await _uploadUtil.SaveImagesAsync(new IFormFile[] { etopic.TopicBanner }, false);
                        if (imglist.Count < 1)
                        {
                            ModelState.AddModelError("", "图片服务器上传出错，请尝试转换图片格式后再次上传。如多次出错，请汇报给管理员。");
                            return View(topic);
                        }
                        shouldDeleteOldBanner = true;
                        bannersaved = true;
                        topic.BannerPath = imglist[0];
                    }
                    else
                    {
                        topic.BannerPath = etopic.BannerPath;
                    }

                    if (shouldDeleteOldBanner && !string.IsNullOrWhiteSpace(originalBanner))
                    {
                        await _uploadUtil.DeleteFileAsync(originalBanner);
                    }
                    if (shouldDeleteOldImage && !string.IsNullOrWhiteSpace(originalImage))
                    {
                        await _uploadUtil.DeleteFilesAsync(new[] { originalImage, originalImage.Replace("/upload/", "/thumbs/") });
                    }
                    topic.UpdateDate = DateTime.Now;
                    topic.TopicTitle = etopic.TopicTitle;
                    topic.CategoryID = etopic.CategoryID;
                    var mention = new MentionHandler(_udb);
                    topic.Content = mention.ParseMentions(BlogHelper.RemoveComments(etopic.Content));
                    mention.SendMentionMsg(_msgUtil, User.Identity.Name, etopic.TopicTitle, Url.Action("Details", new { id = topic.TopicID }));
                    _db.Entry(topic).State = EntityState.Modified;
                    _db.SaveChanges();
                    TriggerEditTopic(topic);
                    if (User.Identity.Name != topic.Author)
                    {
                        _adminUtil.log(User.Identity.Name, "edittopic", topic.TopicID.ToString());
                    }
                }
                catch
                {
                    if (uploadsaved)
                    {
                        await _uploadUtil.DeleteFilesAsync(new[] { topic.ImagePath, topic.ImagePath.Replace("/upload/", "/thumbs/") });
                    }
                    if (bannersaved)
                    {
                        await _uploadUtil.DeleteFileAsync(topic.BannerPath);
                    }
                    throw;
                }
                return RedirectToAction("Details", new { id });
            }
            return View(etopic);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(int? id = 0)
        {
            Topic topic = _db.Topics.Find(id);
            if (topic == null || (!_blogUtil.CheckAdmin() && topic.Author != User.Identity.Name))
            {
                return NotFound();
            }
            if (topic.Author != User.Identity.Name)
            {
                _adminUtil.log(User.Identity.Name, "deletetopic", topic.TopicID + ": " + topic.TopicTitle);
            }
            if (topic.isLocalImg)
            {
                await _uploadUtil.DeleteFilesAsync(new[] { topic.ImagePath, topic.ImagePath.Replace("/upload/", "/thumbs/") });
            }
            TriggerDeleteTopic(topic);
            _db.Topics.Remove(topic);
            _db.SaveChanges();
            return RedirectToAction("List");
        }
    }
}