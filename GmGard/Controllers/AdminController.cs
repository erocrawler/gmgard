using Microsoft.AspNetCore.Authorization;
using GmGard.Filters;
using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GmGard.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using GmGard.Extensions;
using System.ComponentModel.DataAnnotations;

namespace GmGard.Controllers
{
    [Authorize(Policy = "AdminAccess"), ResponseCache(CacheProfileName = "Never")]
    public class AdminController : Controller
    {
        private BlogContext _db;
        private UsersContext _udb;
        private AdminUtil _adminUtil;
        private CategoryUtil _catUtil;
        private ExpUtil _expUtil;
        private MessageUtil _msgUtil;
        private AppSettingsModel _appSettings;
        private RegisterSettingsModel _regSettings;
        private BackgroundSetting _bgSettings;
        private DataSettingsModel _dataSettings;
        private UserManager<UserProfile> _userManager;
        private IMemoryCache _cache;
        private IWebHostEnvironment _env;
        private IServiceProvider _serviceProvider;

        public AdminController(
            IOptions<AppSettingsModel> appSettings,
            IOptionsSnapshot<RegisterSettingsModel> regSettings,
            IOptionsSnapshot<BackgroundSetting> bgSettings,
            IOptionsSnapshot<DataSettingsModel> dataSettings,
            BlogContext db, 
            UsersContext udb,
            AdminUtil adminUtil, 
            CategoryUtil catUtil,
            ExpUtil expUtil,
            MessageUtil msgUtil,
            UserManager<UserProfile> userManager,
            IMemoryCache cache,
            IWebHostEnvironment env,
            IServiceProvider serviceProvider)
        {
            _db = db;
            _udb = udb;
            _adminUtil = adminUtil;
            _catUtil = catUtil;
            _expUtil = expUtil;
            _msgUtil = msgUtil;
            _appSettings = appSettings.Value;
            _regSettings = regSettings.Value;
            _bgSettings = bgSettings.Value;
            _dataSettings = dataSettings.Value;
            _userManager = userManager;
            _cache = cache;
            _env = env;
            _serviceProvider = serviceProvider;
        }

        public static event EventHandler<SettingsEventArgs> OnSettingsChanged;

        private void TriggerSettingsChanged(DataSettingsModel b)
        {
            OnSettingsChanged?.Invoke(this, new SettingsEventArgs(b));
        }

        private void TriggerSettingsChanged(AppSettingsModel b)
        {
            OnSettingsChanged?.Invoke(this, new SettingsEventArgs(b));
        }

        private void CopyValues<T>(T target, T source)
        {
            Type t = typeof(T);

            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var readonlySetting = prop.GetCustomAttributes(typeof(ReadOnlySettingAttribute), false);
                var value = prop.GetValue(source, null);
                if (value != null && (readonlySetting == null || readonlySetting.Length == 0))
                    prop.SetValue(target, value, null);
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private async Task<bool> TryAddRoleAsync(UserProfile user, string role)
        {
            if (await _userManager.IsInRoleAsync(user, role))
            {
                return false;
            }
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                AddErrors(result);
            }
            return result.Succeeded;
        }

        private async Task<bool> TryRemoveRoleAsync(UserProfile user, string role)
        {
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                return false;
            }
            var result = await _userManager.RemoveFromRoleAsync(user, role);
            if (!result.Succeeded)
            {
                AddErrors(result);
            }
            return result.Succeeded;
        }

        private X.PagedList.IPagedList<AdminLog> GetLog(int pagenum)
        {
            var query = _udb.AdminLogs.OrderByDescending(l => l.LogTime);
            return query.ToPagedList(pagenum, 40);
        }

        private static List<UserProfile> getAdmins(UsersContext db)
        {
            var admins = db.Roles.Where(r => r.Name == "Administrator" || r.Name == "Moderator" || r.Name == "AdManager").SelectMany(r => r.Users.Select(ur => ur.UserId));
            return db.Users.Where(u => admins.Contains(u.Id)).ToList();
        }

        private static List<UserProfile> getAuditors(UsersContext db)
        {
            return db.Users.Include("auditor").Where(u => db.Roles.FirstOrDefault(r => r.Name == "Auditor").Users.Select(ur => ur.UserId).Contains(u.Id)).ToList();
        }

