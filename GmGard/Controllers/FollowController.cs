using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [Authorize, ResponseCache(CacheProfileName = "Never")]
    public class FollowController : Controller
    {
        private UsersContext _udb;
        private BlogContext _db;

        private const int pagesize = 40;

        public FollowController(
            UsersContext udb,
            BlogContext db)
        {
            _udb = udb;
            _db = db;
        }

        public ActionResult Index(int page = 1)
        {
            var userprofile = _udb.Users.Include("follows").SingleOrDefault(u => u.UserName == User.Identity.Name);
            var followedNames = userprofile.follows.Select(u => u.follow.UserName);
            var query = _db.Blogs.Where(b => followedNames.Contains(b.Author) && b.isApproved == true).OrderByDescending(b => b.BlogDate);

            Func<string, object, ActionResult> Result = View;
            if (Request.IsAjaxRequest())
            {
                Result = PartialView;
            }
            return Result("Index", query.ToPagedList(page, pagesize));
        }

        [HttpPost]
        public JsonResult Follow(string name)
        {
            name = name.ToLower();
            if (name == User.Identity.Name)
            {
                return Json(new { success = false, error = "not self" });
            }
            var profiles = _udb.Users.Where(u => u.UserName == name || u.UserName == User.Identity.Name).ToDictionary(u => u.UserName.ToLower());
            if (!profiles.ContainsKey(name))
            {
                return Json(new { success = false, error = "not found" });
            }
            var followprofile = profiles[name];
            var userprofile = profiles[User.Identity.Name.ToLower()];
            if (_udb.Follows.Find(userprofile.Id, followprofile.Id) == null)
            {
                _udb.Entry(new Follow { FollowID = followprofile.Id, UserID = userprofile.Id, FollowTime = DateTime.Now }).State = EntityState.Added;
                _udb.SaveChanges();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult UnFollow(string name)
        {
            name = name.ToLower();
            var profiles = _udb.Users.Where(u => u.UserName == name || u.UserName == User.Identity.Name).ToDictionary(u => u.UserName.ToLower());
            if (!profiles.ContainsKey(name))
            {
                return Json(new { success = false, error = "not found" });
            }
            var followprofile = profiles[name];
            var userprofile = profiles[User.Identity.Name.ToLower()];
            var entity = _udb.Follows.Find(userprofile.Id, followprofile.Id);
            if (entity != null)
            {
                _udb.Entry(entity).State = EntityState.Deleted;
                _udb.SaveChanges();
            }
            return Json(new { success = true });
        }

        [AllowAnonymous]
        public ActionResult List(string name, int page = 1, string view = "follow")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Home");
                }
                name = User.Identity.Name;
            }
            var userprofile = _udb.Users.SingleOrDefault(u => u.UserName == name);
            if (userprofile == null)
            {
                return RedirectToAction("Index", "Home");
            }
            IQueryable<FollowModel> followprofiles;
            if (view == "fans")
            {
                followprofiles = _udb.Follows.Where(f => f.FollowID == userprofile.Id).OrderByDescending(f => f.FollowTime).Select(f => new FollowModel
                {
                    UserName = f.user.UserName,
                    UserComment = f.user.UserComment,
                    Experience = f.user.Experience,
                    FollowEachOther = _udb.Follows.Any(ff => ff.UserID == f.FollowID && ff.FollowID == f.UserID)
                });
            }
            else
            {
                followprofiles = _udb.Follows.Where(f => f.UserID == userprofile.Id).OrderByDescending(f => f.FollowTime).Select(f => new FollowModel
                {
                    UserName = f.follow.UserName,
                    UserComment = f.follow.UserComment,
                    Experience = f.follow.Experience,
                    FollowEachOther = _udb.Follows.Any(ff => ff.UserID == f.FollowID && ff.FollowID == f.UserID)
                });
            }
            Func<object, ActionResult> Result = View;
            //if (Request.IsAjaxRequest())
            //{
            //    Result = this.PartialView;
            //}
            ViewBag.View = view;
            ViewBag.UserName = name;
            return Result(followprofiles.ToPagedList(page, pagesize));
        }

        [HttpGet]
        public PartialViewResult Suggestions()
        {
            var TopUsers = _udb.Users.Where(u => _udb.Users.FirstOrDefault(uu => uu.UserName == User.Identity.Name).follows.Count(f => f.FollowID == u.Id) == 0 && u.UserName != User.Identity.Name)
                            .OrderByDescending(u => u.Experience).Take(18).ToList();
            return PartialView(TopUsers);
        }

        [HttpPost]
        public JsonResult Suggestions(int[] userid)
        {
            var uid = _udb.Users.AsNoTracking().Where(u => u.UserName == User.Identity.Name).Select(u => u.Id).Single();
            foreach (var id in userid)
            {
                if (id == uid)
                {
                    continue;
                }
                _udb.Follows.Add(new Follow { UserID = uid, FollowID = id, FollowTime = DateTime.Now });
            }
            _udb.SaveChanges();
            return Json(new { success = true });
        }

        [AllowAnonymous]
        public ActionResult Status(string name = "")
        {
            return ViewComponent(nameof(ViewComponents.UserStatus), new { name = name });
        }

        [HttpPost]
        public JsonResult CheckFollows(string[] names)
        {
            if (names != null)
            {
                names = names.Where(n => !string.IsNullOrWhiteSpace(n)).ToArray();
            }
            if (names == null || names.Length < 1)
            {
                return new JsonResult(null);
            }
            var follows = _udb.Follows.Where(f => f.user.UserName == User.Identity.Name && names.Contains(f.follow.UserName)).Select(f => f.follow.UserName).ToList();
            var result = names.ToDictionary(n => n, n => follows.Contains(n), StringComparer.OrdinalIgnoreCase);
            return Json(result);
        }
    }
}