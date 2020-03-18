using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [Authorize(Policy = "Harmony"), ResponseCache(CacheProfileName = "Never")]
    public class HanController : Controller
    {
        public HanController(
            IOptions<AppSettingsModel> appSettings,
            BlogContext db,
            UsersContext udb,
            UploadUtil uploadUtil,
            IMemoryCache cache)
        {
            listpagesize = appSettings.Value.ListPageSize;
            _db = db;
            _udb = udb;
            _cache = cache;
            _uploadUtil = uploadUtil;
        }

        private BlogContext _db;
        private UsersContext _udb;
        private IMemoryCache _cache;
        private UploadUtil _uploadUtil;

        private int listpagesize;

        [HttpGet]
        public ActionResult Index(string name, int page = 1)
        {
            var model = new HanDisplay();
            model.grouplist = _cache.Get<List<BlogLink>>(CacheService.HanGroupListCacheKey);
            if (model.grouplist == null)
            {
                model.grouplist = _db.HanGroups.GroupJoin(_db.HanGroupBlogs, hg => hg.HanGroupID, hgb => hgb.HanGroupID, (hg, hgb) => new { hangroup = hg, blogs = hgb })
                    .SelectMany(hghgb => hghgb.blogs.DefaultIfEmpty(), (hg, hgb) => new { hangroup = hg.hangroup, BlogDate = hgb.blog.BlogDate })
                    .GroupBy(hgb => hgb.hangroup).OrderByDescending(hgb => hgb.Max(b => b.BlogDate))
                    .Select(hb => new BlogLink { name = hb.Key.Title, url = hb.Key.GroupUri }).ToList();
                _cache.Set(CacheService.HanGroupListCacheKey, model.grouplist, TimeSpan.FromMinutes(10));
            }
            if (string.IsNullOrEmpty(name) && model.grouplist.Count > 0)
            {
                name = model.grouplist.First().url;
            }
            model.hangroup = _db.HanGroups.Include("blogs").SingleOrDefault(h => h.GroupUri == name);
            if (model.hangroup == null)
            {
                return NotFound();
            }
            var query = model.hangroup.blogs.Select(b => b.blog).OrderByDescending(q => q.BlogDate).AsQueryable();

            model.blogs = query.ToPagedList(page, listpagesize);
            if (Request.IsAjaxRequest())
            {
                return PartialView(model);
            }
            return View(model);
        }

        [HttpPost, ActionName("Index")]
        public ActionResult AjaxIndex(string name, int pagenum = 1)
        {
            return Index(name, pagenum);
        }

        [HttpGet, Authorize, HanMemberFilter]
        public ActionResult Edit(int id = 1)
        {
            var hg = _db.HanGroups.Include("blogs").SingleOrDefault(h => h.HanGroupID == id);
            if (hg == null)
                return RedirectToAction("Index");
            var model = new HanEdit(hg);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(HanEdit model)
        {
            var hg = _db.HanGroups.Include("blogs").Include("members").SingleOrDefault(m => m.HanGroupID == model.ID);
            if (hg == null)
                return NotFound();
            byte[] imgbtye = null;
            if (model.HanImage != null)
            {
                imgbtye = Convert.FromBase64String(model.HanImage);
                if (imgbtye.Length > 4194304)
                {
                    ModelState.AddModelError("", "头像必须是图片，且小于4MB。");
                }
            }
            if (ModelState.IsValid)
            {
                string[] newBlogIDstr = null;
                if (model.blogIDs == null)
                {
                    newBlogIDstr = new string[] { };
                }
                else
                {
                    newBlogIDstr = model.blogIDs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                var newBlogIDs = new List<int>(newBlogIDstr.Length);
                foreach (var blogid in newBlogIDstr)
                {
                    int id;
                    if (int.TryParse(blogid, out id))
                    {
                        newBlogIDs.Add(id);
                    }
                }
                var actual = _db.Blogs.Where(blog => newBlogIDs.Contains(blog.BlogID)).Select(blog => blog.BlogID).ToList();
                if (actual.Count != newBlogIDs.Count)
                {
                    newBlogIDs = newBlogIDs.Intersect(actual).ToList();
                }
                var blogIDs = hg.blogs.Select(h => h.BlogID);
                var Members = hg.members.Select(h => h.Username);
                string[] newmembers = null;
                if (string.IsNullOrWhiteSpace(model.Members))
                    newmembers = new string[] { };
                else
                    newmembers = model.Members.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var errnames = string.Empty;
                var errids = string.Empty;
                var adminm = hg.members.Where(hm => hm.GroupLvl == 1).ToList();
                var m = new List<HanGroupMember>();
                if (imgbtye != null)
                {
                    if (!string.IsNullOrEmpty(hg.Logo))
                    {
                        await _uploadUtil.DeleteFileAsync(hg.Logo);
                    }
                    hg.Logo = _uploadUtil.GenerateImageName(hg.GroupUri);
                    await _uploadUtil.saveImageAsync(imgbtye, hg.Logo);
                }
                hg.Intro = model.Intro;
                hg.Title = model.Title;

                var existnames = _udb.Users.Where(u => newmembers.Contains(u.UserName)).Select(u => u.UserName).ToList();
                foreach (var n in newmembers)
                {
                    var name = n.Trim();
                    if (existnames.Contains(name))
                    {
                        var member = new HanGroupMember();
                        member.HanGroupID = hg.HanGroupID;
                        member.Username = name;
                        member.GroupLvl = 2;
                        m.Add(member);
                    }
                    else
                    {
                        errnames += name + ',';
                    }
                }
                var comp = new HanGroupMemberComparer();
                hg.members = adminm.Union(hg.members.Where(hm => hm.GroupLvl == 2).Intersect(m, comp).Union(m, comp), comp).ToList();
                //member = Group1 Union newGroup2
                var b = new List<HanGroupBlog>();
                foreach (var blogid in newBlogIDs)
                {
                    var blog = new HanGroupBlog();
                    blog.BlogID = blogid;
                    blog.HanGroupID = hg.HanGroupID;
                    b.Add(blog);
                }
                var comparer = new HanGroupBlogComparer();
                hg.blogs = hg.blogs.Intersect(b, comparer).Union(b, comparer).ToList();
                _db.SaveChanges();
                return Json(new { success = true });
            }
            string error = string.Empty;
            foreach (var err in ModelState.Values.SelectMany(v => v.Errors))
            {
                error += "<li>" + err.ErrorMessage + "</li>";
            }
            return Json(new { success = false, err = "<ul>" + error + "</ul>" });
        }
    }
}