        private static List<UserProfile> getBannedUsers(UsersContext db)
        {
            var users = db.Users.AsNoTracking().Where(u => db.Roles.FirstOrDefault(r => r.Name == "Banned").Users.Select(ur => ur.UserId).Contains(u.Id));
            var banned = users.Join(db.AdminLogs.Where(l => l.Action == "封禁").GroupBy(l => new { l.Action, l.Target }).Select(g => g.OrderByDescending(gl => gl.LogTime).FirstOrDefault()),
                                    u => u.UserName, l => l.Target, (u, l) => new { User = u, Reason = l.Reason, Time = l.LogTime });
            return banned.ToList().Select(a =>
            {
                var u = a.User;
                u.LastLoginDate = a.Time;
                u.UserComment = a.Reason;
                return u;
            }).OrderByDescending(u => u.LastLoginDate).ToList();
        }

        public async Task<ActionResult> Manage(string context)
        {
            AdminViewModel model = new AdminViewModel();
            switch (context)
            {
                case "Category":
                    model.AllHanGroup = _db.HanGroups.Include("members").ToList();
                    ViewBag.HgMsg = TempData["HgMsg"];
                    break;

                case "Users":
                    model.Admins = getAdmins(_udb);
                    model.Writers = (await _userManager.GetUsersInRoleAsync("Writers")).ToList();
                    model.Auditors = getAuditors(_udb);
                    model.BannedUsers = getBannedUsers(_udb);
                    break;

                case "AdManage":
                    model.AllAds = _db.Advertisments.ToList();
                    break;

                case "Parameter":
                    model.appsettings = _appSettings;
                    model.datasettings = _dataSettings;
                    model.harmonyblogcount = _db.Blogs.Count(b => b.isHarmony);
                    break;

                case "Register":
                    model.registersettings = _regSettings;
                    break;

                case "Background":
                    model.backgroundsetting = _bgSettings;
                    break;

                default:
                    context = "Data";
                    model.harmonyblogcount = _db.Blogs.Count(b => b.isHarmony);
                    model.auditcount = _db.Blogs.Where(b => b.isApproved == null && b.BlogID > 0).Count();
                    model.bannedusercount = (await _userManager.GetUsersInRoleAsync("Banned")).Count;
                    model.todaynewitem = _db.Blogs.Where(b => DbFunctions.DiffDays(b.BlogDate, DateTime.Now) == 0).Count();
                    model.yesterdaynewitem = _db.Blogs.Where(b => DbFunctions.DiffDays(b.BlogDate, DateTime.Now) == 1).Count();
                    model.totalauditcount = _db.Blogs.Count();
                    model.totalusercount = _udb.Users.Count();
                    ViewBag.AdminLogs = GetLog(1);
                    break;
            }
            ViewBag.Context = context;
            if (Request.IsAjaxRequest())
            {
                return PartialView(context + "Partial", model);
            }
            return View("Manage", model);
        }

        public PartialViewResult Log(int pagenum)
        {
            return PartialView(GetLog(pagenum));
        }

