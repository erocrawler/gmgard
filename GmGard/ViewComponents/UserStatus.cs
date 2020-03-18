using GmGard.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.ViewComponents
{
    public class UserStatus : ViewComponent
    {
        private readonly BlogContext _db;
        private readonly UsersContext _udb;

        public UserStatus(BlogContext db, UsersContext udb)
        {
            _db = db;
            _udb = udb;
        }

        public IViewComponentResult Invoke(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Content(string.Empty);
                }
                UserName = User.Identity.Name;
            }
            var userprofile = _udb.Users.AsNoTracking().SingleOrDefault(u => u.UserName == UserName);
            if (userprofile == null)
            {
                return Content(string.Empty);
            }
            ViewBag.UserBlogs = _db.Blogs.Count(b => b.Author == userprofile.UserName);
            ViewBag.UserFollows = _udb.Follows.Count(f => f.UserID == userprofile.Id);
            ViewBag.UserFans = _udb.Follows.Count(f => f.FollowID == userprofile.Id);
            ViewBag.UserFavorites = _db.Favorites.Count(f => f.Username == userprofile.UserName);
            return View(userprofile);
        }
    }
}
