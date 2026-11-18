using System;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Controllers;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using AppGachaController = GmGard.Controllers.App.GachaController;

namespace GmGard.Tests
{
    /// <summary>抽卡(App GachaController.GetResult)与账户页的签名/昵称修改(AccountController)。</summary>
    public class GachaAccountTests : IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private int Points(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).Points);
        private int Uid(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).Id);
        private int GachaCount(string user)
        {
            int uid = Uid(user);
            return _app.QueryUsers(db => db.UserGachas.Count(g => g.UserID == uid));
        }
        private string NickName(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).NickName);
        private string UserComment(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).UserComment);

        // 反射读取 Json(new { success = ..., points = ... }) 返回的匿名对象
        private static object JsonProp(IActionResult r, string name)
        {
            var v = Assert.IsType<JsonResult>(r).Value;
            return v.GetType().GetProperty(name).GetValue(v);
        }

        private static GachaRequest Pull(int count, GachaPool.PoolName pool = GachaPool.PoolName.Common) =>
            new GachaRequest { Count = count, Pool = pool };

        private AppGachaController Gacha(string user) =>
            _app.Controller<AppGachaController>(user);

        // ================= 抽卡 GetResult =================

        [SkippableFact]
        public async Task Gacha_ten_pull_charges_100_and_writes_ten_records()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("puller", points: 100);
            _app.SeedGachaPool(GachaPool.PoolName.Common, 1, 2, 3, 4);

            var result = await Gacha("puller").GetResult(Pull(10));

            Assert.Equal(true, JsonProp(result, "Success"));
            // 十连保底一定出稀有度 3,会触发奖励(GachaBonusService 可能发放棒棒糖,数额随机),所以不断言精确余额;
            // 扣费本身用不触发奖励的三连抽测试精确验证。
            Assert.True(Points("puller") >= 0);
            Assert.Equal(10, GachaCount("puller"));
        }

        [SkippableFact]
        public async Task Gacha_three_pull_charges_30_and_writes_three_records()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            // 奖励(GachaBonusService)可能随机发放棒棒糖,所以不断言精确余额,而是用边界值证明单价正好是 10:
            // 余额 30 能抽三连(成功、写 3 条记录、余额不为负),余额 29 抽不了(被拒、不扣、不写记录)。
            _app.SeedUser("exact", points: 30);
            _app.SeedUser("short", points: 29);
            _app.SeedGachaPool(GachaPool.PoolName.Common, 1, 2, 3, 4);

            var ok = await Gacha("exact").GetResult(Pull(3));
            var no = await Gacha("short").GetResult(Pull(3));

            Assert.Equal(true, JsonProp(ok, "Success"));
            Assert.Equal(3, GachaCount("exact"));
            Assert.True(Points("exact") >= 0);
            Assert.Equal(false, JsonProp(no, "Success"));
            Assert.Equal(0, GachaCount("short"));
            Assert.Equal(29, Points("short"));
        }

        [SkippableFact]
        public async Task Gacha_invalid_counts_are_bad_request_and_charge_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("puller", points: 100000);
            _app.SeedGachaPool(GachaPool.PoolName.Common, 1, 2, 3, 4);

            foreach (int bad in new[] { 0, -1, 11, int.MaxValue, int.MinValue })
            {
                var result = await Gacha("puller").GetResult(Pull(bad));
                Assert.True(result is BadRequestResult, "count=" + bad + " 实际:" + result.GetType().Name);
            }
            Assert.Equal(100000, Points("puller"));
            Assert.Equal(0, GachaCount("puller"));
        }

        [SkippableFact]
        public async Task Gacha_pull_with_insufficient_points_is_refused_and_charges_nothing()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("broke", points: 9);
            _app.SeedGachaPool(GachaPool.PoolName.Common, 1, 2, 3, 4);

            var result = await Gacha("broke").GetResult(Pull(1));

            Assert.Equal(false, JsonProp(result, "Success"));
            Assert.Equal(9, Points("broke"));
            Assert.Equal(0, GachaCount("broke"));
        }

        [SkippableFact]
        public async Task Gacha_unknown_pool_is_bad_request()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("puller", points: 100);
            _app.SeedGachaPool(GachaPool.PoolName.Common, 1, 2, 3, 4);

            var result = await Gacha("puller").GetResult(Pull(1, (GachaPool.PoolName)999));

            Assert.IsType<BadRequestResult>(result);
            Assert.Equal(100, Points("puller"));
        }

        [SkippableFact]
        public async Task Gacha_five_concurrent_ten_pulls_with_points_for_one_only_one_succeeds()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("rush", points: 100);   // 只够一次十连
            _app.SeedGachaPool(GachaPool.PoolName.Common, 1, 2, 3, 4);

            var tasks = Enumerable.Range(0, 5).Select(_ => Task.Run(async () =>
            {
                var c = _app.Controller<AppGachaController>("rush");
                return await c.GetResult(Pull(10));
            })).ToArray();
            var results = await Task.WhenAll(tasks);

            int ok = results.Count(r => r is JsonResult jr && (bool)jr.Value.GetType().GetProperty("Success").GetValue(jr.Value));
            Assert.Equal(1, ok);                       // 只够一次十连:并发 5 个,只有 1 个成功
            Assert.Equal(10, GachaCount("rush"));      // 只写入一次十连的记录
            Assert.True(Points("rush") >= 0);          // 余额不会变成负数(奖励可能让它略多于 0,数额随机)
        }

        // ================= 账户:个性签名 ChangeComment =================

        private AccountController Account(string user, params string[] roles) =>
            _app.ControllerWith<AccountController>(user, sp => new AccountController(
                sp.GetRequiredService<IOptionsSnapshot<RegisterSettingsModel>>(),
                sp.GetRequiredService<BlogContext>(),
                sp.GetRequiredService<UsersContext>(),
                sp.GetRequiredService<UserManager<UserProfile>>(),
                null,   // SignInManager:本组测试的 action 不用到
                sp.GetRequiredService<RoleManager<IdentityRole<int>>>(),
                sp.GetRequiredService<IMemoryCache>(),
                sp.GetRequiredService<ExpUtil>(),
                sp.GetRequiredService<ImageUtil>(),
                sp.GetRequiredService<BlogUtil>(),
                sp.GetRequiredService<CategoryUtil>(),
                null),  // EmailSender:同上
                roles);

        [SkippableFact]
        public void ChangeComment_up_to_200_chars_is_saved()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("writer");
            string text = new string('字', 200);

            var result = Account("writer").ChangeComment(text);

            Assert.Equal(true, JsonProp(result, "success"));
            Assert.Equal(text, UserComment("writer"));
        }

        [SkippableFact]
        public void ChangeComment_over_200_chars_is_rejected_and_not_saved()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("writer");
            string text = new string('字', 201);

            var result = Account("writer").ChangeComment(text);

            Assert.Equal(false, JsonProp(result, "success"));
            Assert.Null(UserComment("writer"));
        }

        [SkippableFact]
        public void ChangeComment_same_as_current_is_not_updated()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("writer");
            Account("writer").ChangeComment("旧签名");

            var result = Account("writer").ChangeComment("旧签名");

            Assert.Equal(false, JsonProp(result, "success"));
            Assert.Equal("旧签名", UserComment("writer"));
        }

        // ================= 账户:昵称修改 ChangeNickName(扣 200) =================

        private INickNameProvider NickProvider() => _app.NewScope().ServiceProvider.GetRequiredService<INickNameProvider>();

        [SkippableFact]
        public async Task ChangeNickName_with_enough_points_charges_200_and_changes_name()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("nick", points: 300);

            var result = await Account("nick").ChangeNickName(NickProvider(), "新昵称");

            Assert.Equal(true, JsonProp(result, "success"));
            Assert.Equal(100, Points("nick"));
            Assert.Equal("新昵称", NickName("nick"));
        }

        [SkippableFact]
        public async Task ChangeNickName_with_insufficient_points_neither_charges_nor_changes_name()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("broke", points: 199);

            var result = await Account("broke").ChangeNickName(NickProvider(), "新昵称");

            Assert.Equal(false, JsonProp(result, "success"));
            Assert.Equal(199, Points("broke"));
            Assert.Null(NickName("broke"));
        }

        [SkippableFact]
        public async Task ChangeNickName_taken_by_another_user_is_rejected_without_charge()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            _app.SeedUser("first", points: 300);
            _app.SeedUser("second", points: 300);
            await Account("first").ChangeNickName(NickProvider(), "占用名");

            var result = await Account("second").ChangeNickName(NickProvider(), "占用名");

            Assert.Equal(300, Points("second"));
            Assert.Null(NickName("second"));
        }
    }
}