        [HttpPost]
        public async Task<ActionResult> ManageHangroup(string id, string title, string members, string uri, string action)
        {
            int gid;
            HanGroup g;
            switch (action)
            {
                case "new":
                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(uri))
                    {
                        TempData["HgMsg"] = "请输入名称和识别码";
                    }
                    else
                    {
                        g = new HanGroup();
                        g.Title = title;
                        g.GroupUri = uri;
                        var errnames = string.Empty;
                        if (members != null)
                        {
                            var names = members.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                            g.members = new List<HanGroupMember>();
                            foreach (var n in names)
                            {
                                var name = n.Trim();
                                var user = await _userManager.FindByNameAsync(name);
                                if (user != null)
                                {
                                    var member = new HanGroupMember();
                                    member.Username = name;
                                    member.GroupLvl = 1;
                                    g.members.Add(member);
                                }
                                else
                                {
                                    errnames += name + ',';
                                }
                            }
                        }
                        _db.HanGroups.Add(g);
                        _db.SaveChanges();
                        TempData["HgMsg"] = errnames == string.Empty ? "添加成功" : "添加成功，以下用户不存在：" + errnames;
                    }
                    break;

                case "edit":
                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(uri))
                    {
                        TempData["HgMsg"] = "请输入名称和识别码";
                    }
                    else if (int.TryParse(id, out gid))
                    {
                        g = _db.HanGroups.Find(gid);
                        g.Title = title;
                        g.GroupUri = uri;
                        var errnames = string.Empty;
                        if (members != null)
                        {
                            var names = members.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                            var m = new List<HanGroupMember>();
                            var userm = g.members.Where(hm => hm.GroupLvl != 1);
                            var comp = new HanGroupMemberComparer();
                            foreach (var n in names)
                            {
                                var name = n.Trim();
                                var user = await _userManager.FindByNameAsync(name);
                                if (user != null)
                                {
                                    //HanGroupMember member = userm.SingleOrDefault(um => um.Username == name);
                                    //if (member == null)
                                    //{
                                    var member = new HanGroupMember();
                                    member.HanGroupID = g.HanGroupID;
                                    member.Username = name;
                                    //}
                                    member.GroupLvl = 1;
                                    m.Add(member);
                                }
                                else
                                {
                                    errnames += name + ',';
                                }
                            }
                            g.members = g.members.Where(hm => hm.GroupLvl == 1).Intersect(m, comp).Union(m, comp).Union(userm, comp).ToList();
                            //member = newGroup1 Union Group2
                        }
                        _db.SaveChanges();
                        TempData["HgMsg"] = errnames == string.Empty ? "编辑成功" : "编辑成功，以下用户不存在：" + errnames;
                    }
                    break;

