using System.Linq;
using System.Threading.Tasks;
using GmGard.Controllers.App;
using GmGard.Models;
using GmGard.Models.App;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GmGard.Tests
{
    /// <summary>App 版评论/回复接口(ReplyController):新增、删除、冷却、留言板可见性。</summary>
    public class AppReplyControllerTests : System.IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private int PostCount(int itemId, ItemType type, string author) =>
            _app.QueryData(db => db.Posts.Count(p => p.ItemId == itemId && p.IdType == type && p.Author == author));

        private bool PostExists(int postId) => _app.QueryData(db => db.Posts.Any(p => p.PostId == postId));
        private bool ReplyExists(int replyId) => _app.QueryData(db => db.Replies.Any(r => r.ReplyId == replyId));

        private int SeedBlogFor(string owner)
        {
            _app.SeedUser(owner);
            return _app.SeedBlog(owner, _app.SeedCategory());
        }

        // ================= Comment (POST) =================

        [SkippableFact]
        public async Task Comment_on_blog_saves_post_with_author_and_content()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice", level: 1);
            var c = _app.Controller<ReplyController>("alice");

            var result = await c.Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = blog, Content = "<p>写得好</p>" });

            Assert.True(result is JsonResult, "实际:" + result.GetType().Name);
            Assert.Equal(1, PostCount(blog, ItemType.Blog, "alice"));
            var post = _app.QueryData(db => db.Posts.Single(p => p.ItemId == blog && p.Author == "alice"));
            Assert.Contains("写得好", post.Content);
        }

        [SkippableFact]
        public async Task Comment_on_unknown_blog_is_not_found()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice", level: 1);
            var c = _app.Controller<ReplyController>("alice");

            var result = await c.Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = 999999, Content = "<p>x</p>" });

            Assert.IsType<NotFoundResult>(result);
        }

        [SkippableFact]
        public async Task Comment_with_only_image_or_blank_content_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice", level: 1);
            var c = _app.Controller<ReplyController>("alice");

            var result = await c.Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = blog, Content = "   " });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, PostCount(blog, ItemType.Blog, "alice"));
        }

        [SkippableFact]
        public async Task Comment_cooldown_blocks_second_post_within_30s_for_low_level_user()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("newbie", level: 1);

            var first = await _app.Controller<ReplyController>("newbie")
                .Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = blog, Content = "<p>第一条</p>" });
            var second = await _app.Controller<ReplyController>("newbie")
                .Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = blog, Content = "<p>第二条</p>" });

            Assert.True(first is JsonResult, "第一条实际:" + first.GetType().Name);
            var obj = Assert.IsType<ObjectResult>(second);
            Assert.Equal(429, obj.StatusCode);
            Assert.Equal(1, PostCount(blog, ItemType.Blog, "newbie"));
        }

        [SkippableFact]
        public async Task Comment_cooldown_does_not_apply_to_level_10_and_above()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("veteran", level: 10);

            var first = await _app.Controller<ReplyController>("veteran")
                .Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = blog, Content = "<p>第一条</p>" });
            var second = await _app.Controller<ReplyController>("veteran")
                .Comment(new AddCommentRequest { Type = Comment.CommentType.Blog, ItemId = blog, Content = "<p>第二条</p>" });

            Assert.True(first is JsonResult, "第一条实际:" + first.GetType().Name);
            Assert.True(second is JsonResult, "第二条实际:" + second.GetType().Name);
            Assert.Equal(2, PostCount(blog, ItemType.Blog, "veteran"));
        }

        // ================= Comment (DELETE) =================

        [SkippableFact]
        public void Delete_comment_by_author_removes_post()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");

            var result = _app.Controller<ReplyController>("alice").Comment(postId);

            Assert.IsType<OkResult>(result);
            Assert.False(PostExists(postId));
        }

        [SkippableFact]
        public void Delete_comment_by_moderator_removes_post()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            _app.SeedUser("mod");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");

            var result = _app.Controller<ReplyController>("mod", "Moderator").Comment(postId);

            Assert.IsType<OkResult>(result);
            Assert.False(PostExists(postId));
        }

        [SkippableFact]
        public void Delete_comment_by_stranger_is_unauthorized_and_post_kept()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            _app.SeedUser("eve");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");

            var result = _app.Controller<ReplyController>("eve").Comment(postId);

            Assert.IsType<UnauthorizedResult>(result);
            Assert.True(PostExists(postId));
        }

        // ================= CommentReply =================

        [SkippableFact]
        public async Task CommentReply_saves_reply_under_post()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");

            var result = _app.Controller<ReplyController>("bob")
                .CommentReply(new AddCommentReplyRequest { CommentId = postId, Content = "<p>同意</p>" });

            Assert.True(result is JsonResult, "实际:" + result.GetType().Name);
            var replies = _app.QueryData(db => db.Replies.Where(r => r.PostId == postId).ToList());
            Assert.Single(replies);
            Assert.Equal("bob", replies[0].Author);
            Assert.Contains("同意", replies[0].Content);
        }

        [SkippableFact]
        public async Task CommentReply_with_blank_content_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");

            var result = _app.Controller<ReplyController>("bob")
                .CommentReply(new AddCommentReplyRequest { CommentId = postId, Content = "  " });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, _app.QueryData(db => db.Replies.Count(r => r.PostId == postId)));
        }

        [SkippableFact]
        public void Delete_reply_by_stranger_is_unauthorized_and_reply_kept()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            _app.SeedUser("eve");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");
            int replyId = _app.SeedReply(postId, "alice");

            var result = _app.Controller<ReplyController>("eve").CommentReply(replyId);

            Assert.IsType<UnauthorizedResult>(result);
            Assert.True(ReplyExists(replyId));
        }

        [SkippableFact]
        public void Delete_reply_by_author_removes_reply()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            int postId = _app.SeedPost(ItemType.Blog, blog, "alice");
            int replyId = _app.SeedReply(postId, "alice");

            var result = _app.Controller<ReplyController>("alice").CommentReply(replyId);

            Assert.IsType<OkResult>(result);
            Assert.False(ReplyExists(replyId));
        }

        // ================= Comments (GET) 留言板可见性 =================

        private Paged<Comment> ListComments(ReplyController c, int id, Comment.CommentType type)
        {
            var result = c.Comments(id, type);
            var json = Assert.IsType<JsonResult>(result);
            return Assert.IsType<Paged<Comment>>(json.Value);
        }

        [SkippableFact]
        public void Comments_on_normal_blog_is_visible_to_anonymous()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int blog = SeedBlogFor("owner");
            _app.SeedUser("alice");
            _app.SeedPost(ItemType.Blog, blog, "alice", "<p>普通评论</p>");

            var paged = ListComments(_app.Controller<ReplyController>(null), blog, Comment.CommentType.Blog);

            var items = paged.Items.ToList();
            Assert.Single(items);
            Assert.Equal("alice", items[0].Author);
        }

        [SkippableFact]
        public void Suggestions_board_is_invisible_to_anonymous()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedPost(ItemType.Blog, PostConstant.Suggestions, "alice", "<p>意见</p>");

            var paged = ListComments(_app.Controller<ReplyController>(null), PostConstant.Suggestions, Comment.CommentType.Blog);

            Assert.Empty(paged.Items);
        }

        [SkippableFact]
        public void Suggestions_board_shows_only_own_posts_to_regular_user()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            _app.SeedPost(ItemType.Blog, PostConstant.Suggestions, "alice", "<p>alice 的意见</p>");
            _app.SeedPost(ItemType.Blog, PostConstant.Suggestions, "bob", "<p>bob 的意见</p>");

            var paged = ListComments(_app.Controller<ReplyController>("alice"), PostConstant.Suggestions, Comment.CommentType.Blog);

            var items = paged.Items.ToList();
            Assert.Single(items);
            Assert.Equal("alice", items[0].Author);
        }

        [SkippableFact]
        public void Suggestions_board_shows_all_posts_to_administrator()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            _app.SeedUser("admin");
            _app.SeedPost(ItemType.Blog, PostConstant.Suggestions, "alice", "<p>alice 的意见</p>");
            _app.SeedPost(ItemType.Blog, PostConstant.Suggestions, "bob", "<p>bob 的意见</p>");

            var paged = ListComments(_app.Controller<ReplyController>("admin", "Administrator"), PostConstant.Suggestions, Comment.CommentType.Blog);

            Assert.Equal(2, paged.Items.Count());
        }

        [SkippableFact]
        public void Problems_board_shows_only_own_posts_to_regular_user_and_all_to_moderator()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            _app.SeedUser("mod");
            _app.SeedPost(ItemType.Blog, PostConstant.Problems, "alice", "<p>alice 的举报</p>");
            _app.SeedPost(ItemType.Blog, PostConstant.Problems, "bob", "<p>bob 的举报</p>");

            var asAlice = ListComments(_app.Controller<ReplyController>("alice"), PostConstant.Problems, Comment.CommentType.Blog);
            var asMod = ListComments(_app.Controller<ReplyController>("mod", "Moderator"), PostConstant.Problems, Comment.CommentType.Blog);

            Assert.Equal("alice", Assert.Single(asAlice.Items.ToList()).Author);
            Assert.Equal(2, asMod.Items.Count());
        }
    }
}
