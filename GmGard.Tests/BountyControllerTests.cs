using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GmGard.Controllers.App;
using GmGard.Models;
using GmGard.Models.App;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GmGard.Tests
{
    /// <summary>
    /// 求物悬赏的积分流转。钱相关,每个用例都用全新上下文核对积分与落库结果。
    /// Create/Answer 走 JSON 请求体(Request.HasFormContentType=false 时 action 直接从 Request.Body 反序列化)。
    /// </summary>
    public class BountyControllerTests : IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private static void SetJsonBody(ControllerBase c, string json)
        {
            c.HttpContext.Request.ContentType = "application/json";
            c.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(json));
        }

        private static string ToJson(object o) => System.Text.Json.JsonSerializer.Serialize(o);

        private int Points(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).Points);
        private Bounty Row(int id) => _app.QueryData(db => db.Bounties.Single(b => b.BountyId == id));
        private int BountyCount(string author) => _app.QueryData(db => db.Bounties.Count(b => b.Author == author));
        private int AnswerCount(int bountyId) => _app.QueryData(db => db.Answers.Count(a => a.BountyId == bountyId));
        private Answer AnswerRow(int answerId) => _app.QueryData(db => db.Answers.Single(a => a.AnswerId == answerId));

        private static string Describe(IActionResult r) => r.GetType().Name + (r is ObjectResult o ? " " + o.Value : "");

        private static CreateBountyRequest NewRequest(int prize = 40, int helpful = 20) => new CreateBountyRequest
        {
            Title = "求一个东西",
            Content = "<p>求购一个东西,内容足够长。</p>",
            ImageUrls = "",
            Prize = prize,
            HelpfulReward = helpful,
        };

        // ================= Create =================

        [SkippableFact]
        public async Task Create_charges_2x_prize_plus_helpful_and_persists_bounty()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 500);
            var c = _app.Controller<BountyController>("op");
            SetJsonBody(c, ToJson(NewRequest(prize: 40, helpful: 20)));

            var result = await c.Create();

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(400, Points("op"));   // 500 - (40 + 40 押金 + 20)

            var list = _app.QueryData(db => db.Bounties.Where(b => b.Author == "op").ToList());
            Assert.Single(list);
            var b = list[0];
            Assert.Equal(40, b.Prize);
            Assert.Equal(40, b.Deposit);
            Assert.Equal(20, b.HelpfulReward);
            Assert.False(b.IsAccepted);
            Assert.False(b.IsDeleted);
            Assert.Null(b.CloseDate);
            Assert.True(b.ExpiresAt.HasValue && b.ExpiresAt.Value > DateTime.Now.AddDays(29));
        }

        [SkippableFact]
        public async Task Create_with_exact_balance_succeeds_and_leaves_zero()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 100);
            var c = _app.Controller<BountyController>("op");
            SetJsonBody(c, ToJson(NewRequest(prize: 40, helpful: 20)));

            var result = await c.Create();

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(0, Points("op"));
            Assert.Equal(1, BountyCount("op"));
        }

        [SkippableFact]
        public async Task Create_with_insufficient_points_is_rejected_and_nothing_deducted()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("poor", points: 99);
            var c = _app.Controller<BountyController>("poor");
            SetJsonBody(c, ToJson(NewRequest(prize: 40, helpful: 20)));

            var result = await c.Create();

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(99, Points("poor"));
            Assert.Equal(0, BountyCount("poor"));
        }

        [SkippableFact]
        public async Task Create_prize_above_max_reward_is_rejected_without_charging()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 1000000);
            var c = _app.Controller<BountyController>("op");
            SetJsonBody(c, ToJson(NewRequest(prize: 100001, helpful: 20)));

            var result = await c.Create();

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1000000, Points("op"));
            Assert.Equal(0, BountyCount("op"));
        }

        [SkippableFact]
        public async Task Create_helpful_reward_above_max_reward_is_rejected_without_charging()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 1000000);
            var c = _app.Controller<BountyController>("op");
            SetJsonBody(c, ToJson(NewRequest(prize: 40, helpful: 100001)));

            var result = await c.Create();

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1000000, Points("op"));
            Assert.Equal(0, BountyCount("op"));
        }

        [SkippableFact]
        public async Task Create_int_max_amount_is_rejected_before_arithmetic_and_charges_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 1000000);

            // Prize = int.MaxValue:若先做 Prize*2 会溢出(checked 抛异常)。应在 MaxReward 校验处被拒。
            var c1 = _app.Controller<BountyController>("op");
            SetJsonBody(c1, ToJson(NewRequest(prize: int.MaxValue, helpful: 20)));
            Assert.IsType<BadRequestObjectResult>(await c1.Create());

            var c2 = _app.Controller<BountyController>("op");
            SetJsonBody(c2, ToJson(NewRequest(prize: 40, helpful: int.MaxValue)));
            Assert.IsType<BadRequestObjectResult>(await c2.Create());

            Assert.Equal(1000000, Points("op"));
            Assert.Equal(0, BountyCount("op"));
        }

        [SkippableFact]
        public async Task Create_total_below_minimum_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 1000);
            var c = _app.Controller<BountyController>("op");
            // 40 + 40 + 0 = 80 < MinTotal(100)
            SetJsonBody(c, ToJson(NewRequest(prize: 40, helpful: 0)));

            Assert.IsType<BadRequestObjectResult>(await c.Create());
            Assert.Equal(1000, Points("op"));
            Assert.Equal(0, BountyCount("op"));
        }

        [SkippableFact]
        public async Task Create_without_title_is_rejected_before_charging()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 1000);
            var c = _app.Controller<BountyController>("op");
            var req = NewRequest();
            req.Title = null;   // JSON 路径不跑 ModelState,[Required] 不生效
            SetJsonBody(c, ToJson(req));

            var result = await c.Create();

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(1000, Points("op"));
            Assert.Equal(0, BountyCount("op"));
        }

        // ================= Close =================

        [SkippableFact]
        public async Task Close_by_author_refunds_full_amount()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);

            var result = await _app.Controller<BountyController>("op").Close(id);

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(100, Points("op"));
            Assert.NotNull(Row(id).CloseDate);
            Assert.False(Row(id).IsAccepted);
        }

        [SkippableFact]
        public async Task Close_by_admin_refunds_author_not_admin()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            _app.SeedUser("admin", points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);

            var result = await _app.Controller<BountyController>("admin", "Administrator").Close(id);

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(100, Points("op"));
            Assert.Equal(0, Points("admin"));
            Assert.NotNull(Row(id).CloseDate);
        }

        [SkippableFact]
        public async Task Close_by_stranger_is_forbidden_and_refunds_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            _app.SeedUser("eve", points: 0);
            int id = _app.SeedBounty("op");

            var result = await _app.Controller<BountyController>("eve").Close(id);

            Assert.IsType<ForbidResult>(result);
            Assert.Equal(0, Points("op"));
            Assert.Null(Row(id).CloseDate);
        }

        [SkippableFact]
        public async Task Close_twice_is_rejected_and_does_not_refund_again()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            int id = _app.SeedBounty("op");

            await _app.Controller<BountyController>("op").Close(id);
            var second = await _app.Controller<BountyController>("op").Close(id);

            Assert.IsType<BadRequestObjectResult>(second);
            Assert.Equal(100, Points("op"));
        }

        // ================= Delete =================

        [SkippableFact]
        public async Task Delete_by_author_unanswered_open_bounty_refunds_full_amount()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);

            var result = await _app.Controller<BountyController>("op").Delete(id);

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(100, Points("op"));
            Assert.True(Row(id).IsDeleted);
        }

        [SkippableFact]
        public async Task Delete_of_already_closed_bounty_does_not_refund_again()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            await _app.Controller<BountyController>("op").Close(id);   // 关闭时已退 100
            Assert.Equal(100, Points("op"));

            var result = await _app.Controller<BountyController>("op").Delete(id);

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(100, Points("op"));   // 不能再退一次
            Assert.True(Row(id).IsDeleted);
        }

        [SkippableFact]
        public async Task Delete_with_answers_is_rejected_and_nothing_refunded()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            _app.SeedUser("ans", points: 0);
            int id = _app.SeedBounty("op");
            _app.SeedAnswer(id, "ans");

            var result = await _app.Controller<BountyController>("op").Delete(id);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, Points("op"));
            Assert.False(Row(id).IsDeleted);
        }

        [SkippableFact]
        public async Task Delete_by_non_author_is_forbidden_and_refunds_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            _app.SeedUser("eve", points: 0);
            int id = _app.SeedBounty("op");

            var result = await _app.Controller<BountyController>("eve").Delete(id);

            Assert.IsType<ForbidResult>(result);
            Assert.Equal(0, Points("op"));
            Assert.Equal(0, Points("eve"));
            Assert.False(Row(id).IsDeleted);
        }

        // ================= Answer =================

        [SkippableFact]
        public async Task Answer_on_open_bounty_is_saved()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op");
            _app.SeedUser("helper");
            int id = _app.SeedBounty("op");
            var c = _app.Controller<BountyController>("helper");
            SetJsonBody(c, ToJson(new CreateAnswerRequest { BountyId = id, Content = "<p>我有这个东西</p>" }));

            var result = await c.Answer();

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            var answers = _app.QueryData(db => db.Answers.Where(a => a.BountyId == id).ToList());
            Assert.Single(answers);
            Assert.Equal("helper", answers[0].Author);
            Assert.Contains("我有这个东西", answers[0].Content);
            Assert.False(answers[0].IsHelpful);
        }

        [SkippableFact]
        public async Task Answer_by_bounty_author_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op");
            int id = _app.SeedBounty("op");
            var c = _app.Controller<BountyController>("op");
            SetJsonBody(c, ToJson(new CreateAnswerRequest { BountyId = id, Content = "<p>自问自答</p>" }));

            Assert.IsType<BadRequestObjectResult>(await c.Answer());
            Assert.Equal(0, AnswerCount(id));
        }

        [SkippableFact]
        public async Task Answer_on_accepted_bounty_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op");
            _app.SeedUser("helper");
            int id = _app.SeedBounty("op", accepted: true);
            var c = _app.Controller<BountyController>("helper");
            SetJsonBody(c, ToJson(new CreateAnswerRequest { BountyId = id, Content = "<p>晚了</p>" }));

            Assert.IsType<BadRequestObjectResult>(await c.Answer());
            Assert.Equal(0, AnswerCount(id));
        }

        [SkippableFact]
        public async Task Answer_on_deleted_bounty_is_not_found()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op");
            _app.SeedUser("helper");
            int id = _app.SeedBounty("op", deleted: true);
            var c = _app.Controller<BountyController>("helper");
            SetJsonBody(c, ToJson(new CreateAnswerRequest { BountyId = id, Content = "<p>想回答</p>" }));

            Assert.IsType<NotFoundResult>(await c.Answer());
            Assert.Equal(0, AnswerCount(id));
        }

        [SkippableFact]
        public async Task Answer_with_blank_content_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op");
            _app.SeedUser("helper");
            int id = _app.SeedBounty("op");
            var c = _app.Controller<BountyController>("helper");
            SetJsonBody(c, ToJson(new CreateAnswerRequest { BountyId = id, Content = "   " }));

            Assert.IsType<BadRequestObjectResult>(await c.Answer());
            Assert.Equal(0, AnswerCount(id));
        }

        // ================= Accept =================

        private static AcceptRequest Accept(int bountyId, int best, params int[] helpful) =>
            new AcceptRequest { BountyId = bountyId, BestAnswerId = best, HelpfulAnswerIds = helpful };

        [SkippableFact]
        public async Task Accept_pays_prize_deposit_and_helpful_split_exactly()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best", "h1", "h2" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            int best = _app.SeedAnswer(id, "best");
            int h1 = _app.SeedAnswer(id, "h1");
            int h2 = _app.SeedAnswer(id, "h2");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, best, h1, h2));

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(40, Points("best"));          // 最佳答案 = Prize
            Assert.Equal(10, Points("h1"));            // 热心 20 / 2
            Assert.Equal(10, Points("h2"));
            Assert.Equal(40, Points("op"));            // 押金退回,无余数
            Assert.Equal(100, Points("best") + Points("h1") + Points("h2") + Points("op"));

            var b = Row(id);
            Assert.True(b.IsAccepted);
            Assert.Equal(best, b.AcceptedAnswerId);
            Assert.NotNull(b.CloseDate);
            Assert.False(AnswerRow(best).IsHelpful);
            Assert.True(AnswerRow(h1).IsHelpful);
            Assert.True(AnswerRow(h2).IsHelpful);
        }

        [SkippableFact]
        public async Task Accept_remainder_of_helpful_split_goes_to_op()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best", "h1", "h2", "h3" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            int best = _app.SeedAnswer(id, "best");
            int h1 = _app.SeedAnswer(id, "h1");
            int h2 = _app.SeedAnswer(id, "h2");
            int h3 = _app.SeedAnswer(id, "h3");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, best, h1, h2, h3));

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(6, Points("h1"));             // 20 / 3 = 6 余 2
            Assert.Equal(6, Points("h2"));
            Assert.Equal(6, Points("h3"));
            Assert.Equal(42, Points("op"));            // 押金 40 + 余数 2
            Assert.Equal(40, Points("best"));
            Assert.Equal(100, Points("best") + Points("h1") + Points("h2") + Points("h3") + Points("op"));
        }

        [SkippableFact]
        public async Task Accept_without_helpful_answers_refunds_helpful_reward_to_op()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            _app.SeedUser("best", points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            int best = _app.SeedAnswer(id, "best");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, best));

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(40, Points("best"));
            Assert.Equal(60, Points("op"));            // 押金 40 + 无人热心时的 20
        }

        [SkippableFact]
        public async Task Accept_duplicate_helpful_author_is_paid_once()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best", "h1", "h2" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            int best = _app.SeedAnswer(id, "best");
            int h1a = _app.SeedAnswer(id, "h1");
            int h1b = _app.SeedAnswer(id, "h1");   // 同一人两条回答,只算一份
            int h2 = _app.SeedAnswer(id, "h2");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, best, h1a, h1b, h2));

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(10, Points("h1"));            // 2 个不同作者:20 / 2
            Assert.Equal(10, Points("h2"));
            Assert.Equal(40, Points("op"));
        }

        [SkippableFact]
        public async Task Accept_by_admin_pays_out_same_rules()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best", "mod" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            int best = _app.SeedAnswer(id, "best");

            var result = await _app.Controller<BountyController>("mod", "Moderator").Accept(Accept(id, best));

            Assert.True(result is JsonResult, "实际:" + Describe(result));
            Assert.Equal(40, Points("best"));
            Assert.Equal(60, Points("op"));
            Assert.Equal(0, Points("mod"));
        }

        [SkippableFact]
        public async Task Accept_by_stranger_is_forbidden_and_pays_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best", "eve" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op");
            int best = _app.SeedAnswer(id, "best");

            var result = await _app.Controller<BountyController>("eve").Accept(Accept(id, best));

            Assert.IsType<ForbidResult>(result);
            Assert.Equal(0, Points("best"));
            Assert.Equal(0, Points("op"));
            Assert.False(Row(id).IsAccepted);
        }

        [SkippableFact]
        public async Task Accept_best_answer_written_by_op_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("op", points: 0);
            int id = _app.SeedBounty("op");
            int own = _app.SeedAnswer(id, "op");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, own));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, Points("op"));
            Assert.False(Row(id).IsAccepted);
        }

        [SkippableFact]
        public async Task Accept_with_helpful_author_equal_to_best_author_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op");
            int best = _app.SeedAnswer(id, "best");
            int dup = _app.SeedAnswer(id, "best");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, best, dup));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, Points("best"));
            Assert.False(Row(id).IsAccepted);
        }

        [SkippableFact]
        public async Task Accept_twice_does_not_pay_again()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op", prize: 40, deposit: 40, helpfulReward: 20);
            int best = _app.SeedAnswer(id, "best");

            await _app.Controller<BountyController>("op").Accept(Accept(id, best));
            var second = await _app.Controller<BountyController>("op").Accept(Accept(id, best));

            Assert.IsType<BadRequestObjectResult>(second);
            Assert.Equal(40, Points("best"));
            Assert.Equal(60, Points("op"));
        }

        [SkippableFact]
        public async Task Accept_on_closed_bounty_is_rejected_and_pays_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            foreach (var u in new[] { "op", "best" }) _app.SeedUser(u, points: 0);
            int id = _app.SeedBounty("op", closed: true);
            int best = _app.SeedAnswer(id, "best");

            var result = await _app.Controller<BountyController>("op").Accept(Accept(id, best));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, Points("best"));
            Assert.Equal(0, Points("op"));
        }
    }
}
