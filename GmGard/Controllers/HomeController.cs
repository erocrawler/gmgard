using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class HomeController : Controller
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
        private ExpUtil _expUtil;
        private UploadUtil _uploadUtil;
        private RatingUtil _ratingUtil;
        private CacheService _cacheService;
        private readonly IWebHostEnvironment _env;
        private ISearchProvider _searchProvider;

        public HomeController(
            IOptions<AppSettingsModel> appSettings,
            IOptionsSnapshot<DataSettingsModel> dataSettings,
            BlogContext db,
            UsersContext udb,
            AdminUtil adminUtil,
            BlogUtil blogUtil,
            CategoryUtil catUtil,
            ExpUtil expUtil,
            MessageUtil msgUtil,
            TagUtil tagUtil,
            UserManager<UserProfile> userManager,
            UploadUtil uploadUtil,
            RatingUtil ratingUtil,
            CacheService cacheService,
            IMemoryCache cache,
            IWebHostEnvironment env,
            ISearchProvider searchProvider)
        {
            _db = db;
            _udb = udb;
            _adminUtil = adminUtil;
            _catUtil = catUtil;
            _blogUtil = blogUtil;
            _expUtil = expUtil;
            _msgUtil = msgUtil;
            _appSettings = appSettings.Value;
            _dataSettings = dataSettings.Value;
            _userManager = userManager;
            _cache = cache;
            _uploadUtil = uploadUtil;
            _tagUtil = tagUtil;
            _ratingUtil = ratingUtil;
            _cacheService = cacheService;
            _env = env;
            _searchProvider = searchProvider;
        }

        private int homepagesize => _appSettings.HomePageSize;
        private int userpagesize => _appSettings.UserPageSize;

        private List<int> featuredBlogId => _dataSettings.FeaturedBlogIdList();

        protected bool isHarmony => !User.Identity.IsAuthenticated && _appSettings.HarmonySettings.Harmony;

        public ActionResult Index(int page = 1)
        {
            int pagesize = homepagesize;
            var query = _db.Blogs.Where(b => b.isApproved == true);
            string categoryIds = string.Empty;
            bool hideHarmony = false;
            bool cachable = true;
            if (isHarmony)
            {
                query = query.Where(b => b.isHarmony);
            }
            else
            {
                var opt = _blogUtil.GetUserOption(User.Identity.Name, o => new { o.homepageCategories, o.homepageHideHarmony, o.homepageTagBlacklist });
                categoryIds = opt.homepageCategories;
                hideHarmony = opt.homepageHideHarmony;
                List<int> catIdList = new List<int>();
                List<int> tagBlacklist = new List<int>();
                if (!string.IsNullOrEmpty(categoryIds))
                {
                    catIdList = JsonConvert.DeserializeObject<List<int>>(categoryIds);
                }
                if (!string.IsNullOrEmpty(opt.homepageTagBlacklist))
                {
                    tagBlacklist = JsonConvert.DeserializeObject<List<int>>(opt.homepageTagBlacklist);
                    cachable = false;
                }
                if (catIdList.Count > 0)
                {
                    catIdList = _catUtil.GetCategoryWithSubcategories(catIdList);
                }
                var filter = new BlogFilter(_db)
                {
                    Whitelistcategories = catIdList,
                    Whitelistids = featuredBlogId,
                    Blacklisttags = tagBlacklist,
                    BlacklistCategories = _catUtil.GetCategoryList().Where(c => c.HideFromHomePage).Select(c => c.CategoryID),
                };
                query = filter.Filter(query);
                if (hideHarmony == true)
                {
                    query = query.Where(b => !b.isHarmony);
                }
            }

            if (featuredBlogId != null && featuredBlogId.Count > 0)
            {
                ViewBag.featuredBlog = featuredBlogId;
                query = query.OrderByDescending(b => featuredBlogId.Contains(b.BlogID)).ThenByDescending(b => b.BlogDate);
            }
            else
            {
                query = query.OrderByDescending(b => b.BlogDate);
            }

            int itemCount = -1, pageCount = -1, pageNumber = 1;
            ConcurrentDictionary<string, X.PagedList.IPagedList<BlogDisplay>> cache = null;
            if (cachable)
            {
                var firstPageKey = CacheService.GetHomePageListKey(1, isHarmony, hideHarmony, categoryIds);
                cache = _cache.Get<ConcurrentDictionary<string, X.PagedList.IPagedList<BlogDisplay>>>(CacheService.HomePageCacheKey)
                    ?? new ConcurrentDictionary<string, X.PagedList.IPagedList<BlogDisplay>>();
                if (cache.Count > 0 && cache.ContainsKey(firstPageKey))
                {
                    itemCount = cache[firstPageKey].TotalItemCount;
                    pageCount = cache[firstPageKey].PageCount;
                }
            }
            if (pageCount < 0)
            {
                itemCount = query.Count();
                pageCount = (int)Math.Ceiling(itemCount / (double)pagesize);
            }
            if (page > 0 && page <= pageCount)
            {
                pageNumber = page;
            }

            var cachekey = CacheService.GetHomePageListKey(pageNumber, isHarmony, hideHarmony, categoryIds);
            X.PagedList.IPagedList<BlogDisplay> model;
            if (cache == null || !cache.TryGetValue(cachekey, out model))
            {
                if (cache != null)
                {
                    _cache.Set(CacheService.HomePageCacheKey, cache, new MemoryCacheEntryOptions() { Priority = CacheItemPriority.NeverRemove });
                }
                
                // Get paged blogs first
                var skipCount = pageNumber == 1 ? 0 : (pageNumber - 1) * pagesize;
                var pagedBlogs = query.Skip(skipCount).Take(pagesize).ToList();
                var blogIds = pagedBlogs.Select(b => b.BlogID).ToList();
                
                // Load all tags for these blogs in a single query
                var harmonysettings = _appSettings.HarmonySettings;
                Dictionary<int, List<Tag>> tagsByBlogId;
                
                if (!isHarmony || harmonysettings.WhitelistTags == null)
                {
                    tagsByBlogId = _db.TagsInBlogs
                        .Where(t => blogIds.Contains(t.BlogID))
                        .Select(t => new { t.BlogID, t.tag })
                        .ToList()
                        .GroupBy(t => t.BlogID)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.tag).ToList());
                }
                else
                {
                    tagsByBlogId = _db.TagsInBlogs
                        .Where(t => blogIds.Contains(t.BlogID) && !harmonysettings.WhitelistTags.Contains(t.TagID))
                        .Select(t => new { t.BlogID, t.tag })
                        .ToList()
                        .GroupBy(t => t.BlogID)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.tag).ToList());
                }
                
                // Combine blogs with their tags
                var blogDisplays = pagedBlogs.Select(b => new BlogDisplay
                {
                    blog = b,
                    tag = tagsByBlogId.ContainsKey(b.BlogID) ? tagsByBlogId[b.BlogID] : new List<Tag>()
                }).ToList();
                
                model = new X.PagedList.StaticPagedList<BlogDisplay>(blogDisplays, pageNumber, pagesize, itemCount);
                
                if (cache != null)
                {
                    cache.TryAdd(cachekey, model);
                }
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView(model);
            }
            return View(model);
        }

        public async Task<ActionResult> About()
        {
            var moderators = await _userManager.GetUsersInRoleAsync("Moderator");
            var siteRules = await _db.Blogs.FindAsync(PostConstant.SiteRules);
            ViewBag.Moderators = moderators;
            ViewBag.Content = siteRules.Content;
            return View();
        }

        public ActionResult Donate()
        {
            return View();
        }

        public async Task<ActionResult> Favorite([FromServices]DbBlogSearchProvider searchProvider, SearchModel search, string name = "", int page = 1, string sort = "AddDate_desc")
        {
            if (string.IsNullOrEmpty(name))
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index");
                name = User.Identity.Name;
            }
            var user = await _userManager.FindByNameAsync(name);
            if (user == null)
            {
                return NotFound();
            }
            sort = sort.Split(':')[0];
            search.FavUser = name;
            if (isHarmony)
            {
                search.Harmony = true;
            }
            search.Sort = sort.StartsWith("AddDate") ? sort + ":" + name : sort;
            var result = await searchProvider.SearchBlogAsync(search, page, userpagesize);
            ViewBag.name = name;
            ViewBag.SearchModel = search;
            if (Request.IsAjaxRequest())
            {
                return PartialView(result);
            }
            return View(result);
        }

        public async Task<ActionResult> UserInfo(string name, string view, SearchModel search, int page = 1)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index");
                name = User.Identity.Name;
            }
            SearchBlogResult result = null;
            X.PagedList.IPagedList model;
            switch (view)
            {
                case "UserReply":
                    if (!User.Identity.IsAuthenticated)
                    {
                        return Unauthorized();
                    }
                    model = await UserReplyAsync(name, page);
                    break;

                case "UnApprove":
                    if (!User.Identity.IsAuthenticated)
                    {
                        return Unauthorized();
                    }
                    model = await UnApproveAsync(name, page);
                    break;
                case "UserBlog":
                default:
                    view = "UserBlog";
                    result = await UserBlogAsync(name, search, page);
                    model = result.Blogs;
                    break;
            }
            if (Request.IsAjaxRequest())
            {
                return PartialView(view + "Partial", model);
            }

            UserProfile p = await _udb.Users.Include(u => u.quest).Include(u => u.auditor).AsNoTracking().SingleOrDefaultAsync(u => u.UserName == name);
            if (p == null)
            {
                return NotFound();
            }
            UserInfoModel userInfo = new()
            {
                UserBlogs = await _db.Blogs.CountAsync(b => b.Author == name),
                UserPosts = await _db.Posts.CountAsync(b => b.Author == p.UserName),
                UserFans = await _udb.Follows.CountAsync(f => f.FollowID == p.Id),
                UserFavorites = await _db.Favorites.CountAsync(f => f.Username == p.UserName),
                UserFollows = await _udb.Follows.CountAsync(f => f.UserID == p.Id),
                UserRoles = await _userManager.GetRolesAsync(p),
                SearchResult = result,
                UserView = view,
                SubModel = model,
                Profile = p,
                UserBackground = p.quest?.PersonalBackground,
            };
            return View(userInfo);
        }

        private async Task<SearchBlogResult> UserBlogAsync(string name, SearchModel search, int page)
        {
            search.Author = name;
            if (isHarmony)
            {
                search.Harmony = true;
            }
            var result = await _searchProvider.SearchBlogAsync(search, page, userpagesize);
            ViewBag.SearchModel = search;
            ViewBag.UserName = name;
            return result;
        }

        private async Task<X.PagedList.IPagedList<UserReply>> UserReplyAsync(string name, int page = 1)
        {
            IQueryable<Post> PostQuery = _db.Posts.Where(p => p.Author == name && p.ItemId > 0);
            IQueryable<Reply> ReplyQuery = _db.Replies.Include(r => r.post).Where(r => r.Author == name && r.post.ItemId > 0);
            ViewBag.UserName = name;
            var query = PostQuery.Select(p => new UserReply
            {
                ItemID = p.ItemId,
                IdType = p.IdType,
                ReplyDate = p.PostDate,
                ReplyID = p.PostId,
                IsPost = true,
                Author = p.Author,
                Content = p.Content,
                ItemTitle = p.IdType == ItemType.Blog ? _db.Blogs.FirstOrDefault(b => b.BlogID == p.ItemId).BlogTitle :
                            p.IdType == ItemType.Topic ? _db.Topics.FirstOrDefault(t => t.TopicID == p.ItemId).TopicTitle : ""
            }).Union(ReplyQuery.Select(r => new UserReply
            {
                ItemID = r.post.ItemId,
                IdType = r.post.IdType,
                ReplyDate = r.ReplyDate,
                ReplyID = r.ReplyId,
                IsPost = false,
                Author = r.Author,
                Content = r.Content,
                ItemTitle = r.post.IdType == ItemType.Blog ? _db.Blogs.FirstOrDefault(b => b.BlogID == r.post.ItemId).BlogTitle :
                            r.post.IdType == ItemType.Topic ? _db.Topics.FirstOrDefault(t => t.TopicID == r.post.ItemId).TopicTitle : ""
            })).OrderByDescending(r => r.ReplyDate);
            int pagesize = userpagesize * 2;
            return await query.ToPagedListAsync(page, pagesize);
        }

        private async Task<X.PagedList.IPagedList<Blog>> UnApproveAsync(string name, int page = 1)
        {
            int pagesize = userpagesize;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = User.Identity.Name;
            }
            ViewBag.UserName = name;
            var query = _db.Blogs.Where(b => b.Author == name && (b.isApproved == false || b.isApproved == null)).OrderByDescending(b => b.BlogDate);
            return await query.ToPagedListAsync(page, pagesize);
        }

        public ActionResult Suggestions(string pos = null)
        {
            var blog = _db.Blogs.Find(0);
            ViewBag.pos = pos;
            ViewBag.versiondata = blog.Content;
            ViewBag.versiontitle = blog.Links;
            return View();
        }

        public ActionResult Msg(string name)
        {
            if (string.IsNullOrEmpty(name))
                return RedirectToAction("Index", "Home");
            else
            {
                TempData["WriteTo"] = name;
                TempData["DisplayTab"] = "write";
                return RedirectToAction("Index", "Message");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public RedirectToActionResult PunchIn()
        {
            return RedirectToActionPermanent("Do", "PunchIn", new { Area = "App" });
        }

        public async Task<ViewResult> UserRanking()
        {
            string cachename = "userranking";
            if (User.Identity.IsAuthenticated)
            {
                cachename += User.Identity.Name;
            }
            UserRanking model = _cache.Get<UserRanking>(cachename);
            if (model == null)
            {
                model = new UserRanking();
                
                string currentUser = User.Identity.IsAuthenticated ? User.Identity.Name : null;

                // Experience ranking - top 10: only 10 rows projected (Name+Value) from SQL, index added client-side
                var expData = await _udb.Users.AsNoTracking()
                    .OrderByDescending(u => u.Experience)
                    .Select(u => new { u.UserName, Value = u.Experience })
                    .Take(10)
                    .ToListAsync();
                model.Exp = expData.Select((u, index) => new RankTuple
                {
                    Name = u.UserName,
                    Value = u.Value,
                    Ranking = index + 1
                }).ToList();

                // Consecutive sign ranking - top 10: only 10 rows from SQL
                var signData = await _udb.Users.AsNoTracking()
                    .OrderByDescending(u => u.ConsecutiveSign)
                    .Select(u => new { u.UserName, Value = u.ConsecutiveSign })
                    .Take(10)
                    .ToListAsync();
                model.Sign = signData.Select((u, index) => new RankTuple
                {
                    Name = u.UserName,
                    Value = u.Value,
                    Ranking = index + 1
                }).ToList();

                // Blog count ranking - top 10
                var blogData = await _db.Blogs
                    .GroupBy(b => b.Author)
                    .Select(g => new { Name = g.Key, Value = g.Count() })
                    .OrderByDescending(g => g.Value)
                    .Take(10)
                    .ToListAsync();
                model.Blog = blogData.Select((b, index) => new RankTuple
                {
                    Name = b.Name,
                    Value = b.Value,
                    Ranking = index + 1
                }).ToList();

                // Post + Reply count ranking - top 10
                var postCounts = _db.Posts
                    .GroupBy(p => p.Author)
                    .Select(g => new { Name = g.Key, Count = g.Count() });
                var replyCounts = _db.Replies
                    .GroupBy(r => r.Author)
                    .Select(g => new { Name = g.Key, Count = g.Count() });
                var postData = await postCounts
                    .Concat(replyCounts)
                    .GroupBy(x => x.Name)
                    .Select(g => new { Name = g.Key, Value = g.Sum(x => x.Count) })
                    .OrderByDescending(x => x.Value)
                    .Take(10)
                    .ToListAsync();
                model.Post = postData.Select((p, index) => new RankTuple
                {
                    Name = p.Name,
                    Value = p.Value,
                    Ranking = index + 1
                }).ToList();

                if (User.Identity.IsAuthenticated)
                {
                    // Get current user's actual rankings
                    var user = await _udb.Users.FirstOrDefaultAsync(u => u.UserName == currentUser);
                    if (user != null)
                    {
                        var expRank = await _udb.Users.CountAsync(u => u.Experience > user.Experience) + 1;
                        model.MyExp = new Tuple<int, long>(user.Experience, expRank);

                        var signRank = await _udb.Users.CountAsync(u => u.ConsecutiveSign > user.ConsecutiveSign) + 1;
                        model.MySign = new Tuple<int, long>(user.ConsecutiveSign, signRank);

                        var blogCount = await _db.Blogs.CountAsync(b => b.Author == currentUser);
                        var blogRank = await _db.Blogs.GroupBy(b => b.Author)
                            .Select(g => g.Count())
                            .CountAsync(c => c > blogCount) + 1;
                        model.MyBlog = new Tuple<int, long>(blogCount, blogRank);

                        var postCount = await _db.Posts.CountAsync(p => p.Author == currentUser);
                        var replyCount = await _db.Replies.CountAsync(r => r.Author == currentUser);
                        var totalCount = postCount + replyCount;
                        var postCounts2 = _db.Posts.GroupBy(p => p.Author).Select(g => new { Name = g.Key, Count = g.Count() });
                        var replyCounts2 = _db.Replies.GroupBy(r => r.Author).Select(g => new { Name = g.Key, Count = g.Count() });
                        var combinedCounts = postCounts2.Concat(replyCounts2)
                            .GroupBy(x => x.Name)
                            .Select(g => g.Sum(x => x.Count));
                        var postRank = await combinedCounts.CountAsync(c => c > totalCount) + 1;
                        model.MyPost = new Tuple<int, long>(totalCount, postRank);
                    }
                }
                model.rankdate = DateTime.Now;
                _cache.Set(cachename, model, new TimeSpan(1, 0, 0));
            }
            return View(model);
        }

        [Authorize]
        public PartialViewResult Quest()
        {
            return PartialView("QuestPartial");
        }

        [HttpPost, Authorize]
        public JsonResult Mentions(string typed = "")
        {
            typed = typed.Trim();
            object users = new { };
            if (typed.StartsWith("@"))
            {
                typed = typed.Substring(1);
            }
            if (typed.Length >= 2)
            {
                users = _udb.Users.Where(u => u.UserName.StartsWith(typed) || u.NickName.StartsWith(typed))
                    .OrderByDescending(u => u.NickName.StartsWith(typed)).ThenBy(u => u.NickName).Take(5)
                    .Select(u => new { username = u.UserName, nickname = u.NickName }).ToList();
            }
            return Json(users);
        }

        public PartialViewResult Background([FromServices]IOptionsSnapshot<BackgroundSetting> settings)
        {
            return PartialView("BackgroundPartial", settings.Value);
        }

        // Blazor migration: deep links from main site point to /app/* when BlazorApp.Enabled=true
        private static readonly HashSet<string> DefaultBlazorPrefixes = new(StringComparer.OrdinalIgnoreCase)
        {
            "title-helper", "punch-in", "message", "raffle", "admin", "login", "account", "audit-exam", "title-categories"
        };

        public IActionResult App(
            [FromServices] ConstantUtil constantUtil,
            [FromServices] IOptions<SiteConfig> siteConfig,
            string path)
        {
            path = (path ?? "").TrimStart('/');

            // Flip switch: null BlazorApp section = disabled (safe opt-in). Must add Enabled:true to activate.
            var blazorCfg = siteConfig.Value.BlazorApp;
            bool globalEnabled = blazorCfg?.Enabled ?? false;

            var httpCtx = constantUtil.HttpContextAccessor()?.HttpContext;
            var blazorQuery = httpCtx?.Request.Query["blazor"].ToString();
            if (blazorQuery == "0" || blazorQuery == "false") globalEnabled = false;
            else if (blazorQuery == "1" || blazorQuery == "true") globalEnabled = true;

            if (!globalEnabled)
            {
                return Redirect(new Uri(constantUtil.AppHost + path).ToString());
            }

            var firstSegment = path.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

            // per-prefix override: { "bounty": false } keeps route on old Angular host
            if (blazorCfg.RedirectOverrides != null && blazorCfg.RedirectOverrides.TryGetValue(firstSegment, out var overrideVal) && !overrideVal)
            {
                return Redirect(new Uri(constantUtil.AppHost + path).ToString());
            }

            // default migrated prefixes + extra prefixes from config
            bool isBlazorRoute = DefaultBlazorPrefixes.Contains(firstSegment) ||
                (blazorCfg != null && blazorCfg.ExtraPrefixes != null && blazorCfg.ExtraPrefixes.Contains(firstSegment, StringComparer.OrdinalIgnoreCase));

            if (isBlazorRoute)
            {
                return Redirect($"/app/{path}");
            }

            // fallback -> legacy Angular AppHost
            return Redirect(new Uri(constantUtil.AppHost + path).ToString());
        }
    }
}