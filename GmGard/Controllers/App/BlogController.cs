using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using GmGard.Models.App;
using Microsoft.Extensions.Options;
using GmGard.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Data.Entity;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Blog/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class BlogController : AppControllerBase
    {
        private readonly BlogContext db_;
        private readonly UsersContext udb_;
        private readonly BlogUtil blogUtil_;
        private readonly RatingUtil ratingUtil_;
        private readonly AppSettingsModel appSettings_;
        private readonly DbBlogSearchProvider dbBlogSearchProvider_;
        private readonly INickNameProvider nickNameProvider_;
        private readonly CategoryUtil categoryUtil_;

        private bool IsHarmony => !User.Identity.IsAuthenticated && appSettings_.HarmonySettings.Harmony;

        public BlogController(
            IOptions<AppSettingsModel> appSettings, 
            BlogContext db, 
            UsersContext udb,
            CategoryUtil catUtil, 
            BlogUtil blogUtil, 
            RatingUtil ratingUtil,
            DbBlogSearchProvider dbBlogSearchProvider,
            INickNameProvider nickNameProvider)
        {
            db_ = db;
            udb_ = udb;
            blogUtil_ = blogUtil;
            ratingUtil_ = ratingUtil;
            appSettings_ = appSettings.Value;
            dbBlogSearchProvider_ = dbBlogSearchProvider;
            nickNameProvider_ = nickNameProvider;
            categoryUtil_ = catUtil;
        }

        private BlogDetails GetBlogDetails(BlogDetailDisplay bd, User author, Comment[] comments, int? userrating)
        {
            return new BlogDetails
            {
                Id = bd.blog.BlogID,
                Author = author,
                AuthorDesc = bd.AuthorDesc,
                Brief = blogUtil_.GetFirstLine(bd.blog, true).ToString(),
                CategoryId = bd.blog.CategoryID,
                ParentCategoryId = categoryUtil_.GetParentCategoryId(bd.blog.CategoryID),
                Content = BlogHelper.ReplaceContentImage(bd.blog),
                CreateDate = bd.blog.BlogDate,
                ImageUrls = bd.blog.IsLocalImg ? bd.blog.ImagePath?.Split(';') : new[] { bd.blog.ImagePath },
                ThumbUrl = BlogHelper.firstImgPath(bd.blog, true),
                Links = BlogHelper.GetBlogLink(bd.blog.Links),
                IsApproved = bd.blog.isApproved,
                LockTags = bd.Option.LockTags,
                NoComment = bd.Option.NoComment,
                NoRate = bd.Option.NoRate,
                Rating = ratingUtil_.GetRating(bd.blog.BlogID),
                Tags = bd.tag.ToDictionary(t => t.TagID, t => t.TagName),
                Title = bd.blog.BlogTitle,
                VisitCount = bd.blog.BlogVisit,
                TopComments = comments,
                IsFavorite = bd.IsFavorite,
                UserRating = userrating,
            };
        }

        [BlogAuthorize(Redirect = false)]
        public IActionResult Details(int id)
        {
            var bd = blogUtil_.GetDetailDisplay(id);
            if (bd == null)
            {
                return NotFound();
            }
            var author = udb_.Users.SingleOrDefault(u => u.UserName == bd.blog.Author);
            User authorInfo = new User { UserName = bd.blog.Author, NickName = bd.blog.Author };
            if (author != null)
            {
                authorInfo = Models.App.User.FromUserProfile(author, Url.Action("Show", "Avatar", new { name = authorInfo.UserName }, Request.Scheme));
            }
            var comments = db_.Posts.Where(p => p.IdType == ItemType.Blog && p.ItemId == id)
                .GroupJoin(db_.BlogRatings, p => p.PostId, r => r.PostId, (p, r) => new { post = p, ratings = r })
                .SelectMany(a => a.ratings.DefaultIfEmpty(), (p, r) => new {
                    p.post,
                    blograting = r,
                    uc = p.post.Ratings.Count(ra => ra.Value == 1),
                    dc = p.post.Ratings.Count(ra => ra.Value == -1),
                    uv = User.Identity.IsAuthenticated ? p.post.Ratings.FirstOrDefault(ra => ra.Rater == User.Identity.Name) : null
                })
                .OrderByDescending(p => p.post.Rating)
                .ThenByDescending(p => p.post.PostDate)
                .Take(5)
                .Select(c => new Comment
                {
                    Author = c.post.Author,
                    CommentId = c.post.PostId,
                    Content = c.post.Content,
                    ItemId = c.post.ItemId,
                    UpvoteCount = c.uc,
                    DownvoteCount = c.dc,
                    IsUpvoted = c.uv == null ? new bool?() : c.uv.Value == 1,
                    IsDownvoted = c.uv == null ? new bool?() : c.uv.Value == -1,
                    CreateDate = c.post.PostDate,
                    Type = Comment.CommentType.Blog,
                    Rating = c.blograting == null ? new int?() : c.blograting.value
                })
                .ToArray();
            var rating = ratingUtil_.GetUsersRating(id);
            return Json(GetBlogDetails(bd, authorInfo, comments, rating.Rating == null ? new int?() : rating.Rating.value));
        }

        private IQueryable<BlogDetailDisplay> Sorted(string sort, IQueryable<BlogDetailDisplay> detailQuery)
        {
            // TODO: Find way to merge this with other sort logics in BlogHelper.
            switch (sort)
            {
                case "Date":
                    detailQuery = detailQuery.OrderBy(q => q.blog.BlogDate);
                    break;

                case "Visit_desc":
                    detailQuery = detailQuery.OrderByDescending(q => q.blog.BlogVisit);
                    break;

                case "Visit":
                    detailQuery = detailQuery.OrderBy(q => q.blog.BlogVisit);
                    break;

                case "Post":
                    detailQuery = detailQuery.OrderBy(q => db_.Posts.Count(p => p.ItemId == q.blog.BlogID && p.IdType == ItemType.Blog));
                    break;

                case "Post_desc":
                    detailQuery = detailQuery.OrderByDescending(q => db_.Posts.Count(p => p.ItemId == q.blog.BlogID && p.IdType == ItemType.Blog));
                    break;

                case "Rate":
                    detailQuery = detailQuery.OrderBy(q => q.blog.Rating);
                    break;

                case "Rate_desc":
                    detailQuery = detailQuery.OrderByDescending(q => q.blog.Rating);
                    break;

                default:
                    detailQuery = detailQuery.OrderByDescending(q => q.blog.BlogDate);
                    break;
            }
            return detailQuery;
        }

        private Paged<BlogDetails> ToPaged(X.PagedList.IPagedList<BlogDetailDisplay> blogs)
        {
            var names = blogs.Select(b => b.blog.Author).Distinct();
            var authors = udb_.Users.Where(u => names.Contains(u.UserName)).ToDictionary(u => u.UserName);
            var authorInfos = names.Select(n =>
            {
                if (authors.TryGetValue(n, out UserProfile u))
                {
                    var user = Models.App.User.FromUserProfile(u, Url.Action("Show", "Avatar", new { name = u.UserName }, Request.Scheme));
                    return user;
                }
                return new User { UserName = n, NickName = n };
            }).ToDictionary(u => u.UserName);
            ratingUtil_.PrepareRatings(blogs.Select(b => b.blog.BlogID));
            var ids = blogs.Select(b => b.blog.BlogID);
            var posts = db_.Posts.Where(p => p.IdType == ItemType.Blog && ids.Contains(p.ItemId))
                .GroupJoin(db_.BlogRatings, p => p.PostId, r => r.PostId, (p, r) => new { post = p, ratings = r })
                .SelectMany(a => a.ratings.DefaultIfEmpty(), (p, r) => new {
                    p.post,
                    blograting = r,
                    uc = p.post.Ratings.Count(ra => ra.Value == 1),
                    dc = p.post.Ratings.Count(ra => ra.Value == -1),
                    uv = User.Identity.IsAuthenticated ? p.post.Ratings.FirstOrDefault(ra => ra.Rater == User.Identity.Name) : null
                })
                .GroupBy(p => p.post.ItemId, (key, ps) => new
                {
                    Key = key,
                    Posts = ps
                        .OrderByDescending(p => p.post.Rating)
                        .ThenByDescending(p => p.post.PostDate)
                        .Take(5)
                })
                .ToDictionary(p => p.Key, p => p.Posts);
            var userRatings = ratingUtil_.GetUsersRatingValues(ids);
            var items = new X.PagedList.StaticPagedList<BlogDetails>(blogs.Select(bd =>
            {
                blogUtil_.ProcessBlogDetails(bd);
                return GetBlogDetails(bd, 
                    authorInfos[bd.blog.Author], 
                    posts.ContainsKey(bd.blog.BlogID) ? posts[bd.blog.BlogID].Select(c => new Comment
                    {
                        Author = c.post.Author,
                        CommentId = c.post.PostId,
                        Content = c.post.Content,
                        ItemId = c.post.ItemId,
                        UpvoteCount = c.uc,
                        DownvoteCount = c.dc,
                        IsUpvoted = c.uv == null ? new bool?() : c.uv.Value == 1,
                        IsDownvoted = c.uv == null ? new bool?() : c.uv.Value == -1,
                        CreateDate = c.post.PostDate,
                        Type = Comment.CommentType.Blog,
                        Rating = c.blograting == null ? new int?() : c.blograting.value,
                    }).ToArray() : new Comment[0],
                    userRatings.ContainsKey(bd.blog.BlogID) ? userRatings[bd.blog.BlogID] : new int?());
            }), blogs.GetMetaData());
            return new Paged<BlogDetails>(items);
        }
        
        public async Task<IActionResult> List(int? id, bool? harmony, string sort = "", int limit = 10, int skip = 0)
        {
            limit = Math.Min(Math.Max(limit, 1), 100);
            int page = skip <= 0 ? 1 : (skip / limit + 1);
            var query = db_.Blogs.AsNoTracking().Where(b => b.isApproved == true);
            if (IsHarmony || harmony.HasValue)
            {
                query = query.Where(b => b.isHarmony == (IsHarmony || (harmony ?? false)));
            }
            if (id.HasValue)
            {
                var flatCategories = categoryUtil_.GetCategoryWithSubcategories(id.Value);
                query = query.Where(b => flatCategories.Contains(b.CategoryID));
            }
            IQueryable<BlogDetailDisplay> detailQuery;
            if (User.Identity.IsAuthenticated)
            {
                detailQuery = query
                    .GroupJoin(db_.TagsInBlogs.DefaultIfEmpty(),
                        b => b.BlogID,
                        tib => tib.BlogID,
                        (blog, tib) => new { blog, tag = tib.Select(t => t.tag) })
                    .GroupJoin(db_.Favorites.Where(f => f.Username == User.Identity.Name),
                        b => b.blog.BlogID,
                        f => f.BlogID,
                        (b, f) => new BlogDetailDisplay
                        {
                            blog = b.blog,
                            tag = b.tag,
                            Option = b.blog.option,
                            IsFavorite = f.Count() > 0,
                        });
            }
            else
            {
                detailQuery = query.GroupJoin(db_.TagsInBlogs.DefaultIfEmpty(),
                    b => b.BlogID,
                    tib => tib.BlogID,
                    (b, tib) => new BlogDetailDisplay
                    {
                        blog = b,
                        tag = tib.Select(t => t.tag),
                        Option = b.option
                    });
            }

            detailQuery = Sorted(sort, detailQuery);
            var blogs = await detailQuery.ToPagedListAsync(page, limit);
            return Json(ToPaged(blogs));
        }

        [HttpGet]
        public IActionResult Rate(int id)
        {
            return Json(ratingUtil_.GetUsersRating(id));
        }

        [HttpPost]
        public async Task<IActionResult> Rate(RateRequest req)
        {
            var blog = await db_.Blogs.Include(b => b.option).SingleOrDefaultAsync(b => b.BlogID == req.BlogId);
            if (blog == null)
            {
                return NotFound();
            }
            if (blog.option != null && blog.option.NoRate)
            {
                return Forbid();
            }
            string status = ratingUtil_.TryRateBlog(req.BlogId, req.Rating);
            if (status != "ok")
            {
                return BadRequest(new { error = status });
            }
            return Json(new RateResponse {
                Status = status,
                Message = HttpContext.Items["QuestMsg"] as string,
                Rating = ratingUtil_.GetRating(req.BlogId, false)
            });
        }

        [HttpGet, HttpPost, Authorize, ActionName("Favorite")]
        public async Task<IActionResult> AddFavorite(int id)
        {
            var blog = await db_.Blogs.AnyAsync(b => b.BlogID == id);
            if (!blog)
            {
                return NotFound();
            }
            var fav = await db_.Favorites.FindAsync(User.Identity.Name, id);
            if (fav == null)
            {
                fav = new Favorite
                {
                    BlogID = id,
                    Username = User.Identity.Name,
                    AddDate = DateTime.Now
                };
                db_.Favorites.Add(fav);
                await db_.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpDelete, Authorize, ActionName("Favorite")]
        public async Task<IActionResult> DeleteFavorite(int id)
        {
            var fav = await db_.Favorites.FindAsync(User.Identity.Name, id);
            if (fav != null)
            {
                db_.Favorites.Remove(fav);
                await db_.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [Authorize]
        public async Task<ActionResult> Favorites(int? id, string name, string sort = "", int limit = 10, int skip = 0)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index");
                name = User.Identity.Name;
            }
            else if (!await udb_.Users.AnyAsync(u => u.UserName == name))
            {
                return NotFound();
            }
            limit = Math.Min(Math.Max(limit, 1), 100);
            int page = skip <= 0 ? 1 : (skip / limit + 1);
            var query = db_.Blogs.AsNoTracking().Where(b => b.isApproved == true);
            if (id.HasValue)
            {
                var flatCategories = categoryUtil_.GetCategoryWithSubcategories(id.Value);
                query = query.Where(b => flatCategories.Contains(b.CategoryID));
            }
            var queryFav = query
                .Join(db_.Favorites.Where(f => f.Username == name), b => b.BlogID, f => f.BlogID, (b, f) => new { date = f.AddDate, blog = b })
                .GroupJoin(db_.TagsInBlogs.DefaultIfEmpty(),
                        b => b.blog.BlogID,
                        tib => tib.BlogID,
                        (b, tib) => new
                        {
                            b.blog,
                            b.date,
                            b.blog.option,
                            tag = tib.Select(t => t.tag),
                        });
            bool sorted = false;
            if (string.IsNullOrEmpty(sort) || sort.Contains(":") || sort == "AddDate_desc")
            {
                queryFav = queryFav.OrderByDescending(b => b.date);
                sorted = true;
            }
            else if (sort == "AddDate")
            {
                queryFav = queryFav.OrderBy(b => b.date);
                sorted = true;
            }
            IQueryable<BlogDetailDisplay> detailQuery = queryFav
                .Select(b => new BlogDetailDisplay
                {
                    blog = b.blog,
                    tag = b.tag,
                    IsFavorite = true,
                    Option = b.option
                });
            if (!sorted)
            {
                detailQuery = Sorted(sort, detailQuery);
            }
            var blogs = await detailQuery.ToPagedListAsync(page, limit);
            return Json(ToPaged(blogs));
        }

        [HttpPost, ActionName("Search")]
        public Task<JsonResult> PostSearch([FromServices]ISearchProvider searchProvider, [FromBody]SearchModel model, int limit = 10, int skip = 0)
        {
            return Search(searchProvider, model, limit, skip);
        }

        [HttpGet]
        public async Task<JsonResult> Search([FromServices]ISearchProvider searchProvider, [FromQuery]SearchModel model, int limit = 10, int skip = 0)
        {
            limit = Math.Min(Math.Max(limit, 1), 100);
            int page = skip <= 0 ? 1 : (skip / limit + 1);
            if (IsHarmony)
            {
                model.Harmony = true;
            }
            var blogs = await searchProvider.SearchBlogAsync(model, page, limit);
            var blogids = blogs.Blogs.Select(b => b.BlogID).ToList();
            IQueryable<BlogDetailDisplay> details;
            if (User.Identity.IsAuthenticated)
            {
                details = db_.Blogs.Where(b => blogids.Contains(b.BlogID))
                    .GroupJoin(db_.TagsInBlogs.DefaultIfEmpty(),
                        b => b.BlogID,
                        tib => tib.BlogID,
                        (blog, tib) => new { blog, tag = tib.Select(t => t.tag) })
                    .GroupJoin(db_.Favorites.Where(f => f.Username == User.Identity.Name),
                        b => b.blog.BlogID,
                        f => f.BlogID,
                        (b, f) => new BlogDetailDisplay
                        {
                            blog = b.blog,
                            tag = b.tag,
                            Option = b.blog.option,
                            IsFavorite = f.Count() > 0,
                        });
            }
            else
            {
                details = db_.Blogs.Where(b => blogids.Contains(b.BlogID))
                    .GroupJoin(db_.TagsInBlogs.DefaultIfEmpty(),
                        b => b.BlogID,
                        tib => tib.BlogID,
                        (b, tib) => new BlogDetailDisplay
                        {
                            blog = b,
                            tag = tib.Select(t => t.tag),
                            Option = b.option
                        });
            }
            return Json(ToPaged(new X.PagedList.StaticPagedList<BlogDetailDisplay>(
                (await details.ToListAsync()).OrderBy(b => blogids.IndexOf(b.blog.BlogID)),
                blogs.Blogs.GetMetaData())));
        }
    }
}