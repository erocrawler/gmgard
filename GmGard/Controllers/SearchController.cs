using GmGard.Extensions;
using GmGard.Filters;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class SearchController : Controller
    {
        private readonly BlogContext _db;
        private readonly AppSettingsModel _appSettings;
        private readonly IMemoryCache _cache;

        public SearchController(
            IOptions<AppSettingsModel> appSettings,
            BlogContext db,
            IMemoryCache cache)
        {
            _db = db;
            _appSettings = appSettings.Value;
            _cache = cache;
        }

        private bool IsHarmony => _appSettings.HarmonySettings.Harmony && !User.Identity.IsAuthenticated;

        //
        // GET: /Search/Tags/5

        public async Task<ActionResult> Tags(int id = 0, int page = 1, string sort = "")
        {
            if (id == 0)
                return NotFound();
            Tag tag = await _db.Tags.FindAsync(id);
            if (tag == null)
            {
                return NotFound();
            }
            await _db.Database.ExecuteSqlCommandAsync("Update Tags Set TagVisit = TagVisit + 1 Where TagId = @id", new System.Data.SqlClient.SqlParameter("@id", tag.TagID));
            return RedirectToAction("List", "Blog", new { Tags = tag.TagName, sort, page });
        }

        [HttpPost]
        public RedirectToActionResult Start(string name, string searchmethod)
        {
            if (searchmethod == "title")
            {
                return RedirectToAction("List", "Blog", new { Title = name });
            }
            else if (searchmethod == "tag")
            {
                return RedirectToAction("List", "Blog", new { Tags = name });
            }
            return RedirectToAction("List", "Blog", new { Query = name });
        }

        [HttpPost]
        public async Task<JsonResult> FetchTags(string name)
        {
            if (IsHarmony)
            {
                return Json(new { });
            }
            string[] query;
            if (string.IsNullOrWhiteSpace(name))
            {
                query = await _cache.GetOrCreateAsync("~T500T", (c) =>
                {
                    c.SlidingExpiration = TimeSpan.FromMinutes(30);
                    return _db.Tags.OrderByDescending(t => t.TagVisit).Take(500).Select(t => t.TagName).ToArrayAsync();
                });
            }
            else
            {
                query = await _db.Tags.Where(t => t.TagName.StartsWith(name)).OrderByDescending(t => t.TagVisit).Take(10).Select(t => t.TagName).ToArrayAsync();
            }
            return Json(query);
        }
    }
}