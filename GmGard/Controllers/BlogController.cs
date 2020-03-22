using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using X.PagedList;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class BlogController : Controller
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
        private IRecommendationProvider _recommendationProvider;
        private CacheService _cacheService;

        public BlogController(
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
            IRecommendationProvider recommendationProvider,
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
            _recommendationProvider = recommendationProvider;
            _cacheService = cacheService;
        }
        
        private int ListPageSize => _appSettings.ListPageSize;

        public List<int> NolinkCategories => _appSettings.NoLinkCategories;

        private List<int> FeaturedBlogId => _dataSettings.FeaturedBlogIdList();

        private HarmonySettingsModel HarmonySettings => _appSettings.HarmonySettings;

        private bool IsHarmony => !User.Identity.IsAuthenticated && HarmonySettings.Harmony;

        public static event EventHandler<BlogEventArgs> OnNewBlog;

        public static event EventHandler<BlogEventArgs> OnDeleteBlog;

        public static event EventHandler<BlogEventArgs> OnEditBlog;

        public static event EventHandler<TagEventArgs> OnEditTags;

        private void TriggerNewBlog(Blog b, IEnumerable<Tag> t) => OnNewBlog?.Invoke(this, new BlogEventArgs(b, t));

        private void TriggerDeleteBlog(Blog b, bool d = true) => OnDeleteBlog?.Invoke(this, new BlogEventArgs(b, d));
        
        private void TriggerEditBlog(Blog b, IEnumerable<Tag> t) => OnEditBlog?.Invoke(this, new BlogEventArgs(b, t));

        private void TriggerEditTags(Blog b, IEnumerable<Tag> t) => OnEditTags?.Invoke(this, new TagEventArgs(t, b));

        private List<HanGroup> GetUserHanGroup()
        {
            if (!User.Identity.IsAuthenticated)
                return new List<HanGroup>();
            return _db.HanGroupMembers.Where(h => h.Username == User.Identity.Name).Select(h => h.hangroup).ToList();
        }

        public async Task<ActionResult> List([FromServices]ISearchProvider searchProvider, int? id, SearchModel search, int page = 1)
        {
            int pagesize = ListPageSize;
            search = search ?? new SearchModel();
            if (id.HasValue && id.Value > 0)
            {
                if (!_catUtil.GetCategoryList().Any(l => l.CategoryID == id.Value))
                    return NotFound();
                search.CurrentCategory = id;
                ViewBag.CategoryId = id;
            }
            if (IsHarmony)
            {
                search.Harmony = IsHarmony;
            }
            ViewBag.CurrentSort = search.SortOptions();
            ViewBag.SearchModel = search;

            var cache = _cache.Get<ConcurrentDictionary<string, Task<SearchBlogResult>>> (CacheService.BlogListCacheKey);
            
            Func<string, object, ActionResult> Result = View;
            if (Request.IsAjaxRequest())
            {
                Result = PartialView;
            }
            SearchBlogResult result;
            if (search.Cacheable())
            {
                var cachekey = search.CacheKey(page, pagesize);
                cache = cache ?? new ConcurrentDictionary<string, Task<SearchBlogResult>>();
                result = await cache.GetOrAdd(cachekey, (s) =>
                {
                    _cache.Set(CacheService.BlogListCacheKey, cache, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
                    return searchProvider.SearchBlogAsync(search, page, pagesize);
                });
                if (result.HasError)
                {
                    cache.TryRemove(cachekey, out var removed);
                    throw new TimeoutException("查询搜索服务器失败，请刷新重试");
                }
            }
            else
            {
                result = await searchProvider.SearchBlogAsync(search, page, pagesize);
            }
            return Result("List", result);
        }

        [Authorize]
        public ActionResult Create()
        {
            ViewBag.CategoryList = _catUtil.GetCategoryDropdown();
            ViewBag.UserHanGroup = GetUserHanGroup();
            return View();
        }

        [HttpPost, Authorize, ValidateCaptcha]
        public async Task<ActionResult> Create(BlogEdit blog, [FromServices] HtmlSanitizerService sanitizerService)
        {
            ViewBag.CategoryList = _catUtil.GetCategoryDropdown();
            ViewBag.UserHanGroup = GetUserHanGroup();
            string content = blog.Content;
            bool isLocalimg = false;
            Blog newblog;
            List<string> imglist = null;
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new BlogException();
                }
                if (content == null || string.IsNullOrWhiteSpace(BlogHelper.removeAllTags(content)))
                {
                    ModelState.AddModelError("", "内容不能为空或纯图片");
                    throw new BlogException();
                }
                if (NolinkCategories == null || !NolinkCategories.Contains(blog.CategoryID))
                {
                    if (blog.BlogLinks == null)
                    {
                        ModelState.AddModelError("", "链接地址不能为空");
                        throw new BlogException();
                    }
                    else
                    {
                        blog.BlogLinks = blog.BlogLinks.Where(b => !string.IsNullOrWhiteSpace(b.url)).ToArray();
                        if (!BlogHelper.checkBlogLinks(blog.BlogLinks))
                        {
                            ModelState.AddModelError("", "链接地址不能为空，且不得包含javascript");
                            throw new BlogException();
                        }
                    }
                }
                if (!_blogUtil.CheckAdmin())
                {
                    content = sanitizerService.Sanitize(content);
                }
                if (blog.HanGroupID.HasValue && !_db.HanGroupMembers.Any(h => h.Username == User.Identity.Name && h.HanGroupID == blog.HanGroupID))
                {
                    ModelState.AddModelError("", "汉化组ID无效，请刷新重试。");
                    throw new BlogException();
                }
                List<IFormFile> BlogImages = new List<IFormFile>();
                if (Request.Form.Files.Count > 0)
                {
                    for (int i = 0; i < Request.Form.Files.Count; i++)
                    {
                        var file = Request.Form.Files[i];
                        if (file.Length > 0)
                        {
                            if (!file.ContentType.Contains("image"))
                            {
                                ModelState.AddModelError("", "不接受的文件类型");
                                throw new BlogException();
                            }
                            else if (file.Length > 1048576 * 4)
                            {
                                ModelState.AddModelError("", "文件不得超过4MB");
                                throw new BlogException();
                            }
                            isLocalimg = true;
                            BlogImages.Add(file);
                        }
                        else
                        {
                            content = BlogHelper.removeImgPlaceholder(content, i);
                        }
                    }
                }
                if (!isLocalimg)
                {
                    var imgname = BlogHelper.getFirstImg(content);
                    if (imgname == null || imgname.Length < 5)
                    {
                        ModelState.AddModelError("", "请添加预览图！（上传或在文中外链图片）");
                        throw new BlogException();
                    }
                    imglist = new List<string>() { imgname };
                }
                else
                {
                    try
                    {
                        imglist = await _uploadUtil.SaveImagesAsync(BlogImages);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "保存图片时发生异常：(" + e.Message + ")。如多次出错，请汇报给管理员。");
                        throw new BlogException(e.Message, e);
                    }
                    if (imglist.Count < 1)
                    {
                        ModelState.AddModelError("", "图片服务器上传出错，请稍后再试。如多次出错，请汇报给管理员。");
                        throw new BlogException();
                    }
                }
                string imgpath = string.Empty;
                if (imglist != null)
                {
                    imgpath = string.Join(";", imglist);
                }
                bool approve = User.IsInRole("Administrator") || User.IsInRole("Writers") || User.IsInRole("Moderator");
                // Replace 【】（） with []()
                blog.BlogTitle = blog.BlogTitle.ToSingleByteCharacterString();
                blog.ImagePath = imgpath;
                content = BlogHelper.RemoveComments(content);
                newblog = _blogUtil.AddBlog(blog.BlogTitle, content,
                    blog.CategoryID, imgpath, User.Identity.Name, approve, isLocalimg, blog.BlogLinks);
                var taglist = new List<Tag>();
                if (!string.IsNullOrEmpty(blog.BlogTags))
                {
                    string[] tags = TagUtil.SplitTags(blog.BlogTags);
                    taglist = _tagUtil.AddTagsForBlog(newblog.BlogID, tags, newblog.Author);
                }
                var save = false;
                if (BlogHelper.BlogIsHarmony(_db, newblog, HarmonySettings))
                {
                    newblog.isHarmony = true;
                    save = true;
                }
                if (blog.HanGroupID.HasValue)
                {
                    _db.HanGroupBlogs.Add(new HanGroupBlog { BlogID = newblog.BlogID, HanGroupID = blog.HanGroupID.Value });
                    save = true;
                }
                if (blog.Option != null && !blog.Option.IsDefault())
                {
                    newblog.option = blog.Option.OverrideOption(_blogUtil);
                    if (newblog.option.NoApprove)
                    {
                        newblog.isApproved = false;
                    }
                    save = true;
                }
                if (save) _db.SaveChanges();
                TriggerNewBlog(newblog, taglist);
            }
            catch (BlogException e)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        err = e.Message + string.Join(";", ModelState.Values.SelectMany(m => m.Errors)
                            .Select(err => err.ErrorMessage)
                            .ToList())
                    });
                }
                return View(blog);
            }
            catch
            {
                if (isLocalimg && imglist != null)
                {
                    await _uploadUtil.DeleteFilesAsync(imglist.Concat(new[] { blog.ImagePath.Split(';')[0].Replace("/upload/", "/thumbs/") }));
                }
                throw;
            }
            if (Request.IsAjaxRequest())
            {
                return Json(new { id = newblog.BlogID, src = BlogHelper.firstImgPath(newblog, true) });
            }
            return RedirectToAction("Details", new { id = newblog.BlogID });
        }

        [BlogAuthorize]
        public ActionResult Details(int id = 0)
        {
            var bd = _blogUtil.GetDetailDisplay(id);
            if (bd == null)
            {
                return NotFound();
            }
            if (IsHarmony)
            {
                bd.tag = bd.tag.Where(i => !HarmonySettings.WhitelistTags.Contains(i.TagID));
            }
            var bannerBlogId = _dataSettings.BannerBlogIdList();
            bd.Option.NoRate = (FeaturedBlogId != null && FeaturedBlogId.Contains(id)) || (bannerBlogId != null && bannerBlogId.Contains(id)) || bd.Option.NoRate;

            if (User.Identity.IsAuthenticated)
            {
                ViewBag.isFavorite = bd.IsFavorite;
            }

            string referrer = Request.Headers[HeaderNames.Referer];
            if (referrer != null && (referrer.IndexOf("Create", StringComparison.OrdinalIgnoreCase) > 0 || referrer.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) > 0))
            {
                Response.Headers["X-XSS-Protection"] = "0";
            }
            return View(bd);
        }

        public ActionResult Random(int? id)
        {
            IQueryable<Blog> blogs = _db.Blogs;
            if (id.HasValue)
            {
                var catids = _catUtil.GetCategoryWithSubcategories(id.Value);
                blogs = blogs.Where(b => catids.Contains(b.CategoryID));
            }
            var blog = blogs.OrderBy(_ => Guid.NewGuid()).First();
            return RedirectToAction("Details", new { id = blog.BlogID });
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Moderator")]
        public async Task<ActionResult> AdminDelete(int id, string MsgContent, bool sendmsg = false, bool unapprove = false)
        {
            Blog b = _db.Blogs.Find(id);
            if (b != null)
            {
                if (unapprove)
                {
                    string user = User.Identity.Name;
                    bool amend = b.isApproved == true;
                    if (b.isApproved != false)
                    {
                        b.isApproved = false;
                        _db.SaveChanges();
                        _adminUtil.ArchiveAudit(b, user, BlogAudit.Action.Deny, amend, MsgContent);
                    }
                    if (sendmsg)
                    {
                        _msgUtil.SendUnapproveNotice(b.Author, user, MsgContent, Url.Action("Details", new { id = b.BlogID }));
                    }
                    _adminUtil.log(User.Identity.Name, "reject", b.BlogID.ToString(), MsgContent);
                }
                else
                {
                    if (sendmsg)
                    {
                        _msgUtil.SendDeleteBlogNotice(b.Author, User.Identity.Name, MsgContent, b.BlogTitle);
                    }
                    await _blogUtil.DeleteBlogAsync(id, MsgContent);
                }
                TriggerDeleteBlog(b, !unapprove);
            }
            if (unapprove)
            {
                return RedirectToAction("Details", new { id = b.BlogID });
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Delete(int? blogid)
        {
            if (blogid.HasValue)
            {
                Blog b = _db.Blogs.Find(blogid.Value);
                if (b != null)
                {
                    if (b.Author == User.Identity.Name || User.IsInRole("Administrator") || User.IsInRole("Moderator"))
                    {
                        await _blogUtil.DeleteBlogAsync(b.BlogID);
                        TriggerDeleteBlog(b);
                        return RedirectToAction("List");
                    }
                }
            }
            return StatusCode(403);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            Blog blog = _db.Blogs.Find(id);
            if (blog == null)
            {
                return NotFound();
            }
            if (blog.Author != User.Identity.Name && !User.IsInRole("Administrator") && !User.IsInRole("Moderator"))
            {
                return RedirectToAction("Details", new { id = id });
            }
            ViewBag.CategoryList = _catUtil.GetCategoryDropdown(blog.CategoryID);
            ViewBag.id = id;
            var edit = new BlogEdit(_tagUtil.GetTagNamesInBlog(blog.BlogID), blog);
            return View(edit);
        }

        [HttpPost, Authorize]
        public async Task<ActionResult> Edit([FromServices] HtmlSanitizerService sanitizerService, int id, BlogEdit blog, IFormFile[] files, bool setmain = false)
        {
            ViewBag.CategoryList = _catUtil.GetCategoryDropdown(blog.CategoryID);
            ViewBag.id = id;
            if (NolinkCategories == null || !NolinkCategories.Contains(blog.CategoryID))
            {
                if (blog.BlogLinks == null)
                {
                    ModelState.AddModelError("", "链接地址不能为空");
                    return View(blog);
                }
                else
                {
                    blog.BlogLinks = blog.BlogLinks.Where(b => !string.IsNullOrWhiteSpace(b.url)).ToArray();
                    if (!BlogHelper.checkBlogLinks(blog.BlogLinks))
                    {
                        ModelState.AddModelError("", "链接地址不能为空，且不得包含javascript");
                        return View(blog);
                    }
                }
            }
            if (blog.Content == null || string.IsNullOrWhiteSpace(BlogHelper.removeAllTags(blog.Content)))
            {
                ModelState.AddModelError("", "内容不能为空或纯图片");
                return View(blog);
            }
            if (!_blogUtil.CheckAdmin())
            {
                blog.Content = sanitizerService.Sanitize(blog.Content);
            }
            if (ModelState.IsValid)
            {
                Blog originalblog = _db.Blogs.Find(id);
                bool hasupload = false;
                List<string> dellist = null;
                List<string> newlist = null;
                string thumb = null;
                string newthumb = null;
                string[] originalImglist = originalblog.ImagePath?.Split(';') ?? new string[] { };
                string[] currentImglist = blog.ImagePath?.Split(';') ?? new string[] { };
                int imgcount = currentImglist.Length;
                bool[] uploadpos = null;
                if (originalblog.option != null)
                {
                    originalblog.option.MergeWith(_blogUtil, blog.Option);
                }
                else if (!blog.Option.OverrideOption(_blogUtil).IsDefault())
                {
                    originalblog.option = blog.Option;
                }
                if (originalblog.IsLocalImg)
                {
                    dellist = originalImglist.ToList();
                    thumb = dellist.First();
                }
                if (currentImglist.Length != 0) //item has local image & might or might not changed
                {
                    // foreach name in current imglist, if orignal imglist does not contain the name,it is not valid
                    if (!currentImglist.All(n => originalImglist.Contains(n)))
                    {
                        ModelState.AddModelError("", "内部参数错误，请刷新重试");
                        return View(blog);
                    }
                    // foreach name in orignial imglist, if current imglist does not contain the name,delete it
                    dellist = originalImglist.Except(currentImglist).ToList();
                    originalblog.ImagePath = blog.ImagePath;
                }
                if (files != null)
                {
                    uploadpos = new bool[files.Length];
                    imgcount += files.Where(f => f != null).Count();
                    for (int i = 0; i < files.Length; i++)
                    {
                        var data = files[i];
                        if (data != null)
                        {
                            if (data.Length > 1048576 * 4 || !data.ContentType.Contains("image"))
                            {
                                ModelState.AddModelError("", "单个文件不得超过4MB，且必须是图片");
                                return View(blog);
                            }
                            hasupload = true;
                            uploadpos[i] = true;
                        }
                        else
                        {
                            uploadpos[i] = false;
                        }
                    }
                }
                if (hasupload) // has upload file
                {
                    try
                    {
                        newlist = await _uploadUtil.SaveImagesAsync(files, false);
                    }
                    catch (Exception e)
                    {
                        ModelState.AddModelError("", "保存图片时发生异常：(" + e.Message + ")。如多次出错，请汇报给管理员。");
                        return View(blog);
                    }
                    if (newlist.Count < 1)
                    {
                        ModelState.AddModelError("", "图片服务器上传出错，请稍后再试。如多次出错，请汇报给管理员。");
                        return View(blog);
                    }
                    int i = 0;
                    if (originalblog.IsLocalImg && setmain && files[0] != null && currentImglist.Length > 0)
                    {
                        List<string> updatedImgList = new List<string>(currentImglist);
                        updatedImgList.Insert(0, newlist[0]);
                        updatedImgList.AddRange(newlist.Skip(1));
                        originalblog.ImagePath = string.Join(";", updatedImgList);
                        blog.Content = BlogHelper.InsertImgPlaceholder(blog.Content);
                        i++;
                    }
                    else if (!originalblog.IsLocalImg)
                    {
                        originalblog.ImagePath = string.Join(";", newlist);
                    }
                    else
                    {
                        originalblog.ImagePath = string.Join(";", currentImglist.Concat(newlist));
                    }
                    blog.Content = BlogHelper.ReplaceNewImgPlaceholder(blog.Content, i, currentImglist.Length, uploadpos);
                    originalblog.IsLocalImg = true;
                }
                if (!originalblog.IsLocalImg || imgcount == 0) //no img no upload
                {
                    string imgname = BlogHelper.getFirstImg(blog.Content);
                    if (imgname == null || imgname.Length < 5)
                    {
                        ModelState.AddModelError("", "请添加预览图！（上传或外链图片）");
                        blog.ImagePath = originalblog.ImagePath;
                        return View(blog);
                    }
                    originalblog.ImagePath = imgname;
                    originalblog.IsLocalImg = false;
                }
                else
                {
                    newthumb = originalblog.ImagePath.Split(';').ToList().First();
                }

                var mention = new MentionHandler(_udb);
                blog.Content = mention.ParseMentions(blog.Content);
                mention.SendMentionMsg(_msgUtil, originalblog.Author, originalblog.BlogTitle, Url.Action("Details", new { id = originalblog.BlogID }));
                if (blog.Option != null && (originalblog.option != null || !blog.Option.IsDefault()))
                {
                    originalblog.option = blog.Option;
                }
                // else image uploaded before and did not changed
                string[] tags = TagUtil.SplitTags(blog.BlogTags);
                List<Tag> updatedTags = null;
                try
                {
                    // Replace 【】（） with []()
                    originalblog.BlogTitle = blog.BlogTitle.ToSingleByteCharacterString();
                    originalblog.Content = BlogHelper.RemoveComments(blog.Content);
                    originalblog.CategoryID = blog.CategoryID;
                    originalblog.Links = Newtonsoft.Json.JsonConvert.SerializeObject(blog.BlogLinks);
                    if (blog.Option.NoApprove)
                    {
                        originalblog.isApproved = false;
                    }
                    else if (originalblog.isApproved == false)
                    {
                        originalblog.isApproved = null;
                    }
                    else if (originalblog.isApproved == null)
                    {
                        // Remove pending votes since the blog has changed.
                        var audits = _db.BlogAudits.Where(b => b.BlogID == originalblog.BlogID).ToList();
                        var lastDecision = audits.Where(ba => ba.AuditAction == BlogAudit.Action.Approve || ba.AuditAction == BlogAudit.Action.Deny).OrderByDescending(ba => ba.BlogVersion).FirstOrDefault();
                        int lastVersion = lastDecision == null ? 0 : lastDecision.BlogVersion;
                        _db.BlogAudits.RemoveRange(audits.Where(ba => ba.BlogVersion > lastVersion && (ba.AuditAction == BlogAudit.Action.VoteApprove || ba.AuditAction == BlogAudit.Action.VoteDeny)));
                    }
                    updatedTags = _tagUtil.SetTagsForBlog(originalblog.BlogID, tags, originalblog.Author);
                    originalblog.isHarmony = BlogHelper.BlogIsHarmony(_db, originalblog, HarmonySettings);
                    _db.SaveChanges();
                }
                catch
                {
                    if (originalblog.IsLocalImg && newlist != null)
                    {
                        await _uploadUtil.DeleteFilesAsync(newlist);
                    }
                    throw;
                }
                if (dellist != null && dellist.Count > 0)
                {
                   await _uploadUtil.DeleteFilesAsync(dellist);
                }
                if (thumb != null && thumb != newthumb && blog.IsLocalImg)
                {
                   await  _uploadUtil.DeleteFileAsync(thumb.Replace("/upload/", "/thumbs/"));
                }
                if (originalblog.IsLocalImg && thumb != newthumb)
                {
                    await _uploadUtil.SaveThumbAsync(originalblog.ImagePath);
                }
                if (originalblog.Author != User.Identity.Name)
                {
                    _adminUtil.log(User.Identity.Name, "editblog", originalblog.BlogID.ToString());
                }
                TriggerEditBlog(originalblog, updatedTags);
                return RedirectToAction("Details", new { id });
            }
            return View(blog);
        }

        [HttpPost, Authorize]
        public EmptyResult AddFav(int id = 0)
        {
            var blog = _db.Blogs.Find(id);
            var fav = _db.Favorites.Find(User.Identity.Name, id);
            if (blog != null && fav == null)
            {
                fav = new Favorite
                {
                    BlogID = blog.BlogID,
                    Username = User.Identity.Name,
                    AddDate = DateTime.Now
                };
                _db.Favorites.Add(fav);
                _db.SaveChanges();
                _cache.Remove("favcount" + User.Identity.Name.ToLower());
                _cache.Set(CacheService.GetIsFavCacheKey(id, User.Identity.Name), true);
            }
            return new EmptyResult();
        }

        [HttpPost, Authorize]
        public async Task<EmptyResult> RemoveFav(int id = 0)
        {
            var fav = await _db.Favorites.FindAsync(User.Identity.Name, id);
            if (fav != null)
            {
                _db.Favorites.Remove(fav);
                await _db.SaveChangesAsync();
                _cache.Remove("favcount" + User.Identity.Name.ToLower());
                _cache.Set(CacheService.GetIsFavCacheKey(id, User.Identity.Name), false);
            }
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult Rate(string blogid, string rating)
        {
            int id = int.Parse(blogid);
            int value = int.Parse(rating);
            string status = _ratingUtil.TryRateBlog(id, value);
            if (status == "login")
            {
                 return Json(new { errmsg = "请登录后再评分！" });
            }
            else if (status == "rated")
            {
                return Json(new { errmsg = "您已经评过分了！" });
            } 
            else if (status == "rated_today")
            {
                return Json(new { errmsg = "今天已经评过分了！" });
            }
            else if (status == "error")
            {
                return Json(new { errmsg = "无效的评分，请刷新重试" });
            }
            var total = _ratingUtil.GetRating(id, false);
            if (HttpContext.Items["QuestMsg"] is string QuestMsg)
                return Json(new { rating = total, msg = QuestMsg });
            return Json(new { rating = total });
        }

        [HttpPost]
        public ActionResult GetRate(int blogid)
        {
            return Json(new { rating = _ratingUtil.getRating(blogid, false) });
        }

        [HttpPost]
        public ActionResult GetUsersRate(int blogid)
        {
            var rating = _ratingUtil.GetUsersRating(blogid);
            return Json(new { rating = rating.Rating == null ? null : new { value = rating.Rating.value, HasPost = rating.HasPost } });
        }

        [Authorize]
        public JsonResult GetMagnet(string data)
        {
            if (string.IsNullOrEmpty(data))
                return Json(null);
            return Json(TorrentHelper.GetMagnetURL(System.Convert.FromBase64String(data)));
        }

        [Authorize, HttpPost]
        public async Task<JsonResult> EditTag(string TagToAdd, int[] TagToDel, int BlogID)
        {
            var TagsToAdd = TagUtil.SplitTags(TagToAdd);
            if (TagsToAdd.Any(t => t.Length > 20))
            {
                return Json(new { errmsg = "标签不得超过20字符" });
            }
            else
            {
                Blog b = _db.Blogs.Find(BlogID);
                if (b == null || BlogID <= 0 || (b.option != null && b.option.LockTags))
                {
                    return Json(new { errmsg = "无效id，请刷新重试" });
                }
                var tibs = await _db.TagsInBlogs.Include(tib => tib.tag).Where(t => t.BlogID == BlogID).ToListAsync();
                // Remove TagToAdd items from TagsToDel
                var TagsToDel = TagToDel == null ?
                    new TagsInBlog[0] :
                    tibs.Where(tib => TagToDel.Contains(tib.TagID) && !TagsToAdd.Contains(tib.tag.TagName, SqlStringComparer.Instance)).ToArray();
                // Remove already exist tags from TagsToAdd
                TagsToAdd = TagsToAdd.Where(s => tibs.All(tib => !SqlStringComparer.Instance.Equals(tib.tag.TagName, s))).ToArray();

                if (tibs.Any(tib => !tib.IsRemovable(b.Author, User.Identity.Name, _blogUtil.CheckAdmin(), HarmonySettings.BlacklistTags) && TagToDel.Contains(tib.TagID)))
                {
                    return Json(new { errmsg = "您不能删除作者添加的或黑名单标签" });
                }
                if (tibs.Count + TagsToAdd.Length - TagsToDel.Length > 10)
                {
                    return Json(new { errmsg = "标签不得超过10个" });
                }
                else if (tibs.Count + TagsToAdd.Length - TagsToDel.Length < 1)
                {
                    return Json(new { errmsg = "请至少添加一个标签" });
                }
                List<Tag> AddedTags = _tagUtil.AddTagsForBlog(BlogID, TagsToAdd, User.Identity.Name);
                _db.TagHistories.AddRange(TagsToDel.Select(tib => new TagHistory
                {
                    AddBy = tib.AddBy,
                    BlogID = BlogID,
                    DeleteBy = User.Identity.Name,
                    TagName = tib.tag.TagName,
                    Time = DateTime.Now
                }));
                _db.TagsInBlogs.RemoveRange(TagsToDel);

                bool isHarmony = BlogHelper.BlogIsHarmony(_db, b, HarmonySettings);
                if (isHarmony != b.isHarmony)
                {
                    b.isHarmony = isHarmony;
                    _cacheService.ClearHPCache(this, null);
                }
                await _db.SaveChangesAsync();
                var currentTags = tibs.Except(TagsToDel).Select(tib => tib.tag).Concat(AddedTags).OrderBy(t => t.TagName);
                TriggerEditTags(b, currentTags);
                return Json(AddedTags.ToDictionary(t => t.TagID.ToString(), t => t.TagName));
            }
        }

        [Authorize, HttpGet]
        public ActionResult EditTag(int id)
        {
            var blog = _db.Blogs.Include(b => b.option).SingleOrDefault(b => b.BlogID == id);
            if (blog == null || (blog.option != null && blog.option.LockTags))
            {
                return NotFound();
            }
            return PartialView("TagPartial", new EditTagModel(_blogUtil, _db, id, blog.Author, HarmonySettings.BlacklistTags));
        }

        [Authorize]
        public ActionResult FetchBlog(int id = 0)
        {
            if (id != 0)
            {
                var blog = _db.Blogs.Find(id);
                if (blog != null)
                {
                    return PartialView("_ListBriefPartial", blog);
                }
            }
            return new EmptyResult();
        }

        [Authorize]
        public ActionResult FetchBlogs(string view, List<int> ids)
        {
            if (ids != null && ids.Count > 0)
            {
                if (!new[] { "_ListPartial", "_ListBriefPartial", "_ListUserBlogPartial" }.Contains(view))
                {
                    view = "_ListBriefPartial";
                }
                ViewBag.PartialView = view;
                var blogs = _db.Blogs.Where(b => ids.Contains(b.BlogID)).ToList().OrderBy(b => ids.IndexOf(b.BlogID));
                return PartialView("BlogListPartial", blogs);
            }
            return new EmptyResult();
        }

        [Authorize, HttpGet]
        public ActionResult Patch(string name)
        {
            if (!User.Identity.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }
            return PartialView("PatchPartial");
        }

        [Authorize, HttpPost]
        public ActionResult Patch(int BlogId, BlogLink[] BlogPasswords, BlogLink[] BlogLinks)
        {
            var blog = _db.Blogs.Find(BlogId);
            if (blog == null || !User.Identity.Name.Equals(blog.Author, StringComparison.OrdinalIgnoreCase))
            {
                return NotFound();
            }
            var links = BlogHelper.GetBlogLink(blog.Links);
            if (links != null)
            {
                BlogLinks = links.Concat(BlogLinks).ToArray();
            }
            blog.Links = Newtonsoft.Json.JsonConvert.SerializeObject(BlogLinks.Where(b => !string.IsNullOrWhiteSpace(b.url)));
            if (BlogPasswords != null)
            {
                BlogPasswords = BlogPasswords.Where(b => !string.IsNullOrWhiteSpace(b.url)).ToArray();
                if (BlogPasswords.Length > 0)
                {
                    blog.Content = blog.Content + string.Format("<p>{0:yyyy年MM月dd日 HH:mm} 补档：</p>", DateTime.Now);
                    blog.Content = BlogHelper.appendPassToContent(blog.Content, BlogPasswords);
                }
            }
            _db.SaveChanges();
            return Json(new { success = true });
        }
    }
}