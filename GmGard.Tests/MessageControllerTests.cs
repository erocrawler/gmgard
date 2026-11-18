using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Models.App;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;
using AppMessageController = GmGard.Controllers.App.MessageController;
using OldMessageController = GmGard.Controllers.MessageController;

namespace GmGard.Tests
{
    /// <summary>
    /// 站内信:旧版 MVC MessageController(Write/MsgAction/Report)与 App 版(Send/Delete/Content)。
    /// 直接调用 action,不经过 ValidateAntiForgeryToken/ValidateCaptcha 等过滤器。
    /// </summary>
    public class MessageControllerTests : System.IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private Message Msg(int id) => _app.QueryUsers(db => db.Messages.SingleOrDefault(m => m.MsgId == id));
        private int MsgCount(string recipient) => _app.QueryUsers(db => db.Messages.Count(m => m.Recipient == recipient));

        // ================= 旧版 Write =================

        [SkippableFact]
        public void Old_Write_stores_message_with_escaped_title_and_content()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            var c = _app.Controller<OldMessageController>("alice");

            var result = c.Write(new Message
            {
                Recipient = "bob",
                MsgTitle = "<b>标题</b>",
                MsgContent = "<script>alert(1)</script>正文",
            });

            Assert.IsType<RedirectToActionResult>(result);
            var list = _app.QueryUsers(db => db.Messages.Where(m => m.Recipient == "bob").ToList());
            Assert.Single(list);
            Assert.Equal("alice", list[0].Sender);
            Assert.Equal("&lt;b&gt;标题&lt;/b&gt;", list[0].MsgTitle);
            Assert.Equal("&lt;script&gt;alert(1)&lt;/script&gt;正文", list[0].MsgContent);
        }

        // ================= 旧版 MsgAction =================

        [SkippableFact]
        public void Old_MsgAction_delete_on_GET_is_405_and_message_kept()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int id = _app.SeedMessage("alice", "bob");
            var c = _app.Controller<OldMessageController>("bob");   // 默认请求方法为 GET

            var result = c.MsgAction(id.ToString(), "delete");

            var status = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(405, status.StatusCode);
            Assert.False(Msg(id).IsRecipientDelete);
        }

        [SkippableFact]
        public void Old_MsgAction_delete_on_POST_by_recipient_sets_recipient_flag()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int id = _app.SeedMessage("alice", "bob");
            var c = _app.Controller<OldMessageController>("bob");
            c.HttpContext.Request.Method = HttpMethods.Post;

            var result = c.MsgAction(id.ToString(), "delete");

            Assert.IsType<RedirectToActionResult>(result);
            var m = Msg(id);
            Assert.NotNull(m);                 // 发件人还没删,保留
            Assert.True(m.IsRecipientDelete);
            Assert.False(m.IsSenderDelete);
        }

        [SkippableFact]
        public void Old_MsgAction_on_someone_elses_message_is_not_found_and_untouched()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            _app.SeedUser("eve");
            int id = _app.SeedMessage("alice", "bob");
            var c = _app.Controller<OldMessageController>("eve");
            c.HttpContext.Request.Method = HttpMethods.Post;

            var result = c.MsgAction(id.ToString(), "delete");

            Assert.IsType<NotFoundResult>(result);
            var m = Msg(id);
            Assert.False(m.IsRecipientDelete);
            Assert.False(m.IsSenderDelete);
        }

        [SkippableFact]
        public void Old_MsgAction_reply_and_forward_work_on_GET()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int id = _app.SeedMessage("alice", "bob");

            var replyCtrl = _app.Controller<OldMessageController>("bob");
            var replyResult = replyCtrl.MsgAction(id.ToString(), "reply");
            Assert.IsType<RedirectToActionResult>(replyResult);
            Assert.True(replyCtrl.TempData.ContainsKey("WriteMessage"));

            var fwdCtrl = _app.Controller<OldMessageController>("bob");
            var fwdResult = fwdCtrl.MsgAction(id.ToString(), "forward");
            Assert.IsType<RedirectToActionResult>(fwdResult);
            Assert.True(fwdCtrl.TempData.ContainsKey("WriteMessage"));

            // 只读操作,不改变数据
            Assert.False(Msg(id).IsRecipientDelete);
        }

        // ================= 旧版 Report =================

        [SkippableFact]
        public void Old_Report_to_author_sends_html_encoded_message_to_blog_author()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("owner");
            _app.SeedUser("reporter");
            int cat = _app.SeedCategory();
            int blog = _app.SeedBlog("owner", cat);
            var c = _app.Controller<OldMessageController>("reporter");

            var result = c.Report(blog, ItemType.Blog, null, "<img src=x onerror=1>违规", "rpt-author");

            Assert.IsType<JsonResult>(result);
            var list = _app.QueryUsers(db => db.Messages.Where(m => m.Recipient == "owner").ToList());
            Assert.Single(list);
            Assert.Equal("reporter", list[0].Sender);
            Assert.Contains("&lt;img src=x onerror=1&gt;违规", list[0].MsgContent);
            Assert.DoesNotContain("<img", list[0].MsgContent);
        }

        [SkippableFact]
        public void Old_Report_to_author_of_unknown_blog_is_not_found()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("reporter");
            var c = _app.Controller<OldMessageController>("reporter");

            var result = c.Report(999999, ItemType.Blog, null, "违规", "rpt-author");

            Assert.IsType<NotFoundResult>(result);
            Assert.Equal(0, _app.QueryUsers(db => db.Messages.Count()));
        }

        [SkippableFact]
        public void Old_Report_to_admin_board_saves_post_on_problem_board()
        {
            // "汇报给管理员"(type=rpt-admin)把举报写到问题举报板(ItemId=-1);生产库里占位博客 -1 存在,测试库要先种子。
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedPlaceholderBlogs();
            _app.SeedUser("reporter");
            var c = _app.Controller<OldMessageController>("reporter");

            var result = c.Report(999999, ItemType.Blog, null, "违规内容", "rpt-admin");

            Assert.IsType<JsonResult>(result);
            Assert.Equal(1, _app.QueryData(db => db.Posts.Count(p => p.ItemId == PostConstant.Problems && p.Author == "reporter")));
        }

        // ================= App 版 Send =================

        private static IOptions<ApiBehaviorOptions> ApiOptions()
        {
            var opts = new ApiBehaviorOptions
            {
                InvalidModelStateResponseFactory = ctx => new BadRequestObjectResult(ctx.ModelState),
            };
            return Options.Create(opts);
        }

        [SkippableFact]
        public async Task App_Send_stores_html_encoded_content()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            var c = _app.Controller<AppMessageController>("alice");

            var result = await c.Send(ApiOptions(), new SendMessageRequest { Recipient = "bob", Title = "你好", Content = "<b>hi</b>" });

            Assert.IsType<OkResult>(result);
            var list = _app.QueryUsers(db => db.Messages.Where(m => m.Recipient == "bob").ToList());
            Assert.Single(list);
            Assert.Equal("alice", list[0].Sender);
            Assert.Equal("你好", list[0].MsgTitle);
            Assert.Equal("&lt;b&gt;hi&lt;/b&gt;", list[0].MsgContent);
        }

        [SkippableFact]
        public async Task App_Send_to_unknown_user_is_rejected_and_nothing_stored()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            var c = _app.Controller<AppMessageController>("alice");

            var result = await c.Send(ApiOptions(), new SendMessageRequest { Recipient = "nobody", Title = "x", Content = "y" });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, _app.QueryUsers(db => db.Messages.Count()));
        }

        // ================= App 版 Delete / Content =================

        [SkippableFact]
        public async Task App_Delete_by_stranger_is_forbidden_and_kept()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            _app.SeedUser("eve");
            int id = _app.SeedMessage("alice", "bob");

            var result = await _app.Controller<AppMessageController>("eve").Delete(id);

            Assert.IsType<ForbidResult>(result);
            Assert.NotNull(Msg(id));
        }

        [SkippableFact]
        public async Task App_Delete_by_both_parties_removes_message()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int id = _app.SeedMessage("alice", "bob");

            var first = await _app.Controller<AppMessageController>("bob").Delete(id);
            Assert.IsType<OkResult>(first);
            Assert.True(Msg(id).IsRecipientDelete);
            Assert.NotNull(Msg(id));   // 发件人还没删

            var second = await _app.Controller<AppMessageController>("alice").Delete(id);
            Assert.IsType<OkResult>(second);
            Assert.Null(Msg(id));
        }

        [SkippableFact]
        public async Task App_Content_by_stranger_is_forbidden()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            _app.SeedUser("eve");
            int id = _app.SeedMessage("alice", "bob");

            var result = await _app.Controller<AppMessageController>("eve").Content(id, markRead: true);

            Assert.IsType<ForbidResult>(result);
            Assert.False(Msg(id).IsRead);
        }

        [SkippableFact]
        public async Task App_Content_by_recipient_with_markRead_marks_as_read()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("alice");
            _app.SeedUser("bob");
            int id = _app.SeedMessage("alice", "bob", content: "<p>正文</p>");

            var result = await _app.Controller<AppMessageController>("bob").Content(id, markRead: true);

            Assert.False(result is ForbidResult || result is NotFoundResult, "实际:" + result.GetType().Name);
            Assert.True(Msg(id).IsRead);
        }

        [SkippableFact]
        public async Task App_Content_of_unknown_message_is_not_found()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("bob");

            var result = await _app.Controller<AppMessageController>("bob").Content(999999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
