using GmGard.Extensions;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.ViewComponents
{
    public class ReplyView : ViewComponent
    {
        private readonly BlogContext _db;
        private readonly BlogUtil _blogUtil;
        private readonly IOptions<AppSettingsModel> _appSettings;

        public ReplyView(BlogContext db, BlogUtil blogUtil, IOptions<AppSettingsModel> appSettings)
        {
            _db = db;
            _blogUtil = blogUtil;
            _appSettings = appSettings;
        }

        private int ReplyPageSize => _appSettings.Value.ReplyPageSize;

        public IViewComponentResult Invoke(int itemid, string name = "", int pagenum = 1, ItemType idtype = ItemType.Blog, bool hottest = false)
        {
            ViewData["idtype"] = idtype;
            ViewData["itemid"] = itemid;
            ViewData["hottest"] = hottest;
            var posts = _db.Posts.Include("replies").Where(p => p.ItemId == itemid && p.IdType == idtype);
            if (!string.IsNullOrEmpty(name))
            {
                posts = posts.Where(p => p.Author == name);
                ViewBag.relplyusername = name;
            }
            PagedList.IPagedList<Post> model;
            if (idtype == ItemType.Blog)
            {
                var query = posts.GroupJoin(_db.BlogRatings, p => p.PostId, r => r.PostId, (p, r) => new { post = p, ratings = r })
                    .SelectMany(a => a.ratings.DefaultIfEmpty(), (p, r) => new { post = p.post, blograting = r });
                if (hottest)
                {
                    query = query.OrderByDescending(p => p.post.Rating).ThenByDescending(p => p.post.PostDate);
                }
                else
                {
                    query = query.OrderByDescending(p => p.post.PostDate);
                }
                var paged = query.ToPagedList(pagenum, ReplyPageSize);
                model = new PagedList.StaticPagedList<Post>(paged.Select(a => a.post), paged.GetMetaData());
                IDictionary<int, BlogRating> ratings = query.Where(q => q.blograting != null).ToDictionary(q => q.post.PostId, q => q.blograting);
                ViewBag.ratings = ratings;
            }
            else
            {
                IQueryable<Post> query;
                if (hottest)
                {
                    query = posts.OrderByDescending(p => p.Rating).ThenByDescending(p => p.PostDate);
                }
                else
                {
                    query = posts.OrderByDescending(p => p.PostDate);
                }
                model = posts.OrderByDescending(p => p.PostDate).ToPagedList(pagenum, ReplyPageSize);
            }
            ViewBag.nicknames = _blogUtil.GetNickNames(model.Select(p => p.Author).Concat(model.SelectMany(p => p.Replies).Select(r => r.Author)));
            return View(model);
        }
    }
}
