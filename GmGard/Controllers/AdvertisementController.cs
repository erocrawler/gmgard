using GmGard.Filters;
using GmGard.Models;
using GmGard.ViewComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class AdvertisementController : Controller
    {
        private BlogContext _db;

        public AdvertisementController(BlogContext db)
        {
            _db = db;
        }

        public JsonResult Click(string adid)
        {
            int id;
            if (!int.TryParse(adid, out id))
                return Json(null);
            else
            {
                var ad = _db.Advertisments.Find(id);
                if (ad != null)
                {
                    ad.ClickCount++;
                    _db.SaveChanges();
                    return Json(ad.ClickCount);
                }
                else
                    return Json(null);
            }
        }

        public ActionResult Carousel()
        {
            return ViewComponent(nameof(Advertisement), new { AdType = AdvertisementType.Carousel });
        }

        [Route("Ad/{key}")]
        public ActionResult Details(
            [FromServices]ICompositeViewEngine compositeViewEngine, 
            [FromServices]IActionContextAccessor actionContextAccessor, 
            string key)
        {
            var result = compositeViewEngine.FindView(actionContextAccessor.ActionContext, key, false);
            if (result.Success)
            {
                return View(key);
            }
            return NotFound();
        }
    }
}