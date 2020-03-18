using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.ViewComponents
{
    public class SiteHeader : ViewComponent
    {
        private readonly BlogContext _db;
        private readonly CategoryUtil _catUtil;
        private readonly IMemoryCache _cache;
        private readonly IOptions<AppSettingsModel> _appSetting;

        public SiteHeader(BlogContext db, CategoryUtil catUtil, IMemoryCache cache, IOptions<AppSettingsModel> appSetting)
        {
            _db = db;
            _catUtil = catUtil;
            _cache = cache;
            _appSetting = appSetting;
        }

        public IViewComponentResult Invoke()
        {
            List<Category> categories = _catUtil.GetCategoryList();
            Dictionary<int, HeaderDisplay> model = _cache.Get<Dictionary<int, HeaderDisplay>>("HomeHeader");
            if (model == null)
            {
                model = new Dictionary<int, HeaderDisplay>(categories.Count);
                var counts = _db.Blogs.Where(b => b.isApproved == true && DbFunctions.DiffDays(b.BlogDate, DateTime.Now) <= 1)
                    .GroupBy(b => b.CategoryID)
                    .Select(b => new { CategoryID = b.Key, newItems = b.Count() })
                    .ToList();
                foreach (var c in categories)
                {
                    int count = counts.Where(i => i.CategoryID == c.CategoryID).Sum(i => i.newItems);
                    model.Add(c.CategoryID, new HeaderDisplay { category = c, newItems = count });
                }
                int CalculateTotalItems(Category c)
                {
                    if (c == null)
                    {
                        return 0;
                    }
                    if (c.SubCategories != null)
                    {
                        foreach (var subcat in c.SubCategories)
                        {
                            int count = CalculateTotalItems(subcat);
                            model[c.CategoryID].newItems += count;
                        }
                    }
                    return model[c.CategoryID].newItems;
                }

                foreach (var main in model.Values.Where(h => !h.category.ParentCategoryID.HasValue))
                {
                    CalculateTotalItems(main.category);
                }
                _cache.Set("HomeHeader", model, TimeSpan.FromMinutes(10));
            }
            if (_appSetting.Value.HarmonySettings.Harmony && !User.Identity.IsAuthenticated)
            {
                model = model.Where(p => !_appSetting.Value.HarmonySettings.BlacklistCategories.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value);
            }
            return View(model);
        }
    }
}
