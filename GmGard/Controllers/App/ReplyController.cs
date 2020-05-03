using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Extensions;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Reply/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class ReplyController : AppControllerBase
    {
        private BlogContext _db;
        private AdminUtil _adminUtil;
        private BlogUtil _blogUtil;
        private RatingUtil _ratingUtil;
        private readonly HtmlSanitizerService _sanitizerService;

        public ReplyController(
            BlogContext db,
            AdminUtil adminUtil,
            BlogUtil blogUtil,
            RatingUtil ratingUtil,
            HtmlSanitizerService sanitizerService)
        {
            _db = db;
            _adminUtil = adminUtil;
            _blogUtil = blogUtil;
            _ratingUtil = ratingUtil;
            _sanitizerService = sanitizerService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Comment([FromBody]AddCommentRequest request)
        {
            if (string.IsNullOrWhiteSpace(BlogHelper.removeAllTags(request.Content)))
            {
                return BadRequest(new { error = "内容不能为空或纯图片。" });
            }
            Post post;
            switch (request.Type)
            {
                case Models.App.Comment.CommentType.Blog:
                    var blog = await _db.Blogs.Include("option").SingleOrDefaultAsync(b => b.BlogID == request.ItemId);
                    if (blog == null)
                    {
                        return NotFound();
                    }
                    if (blog.option != null && blog.option.NoComment)
                    {
                        return Forbid();
                    }
                    UsersRating userrate = null;
                    if (request.Rating != null && RatingUtil.RatingValue.ContainsKey(request.Rating.Value))
                    {
                        if (blog.option.NoRate)
                        {
                            return Forbid();
                        }
                        userrate = _ratingUtil.GetUsersRating(request.ItemId);
                        if (userrate.Rating != null && (userrate.HasPost || userrate.Rating.value != RatingUtil.RatingValue[request.Rating.Value]))
                        {
                            return BadRequest(new { error = userrate.RateWithAccount ? "您已经评过分了" : "今天已经评过分了" });
                        }
                    }
                    post = _blogUtil.AddBlogPost(request.ItemId, User.Identity.Name, _sanitizerService.Sanitize(request.Content));
                    if (userrate != null)
                    {
                        if (userrate.Rating == null)
                        {
                            var Rate = _ratingUtil.AddBlogRating(request.ItemId, request.Rating.Value, userrate.credential, post.PostId);
                            if (Rate != null)
                            {
                                _ratingUtil.getRating(request.ItemId, false);
                            }
                        }
                        else
                        {
                            userrate.Rating.PostId = post.PostId;
                            await _db.SaveChangesAsync();
                        }
                    }
                    break;
                default:
                    return BadRequest();
            }
            TriggerAddPost(post);
            string expmsg = HttpContext.Items["QuestMsg"] as string;
            return Json(new AddReplyResponse { CommentId = post.PostId, Message = expmsg });
        }

        [HttpDelete]
        [Authorize]
        public ActionResult Comment(int id)
        {
            var post = _db.Posts.Find(id);
            if (post == null)
                return NotFound();
            if (post.Author != User.Identity.Name && !User.IsInRole("Administrator") && !User.IsInRole("Moderator"))
            {
                return Unauthorized();
            }
            if (User.Identity.Name != post.Author)
            {
                _adminUtil.log(User.Identity.Name, "deletepost", _blogUtil.GetPostLink(post));
            }
            TriggerDeletePost(post);
            _db.Posts.Remove(post);
            _db.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public ActionResult CommentReply([FromBody]AddCommentReplyRequest request)
        {
            if (string.IsNullOrWhiteSpace(BlogHelper.removeAllTags(request.Content)))
            {
                return BadRequest(new { error = "内容不能为空或纯图片。" });
            }
            var reply = _blogUtil.AddPostReply(request.CommentId, User.Identity.Name, _sanitizerService.Sanitize(request.Content));
            TriggerAddReply(reply);
            string expmsg = HttpContext.Items["QuestMsg"] as string;
            return Json(new AddReplyResponse { CommentId = reply.PostId, ReplyId = reply.ReplyId, Message = expmsg });
        }

        [HttpDelete]
        [Authorize]
        public ActionResult CommentReply(int id)
        {
            var reply = _db.Replies.Find(id);
            if (reply == null)
                return NotFound();
            if (reply.Author != User.Identity.Name && !User.IsInRole("Administrator") && !User.IsInRole("Moderator"))
            {
                return Unauthorized();
            }
            if (User.Identity.Name != reply.Author)
            {
                _adminUtil.log(User.Identity.Name, "deletereply", _blogUtil.GetPostLink(reply.post));
            }
            _db.Replies.Remove(reply);
            _db.SaveChanges();
            return Ok();
        }

        [HttpPost, Authorize]
        public async Task<ActionResult> Upvote(int id, int value)
        {
            if (value != 0 && Math.Abs(value) != 1)
            {
                return BadRequest();
            }
            var p = await _db.Posts.Include("Ratings").SingleOrDefaultAsync(pp => pp.PostId == id);
            if (p == null)
            {
                return NotFound();
            }
            var rating = p.Ratings.SingleOrDefault(r => r.Rater.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase));
            if (rating == null && value != 0)
            {
                rating = new PostRating { PostId = id, Rater = User.Identity.Name, RatingID = Guid.NewGuid() };
                p.Ratings.Add(rating);
            }
            if (rating != null)
            {
                rating.Value = value;
                await _db.SaveChangesAsync();
                TriggerRatePost(rating);
            }
            return Json(new { upvotes = p.Ratings.Count(r => r.Value > 0), downvotes = p.Ratings.Count(r => r.Value < 0) });
        }

        public ActionResult Comments(int id, Comment.CommentType commentType, string sort = "", int limit = 10, int skip = 0)
        {
            ItemType itemType = (ItemType)commentType;
            var posts = _db.Posts.Include(r => r.Ratings).Include(r => r.Replies).Where(p => p.ItemId == id && p.IdType == itemType)
                .Select(p => new
                {
                    post = p,
                    uc = p.Ratings.Count(ra => ra.Value == 1),
                    dc = p.Ratings.Count(ra => ra.Value == -1),
                    uv = User.Identity.IsAuthenticated ? p.Ratings.FirstOrDefault(ra => ra.Rater == User.Identity.Name) : null
                });
            IQueryable<Comment> query;
            if (itemType == ItemType.Blog)
            {
                query = posts.GroupJoin(_db.BlogRatings, p => p.post.PostId, r => r.PostId, (p, r) => new {
                    p.post,
                    p.uc,
                    p.uv,
                    p.dc,
                    ratings = r,
                })
                    .SelectMany(a => a.ratings.DefaultIfEmpty(), (p, r) => new Comment
                    {
                        Author = p.post.Author,
                        CommentId = p.post.PostId,
                        Content = p.post.Content,
                        ItemId = p.post.ItemId,
                        UpvoteCount = p.uc,
                        DownvoteCount = p.dc,
                        IsUpvoted = p.uv == null ? new bool?() : p.uv.Value == 1,
                        IsDownvoted = p.uv == null ? new bool?() : p.uv.Value == -1,
                        Type = (Comment.CommentType)p.post.IdType,
                        CreateDate = p.post.PostDate,
                        Rating = r == null ? new int?() : r.value,
                        Replies = p.post.Replies.Select(rr => new CommentReply
                        {
                            ReplyId = rr.ReplyId,
                            Author = rr.Author,
                            CommentId = rr.PostId,
                            Content = rr.Content,
                            CreateDate = rr.ReplyDate
                        })
                    });
            }
            else
            {
                query = posts.Select(p => new Comment
                {
                    Author = p.post.Author,
                    CommentId = p.post.PostId,
                    Content = p.post.Content,
                    ItemId = p.post.ItemId,
                    UpvoteCount = p.uc,
                    DownvoteCount = p.dc,
                    IsUpvoted = p.uv == null ? new bool?() : p.uv.Value == 1,
                    IsDownvoted = p.uv == null ? new bool?() : p.uv.Value == -1,
                    Type = (Comment.CommentType)p.post.IdType,
                    CreateDate = p.post.PostDate,
                    Replies = p.post.Replies.Select(rr => new CommentReply
                    {
                        ReplyId = rr.ReplyId,
                        Author = rr.Author,
                        CommentId = rr.PostId,
                        Content = rr.Content,
                        CreateDate = rr.ReplyDate
                    })
                });
            }
            if (sort.Equals("upvotes", StringComparison.OrdinalIgnoreCase))
            {
                query = query.OrderByDescending(p => p.UpvoteCount - p.DownvoteCount).ThenByDescending(p => p.CreateDate);
            }
            else
            {
                query = query.OrderByDescending(p => p.CreateDate);
            }
            limit = Math.Min(Math.Max(limit, 1), 100);
            int page = skip <= 0 ? 1 : (skip / limit + 1);
            return Json(new Paged<Comment>(query.ToPagedList(page, limit)));
        }

        private void TriggerDeletePost(Post post)
        {
            OnDeletePost?.Invoke(this, new PostEventArgs(post));
        }

        private void TriggerAddPost(Post post)
        {
            OnAddPost?.Invoke(this, new PostEventArgs(post));
        }

        private void TriggerAddReply(Reply r)
        {
            OnAddReply?.Invoke(this, new ReplyEventArgs(r));
        }

        private void TriggerRatePost(PostRating r)
        {
            OnRatePost?.Invoke(this, new RatePostEventArgs(r));
        }

        public static event EventHandler<PostEventArgs> OnAddPost;

        public static event EventHandler<PostEventArgs> OnDeletePost;

        public static event EventHandler<ReplyEventArgs> OnAddReply;

        public static event EventHandler<RatePostEventArgs> OnRatePost;
    }
}