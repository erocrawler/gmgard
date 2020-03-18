using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using GmGard.Models;

namespace GmGard.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class ErrorController : Controller
    {
        public IActionResult Index(int id = 404, string prev = "")
        {
            var env = HttpContext.RequestServices.GetService<IHostingEnvironment>();
            var exceptionHandler = HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandler != null)
            {
                var actionContext = HttpContext.RequestServices.GetService<IActionContextAccessor>();
                var model = new ErrorContextModel { Exception = exceptionHandler.Error };
                return View("Error", model);
            }

            if (id == 404)
            {
                if (string.IsNullOrEmpty(prev))
                {
                    return PhysicalFile(System.IO.Path.Combine(env.WebRootPath, id + ".html"), "text/html");
                }
                return View("NotFound");
            }
            Response.StatusCode = id;
            if (id >= 500 && id <= 599)
            {
                return PhysicalFile(System.IO.Path.Combine(env.WebRootPath, 500 + ".html"), "text/html");
            }
            else if (id == 401)
            {
                return Content("无权访问，错误代码: " + id);
            }
            else if (id >= 400 && id <= 499)
            {
                return Content("无效的请求，错误代码: " + id);
            }
            return Content("Status Code: " + id);
        }
    }
}
