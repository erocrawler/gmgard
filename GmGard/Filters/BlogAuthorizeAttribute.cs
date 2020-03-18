using GmGard.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace GmGard.Filters
{
    public class BlogAuthorizeAttribute : ActionFilterAttribute
    {
        public bool Redirect { get; set; } = true;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var setting = filterContext.HttpContext.RequestServices.GetService<IOptions<AppSettingsModel>>();
            var harmony = setting.Value.HarmonySettings.Harmony;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated || harmony != true)
            {
                return;
            }
            var idstr = filterContext.RouteData.Values["id"] as string;
            int id = 0;
            if (idstr == null || int.TryParse(idstr, out id) == false)
            {
                return;
            }
            var cache = filterContext.HttpContext.RequestServices.GetService<IMemoryCache>();
            var authorize = cache.Get<bool?>("blogauth" + idstr);
            if (authorize != null && authorize.Value)
            {
                return;
            }
            var db = filterContext.HttpContext.RequestServices.GetService<BlogContext>();
            bool isHarmony = db.Blogs.Where(b => b.BlogID == id).Select(b => b.isHarmony).DefaultIfEmpty(true).SingleOrDefault();
            cache.Set<bool?>("blogauth" + idstr, isHarmony, new TimeSpan(0, 10, 0));
            if (isHarmony)
            {
                return;
            }
            else if (Redirect)
            {
                var url = filterContext.HttpContext.RequestServices.GetService<IUrlHelper>();
                filterContext.Result = new RedirectToActionResult("Index", "Error", new { id = 404, prev = url.Action("Details", "Blog", new { id }) });
            }
            else
            {
                filterContext.Result = new UnauthorizedResult();
            }
        }

        private bool CheckBots(HttpRequest Request)
        {
            // string referer = Request.Headers[HeaderNames.Referer];
            // TODO: see if we need to allow other crawlers anymore.
            //if (Request.Browser.Crawler)
            //{
            //    return true;
            //}
            return false;
        }
    }
}