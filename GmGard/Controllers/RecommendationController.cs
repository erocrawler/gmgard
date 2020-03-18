using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GmGard.Services;
using GmGard.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data.Entity;

namespace GmGard.Controllers
{
    [Authorize(Policy = "Harmony")]
    public class RecommendationController : Controller
    {
        private IRecommendationProvider _recommendationService;
        private BlogContext _db;

        public RecommendationController(IRecommendationProvider recommendationService, BlogContext blogContext)
        {
            _recommendationService = recommendationService;
            _db = blogContext;
        }

        public async Task<IActionResult> Blog(int id, int count = 5)
        {
            if (!_recommendationService.IsValid())
            {
                return new EmptyResult();
            }
            var blog = await _db.Blogs.Where(b => b.BlogID == id)
                .GroupJoin(_db.TagsInBlogs.DefaultIfEmpty(),
                    b => b.BlogID,
                    tib => tib.BlogID,
                    (b, tib) => new BlogDetailDisplay { blog = b, tag = tib.Select(t => t.tag) })
                .SingleOrDefaultAsync();
            if (blog == null)
            {
                return NotFound();
            }
            var blogs = (await _recommendationService.GetRecommendationAsync(blog.blog, blog.tag.Select(t => t.TagName), count));
            if (blogs.Count() == 0)
            {
                return new EmptyResult();
            }
            return PartialView(blogs);
        }

        public RedirectResult BlogVisit(int id)
        {
            return Redirect(Url.Action("Details", "Blog", new { id = id }));
        }
    }
}