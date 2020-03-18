using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Category/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class CategoryController : AppControllerBase
    {
        private readonly CategoryUtil categoryUtil_;
        private readonly Models.BlogContext db_;

        public CategoryController(CategoryUtil categoryUtil, Models.BlogContext blogContext)
        {
            categoryUtil_ = categoryUtil;
            db_ = blogContext;
        }

        [HttpGet]
        public IActionResult List()
        {
            IEnumerable<Models.Category> list = categoryUtil_.GetCategoryList();
            if (!User.Identity.IsAuthenticated)
            {
                list = categoryUtil_.GetCategoryList().Where(c => c.ParentCategoryID == null);
            }
            return Json(list.Select(c => new Models.App.Category
            {
                Id = c.CategoryID,
                Name = c.CategoryName,
                ParentId = c.ParentCategoryID
            }));
        }

        [HttpGet]
        public async Task<IActionResult> NewItemCount(DateTime? since)
        {
            if (!since.HasValue)
            {
                since = DateTime.Now.AddDays(-1);
            }
            if (DateTime.Now - since.Value > TimeSpan.FromDays(7))
            {
                since = DateTime.Now.AddDays(-7);
            }
            IEnumerable<Models.Category> categories = categoryUtil_.GetCategoryList();
            var query = db_.Blogs.Where(b => b.isApproved == true && b.BlogDate >= since.Value);
            if (!User.Identity.IsAuthenticated)
            {
                categories = categories.Where(c => c.ParentCategoryID == null);
                query = query.Where(b => b.isHarmony);
            }
            var counts = await query
                .GroupBy(b => b.CategoryID)
                .ToDictionaryAsync(b => b.Key, b => b.Count());
            int CalculateTotalItems(Models.Category c)
            {
                if (c == null)
                {
                    return 0;
                }
                counts.TryGetValue(c.CategoryID, out int count);
                if (c.SubCategories != null)
                {
                    foreach (var subcat in c.SubCategories)
                    {
                        count += CalculateTotalItems(subcat);
                    }
                    counts[c.CategoryID] = count;
                }
                return count;
            }
            int total = 0;
            foreach (var main in categories.Where(h => !h.ParentCategoryID.HasValue))
            {
                total += CalculateTotalItems(main);
            }
            var byCategories = categories.Select(c => new Models.App.NewItemCount.ByCategory
            {
                Id = c.CategoryID,
                Count = counts.ContainsKey(c.CategoryID) ? counts[c.CategoryID] : 0
            });
            return Json(new Models.App.NewItemCount
            {
                Since = since.Value,
                Total = total,
                ByCategories = byCategories.ToArray(),
            });
        }
    }
}