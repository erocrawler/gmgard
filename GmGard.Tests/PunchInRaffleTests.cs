using System;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Controllers.App;
using GmGard.Models;
using GmGard.Models.App;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace GmGard.Tests
{
    /// <summary>签到(PunchInController.Do)与抽奖(RaffleController.PostAsync):积分扣减、重复参与、并发串行化。</summary>
    public class PunchInRaffleTests : IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        // 签到会读 HttpContext.Connection.RemoteIpAddress(ExpUtil.GetIPAddress,无空判断);夹具的 DefaultHttpContext 没有远端地址,这里补上
        private PunchInController PunchInCtrl(string user)
        {
            var c = _app.Controller<PunchInController>(user);
            c.HttpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Loopback;
            return c;
        }

        private int Uid(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).Id);
        private int Points(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).Points);
        private int Tickets(string user)
        {
            int uid = Uid(user);
            return _app.QueryUsers(db => db.UserQuests.Single(q => q.UserId == uid).PunchInTicket);
        }
        private int ConsecutiveSign(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).ConsecutiveSign);
        private int SignCount(string user)
        {
            int uid = Uid(user);
            return _app.QueryUsers(db => db.PunchInHistories.Count(p => p.UserID == uid));
        }
        private bool HasSignOn(string user, DateTime day, bool? makeup = null)
        {
            int uid = Uid(user);
            DateTime d = day.Date;
            return _app.QueryUsers(db => db.PunchInHistories.Any(p => p.UserID == uid && p.TimeStamp.Date == d && (makeup == null || p.IsMakeup == makeup.Value)));
        }

        // ================= 签到 PunchIn.Do =================

        [SkippableFact]
        public async Task PunchIn_first_sign_today_persists_one_record_and_streak_of_one()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("sign", points: 0);
            _app.SeedQuest("sign");
            var c = PunchInCtrl("sign");

            var result = await c.Do(new PunchInRequest());

            Assert.True(result is JsonResult, "实际:" + result.GetType().Name);
            Assert.Equal(1, SignCount("sign"));
            Assert.True(HasSignOn("sign", DateTime.Today, makeup: false));
            Assert.Equal(1, ConsecutiveSign("sign"));
        }

        [SkippableFact]
        public async Task PunchIn_second_sign_same_day_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("sign", points: 0);
            _app.SeedQuest("sign");

            var first = await PunchInCtrl("sign").Do(new PunchInRequest());
            var second = await PunchInCtrl("sign").Do(new PunchInRequest());

            Assert.True(first is JsonResult, "第一次实际:" + first.GetType().Name);
            Assert.IsType<BadRequestObjectResult>(second);
            Assert.Equal(1, SignCount("sign"));
        }

        [SkippableFact]
        public async Task PunchIn_makeup_day_extends_streak_and_uses_one_ticket()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("streak", points: 0);
            _app.SeedQuest("streak", punchInTickets: 1);
            await PunchInCtrl("streak").Do(new PunchInRequest());

            var makeup = await PunchInCtrl("streak")
                .Do(new PunchInRequest { Date = DateTime.Today.AddDays(-1), UseTicket = true });

            Assert.True(makeup is JsonResult, "实际:" + makeup.GetType().Name);
            Assert.Equal(2, ConsecutiveSign("streak"));
            Assert.Equal(0, Tickets("streak"));
            Assert.True(HasSignOn("streak", DateTime.Today.AddDays(-1), makeup: true));
        }

        [SkippableFact]
        public async Task PunchIn_makeup_with_ticket_but_no_tickets_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("noticket", points: 0);
            _app.SeedQuest("noticket", punchInTickets: 0);

            var result = await PunchInCtrl("noticket")
                .Do(new PunchInRequest { Date = DateTime.Today.AddDays(-1), UseTicket = true });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(0, SignCount("noticket"));
        }

        [SkippableFact]
        public async Task PunchIn_makeup_with_insufficient_points_is_rejected_and_nothing_deducted()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("broke", points: 99);
            _app.SeedQuest("broke");

            // 3 天前补签,费用 100
            var result = await PunchInCtrl("broke")
                .Do(new PunchInRequest { Date = DateTime.Today.AddDays(-3), UseTicket = false });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(99, Points("broke"));
            Assert.Equal(0, SignCount("broke"));
        }

        [SkippableFact]
        public async Task PunchIn_makeup_with_enough_points_charges_cost_and_records_makeup()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("payer", points: 100);
            _app.SeedQuest("payer");

            var result = await PunchInCtrl("payer")
                .Do(new PunchInRequest { Date = DateTime.Today.AddDays(-3), UseTicket = false });

            Assert.True(result is JsonResult, "实际:" + result.GetType().Name);
            Assert.Equal(0, Points("payer"));
            Assert.True(HasSignOn("payer", DateTime.Today.AddDays(-3), makeup: true));
        }

        [SkippableFact]
        public async Task PunchIn_ten_concurrent_requests_same_day_create_exactly_one_record()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("rush", points: 0);
            _app.SeedQuest("rush");

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                var c = PunchInCtrl("rush");
                return await c.Do(new PunchInRequest());
            })).ToArray();
            var results = await Task.WhenAll(tasks);

            Assert.Equal(1, results.Count(r => r is JsonResult));
            Assert.Equal(9, results.Count(r => r is BadRequestObjectResult));
            Assert.Equal(1, SignCount("rush"));
        }

        // ================= 抽奖 Raffle.PostAsync =================

        [SkippableFact]
        public async Task Raffle_entry_charges_cost_and_creates_one_entry()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("gambler", points: 100);
            int cfg = _app.SeedRaffleConfig(50, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            int uid = Uid("gambler");

            var result = await _app.ControllerWithId<RaffleController>("gambler").PostAsync(cfg);

            Assert.IsType<OkResult>(result);
            Assert.Equal(50, Points("gambler"));
            Assert.Equal(1, _app.QueryUsers(db => db.UserRaffles.Count(r => r.UserID == uid && r.ConfigId == cfg)));
        }

        [SkippableFact]
        public async Task Raffle_second_entry_returns_conflict_and_charges_once()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("gambler", points: 100);
            int cfg = _app.SeedRaffleConfig(50, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

            var first = await _app.ControllerWithId<RaffleController>("gambler").PostAsync(cfg);
            var second = await _app.ControllerWithId<RaffleController>("gambler").PostAsync(cfg);

            Assert.IsType<OkResult>(first);
            Assert.IsType<ConflictResult>(second);
            Assert.Equal(50, Points("gambler"));
        }

        [SkippableFact]
        public async Task Raffle_entry_with_insufficient_points_is_bad_request()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("broke", points: 49);
            int cfg = _app.SeedRaffleConfig(50, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

            var result = await _app.ControllerWithId<RaffleController>("broke").PostAsync(cfg);

            Assert.IsType<BadRequestResult>(result);
            Assert.Equal(49, Points("broke"));
            Assert.Equal(0, _app.QueryUsers(db => db.UserRaffles.Count(r => r.ConfigId == cfg)));
        }

        [SkippableFact]
        public async Task Raffle_entry_outside_active_window_is_bad_request()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("late", points: 100);
            int cfg = _app.SeedRaffleConfig(50, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-1));

            var result = await _app.ControllerWithId<RaffleController>("late").PostAsync(cfg);

            Assert.IsType<BadRequestResult>(result);
            Assert.Equal(100, Points("late"));
        }

        [SkippableFact]
        public async Task Raffle_entry_for_unknown_config_is_not_found()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("gambler", points: 100);

            var result = await _app.ControllerWithId<RaffleController>("gambler").PostAsync(999999);

            Assert.IsType<NotFoundResult>(result);
            Assert.Equal(100, Points("gambler"));
        }

        [SkippableFact]
        public async Task Raffle_ten_concurrent_entries_only_one_succeeds_and_charges_once()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("rush", points: 500);   // 足够 10 次,用于检验"只扣一次"
            int cfg = _app.SeedRaffleConfig(50, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));

            var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                var c = _app.ControllerWithId<RaffleController>("rush");
                return await c.PostAsync(cfg);
            })).ToArray();
            var results = await Task.WhenAll(tasks);

            Assert.Equal(1, results.Count(r => r is OkResult));
            Assert.Equal(9, results.Count(r => r is ConflictResult));
            Assert.Equal(450, Points("rush"));
            Assert.Equal(1, _app.QueryUsers(db => db.UserRaffles.Count(r => r.ConfigId == cfg)));
        }
    }
}
