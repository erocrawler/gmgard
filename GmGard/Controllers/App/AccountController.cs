using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GmGard.Models;
using AspNet.Identity.EntityFramework6;
using GmGard.Filters;
using Microsoft.AspNetCore.Antiforgery;
using GmGard.Services;
using System.Text;
using System.Text.Encodings.Web;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Account/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class AccountController : AppControllerBase
    {
        private readonly UserManager<UserProfile> _userManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly RoleManager<AspNet.Identity.EntityFramework6.IdentityRole> _roleManager;
        private readonly IAntiforgery _antiforgery;
        private readonly INickNameProvider _nickNameProvider;
        private readonly UrlEncoder _urlEncoder;

        public AccountController(
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager,
            RoleManager<AspNet.Identity.EntityFramework6.IdentityRole> roleManager,
            INickNameProvider nickNameProvider,
            IAntiforgery antiforgery,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _antiforgery = antiforgery;
            _nickNameProvider = nickNameProvider;
            _urlEncoder = urlEncoder;
        }
        
        public JsonResult IsAuthenticated()
        {
            return Json(new { User.Identity.IsAuthenticated });
        }

        public async Task<ActionResult> GetUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }
                username = User.Identity.Name;
            }
            var profile = await _userManager.FindByNameAsync(username);
            if (profile == null)
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(profile);
            var user = Models.App.User.FromUserProfile(profile, Url.Action("Show", "Avatar", new { name = profile.UserName }, Request.Scheme));
            user.NickName = _nickNameProvider.GetNickName(user.UserName);
            user.Avatar = Url.Action("Show", "Avatar", new { name = profile.UserName }, Request.Scheme);
            user.Roles = roles;
            return Json(user);
        }

        public JsonResult GetAntiForgeryToken()
        {
            var tokenset = _antiforgery.GetAndStoreTokens(HttpContext);
            return Json(tokenset);
        }

        [HttpPost]
        public async Task<JsonResult> Login([FromServices]BlogUtil util, [FromBody]LoginModel model)
        {
            if (util.CheckCaptchaError(model.Captcha, ""))
            {
                ModelState.AddModelError("Captcha", "验证码计算错误，请重试。");
            }
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Banned"))
                    {
                        return Json(new
                        {
                            success = false,
                            errors = new[] { "此账户已被封禁，如有疑问请联系管理员邮箱。" },
                        });
                    }
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        user.LastLoginDate = DateTime.Now;
                        user.LastLoginIP = ExpUtil.GetIPAddress(HttpContext);
                        await _userManager.UpdateAsync(user);
                        return Json(new { success = true });
                    }
                    if (result.IsLockedOut)
                    {
                        return Json(new
                        {
                            success = false,
                            errors = new[] { "此账户由于登陆尝试次数过多已被暂时锁定，请稍后再试。" },
                        });
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        return Json(new
                        {
                            success = true,
                            require2fa = true,
                        });
                    }
                }
                // 如果我们进行到这一步时某个地方出代楷则重新显示表单
                ModelState.AddModelError("", "提供的用户名或密码不正确");
            }

            return Json(new {
                success = false,
                errors = ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> TwoFactorAuth(Login2FaModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"两步验证用户读取异常。");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            if (result.IsLockedOut)
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "此账户由于登陆尝试次数过多已被暂时锁定，请稍后再试。" },
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "秘钥错误" },
                });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RecoveryCode(LoginWithRecoveryCodeModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    errors = ModelState.Values.SelectMany(m => m.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"两步验证用户读取异常。");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            if (result.IsLockedOut)
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "此账户由于登陆尝试次数过多已被暂时锁定，请稍后再试。" },
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    errors = new[] { "无效的应急密码" },
                });
            }
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return Ok();
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Manage2Fa()
        {
            var user = await _userManager.GetUserAsync(User);
            return Json(new Models.App.TwoFactorAuthenticationModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            });
        }

        [HttpGet, Authorize]
        public async Task<ActionResult> Get2FaKeys()
        {
            var user = await _userManager.GetUserAsync(User);
            return Json(await LoadSharedKeyAndQrCodeUriAsync(user));
        }

        [HttpPost, Authorize]
        public async Task<ActionResult> Enable2Fa(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest();
            }
            var verificationCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var user = await _userManager.GetUserAsync(User);
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                return BadRequest(new { error = "秘钥错误" });
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                return Json(recoveryCodes);
            }
            else
            {
                return Ok();
            }
        }

        [HttpPost, Authorize]
        public async Task<ActionResult> Disable2Fa(bool reset = false)
        {
            var user = await _userManager.GetUserAsync(User);
            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (reset)
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                await _signInManager.RefreshSignInAsync(user);
            }
            return Ok();
        }

        [HttpPost, Authorize]
        public async Task<ActionResult> GenerateRecoveryCodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                return BadRequest($"Cannot generate recovery codes for user with ID '{userId}' as they do not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            return Json(recoveryCodes);
        }

        [HttpPost, Authorize]
        public async Task<ActionResult> ForgetClient()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            return Ok();
        }

        private async Task<Models.App.TwoFactorAuthSharedKey> LoadSharedKeyAndQrCodeUriAsync(UserProfile user)
        {
            // Load the authenticator key & QR code URI to display on the form
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            var email = await _userManager.GetEmailAsync(user);
            return new Models.App.TwoFactorAuthSharedKey
            {
                SharedKey = FormatKey(unformattedKey),
                AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey),
            };
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("GmGard"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
    }
}