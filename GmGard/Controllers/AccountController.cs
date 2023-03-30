using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GmGard.Filters;
using GmGard.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using AspNetCore.Identity.EntityFramework6;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Data.Entity;
using GmGard.Extensions;
using System.Collections.Generic;

namespace GmGard.Controllers
{
    [Authorize, ResponseCache(CacheProfileName = "Never")]
    public class AccountController : Controller
    {
        public AccountController(
            IOptionsSnapshot<RegisterSettingsModel> registerSettings,
            BlogContext bdb,
            UsersContext udb,
            UserManager<UserProfile> userManager,
            SignInManager<UserProfile> signInManager,
            RoleManager<AspNetCore.Identity.EntityFramework6.IdentityRole> roleManager,
            IMemoryCache cache,
            ExpUtil expUtil,
            ImageUtil imgUtil,
            BlogUtil blogUtil,
            CategoryUtil catUtil,
            EmailSender emailSender)
        {
            _registerSettings = registerSettings.Value ?? new RegisterSettingsModel();
            _db = udb;
            _bdb = bdb;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _expUtil = expUtil;
            _imgUtil = imgUtil;
            _blogUtil = blogUtil;
            _catUtil = catUtil;
            _cache = cache;
            _emailSender = emailSender;
        }

        private readonly UserManager<UserProfile> _userManager;
        private readonly SignInManager<UserProfile> _signInManager;
        private readonly RoleManager<AspNetCore.Identity.EntityFramework6.IdentityRole> _roleManager;
        private readonly IMemoryCache _cache;
        private RegisterSettingsModel _registerSettings;
        private BlogContext _bdb;
        private UsersContext _db;
        private ExpUtil _expUtil;
        private ImageUtil _imgUtil;
        private BlogUtil _blogUtil;
        private CategoryUtil _catUtil;
        private EmailSender _emailSender;
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken, ValidateCaptcha]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Banned"))
                    {
                        ModelState.AddModelError("", "此账户已被封禁，如有疑问请联系管理员。");
                        return View();
                    }
                    var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                    if (result.Succeeded)
                    {
                        user.LastLoginDate = DateTime.Now;
                        user.LastLoginIP = ExpUtil.GetIPAddress(HttpContext);
                        await _userManager.UpdateAsync(user);
                        return RedirectToLocal(returnUrl);
                    }
                    if (result.IsLockedOut)
                    {
                        return RedirectToAction("Lockout");
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        return RedirectToAction(nameof(TwoFactorAuth), new { ReturnUrl = returnUrl });
                    }
                }
                // 如果我们进行到这一步时某个地方出错，则重新显示表单
                ModelState.AddModelError("", "提供的用户名或密码不正确。");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> TwoFactorAuth(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }
            return View(new Login2FaModel { ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> TwoFactorAuth(Login2FaModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"两步认证用户读取异常。");
            }

            var authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, model.RememberMachine);

            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "无效的验证码。");
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> RecoveryCode(string returnUrl = null)
        {
            // Ensure the user has gone through the username & password screen first
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return RedirectToAction(nameof(Login), new { ReturnUrl = returnUrl });
            }

            return View(new LoginWithRecoveryCodeModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RecoveryCode(LoginWithRecoveryCodeModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"两步认证用户读取异常。");
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "无效的应急密码");
                return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Lockout()
        {
            return View();
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            var model = new RegisterModel { X = 0, Y = 0, W = 500, H = 500, RegisterQuestionIndex = -1 };
            var setting = _registerSettings;
            if (setting.RegisterQuestionEnabled && setting.RegisterQuestions != null && setting.RegisterQuestions.Count > 0)
            {
                var questions = setting.RegisterQuestions;
                Random r = new Random();
                model.RegisterQuestionIndex = r.Next(questions.Count);
            }
            if (Request.IsAjaxRequest() && (setting == null || setting.AllowBackdoor))
            {
                return PartialView(model);
            }
            
            ViewBag.AllowBackdoor = setting == null ? true : setting.AllowBackdoor;
            string viewname = AllowRegister() ? "Register" : "RegisterClosed";
            return View(viewname, model);
        }

        protected bool AllowRegister()
        {
            var setting = _registerSettings;
            if (setting.AllowRegister || (Request.Method == "POST" && setting.AllowBackdoor))
            {
                return true;
            }
            if (User.Identity.IsAuthenticated && setting.AllowMemberRegisterLevel > 0)
            {
                return _expUtil.getUserLvl(User.Identity.Name) >= setting.AllowMemberRegisterLevel;
            }
            return false;
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken, ValidateCaptcha]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid && AllowRegister())
            {
                if (_registerSettings.RegisterQuestionEnabled && _registerSettings.RegisterQuestions != null && _registerSettings.RegisterQuestions.Count > 0)
                {
                    var questions = _registerSettings.RegisterQuestions;
                    if (model.RegisterQuestionIndex < 0 || model.RegisterQuestionIndex >= questions.Count)
                    {
                        ModelState.AddModelError("RegisterAnswer", "请重新回答注册问题");
                        Random r = new Random();
                        model.RegisterQuestionIndex = r.Next(questions.Count);
                        return View(model);
                    }
                    else if (!questions[model.RegisterQuestionIndex].Answer.Equals(model.RegisterAnswer, StringComparison.OrdinalIgnoreCase))
                    {
                        ModelState.AddModelError("RegisterAnswer", "回答错误");
                        return View(model);
                    }
                }
                if (string.IsNullOrWhiteSpace(model.NickName))
                {
                    model.NickName = model.UserName;
                    if (CheckNickName(model.NickName) != null)
                    {
                        ModelState.SetModelValue("NickName", new ValueProviderResult(model.UserName));
                        ModelState.AddModelError("NickName", "昵称已被使用，请更改。");
                        return View(model);
                    }
                }
                Guid code = new Guid();
                if (_registerSettings.RequireCode)
                {
                    if (!Guid.TryParse(model.RegisterCode, out code) || !_db.UserCodes.Any(c => c.UsedBy == null && c.Code == code))
                    {
                        ModelState.AddModelError("RegisterCode", "无效的邀请码");
                        return View(model);
                    }
                }
                // 尝试注册用户
                var user = new UserProfile
                {
                    UserName = model.UserName,
                    NickName = model.NickName,
                    Email = model.Email,
                    LastLoginIP = ExpUtil.GetIPAddress(HttpContext),
                    LastLoginDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    Points = 0,
                    ConsecutiveSign = 0,
                    Experience = 1,
                    Level = 1,
                    LastSignDate = System.Data.SqlTypes.SqlDateTime.MinValue.Value,
                };
                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (createResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (_registerSettings.RequireCode)
                    {
                        _db.UserCodes.Find(code).UsedBy = user.Id;
                        _db.SaveChanges();
                    }

                    if (model.avatar != null)
                    {
                        byte[] fileData = null;
                        using (var reader = new BinaryReader(model.avatar.OpenReadStream()))
                        {
                            fileData = reader.ReadBytes((int)model.avatar.Length);
                            _imgUtil.AddPic(model.UserName, model.avatar.ContentType, _imgUtil.Crop(fileData, model.W, model.H, model.X, model.Y));
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }
                AddErrors(createResult);
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return View(model);
        }

        //
        // GET: /Account/Manage
        public async Task<ActionResult> Manage(ManageMessageId? message)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");
            var user = await GetCurrentUserAsync();
            ViewBag.HpSettings = GetHpSettings();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(LocalPasswordModel model)
        {
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    ViewBag.Success = true;
                    return PartialView("_ChangePasswordPartial");
                }
                else
                {
                    AddErrors(result);
                }
            }

            // 如果我们进行到这一步时某个地方出错，则重新显示表单
            return PartialView("_ChangePasswordPartial", model);
        }

        [HttpPost]
        public ActionResult ChangeAvatar(string avatardata, string filetype, string x, string y, string w, string h, string op)
        {
            byte[] b = Convert.FromBase64String(avatardata);
            if (avatardata == null || !filetype.Contains("image") || b.Length > 524288 * 2)
            {
                ModelState.AddModelError("", "头像必须是图片，且小于1MB。");
            }
            else
            {
                int width, height, xcoord, ycoord;
                if (!(int.TryParse(x, out xcoord) && int.TryParse(y, out ycoord) && int.TryParse(w, out width) && int.TryParse(h, out height)))
                {
                    xcoord = 0; ycoord = 0; width = 0; height = 0;
                }
                if (op == "original" && _expUtil.getUserLvl(User.Identity.Name) >= 6)
                {
                    _imgUtil.UpdateOrAddPic(User.Identity.Name, filetype, b);
                }
                else if (width > 0 && height > 0)
                {
                    _imgUtil.UpdateOrAddPic(User.Identity.Name, filetype, _imgUtil.Crop(b, width, height, xcoord, ycoord));
                }

                _cache.Remove(CacheService.GetAvatarCacheKey(User.Identity.Name));
                ViewBag.ChangeSuccess = "修改成功";
            }

            return PartialView("_ChangeAvatarPartial");
        }

        [HttpPost]
        public ActionResult ChangeComment(string comment = "")
        {
            UserProfile user = _db.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            bool success = false;
            if (comment.Length <= 200 && user.UserComment != comment)
            {
                user.UserComment = comment;
                _db.SaveChanges();
                _cache.Set("desc" + user.UserName.ToLower(), comment);
                success = true;
            }
            return Json(new { success = success });
        }

        /// <summary>
        /// 检查昵称的合法性。此方法由用户账户管理页面的ajax调用。。。
        /// </summary>
        /// <param name="name">输入昵称</param>
        /// <returns>json格式的结果</returns>
        [HttpPost]
        public JsonResult CheckNickName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(new { success = false, err = "empty" });
            else if (name.Length > 20)
                return Json(new { success = false, err = "length" });
            name = Regex.Replace(name, @"\s+", string.Empty);
            var n = _db.Users.SingleOrDefault(u => u.NickName == name);
            if (n != null && !n.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                return Json(new { success = "err", err = "dup" });
            return Json(null);
        }

        /// <summary>
        /// 检查昵称是否已被使用。注意本方法由RegisterModel中的验证属性自动调用
        /// </summary>
        /// <param name="nickname">输入昵称</param>
        /// <returns>json格式的结果</returns>
        [AllowAnonymous]
        public JsonResult CheckNicknameUsed(string nickname)
        {
            nickname = Regex.Replace(nickname, @"\s+", string.Empty);
            if (_db.Users.Any(u => u.NickName == nickname))
            {
                return Json("该昵称已注册");
            }
            else
                return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> ChangeNickName([FromServices]INickNameProvider nicknameProvider, string name)
        {
            bool success;
            name = Regex.Replace(name, @"\s+", string.Empty);
            var ret = CheckNickName(name);
            if (ret.Value != null)
                return ret;
            UserProfile user = _db.Users.SingleOrDefault(u => u.UserName == User.Identity.Name);
            
            var result = await _expUtil.trySpendPointAsync(User.Identity.Name, 200);
            int points = result.Item2;
            if (result.Item1 == false)
            {
                success = false;
            }
            else
            {
                user.NickName = name;
                await _db.SaveChangesAsync();
                nicknameProvider.UpdateNickNameCache(user);
                success = true;
            }
            return Json(new { success = success, points = points });
        }

        [HttpPost]
        public async Task<PartialViewResult> UserOptions(UserOption model)
        {
            UserProfile user = await _db.Users.Include("option").SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user.option == null)
            {
                user.option = model;
            }
            else
            {
                user.option.sendNoticeForNewReply = model.sendNoticeForNewReply;
                user.option.sendNoticeForNewPostReply = model.sendNoticeForNewPostReply;
                user.option.addFavFlameEffect = model.addFavFlameEffect;
                user.option.ShowBlogDateOnLists = model.ShowBlogDateOnLists;
                _blogUtil.CacheUserOption(user.option, user.UserName);
            }
            await _db.SaveChangesAsync();
            ViewBag.Success = true;
            return PartialView(model);
        }

        [HttpGet]
        public PartialViewResult HPSetting()
        {
            return PartialView("_HPSettingPartial", GetHpSettings());
        }

        [HttpPost]
        public async Task<PartialViewResult> HPSetting(HPSettingsModel model)
        {
            string option, tagBlacklist = null;
            var categories = model.GetCategoryOptions();
            if (categories.SelectedIds.Count == 0)
            {
                option = string.Empty;
            }
            else
            {
                option = JsonConvert.SerializeObject(categories.SelectedIds);
            }
            if (!string.IsNullOrWhiteSpace(model.BlacklistTagNames))
            {
                var taglist = TagUtil.SplitTags(model.BlacklistTagNames);
                if (taglist != null && taglist.Length > 0)
                {
                    var tags = await _bdb.Tags.Where(t => taglist.Contains(t.TagName)).ToDictionaryAsync(t => t.TagName.ToLower(), t => t.TagID);
                    var notfound = taglist.Where(n => !tags.ContainsKey(n.ToLower()));
                    if (notfound.Any())
                    {
                        ViewBag.NotFoundTags = string.Join(",", notfound);
                        ViewBag.Success = false;
                        return PartialView("_HPSettingPartial", model);
                    }
                    tagBlacklist = JsonConvert.SerializeObject(tags.Values.AsEnumerable());
                }
            }
            UserProfile user = _db.Users.Include("option").SingleOrDefault(u => u.UserName == User.Identity.Name);
            if (user.option == null)
            {
                user.option = new UserOption();
            }
            user.option.homepageCategories = option;
            user.option.homepageHideHarmony = model.HideHarmony;
            user.option.homepageTagBlacklist = tagBlacklist;
            _blogUtil.CacheUserOption(user.option, User.Identity.Name);
            await _db.SaveChangesAsync();
            ViewBag.Success = true;
            return PartialView("_HPSettingPartial", model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult ResetPW(string email, string token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return NotFound();
            }
            return View(new PasswordResetModel { Email = email, Code = token });
        }

        [HttpPost]
        [AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<ViewResult> ResetPW(PasswordResetModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "无效的邮箱。");
                    return View(model);
                }
                var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
                if (result.Succeeded)
                {
                    ViewBag.ok = true;
                }
                else
                {
                    AddErrors(result);
                }
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public ViewResult Forget()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous, ValidateAntiForgeryToken]
        public async Task<JsonResult> Forget(string email, string Captcha)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(Captcha))
            {
                return Json(new { err = "请输入邮箱地址和验证码。" });
            }
            else if (_blogUtil.CheckCaptchaError(Captcha, null))
            {
                return Json(new { err = "验证码计算错误，请重试。" });
            }
            else
            {
                DateTime? lastpost = HttpContext.Session.GetDateTime("LastPostTime");
                if (lastpost.HasValue)
                {
                    var diff = DateTime.Now - lastpost.Value;
                    if (diff.TotalSeconds < 60)
                    {
                        return Json(new { err = "请不要短时间内多次发送邮件！" });
                    }
                }
                UserProfile user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { err = "该电子邮件地址未被注册。" });
                }
                else
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    await _emailSender.SendPWEmailForUserAsync(user, Url.Action("ResetPW", null, new { token = token, email = email }, Request.Scheme));
                    HttpContext.Session.SetDateTime("LastPostTime", DateTime.Now);
                }
            }
            return Json(new { ok = true });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CheckUsername(string username)
        {
            if (await _userManager.FindByNameAsync(username) != null)
            {
                return Json("该用户名已注册");
            }
            else
                return Json(true);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CheckEmail(string Email)
        {
            UserProfile user = await _userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                if (user.UserName.Equals(User.Identity.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return Json(false);
                }
                return Json("该邮箱已被注册");
            }
            else
                return Json(true);
        }

        [HttpGet]
        public async Task<ActionResult> Invite()
        {
            UserProfile user = await GetCurrentUserAsync();
            var Codes = await _db.UserCodes.Include("UsedByUser").Where(c => c.UserId == user.Id).ToListAsync();
            var lastBuy = Codes.Max(c => c.BuyDate);
            if (lastBuy.HasValue && (DateTime.Today - lastBuy.Value).Days <= RegisterSettingsModel.CodeCoolDownDays) {
                ViewBag.CoolDown = lastBuy.Value.AddDays(RegisterSettingsModel.CodeCoolDownDays);
            }
            ViewBag.Enabled = _registerSettings.RequireCode;
            ViewBag.User = user;
            ViewBag.Cost = _registerSettings.CodeCost + Codes.Count() * _registerSettings.CodeCostIncrement;
            return View(Codes);
        }

        [HttpPost]
        public async Task<JsonResult> BuyCode()
        {
            var user = await GetCurrentUserAsync();
            DateTime? lastBuyDate = await _db.UserCodes.Where(c => c.UserId == user.Id).MaxAsync(d => d.BuyDate);
            if (lastBuyDate.HasValue && (DateTime.Now - lastBuyDate.Value).Days < RegisterSettingsModel.CodeCoolDownDays)
            {
                return Json(new { success = false, cooldown = lastBuyDate.Value.AddDays(RegisterSettingsModel.CodeCoolDownDays).ToString() });
            }
            int codeCount = await _db.UserCodes.Where(c => c.UserId == user.Id).CountAsync();
            int curCost = _registerSettings.CodeCost + codeCount * _registerSettings.CodeCostIncrement;
            var result = await _expUtil.trySpendPointAsync(User.Identity.Name, curCost);
            int remain = result.Item2;
            if (result.Item1)
            {
                UserCode code = new UserCode
                {
                    UserId = user.Id,
                    BuyDate = DateTime.Now,
                };
                _db.UserCodes.Add(code);
                await _db.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    code = code.Code,
                    cooldown = code.BuyDate.Value.AddDays(RegisterSettingsModel.CodeCoolDownDays).ToString(),
                    remain,
                    newcost = curCost + _registerSettings.CodeCostIncrement
                });
            }
            return Json(new { success = false, remain, newcost = curCost });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeTitle([FromServices]INickNameProvider nicknameProvider, UserQuest.UserProfession? title)
        {
            if (!title.HasValue)
            {
                return Json(new { success = false });
            }
            var user = await GetCurrentUserAsync();
            if (user.quest == null || !user.quest.HasTitle(title.Value))
            {
                return Json(new { success = false });
            }
            user.quest.Title = title.Value;
            var result = await _userManager.UpdateAsync(user);
            nicknameProvider.UpdateNickNameCache(user);
            return Json(new { success = result.Succeeded, current = user.quest.Title.ToString() });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeBackground(string name)
        {
            var user = await GetCurrentUserAsync();
            var possibleTitles = UserQuest.AllTitleBackground.Where(kvp => kvp.Value == name);
            if (user.quest == null || !(string.IsNullOrEmpty(name) || possibleTitles.Any(t => user.quest.HasTitle(t.Key))))
            {
                return Json(new { success = false });
            }
            user.quest.PersonalBackground = name;
            var result = await _userManager.UpdateAsync(user);
            return Json(new { success = result.Succeeded, current = user.quest.PersonalBackground });
        }

        #region 帮助程序

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            GetRewardSuccess,
            GetRewardFailed
        }

        private async Task<UserProfile> GetCurrentUserAsync()
        {
            return await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private HPSettingsModel GetHpSettings()
        {
            var option = _db.UserOptions.SingleOrDefault(u => u.user.UserName == User.Identity.Name) ?? new UserOption();
            string tagnames = "";
            if (option.homepageTagBlacklist != null)
            {
                var tagids = JsonConvert.DeserializeObject<int[]>(option.homepageTagBlacklist);
                tagnames = string.Join(",", _bdb.Tags.Where(t => tagids.Contains(t.TagID)).Select(t => t.TagName));
            }
            return new HPSettingsModel
            {
                CategoryIds = option.homepageCategories == null ? null : JsonConvert.DeserializeObject<IEnumerable<int>>(option.homepageCategories),
                HideHarmony = option.homepageHideHarmony,
                BlacklistTagNames = tagnames
            };
        }

        #endregion 帮助程序
    }
}