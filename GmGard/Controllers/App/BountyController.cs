using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Extensions;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Bounty/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class BountyController : AppControllerBase
    {
        public static event EventHandler<Services.BountyEventArgs> OnCreateBounty;
        public static event EventHandler<Services.BountyEventArgs> OnAnswerBounty;
        public static event EventHandler<Services.BountyEventArgs> OnDeleteBounty;
        public static event EventHandler<Services.BountyEventArgs> OnAcceptBounty;

        private readonly BlogContext db_;
        private readonly UsersContext udb_;
        private readonly ExpUtil expUtil_;
        private readonly UserManager<UserProfile> userManager_;
        private readonly UploadUtil uploadUtil_;
        private readonly HtmlSanitizerService sanitizer_;
        private readonly BountyConfig cfg_;
        private readonly IVisitCounter visitCounter_;
        private readonly MessageUtil msgUtil_;
        private readonly BlogUtil blogUtil_;
        private readonly Services.ElasticSearchProvider? esProvider_;

        public BountyController(BlogContext db, UsersContext udb, ExpUtil expUtil, UserManager<UserProfile> userManager, UploadUtil uploadUtil, HtmlSanitizerService sanitizer, IOptions<BountyConfig> bountyConfig, IVisitCounter visitCounter, MessageUtil msgUtil, BlogUtil blogUtil, Services.ElasticSearchProvider? esProvider = null)
        {
            db_ = db;
            udb_ = udb;
            expUtil_ = expUtil;
            userManager_ = userManager;
            uploadUtil_ = uploadUtil;
            sanitizer_ = sanitizer;
            cfg_ = bountyConfig.Value;
            visitCounter_ = visitCounter;
            msgUtil_ = msgUtil;
            blogUtil_ = blogUtil;
            esProvider_ = esProvider;
        }

        private string AvatarBase => Url.Action("Show", "Avatar", null, Request.Scheme) + "/";

        [AllowAnonymous]
        public IActionResult List(int page = 1, BountyShowType showType = BountyShowType.All, bool onlyMine = false, bool includeDeleted = false)
        {
            IQueryable<Bounty> query = db_.Bounties.AsQueryable();
            var currentUser = User.Identity?.Name;

            // onlyMine filter composes with showType
            if (onlyMine)
            {
                if (!User.Identity.IsAuthenticated) return Unauthorized();
                query = query.Where(b => b.Author == currentUser);
            }

            if (showType == BountyShowType.Deleted)
            {
                if (!User.Identity.IsAuthenticated) return Unauthorized();
                if (!onlyMine)
                    query = query.Where(b => b.Author == currentUser);
                query = query.Where(b => b.IsDeleted);
            }
            else
            {
                // Handle deleted visibility
                if (!includeDeleted)
                {
                    query = query.Where(b => !b.IsDeleted);
                }
                else
                {
                    if (!User.Identity.IsAuthenticated) return Unauthorized();
                    if (!onlyMine)
                        query = query.Where(b => !b.IsDeleted || b.Author == currentUser);
                    // if onlyMine, keep all (including deleted) already filtered to owner
                }

                if (showType == BountyShowType.Answered)
                    query = query.Where(b => b.IsAccepted);
                else if (showType == BountyShowType.Pending)
                    query = query.Where(b => !b.IsAccepted && b.CloseDate == null);
                // All => no extra filter (includes expired archives, they show as 已过期/已结贴 via badge)
            }

            string avatarUrlBase = AvatarBase;
            var model = query.Select(q => new BountyPreview
            {
                Id = q.BountyId,
                Author = q.Author,
                AuthorAvatar = avatarUrlBase + q.Author,
                Content = q.Content.Length > 200 ? q.Content.Substring(0, 200) : q.Content,
                CreateDate = q.CreateDate,
                Prize = q.Prize,
                Title = q.Title,
                AnswerCount = q.Answers.Count,
                IsAccepted = q.IsAccepted,
                Deposit = q.Deposit,
                HelpfulReward = q.HelpfulReward,
                ViewCount = q.ViewCount,
                ExpiresAt = q.ExpiresAt,
                Image = q.ImageUrls,
                IsDeleted = q.IsDeleted,
                CloseDate = q.CloseDate,
            }).OrderByDescending(q => q.CreateDate);
            var pagedList = model.ToPagedList(page, cfg_.PageSize);
            try
            {
                var ids = pagedList.Select(b => b.Id).ToList();
                visitCounter_.PrepareBountyVisits(ids);
                foreach (var b in pagedList)
                    b.ViewCount = (int)visitCounter_.GetBountyVisit(b.Id);
            }
            catch { }
            var paged = new Paged<BountyPreview>(pagedList);
            return Json(paged);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetConfig()
        {
            return Json(cfg_);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var bounty = await db_.Bounties.Include(b => b.Answers).FirstOrDefaultAsync(b => b.BountyId == id);
            if (bounty == null) return NotFound();
            // If deleted, only author can view details
            if (bounty.IsDeleted)
            {
                if (!User.Identity.IsAuthenticated || User.Identity.Name != bounty.Author) return NotFound();
            }

            // Use VisitCounter batched cache (same as Blog/Topic) to avoid per-view DB write
            var cachedView = visitCounter_.GetBountyVisit(bounty.BountyId, increment: true);

            string avatarBase = AvatarBase;
            bool canAccept = false;
            bool canAnswer = false;
            if (User.Identity.IsAuthenticated)
            {
                bool isClosed = bounty.CloseDate.HasValue;
                bool isExpired = bounty.ExpiresAt.HasValue && bounty.ExpiresAt.Value < DateTime.Now;
                canAccept = !bounty.IsAccepted && !isClosed && !isExpired && User.Identity.Name == bounty.Author;
                canAnswer = !bounty.IsAccepted && !isClosed && !isExpired && User.Identity.Name != bounty.Author;
            }

            var detail = new BountyDetail
            {
                Id = bounty.BountyId,
                Title = bounty.Title,
                Content = bounty.Content,
                ImageUrls = ParseBountyImageUrls(bounty.ImageUrls),
                Author = bounty.Author,
                AuthorAvatar = avatarBase + bounty.Author,
                CreateDate = bounty.CreateDate,
                ExpiresAt = bounty.ExpiresAt,
                CloseDate = bounty.CloseDate,
                Prize = bounty.Prize,
                Deposit = bounty.Deposit,
                HelpfulReward = bounty.HelpfulReward,
                IsAccepted = bounty.IsAccepted,
                ViewCount = (int)cachedView,
                BestAnswerId = bounty.AcceptedAnswerId,
                AnswerCount = bounty.Answers?.Count ?? 0,
                CanAccept = canAccept,
                CanAnswer = canAnswer,
                Answers = new List<AnswerDto>(), // placeholder filled below
            };

            // Load Answer Replies via Post system (Option 2): Post.IdType=Answer, ItemId=AnswerId, plus nested Reply
            var answerIds = bounty.Answers?.Select(a => a.AnswerId).ToList() ?? new List<int>();
            var postsForAnswers = answerIds.Any()
                ? await db_.Posts.Where(p => p.IdType == ItemType.Answer && answerIds.Contains(p.ItemId))
                    .Include(p => p.Replies).OrderBy(p => p.PostDate).ToListAsync()
                : new List<Post>();

            var postsLookup = postsForAnswers.GroupBy(p => p.ItemId).ToDictionary(g => g.Key, g => g.ToList());

            detail.Answers = bounty.Answers?.OrderBy(a => a.AnswerId == bounty.AcceptedAnswerId ? 0 : a.IsHelpful ? 1 : 2).ThenBy(a => a.CreateDate).Select(a =>
            {
                postsLookup.TryGetValue(a.AnswerId, out var posts);
                var ansImgs = ParseBountyImageUrls(a.ImageUrl);
                var ansImgJsonOrFirst = a.ImageUrl;
                return new AnswerDto
                {
                    AnswerId = a.AnswerId,
                    BountyId = a.BountyId,
                    Author = a.Author,
                    AuthorAvatar = avatarBase + a.Author,
                    Content = a.Content ?? "",
                    ImageUrl = ansImgJsonOrFirst ?? "",
                    CreateDate = a.CreateDate,
                    IsBest = a.AnswerId == bounty.AcceptedAnswerId,
                    IsHelpful = a.IsHelpful,
                    Replies = posts?.Select(p => new PostDto
                    {
                        PostId = p.PostId,
                        Author = p.Author,
                        AuthorAvatar = avatarBase + p.Author,
                        Content = p.Content,
                        CreateDate = p.PostDate,
                        Replies = p.Replies?.OrderBy(r => r.ReplyDate).Select(r => new ReplyDto
                        {
                            ReplyId = r.ReplyId,
                            Author = r.Author,
                            AuthorAvatar = avatarBase + r.Author,
                            Content = r.Content,
                            CreateDate = r.ReplyDate
                        }).ToList() ?? new List<ReplyDto>()
                    }).ToList() ?? new List<PostDto>()
                };
            }).ToList() ?? new List<AnswerDto>();

            // Bounty-level comments: Post IdType=Bounty, allowed even after closed/expired
            var bountyPosts = await db_.Posts
                .Where(p => p.IdType == ItemType.Bounty && p.ItemId == bounty.BountyId)
                .Include(p => p.Replies)
                .OrderBy(p => p.PostDate)
                .ToListAsync();
            detail.BountyComments = bountyPosts.Select(p => new PostDto
            {
                PostId = p.PostId,
                Author = p.Author,
                AuthorAvatar = avatarBase + p.Author,
                Content = p.Content,
                CreateDate = p.PostDate,
                Replies = p.Replies?.OrderBy(r => r.ReplyDate).Select(r => new ReplyDto
                {
                    ReplyId = r.ReplyId,
                    Author = r.Author,
                    AuthorAvatar = avatarBase + r.Author,
                    Content = r.Content,
                    CreateDate = r.ReplyDate
                }).ToList() ?? new List<ReplyDto>()
            }).ToList();

            return Json(detail);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            // Support both application/json and multipart/form-data in same endpoint
            CreateBountyRequest req;
            List<string> uploadedUrls = new();

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                req = new CreateBountyRequest
                {
                    Title = form["Title"].ToString(),
                    Content = form["Content"].ToString(),
                    ImageUrls = form["ImageUrls"].ToString(),
                    Prize = int.TryParse(form["Prize"], out var p) ? p : 0,
                    HelpfulReward = int.TryParse(form["HelpfulReward"], out var h) ? h : 0
                };

                // Handle files: field name "Files" or "images"
                var files = form.Files;
                if (files != null && files.Count > 0)
                {
                    // Limit: max 9 images, each <10MB, image/* only
                    if (files.Count > 9) return BadRequest(new { error = "最多9张图片" });
                    foreach (var f in files)
                    {
                        if (f.Length > 10 * 1024 * 1024) return BadRequest(new { error = $"图片 {f.FileName} 超过10MB" });
                        if (!f.ContentType.StartsWith("image/")) return BadRequest(new { error = $"文件 {f.FileName} 不是图片" });
                    }
                    uploadedUrls = await uploadUtil_.SaveImagesAsync(files, false);
                    if (uploadedUrls.Count == 0 && files.Count > 0) return StatusCode(500, new { error = "图片上传失败" });
                }
            }
            else
            {
                // JSON
                req = await System.Text.Json.JsonSerializer.DeserializeAsync<CreateBountyRequest>(Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (req == null) return BadRequest(new { error = "请求体为空" });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (req.Title?.Length > cfg_.MaxTitleLength) return BadRequest(new { error = $"标题最多{cfg_.MaxTitleLength}字" });
            if (string.IsNullOrWhiteSpace(req.Content) || req.Content.Trim().Length < cfg_.MinContentLength) return BadRequest(new { error = $"内容至少{cfg_.MinContentLength}字" });
            if (req.Content.Length > cfg_.MaxContentLength) return BadRequest(new { error = $"内容最多{cfg_.MaxContentLength}字" });
            if (req.Prize < cfg_.MinPrize) return BadRequest(new { error = $"最佳答案奖励至少{cfg_.MinPrize}" });
            if (req.HelpfulReward < 0) req.HelpfulReward = 0;
            if (!cfg_.AllowZeroHelpfulReward && req.HelpfulReward == 0) return BadRequest(new { error = $"必须设置热心助人奖励至少{cfg_.MinHelpfulReward}" });
            if (req.HelpfulReward != 0 && req.HelpfulReward < cfg_.MinHelpfulReward)
                return BadRequest(new { error = $"热心助人奖励至少{cfg_.MinHelpfulReward}，不需要可设0" });

            int deposit = req.Prize;
            int totalCost = req.Prize + deposit + req.HelpfulReward;
            if (totalCost < cfg_.MinTotal)
                return BadRequest(new { error = $"总计至少{cfg_.MinTotal}棒棒糖(最佳{req.Prize}+押金{deposit}+热心助人{req.HelpfulReward}={totalCost})" });

            var user = await udb_.Users.SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user == null) return Unauthorized();
            if (user.Points < totalCost)
                return BadRequest(new { error = "棒棒糖不足", current = user.Points, need = totalCost });

            expUtil_.AddPoint(user, -totalCost);
            await udb_.SaveChangesAsync();

            // Merge uploaded + provided urls - JSON list storage (fixes ";" in URLs like 1743)
            var existingList = ParseBountyImageUrls(req.ImageUrls ?? "");
            var allUrls = existingList.Concat(uploadedUrls).Where(s=>!string.IsNullOrWhiteSpace(s)).Distinct().Take(9).ToList();
            string finalImageUrls = System.Text.Json.JsonSerializer.Serialize(allUrls);

            // Sanitize HTML content (like BlogController does) - allow smiley/img etc via sanitizer config
            var sanitizedContent = sanitizer_.Sanitize(req.Content.Trim());
            // Keep title plain text - strip tags
            var sanitizedTitle = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Replace(req.Title.Trim(), "<.*?>", "")).Trim();

            var bounty = new Bounty
            {
                Title = sanitizedTitle,
                Content = sanitizedContent,
                ImageUrls = finalImageUrls,
                Author = User.Identity.Name,
                CreateDate = DateTime.Now,
                IsDeleted = false,
                Prize = req.Prize,
                Deposit = deposit,
                HelpfulReward = req.HelpfulReward,
                IsAccepted = false,
                ViewCount = 0,
                ExpiresAt = DateTime.Now.AddDays(cfg_.ExpireDays),
                Answers = new List<Answer>(),
            };
            db_.Bounties.Add(bounty);
            await db_.SaveChangesAsync();

            try { OnCreateBounty?.Invoke(this, new Services.BountyEventArgs { BountyId = bounty.BountyId, Bounty = bounty }); } catch { }
            return Json(new { id = bounty.BountyId, totalCost, remaining = user.Points, images = finalImageUrls });
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string q, int page = 1, BountyShowType showType = BountyShowType.All, bool onlyMine = false, bool includeDeleted = false)
        {
            if (string.IsNullOrWhiteSpace(q)) return await Task.FromResult(List(page, showType, onlyMine, includeDeleted));

            q = q.Trim();
            if (q.Length > 100) q = q.Substring(0, 100);

            IQueryable<Bounty> query = db_.Bounties.AsQueryable();
            var currentUser = User.Identity?.Name;
            bool isAuth = User.Identity?.IsAuthenticated == true;

            // same filters as List
            if (onlyMine)
            {
                if (!isAuth) return Unauthorized();
                query = query.Where(b => b.Author == currentUser);
            }
            if (showType == BountyShowType.Deleted)
            {
                if (!isAuth) return Unauthorized();
                if (!onlyMine) query = query.Where(b => b.Author == currentUser);
                query = query.Where(b => b.IsDeleted);
            }
            else
            {
                if (!includeDeleted) query = query.Where(b => !b.IsDeleted);
                else
                {
                    if (!isAuth) return Unauthorized();
                    if (!onlyMine) query = query.Where(b => !b.IsDeleted || b.Author == currentUser);
                }
                if (showType == BountyShowType.Answered) query = query.Where(b => b.IsAccepted);
                else if (showType == BountyShowType.Pending) query = query.Where(b => !b.IsAccepted && b.CloseDate == null);
            }

            int pageSize = cfg_.PageSize;
            List<int> orderedIds = null;

            // ES if available
            if (esProvider_ != null && esProvider_.IsValid())
            {
                try
                {
                    var esResult = await esProvider_.SearchBountyAsync(q, 1, 1000);
                    if (esResult.Ids.Any())
                    {
                        orderedIds = esResult.Ids;
                        query = query.Where(b => orderedIds.Contains(b.BountyId));
                    }
                    else
                    {
                        // ES returned no hits - return empty paged
                        var empty = new X.PagedList.StaticPagedList<BountyPreview>(new List<BountyPreview>(), page, pageSize, 0);
                        return Json(new Paged<BountyPreview>(empty));
                    }
                }
                catch { /* fallback to DB LIKE below */ }
            }

            if (orderedIds == null)
            {
                // DB fallback: title/content/answers LIKE
                var qLower = q.ToLower();
                query = query.Where(b => b.Title.Contains(q) || b.Content.Contains(q) || b.Answers.Any(a => a.Content.Contains(q)));
            }

            string avatarUrlBase = AvatarBase;
            var proj = query.Select(b => new BountyPreview
            {
                Id = b.BountyId,
                Author = b.Author,
                AuthorAvatar = avatarUrlBase + b.Author,
                Content = b.Content.Length > 200 ? b.Content.Substring(0, 200) : b.Content,
                CreateDate = b.CreateDate,
                Prize = b.Prize,
                Title = b.Title,
                AnswerCount = b.Answers.Count,
                IsAccepted = b.IsAccepted,
                Deposit = b.Deposit,
                HelpfulReward = b.HelpfulReward,
                ViewCount = b.ViewCount,
                ExpiresAt = b.ExpiresAt,
                Image = b.ImageUrls,
                IsDeleted = b.IsDeleted,
                CloseDate = b.CloseDate,
            });

            // manual ToPagedList via static list to avoid extension missing in this version
            X.PagedList.IPagedList<BountyPreview> pagedList;
            if (orderedIds != null)
            {
                var list = await proj.ToListAsync();
                var ordered = orderedIds.Where(id => list.Any(x => x.Id == id)).Select(id => list.First(x => x.Id == id)).ToList();
                var remaining = list.Where(x => !orderedIds.Contains(x.Id)).ToList();
                var allOrdered = ordered.Concat(remaining).ToList();
                pagedList = new X.PagedList.StaticPagedList<BountyPreview>(allOrdered.Skip((page - 1) * pageSize).Take(pageSize), page, pageSize, allOrdered.Count);
            }
            else
            {
                var total = await proj.CountAsync();
                var items = await proj.OrderByDescending(b => b.CreateDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
                pagedList = new X.PagedList.StaticPagedList<BountyPreview>(items, page, pageSize, total);
            }

            try
            {
                var ids = pagedList.Select(b => b.Id).ToList();
                visitCounter_.PrepareBountyVisits(ids);
                foreach (var b in pagedList) b.ViewCount = (int)visitCounter_.GetBountyVisit(b.Id);
            }
            catch { }

            return Json(new Paged<BountyPreview>(pagedList));
        }

        // Dedicated upload endpoint for progressive upload (also usable standalone)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UploadImages()
        {
            if (!Request.HasFormContentType) return BadRequest(new { error = "需要 multipart/form-data" });
            var form = await Request.ReadFormAsync();
            var files = form.Files;
            if (files == null || files.Count == 0) return BadRequest(new { error = "未收到文件" });
            if (files.Count > 9) return BadRequest(new { error = "最多9张" });
            foreach (var f in files)
            {
                if (f.Length > 10 * 1024 * 1024) return BadRequest(new { error = $"文件 {f.FileName} 过大" });
                if (!f.ContentType.StartsWith("image/")) return BadRequest(new { error = "仅支持图片" });
            }
            var urls = await uploadUtil_.SaveImagesAsync(files, false);
            return Json(new { urls });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Answer()
        {
            CreateAnswerRequest req;
            string uploadedUrl = null;
            List<string> uploadedUrls = new();

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                req = new CreateAnswerRequest
                {
                    BountyId = int.TryParse(form["BountyId"], out var bid) ? bid : 0,
                    Content = form["Content"].ToString(),
                    ImageUrl = form["ImageUrl"].ToString()
                };
                var files = form.Files;
                if (files != null && files.Count > 0)
                {
                    if (files.Count > 5) return BadRequest(new { error = "回答最多5张图" });
                    foreach (var f in files)
                    {
                        if (f.Length > 10 * 1024 * 1024) return BadRequest(new { error = "图片过大" });
                        if (!f.ContentType.StartsWith("image/")) return BadRequest(new { error = "仅支持图片" });
                    }
                    uploadedUrls = await uploadUtil_.SaveImagesAsync(files, false);
                    if (uploadedUrls.Any()) uploadedUrl = uploadedUrls.First();
                }
            }
            else
            {
                req = await System.Text.Json.JsonSerializer.DeserializeAsync<CreateAnswerRequest>(Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (req == null) return BadRequest(new { error = "请求体为空" });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(req.Content)) return BadRequest(new { error = "请输入回答内容" });

            var bounty = await db_.Bounties.Include(b => b.Answers).FirstOrDefaultAsync(b => b.BountyId == req.BountyId && !b.IsDeleted);
            if (bounty == null) return NotFound();
            if (bounty.IsAccepted) return BadRequest(new { error = "求物已结贴，不能再回答" });
            if (bounty.ExpiresAt.HasValue && bounty.ExpiresAt.Value < DateTime.Now) return BadRequest(new { error = "求物已过期" });
            if (bounty.Author == User.Identity.Name) return BadRequest(new { error = "不能给自己的求物回答" });

            // Store as JSON array for robustness (semicolon in URL issue like bounty 1743)
            string finalImg;
            if (uploadedUrls.Any())
                finalImg = System.Text.Json.JsonSerializer.Serialize(uploadedUrls);
            else if (!string.IsNullOrWhiteSpace(req.ImageUrl))
            {
                // req.ImageUrl might be legacy ";" or json – normalize to json
                var parsed = ParseBountyImageUrls(req.ImageUrl);
                finalImg = System.Text.Json.JsonSerializer.Serialize(parsed);
            }
            else finalImg = "";

            var combined = req.Content?.Trim() ?? "";
            var sanitizedAns = sanitizer_.Sanitize(combined);

            var ans = new Answer
            {
                BountyId = bounty.BountyId,
                Author = User.Identity.Name,
                Content = sanitizedAns,
                ImageUrl = finalImg,
                CreateDate = DateTime.Now,
                IsHelpful = false,
            };
            // Parse mentions before save (rewrites /User/... and collects usernames)
            var mentionHandler = new MentionHandler(udb_);
            ans.Content = mentionHandler.ParseMentions(ans.Content);

            db_.Answers.Add(ans);
            await db_.SaveChangesAsync();

            // Notifications (non-blocking, best-effort)
            try
            {
                var url = Url.Action("App", "Home", new { path = $"bounty/{bounty.BountyId}" }) + $"#postcontent{ans.AnswerId}";
                var title = bounty.Title;

                // 1) Notify bounty author about new answer (if option enabled and not self – self already blocked)
                var bountyOwner = await udb_.Users.Include(u => u.option).FirstOrDefaultAsync(u => u.UserName == bounty.Author);
                if (bountyOwner != null)
                {
                    var opt = bountyOwner.option;
                    if (opt == null || opt.sendNoticeForNewPostReply)
                    {
                        // Reuse NewReply notice type – shows as "有人回复了你的XXX"
                        msgUtil_.SendNewReplyNotice(bountyOwner.UserName, ans.Author, title, url);
                    }
                }

                // 2) Mention notices
                if (mentionHandler.HasMentions())
                {
                    mentionHandler.SendMentionMsg(msgUtil_, ans.Author, title, url);
                }
            }
            catch { /* ignore notification failures */ }

            try { OnAnswerBounty?.Invoke(this, new Services.BountyEventArgs { BountyId = bounty.BountyId }); } catch { }

            return Json(new { answerId = ans.AnswerId, imageUrl = finalImg });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Accept([FromBody] AcceptRequest req)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var bounty = await db_.Bounties.Include(b => b.Answers).FirstOrDefaultAsync(b => b.BountyId == req.BountyId && !b.IsDeleted);
            if (bounty == null) return NotFound();
            if (bounty.IsAccepted) return BadRequest(new { error = "已经结贴" });
            if (bounty.Author != User.Identity.Name) return Forbid();

            var best = bounty.Answers.FirstOrDefault(a => a.AnswerId == req.BestAnswerId);
            if (best == null) return BadRequest(new { error = "最佳答案不存在" });
            if (best.Author == bounty.Author) return BadRequest(new { error = "不能选择自己的回答" });
            if (best.BountyId != bounty.BountyId) return BadRequest(new { error = "答案不属于此悬赏" });

            var helpfulIds = req.HelpfulAnswerIds?.Distinct().Where(id => id != req.BestAnswerId).ToArray() ?? Array.Empty<int>();
            var rawHelpful = new List<Answer>();
            foreach (var hid in helpfulIds)
            {
                var ha = bounty.Answers.FirstOrDefault(a => a.AnswerId == hid);
                if (ha == null) return BadRequest(new { error = $"热心助人答案 {hid} 不存在" });
                if (ha.Author == bounty.Author) return BadRequest(new { error = "热心助人答案不能是楼主自己的" });
                if (ha.Author == best.Author) return BadRequest(new { error = "热心助人不能包含最佳答案作者（已得最佳）" });
                rawHelpful.Add(ha);
            }
            // Unique by author – one share per distinct user, keep earliest answer per author
            var helpfulAnswers = rawHelpful
                .GroupBy(a => a.Author)
                .Select(g => g.OrderBy(a => a.CreateDate).First())
                .ToList();

            // No self-claim check: best author != OP already checked, helpful authors also checked above but also ensure not duplicate same alt? Can't check IP here.
            // Distribution using transaction across both contexts - need to be careful.
            // We'll use explicit transactions on both contexts sequentially.

            using var blogTx = await db_.Database.BeginTransactionAsync();
            using var userTx = await udb_.Database.BeginTransactionAsync();

            try
            {
                // Update bounty
                bounty.AcceptedAnswerId = best.AnswerId;
                bounty.IsAccepted = true;
                bounty.CloseDate = DateTime.Now;
                best.IsHelpful = false; // best not marked helpful
                foreach (var ha in helpfulAnswers) ha.IsHelpful = true;

                await db_.SaveChangesAsync();

                // Points distribution
                // Deposit refund to OP
                var opUser = await udb_.Users.SingleOrDefaultAsync(u => u.UserName == bounty.Author);
                if (opUser != null)
                {
                    expUtil_.AddPoint(opUser, bounty.Deposit);
                    // If no helpful, refund helpfulReward to OP too
                    if (helpfulAnswers.Count == 0 && bounty.HelpfulReward > 0)
                    {
                        expUtil_.AddPoint(opUser, bounty.HelpfulReward);
                    }
                }

                // Best answer author gets Prize
                var bestUser = await udb_.Users.SingleOrDefaultAsync(u => u.UserName == best.Author);
                if (bestUser != null)
                {
                    expUtil_.AddPoint(bestUser, bounty.Prize);
                }

                // Helpful split
                if (helpfulAnswers.Count > 0 && bounty.HelpfulReward > 0)
                {
                    int per = bounty.HelpfulReward / helpfulAnswers.Count;
                    int remainder = bounty.HelpfulReward % helpfulAnswers.Count;
                    foreach (var ha in helpfulAnswers)
                    {
                        var hu = await udb_.Users.SingleOrDefaultAsync(u => u.UserName == ha.Author);
                        if (hu != null)
                        {
                            expUtil_.AddPoint(hu, per);
                        }
                    }
                    // remainder goes to OP or best? give to OP
                    if (remainder > 0 && opUser != null)
                    {
                        expUtil_.AddPoint(opUser, remainder);
                    }
                }

                await udb_.SaveChangesAsync();
                await userTx.CommitAsync();
                await blogTx.CommitAsync();

                // Notifications: best + helpful
                try
                {
                    var url = Url.Action("App", "Home", new { path = $"bounty/{bounty.BountyId}" }) + $"#postcontent{best.AnswerId}";
                    var title = bounty.Title;
                    // Best
                    msgUtil_.SendBountyAcceptedNotice(best.Author, User.Identity.Name, title, url);
                    // Helpful
                    foreach (var ha in helpfulAnswers)
                    {
                        var hUrl = Url.Action("App", "Home", new { path = $"bounty/{bounty.BountyId}" }) + $"#postcontent{ha.AnswerId}";
                        msgUtil_.SendBountyHelpfulNotice(ha.Author, User.Identity.Name, title, hUrl);
                    }
                }
                catch { }
                try { OnAcceptBounty?.Invoke(this, new Services.BountyEventArgs { BountyId = bounty.BountyId }); } catch { }
            }
            catch (Exception ex)
            {
                await userTx.RollbackAsync();
                await blogTx.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }

            return Json(new { success = true, bestAnswerId = best.AnswerId, helpfulCount = helpfulAnswers.Count });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var bounty = await db_.Bounties.FirstOrDefaultAsync(b => b.BountyId == id && !b.IsDeleted);
            if (bounty == null) return NotFound();
            if (bounty.Author != User.Identity.Name) return Forbid();
            if (bounty.IsAccepted) return BadRequest(new { error = "已结贴不能删除" });
            if (bounty.Answers != null && bounty.Answers.Any()) return BadRequest(new { error = "已有回答不能删除" });

            // refund total
            using var userTx = await udb_.Database.BeginTransactionAsync();
            try
            {
                var user = await udb_.Users.SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user != null)
                {
                    int total = bounty.Prize + bounty.Deposit + bounty.HelpfulReward;
                    expUtil_.AddPoint(user, total);
                    await udb_.SaveChangesAsync();
                }
                bounty.IsDeleted = true;
                await db_.SaveChangesAsync();
                await userTx.CommitAsync();
                try { OnDeleteBounty?.Invoke(this, new Services.BountyEventArgs { BountyId = bounty.BountyId }); } catch { }
            }
            catch (Exception ex)
            {
                await userTx.RollbackAsync();
                return StatusCode(500, new { error = ex.Message });
            }
            return Json(new { success = true });
        }

        // Bounty-level comments (Post IdType=Bounty) – allowed even after bounty closed/expired
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CommentBounty()
        {
            var req = await System.Text.Json.JsonSerializer.DeserializeAsync<BountyCommentRequest>(Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (req == null || req.BountyId == 0 || string.IsNullOrWhiteSpace(req.Content)) return BadRequest(new { error = "内容不能为空" });
            var bounty = await db_.Bounties.FirstOrDefaultAsync(b => b.BountyId == req.BountyId);
            if (bounty == null) return NotFound();
            if (bounty.IsDeleted) return BadRequest(new { error = "悬赏已删除" });
            // Allowed even if IsAccepted / CloseDate != null / Expired – that's the feature

            var mention = new MentionHandler(udb_);
            var raw = sanitizer_.Sanitize(req.Content.Trim());
            if (raw.Length < 2) return BadRequest(new { error = "内容太短" });
            if (raw.Length > 5000) return BadRequest(new { error = "内容太长" });
            var parsed = mention.ParseMentions(raw);

            var post = new Post
            {
                IdType = ItemType.Bounty,
                ItemId = req.BountyId,
                Author = User.Identity.Name,
                Content = parsed,
                PostDate = DateTime.Now,
                Rating = 0
            };
            db_.Posts.Add(post);
            await db_.SaveChangesAsync();

            try
            {
                var url = Url.Action("App", "Home", new { path = $"bounty/{bounty.BountyId}" }) + $"#bountyComment{post.PostId}";
                var title = bounty.Title;

                if (bounty.Author != User.Identity.Name)
                {
                    var target = await udb_.Users.Include(u => u.option).FirstOrDefaultAsync(u => u.UserName == bounty.Author);
                    if (target != null && (target.option == null || target.option.sendNoticeForNewPostReply))
                    {
                        msgUtil_.SendNewReplyNotice(target.UserName, User.Identity.Name, title, url);
                    }
                }
                if (mention.HasMentions())
                {
                    mention.SendMentionMsg(msgUtil_, User.Identity.Name, title, url);
                }
            }
            catch { }

            return Json(new { id = post.PostId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReplyAnswer()
        {
            var req = await System.Text.Json.JsonSerializer.DeserializeAsync<ReplyAnswerRequest>(Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (req == null || req.AnswerId == 0 || string.IsNullOrWhiteSpace(req.Content)) return BadRequest(new { error = "内容不能为空" });
            var answer = await db_.Answers.Include(a => a.Bounty).FirstOrDefaultAsync(a => a.AnswerId == req.AnswerId);
            if (answer == null) return NotFound();
            if (answer.Bounty.IsDeleted) return BadRequest(new { error = "悬赏已删除" });
            var mention = new MentionHandler(udb_);
            var raw = sanitizer_.Sanitize(req.Content.Trim());
            var parsed = mention.ParseMentions(raw);
            var post = new Post
            {
                IdType = ItemType.Answer,
                ItemId = req.AnswerId,
                Author = User.Identity.Name,
                Content = parsed,
                PostDate = DateTime.Now,
                Rating = 0
            };
            db_.Posts.Add(post);
            await db_.SaveChangesAsync();

            try
            {
                var url = Url.Action("App", "Home", new { path = $"bounty/{answer.BountyId}" }) + $"#postcontent{answer.AnswerId}";
                var title = answer.Bounty.Title;

                // Notify answer author (post reply)
                if (answer.Author != User.Identity.Name)
                {
                    var target = await udb_.Users.Include(u => u.option).FirstOrDefaultAsync(u => u.UserName == answer.Author);
                    if (target != null)
                    {
                        var opt = target.option;
                        if (opt == null || opt.sendNoticeForNewPostReply)
                        {
                            msgUtil_.SendNewReplyNotice(target.UserName, User.Identity.Name, title, url);
                        }
                    }
                }
                // Mention notices
                if (mention.HasMentions())
                {
                    mention.SendMentionMsg(msgUtil_, User.Identity.Name, title, url);
                }
            }
            catch { }

            return Json(new { id = post.PostId });
        }

        // Reply to a Post (nested Reply table already exists)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ReplyPost()
        {
            var req = await System.Text.Json.JsonSerializer.DeserializeAsync<ReplyPostRequest>(Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (req == null || req.PostId == 0 || string.IsNullOrWhiteSpace(req.Content)) return BadRequest(new { error = "内容不能为空" });
            var post = await db_.Posts.Include(p => p.Replies).FirstOrDefaultAsync(p => p.PostId == req.PostId);
            if (post == null) return NotFound();
            if (post.IdType != ItemType.Bounty && post.IdType != ItemType.Answer) return BadRequest(new { error = "不支持的帖子类型" });

            var mention2 = new MentionHandler(udb_);
            var raw2 = sanitizer_.Sanitize(req.Content.Trim());
            var parsed2 = mention2.ParseMentions(raw2);

            var reply = new Reply
            {
                PostId = req.PostId,
                Author = User.Identity.Name,
                Content = parsed2,
                ReplyDate = DateTime.Now
            };
            db_.Replies.Add(reply);
            await db_.SaveChangesAsync();

            try
            {
                // Resolve bountyId for url
                int bountyId = 0;
                if (post.IdType == ItemType.Answer) bountyId = (await db_.Answers.AsNoTracking().FirstOrDefaultAsync(a => a.AnswerId == post.ItemId))?.BountyId ?? 0;
                else if (post.IdType == ItemType.Bounty) bountyId = post.ItemId;
                var url = bountyId != 0 ? Url.Action("App", "Home", new { path = $"bounty/{bountyId}" }) + $"#postcontent{post.ItemId}" : Url.Action("App", "Home", new { path = "bounty" });
                var title = bountyId != 0 ? (await db_.Bounties.AsNoTracking().Where(b => b.BountyId == bountyId).Select(b => b.Title).FirstOrDefaultAsync()) ?? "悬赏" : "悬赏";

                // Notify post author about reply to their floor
                if (post.Author != User.Identity.Name)
                {
                    var target2 = await udb_.Users.Include(u => u.option).FirstOrDefaultAsync(u => u.UserName == post.Author);
                    if (target2 != null)
                    {
                        var opt = target2.option;
                        if (opt == null || opt.sendNoticeForNewPostReply)
                        {
                            msgUtil_.SendNewReplyNotice(target2.UserName, User.Identity.Name, title, url);
                        }
                    }
                }
                // Also notify bounty OP when reply happens in his bounty threads (if not already notified)
                if (bountyId != 0)
                {
                    try
                    {
                        var bountyAuthor = await db_.Bounties.AsNoTracking().Where(b => b.BountyId == bountyId).Select(b => b.Author).FirstOrDefaultAsync();
                        if (!string.IsNullOrEmpty(bountyAuthor) && bountyAuthor != User.Identity.Name && bountyAuthor != post.Author)
                        {
                            var opTarget = await udb_.Users.Include(u => u.option).FirstOrDefaultAsync(u => u.UserName == bountyAuthor);
                            if (opTarget != null && (opTarget.option == null || opTarget.option.sendNoticeForNewPostReply))
                                msgUtil_.SendNewReplyNotice(opTarget.UserName, User.Identity.Name, title, url);
                        }
                    }
                    catch { }
                }
                // Mentions
                if (mention2.HasMentions())
                {
                    mention2.SendMentionMsg(msgUtil_, User.Identity.Name, title, url);
                }
            }
            catch { }

            return Json(new { id = reply.ReplyId });
        }

        // Report bounty or answer - mirrors MessageController.Report for legacy Blogs
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Report()
        {
            try
            {
                var req = await System.Text.Json.JsonSerializer.DeserializeAsync<ReportRequest>(Request.Body, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (req == null || req.Id == 0 || string.IsNullOrWhiteSpace(req.MsgContent))
                    return BadRequest(new { error = "举报内容不能为空" });
                var msgContent = req.MsgContent.Trim();
                if (msgContent.Length > 2000) return BadRequest(new { error = "内容过长" });

                // Resolve author and url - use relative Home/App redirector like legacy blogs use Url.Action("Details")
                string url;
                string author = null;
                string title;
                if (req.ItemType == ItemType.Bounty)
                {
                    var bounty = await db_.Bounties.AsNoTracking().FirstOrDefaultAsync(b => b.BountyId == req.Id);
                    if (bounty == null) return NotFound(new { error = "悬赏不存在" });
                    author = bounty.Author;
                    title = bounty.Title;
                    // goes through Home/App toggle -> redirects to /app/bounty/{id} if Blazor enabled
                    url = Url.Action("App", "Home", new { path = $"bounty/{req.Id}" }) + (req.PostId.HasValue ? $"#postcontent{req.PostId}" : "");
                }
                else if (req.ItemType == ItemType.Answer)
                {
                    var ans = await db_.Answers.Include(a => a.Bounty).AsNoTracking().FirstOrDefaultAsync(a => a.AnswerId == req.Id);
                    if (ans == null) return NotFound(new { error = "回答不存在" });
                    author = ans.Author;
                    title = ans.Bounty.Title;
                    url = Url.Action("App", "Home", new { path = $"bounty/{ans.BountyId}" }) + $"#postcontent{ans.AnswerId}";
                }
                else if (req.ItemType == ItemType.Blog)
                {
                    var blog = await db_.Blogs.AsNoTracking().FirstOrDefaultAsync(b => b.BlogID == req.Id);
                    if (blog == null) return NotFound();
                    author = blog.Author;
                    title = blog.BlogTitle;
                    url = Url.Action("Details", "Blog", new { id = req.Id }) + (req.PostId.HasValue ? $"#listpost{req.PostId}" : "");
                }
                else if (req.ItemType == ItemType.Topic)
                {
                    var topic = await db_.Topics.AsNoTracking().FirstOrDefaultAsync(t => t.TopicID == req.Id);
                    if (topic == null) return NotFound();
                    author = topic.Author;
                    title = topic.TopicTitle;
                    url = Url.Action("Details", "Topic", new { id = req.Id });
                }
                else
                {
                    return BadRequest(new { error = "不支持的类型" });
                }

                var content = System.Net.WebUtility.HtmlEncode(msgContent) + $"<br>地址：<br><a href='{url}'>{url}</a>";
                if (req.Type == "rpt-author")
                {
                    if (string.IsNullOrEmpty(author)) return NotFound();
                    msgUtil_.AddMsg(User.Identity.Name, author, req.ItemType == ItemType.Bounty || req.ItemType == ItemType.Answer ? "报告悬赏问题" : "报告投稿问题", content);
                }
                else
                {
                    blogUtil_.AddBlogPost(-1, User.Identity.Name, content);
                }
                return Json(new { msg = "已成功报告。" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class ReportRequest
        {
            public int Id { get; set; }
            public ItemType ItemType { get; set; }
            public int? PostId { get; set; }
            public string MsgContent { get; set; }
            public string Type { get; set; } // rpt-author or rpt-admin
        }

        private static string[] ParseBountyImageUrls(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return Array.Empty<string>();
            var trimmed = raw.Trim();
            if (trimmed.StartsWith("["))
            {
                try
                {
                    var arr = System.Text.Json.JsonSerializer.Deserialize<string[]>(trimmed);
                    if (arr != null)
                        return arr.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                }
                catch { }
            }
            return Array.Empty<string>();
        }

        [Authorize]
        [HttpGet]
        [Obsolete("Use List with onlyMine=true")]
        public IActionResult My(int page = 1, bool includeDeleted = false, bool onlyMine = true)
        {
            var query = db_.Bounties.Where(b => b.Author == User.Identity.Name);
            if (!includeDeleted) query = query.Where(b => !b.IsDeleted);
            string avatarUrlBase = AvatarBase;
            var model = query.Select(q => new BountyPreview
            {
                Id = q.BountyId,
                Author = q.Author,
                AuthorAvatar = avatarUrlBase + q.Author,
                Content = q.Content.Length > 200 ? q.Content.Substring(0, 200) : q.Content,
                CreateDate = q.CreateDate,
                Prize = q.Prize,
                Title = q.Title,
                AnswerCount = q.Answers.Count,
                IsAccepted = q.IsAccepted,
                Deposit = q.Deposit,
                HelpfulReward = q.HelpfulReward,
                ViewCount = q.ViewCount,
                ExpiresAt = q.ExpiresAt,
                Image = q.ImageUrls,
                IsDeleted = q.IsDeleted,
                CloseDate = q.CloseDate,
            }).OrderByDescending(q => q.CreateDate);
            var pagedList = model.ToPagedList(page, cfg_.PageSize);
            try
            {
                var ids = pagedList.Select(b => b.Id).ToList();
                visitCounter_.PrepareBountyVisits(ids);
                foreach (var b in pagedList) b.ViewCount = (int)visitCounter_.GetBountyVisit(b.Id);
            }
            catch { }
            var paged = new Paged<BountyPreview>(pagedList);
            return Json(paged);
        }
    }
}