                case "del":
                    if (int.TryParse(id, out gid))
                    {
                        g = _db.HanGroups.Find(gid);
                        if (g.blogs == null || g.blogs.Count == 0)
                        {
                            _db.HanGroups.Remove(g);
                            _db.SaveChanges();
                            TempData["HgMsg"] = "删除成功";
                        }
                        else
                        {
                            TempData["HgMsg"] = "不可删除非空汉化组";
                        }
                    }
                    break;
            }
            _cache.Remove(CacheService.HanGroupListCacheKey);
            return RedirectToAction("Manage", new { context = "Category" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<string> ManageRole(string name, string behaviour)
        {
            var user = await _userManager.FindByNameAsync(name);
            if (user == null)
            {
                return "查无此人";
            }
            if (behaviour == "添加作者")
            {
                if (!await TryAddRoleAsync(user, "Writers"))
                {
                    return "此人已是作者！";
                }
            }
            else if (behaviour == "删除作者")
            {
                if (!await TryRemoveRoleAsync(user, "Writers"))
                {
                    return "此人不是作者！";
                }
            }
            else if (behaviour == "加入审核组")
            {
                if (!await TryAddRoleAsync(user, "Auditor"))
                {
                    return "此人已是审核者！";
                }
            }
            else if (behaviour == "移除审核组")
            {
                if (!await TryRemoveRoleAsync(user, "Auditor"))
                {
                    return "此人不是审核者！";
                }
            }
            await _userManager.UpdateSecurityStampAsync(user);
            _adminUtil.log(User.Identity.Name, behaviour, name);
            return behaviour + "成功";
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<string> ManageAdmin(string adminname, string behaviour, bool asadmin = false, bool admanager = false)
        {
            string msg = string.Empty;
            var user = await _userManager.FindByNameAsync(adminname);
            if (user == null)
            {
                return "查无此人";
            }
            if (behaviour == "添加管理员")
            {
                if (admanager)
                {
                    if (!await TryAddRoleAsync(user, "AdManager"))
                        return "此人已是广告管理员！";
                    return "添加成功";
                }
                if (asadmin)
                {
                    if (!await TryAddRoleAsync(user, "Administrator"))
                    {
                        return "此人已是管理员！";
                    }
                    user.Level = 99;
                }
                else
                {
                    if (!await TryAddRoleAsync(user, "Moderator"))
                    {
                        return "此人已是管理员！";
                    }
                    user.Level = 90;
                }
                await _userManager.UpdateAsync(user);
                msg = "添加成功";
            }
            else if (behaviour == "删除管理员")
            {
                if (admanager)
                {
                    if (!await TryRemoveRoleAsync(user, "AdManager"))
                    {
                        return "此人不是广告管理员！";
                    }
                }
                else if (await _userManager.IsInRoleAsync(user, "Administrator"))
                {
                    return "不可删除Admin";
                }
                else
                {
                    if (!await TryRemoveRoleAsync(user, "Moderator"))
                    {
                        return "此人不是管理员！";
                    }
                }
                user.Level = _expUtil.calculateUserLevel(user.Id);
                await _userManager.UpdateAsync(user);
                msg = "删除成功";
            }
            await _userManager.UpdateSecurityStampAsync(user);
            _cache.Remove(ExpUtil.LevelCacheKey + adminname);
            return msg;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<string> ManageBan(string banname, string behaviour, string bancomment = null, bool deletecomment = false)
        {
            string msg = string.Empty;
            var user = await _userManager.FindByNameAsync(banname);
            if (user == null)
            {
                return "查无此人";
            }
            if (behaviour == "封禁")
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Banned"))
                {
                    return "此人已死";
                }
                else if (roles.Count > 0)
                {
                    return "不可封禁有职位的用户";
                }
                if (deletecomment)
                {
                    _db.Database.ExecuteSqlCommand("delete from Posts where Author = @banname; delete from Replies where Author = @banname;", new SqlParameter("@banname", banname));
                }
                user.Level = 0;
                await _userManager.AddToRoleAsync(user, "Banned");
                await _userManager.UpdateAsync(user);
                msg = "添加成功";
            }
            else if (behaviour == "解封")
            {
                if (!await TryRemoveRoleAsync(user, "Banned"))
                {
                    return "此人未被封禁";
                }
                user.Level = _expUtil.calculateUserLevel(user.Id);
                await _userManager.UpdateAsync(user);
                msg = "解禁成功";
            }
            await _userManager.UpdateSecurityStampAsync(user);
            _adminUtil.log(User.Identity.Name, behaviour, banname, bancomment);
            _cache.Remove(ExpUtil.LevelCacheKey + banname);
            return msg;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<string> ManageEmail(string username, string oldEmail, string newEmail, string reason)
        {
            if (string.IsNullOrEmpty(username))
            {
                return "请输入用户名";
            }
            username = username.ToLower();
            if (!new EmailAddressAttribute().IsValid(newEmail))
            {
                return "新邮箱无效";
            }
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return "查无此人";
            }
            if (!user.Email.Equals(oldEmail, StringComparison.OrdinalIgnoreCase))
            {
                return "旧邮箱有误";
            }
            if (_udb.Users.Any(u => u.Email == newEmail))
            {
                return "新邮箱已被注册";
            }
            user.Email = newEmail;
            await _userManager.UpdateAsync(user);
            var msg = string.Format("旧：{0}，新：{1}", oldEmail, newEmail);
            _msgUtil.SendEmailUpdateNotice(username, User.Identity.Name, msg);
            _adminUtil.log(User.Identity.Name, "修改邮箱", msg, reason);
            return "修改成功";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<string> ManageExp(string expname, string expcount, string ptscount, string reason, bool reasoncheck = false)
        {
            if (string.IsNullOrEmpty(expname))
            {
                return "请输入用户名";
            }
            expname = expname.ToLower();
            string msg = string.Empty;
            int exp = 0;
            int pts = 0;
            if (string.IsNullOrEmpty(expcount) && string.IsNullOrEmpty(ptscount))
            {
                return "请输入经验或积分";
            }
            bool b = !int.TryParse(expcount, out exp);
            if (!int.TryParse(ptscount, out pts) && b)
            {
                return "无效数值";
            }
            var user = await _userManager.FindByNameAsync(expname);
            if (user == null)
            {
                return "查无此人";
            }
            user.Experience += exp;
            user.Points += pts;
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Any(r => r == "Moderator" || r == "Administrator" || r == "Banned"))
            {
                user.Level = _expUtil.calculateUserLevel(user.Id);
            }
            else
            {
                _cache.Remove(ExpUtil.ExpCacheKey + expname);
                _cache.Remove(ExpUtil.PointsCacheKey + expname);
            }

            await _userManager.UpdateAsync(user);
            msg = string.Format("修改成功，{0} 的当前绅士度：{1}，棒棒糖：{3}，等级：{2}", expname, user.Experience, user.Level, user.Points);
            if (reasoncheck == true)
            {
                string tmpreason = string.Empty;
                if (exp != 0)
                {
                    tmpreason += " 绅士度" + (exp > 0 ? "+" + exp : exp.ToString());
                }
                if (pts != 0)
                {
                    tmpreason += " 棒棒糖" + (pts > 0 ? "+" + pts : pts.ToString());
                }
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    tmpreason += "<br>原因：" + reason;
                }
                _msgUtil.SendExpChangeNotice(expname, User.Identity.Name, tmpreason);
            }
            _adminUtil.log(User.Identity.Name, "editexp", expname + " (" + expcount + '/' + ptscount + ')', reason);
            return msg;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ManageRegisterQuestions(RegisterSettingsModel model)
        {
            if (model.RegisterQuestions != null)
            {
                model.RegisterQuestions = model.RegisterQuestions.Where(q => !string.IsNullOrWhiteSpace(q.Answer) && !string.IsNullOrWhiteSpace(q.Question)).ToList();
            } else
            {
                model.RegisterQuestions = new List<RegisterQuestion>();
            }
            string result = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            string path = Path.Combine(_env.ContentRootPath, "App_Data/RegisterSettings.json");
            System.IO.File.WriteAllText(path, result);
            CopyValues(_regSettings, model);
            return RedirectToAction("Manage", new { context = "Register" });
        }

        [HttpGet]
        public ActionResult ManageBackgrounds()
        {
            return RedirectToAction("Manage", new { context = "Background" });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ManageBackgrounds(BackgroundSetting model)
        {
            if (model != null && model.Backgrounds.GroupBy(b => b.Name).Any(g => g.Count() > 1))
            {
                ModelState.AddModelError("", "背景名不能有重复");
            }
            if (ModelState.IsValid)
            {
                if (model.Backgrounds.Count() > 0)
                {
                    model.Backgrounds.Last().Name = "new";
                }
                string result = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
                string path = Path.Combine(_env.ContentRootPath, "App_Data/BackgroundSetting.json");
                System.IO.File.WriteAllText(path, result);
                CopyValues(_bgSettings, model);
            }
            ViewBag.Context = "Background";
            var adminmodel = new AdminViewModel();
            adminmodel.backgroundsetting = model;
            return View("Manage", adminmodel);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public string DataSettings(DataSettingsModel model)
        {
            List<int>[] idlist = new List<int>[] { new List<int>(5), new List<int>(5), new List<int>(5) };
            string[] properties = new string[] { "FeaturedBlogIds", "BannerBlogIds", "BannerTopicIds" };
            for (int i = 0; i < properties.Length; i++)
            {
                var s = model.GetType().GetProperty(properties[i]).GetValue(model) as string;
                if (!string.IsNullOrWhiteSpace(s))
                {
                    string[] blogids = s.Split(',');
                    int count = 0;
                    foreach (var ids in blogids)
                    {
                        int id;
                        if (!int.TryParse(ids, out id))
                        {
                            return "数据格式无效";
                        }
                        if (++count > 5)
                        {
                            return "置顶不得超过5个";
                        }
                        idlist[i].Add(id);
                    }
                }
            }

            string result = Newtonsoft.Json.JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);
            string path = Path.Combine(_env.ContentRootPath, "App_Data/DataSettings.json");
            System.IO.File.WriteAllText(path, result);
            CopyValues(_dataSettings, model);
            TriggerSettingsChanged(model);
            return "设置成功";
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult BanIP(string blockip)
        //{
        //    blockip = blockip ?? string.Empty;
        //    blockip = blockip.Replace("\r\n", ";");
        //    blockip = blockip.Replace("\n", ";");
        //    Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
        //    //ConfigurationManager.AppSettings.Set("blockip", blockip);
        //    config.AppSettings.Settings["blockip"].Value = blockip;
        //    config.Save(ConfigurationSaveMode.Minimal);
        //    return RedirectToAction("Manage");
        //}

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ManageSettings(AppSettingsModel appsettings)
        {
            var filter = new BlogFilter(_db);
            if (appsettings.HarmonySettings == null)
            {
                appsettings.HarmonySettings = new HarmonySettingsModel();
            }
            filter.Blacklisttags = appsettings.HarmonySettings.BlacklistTags;
            filter.Whitelistcategories = appsettings.HarmonySettings.WhitelistCategories;
            filter.Whitelistids = appsettings.HarmonySettings.WhitelistIds;
            filter.UpdateDatabase();

            string result = Newtonsoft.Json.JsonConvert.SerializeObject(new { ApplicationSettings = appsettings }, Newtonsoft.Json.Formatting.Indented);
            string path = Path.Combine(_env.ContentRootPath, "appsettings.override.json");
            System.IO.File.WriteAllText(path, result);
            CopyValues(_appSettings, appsettings);
            TriggerSettingsChanged(_appSettings);
            return RedirectToAction("Manage", new { context = "Parameter" });
        }

        [HttpPost]
        public async Task<ActionResult> EditVersion(string data, string version)
        {
            var blog = await _db.Blogs.FindAsync(PostConstant.SiteVersionNotes);
            blog.Content = data;
            blog.Links = version;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> EditSiteRules(string data)
        {
            var blog = await _db.Blogs.FindAsync(PostConstant.SiteRules);
            blog.Content = data;
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AdManage(Advertisment ad, string action)
        {
            AdvertisementType AdType = ad.AdType;
            switch (action)
            {
                case "new":
                    if (string.IsNullOrWhiteSpace(ad.AdUrl))
                    {
                        return Json(new { err = "请输入URL" });
                    }
                    else if (string.IsNullOrWhiteSpace(ad.AdTitle) && ad.AdType == AdvertisementType.Carousel)
                    {
                        return Json(new { err = "请输入标题" });
                    }
                    else
                    {
                        _db.Advertisments.Add(ad);
                        _db.SaveChanges();
                    }
                    break;

                case "edit":
                    if (string.IsNullOrWhiteSpace(ad.AdUrl))
                    {
                        return Json(new { err = "请输入URL" });
                    }
                    else
                    {
                        var a = _db.Advertisments.Find(ad.AdID);
                        if (a != null)
                        {
                            a.AdTitle = ad.AdTitle;
                            a.AdUrl = ad.AdUrl;
                            a.ImgUrl = ad.ImgUrl;
                            a.AdOrder = ad.AdOrder;
                            _db.SaveChanges();
                            AdType = a.AdType;
                        }
                        else
                        {
                            return Json(new { err = "未找到" });
                        }
                    }
                    break;

                case "del":
                    {
                        var a = _db.Advertisments.Find(ad.AdID);
                        if (a != null)
                        {
                            _db.Advertisments.Remove(a);
                            _db.SaveChanges();
                            AdType = a.AdType;
                        }
                        else
                        {
                            return Json(new { err = "未找到" });
                        }
                    }
                    break;
            }
            _cache.Remove(GetAdCacheKey(AdType));
            if (!User.IsInRole("Administrator"))
            {
                _adminUtil.log(User.Identity.Name, action + " AD", AdType.ToString(), ad.AdID + " " + ad.AdTitle);
            }
            return PartialView(GetAdViewName(AdType), new AdViewModel { Ads = _db.Advertisments.Where(aa => aa.AdType == AdType).ToList(), Type = AdType });
        }

        private string GetAdViewName(AdvertisementType t)
        {
            switch (t)
            {
                case AdvertisementType.SidebarBanner1:
                case AdvertisementType.SidebarBanner2:
                    return "_SidebarBannerManage";
                default:
                    return $"_{t.ToString()}Manage";
            }
            throw new ArgumentException();
        }

        private string GetAdCacheKey(AdvertisementType t)
        {
            switch (t)
            {
                case AdvertisementType.Banner:
                    return CacheService.BannerAdKey;

                case AdvertisementType.Carousel:
                case AdvertisementType.CarouselBanner:
                    return CacheService.CarouselAdKey;

                case AdvertisementType.Sidebar:
                    return CacheService.SidebarAdKey;

                case AdvertisementType.SidebarBanner1:
                case AdvertisementType.SidebarBanner2:
                    return CacheService.SidebarBannerAdKey;
            }
            throw new ArgumentException();
        }
    }
}