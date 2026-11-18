using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GmGard.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AppAccountController = GmGard.Controllers.App.AccountController;

namespace GmGard.Tests
{
    /// <summary>
    /// M8:关闭两步验证、查看共享密钥、重新生成应急密码都要求重新输入账号密码
    /// (XSS 或被窃取的会话 Cookie 不能静默关闭 2FA;用应急密码登录的用户第一步输入过密码,所以应急流程不受影响)。
    /// </summary>
    public class TwoFactorTests : IDisposable
    {
        private const string Pwd = "Passw0rd!";
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private async Task SeedUserWithPassword(string name, bool twoFactorEnabled)
        {
            using var scope = _app.NewScope();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<UserProfile>>();
            var user = new UserProfile
            {
                UserName = name, Email = name + "@t.test", Points = 0, Level = 1,
                CreateDate = DateTime.Now.AddYears(-1), LastLoginDate = DateTime.Now,
            };
            var r = await um.CreateAsync(user, Pwd);
            Assert.True(r.Succeeded, string.Join(";", r.Errors.Select(e => e.Description)));
            if (twoFactorEnabled)
            {
                await um.ResetAuthenticatorKeyAsync(user);
                await um.SetTwoFactorEnabledAsync(user, true);
            }
        }

        private AppAccountController Account(string user)
        {
            var c = _app.ControllerWith<AppAccountController>(user, sp => new AppAccountController(
                sp.GetRequiredService<UserManager<UserProfile>>(), null, sp.GetRequiredService<RoleManager<IdentityRole<int>>>(), null, null, UrlEncoder.Default));
            int uid = _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).Id);
            c.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user), new Claim(ClaimTypes.NameIdentifier, uid.ToString()),
            }, "test", ClaimTypes.Name, ClaimTypes.Role));
            return c;
        }

        private bool TwoFactorEnabled(string user) => _app.QueryUsers(db => db.Users.Single(u => u.UserName == user).TwoFactorEnabled);

        // ---------- Disable2Fa ----------

        [SkippableTheory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("wrong-password")]
        public async Task Disable2Fa_without_or_with_wrong_password_is_rejected_and_keeps_2fa(string password)
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);

            var result = await Account("alice").Disable2Fa(password, reset: false);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.True(TwoFactorEnabled("alice"));
        }

        [SkippableFact]
        public async Task Disable2Fa_with_correct_password_disables_2fa()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);

            var result = await Account("alice").Disable2Fa(Pwd, reset: false);

            Assert.IsType<OkResult>(result);
            Assert.False(TwoFactorEnabled("alice"));
        }

        [SkippableFact]
        public async Task Disable2Fa_password_of_another_user_does_not_work()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);
            using (var scope = _app.NewScope())
            {
                var um = scope.ServiceProvider.GetRequiredService<UserManager<UserProfile>>();
                var other = new UserProfile { UserName = "bob", Email = "bob@t.test", CreateDate = DateTime.Now, LastLoginDate = DateTime.Now };
                Assert.True((await um.CreateAsync(other, "An0ther-Pass!")).Succeeded);
            }

            var result = await Account("alice").Disable2Fa("An0ther-Pass!", reset: false);

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.True(TwoFactorEnabled("alice"));
        }

        // ---------- GenerateRecoveryCodes ----------

        [SkippableFact]
        public async Task GenerateRecoveryCodes_with_wrong_password_is_rejected()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);

            var result = await Account("alice").GenerateRecoveryCodes("nope");

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [SkippableFact]
        public async Task GenerateRecoveryCodes_with_correct_password_returns_codes()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);

            var result = await Account("alice").GenerateRecoveryCodes(Pwd);

            var json = Assert.IsType<JsonResult>(result);
            var codes = Assert.IsAssignableFrom<System.Collections.Generic.IEnumerable<string>>(json.Value);
            Assert.NotEmpty(codes);
        }

        // ---------- Get2FaKeys / Show2FaKeys ----------

        [SkippableFact]
        public async Task Get2FaKeys_when_2fa_enabled_requires_password()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);

            var result = await Account("alice").Get2FaKeys();

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(403, obj.StatusCode);
            Assert.Contains("password_required", obj.Value.ToString());
        }

        [SkippableFact]
        public async Task Get2FaKeys_when_2fa_not_enabled_returns_key_for_setup()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: false);

            var result = await Account("alice").Get2FaKeys();

            Assert.IsType<JsonResult>(result);
        }

        [SkippableFact]
        public async Task Show2FaKeys_with_wrong_password_is_rejected_and_with_correct_password_returns_key()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            await SeedUserWithPassword("alice", twoFactorEnabled: true);

            Assert.IsType<BadRequestObjectResult>(await Account("alice").Show2FaKeys("nope"));
            Assert.IsType<BadRequestObjectResult>(await Account("alice").Show2FaKeys(null));
            Assert.IsType<JsonResult>(await Account("alice").Show2FaKeys(Pwd));
        }
    }
}
