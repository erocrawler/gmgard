using GmGard.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace GmGard.Controllers
{
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
    public class RssController : Controller
    {
        private IWebHostEnvironment _env;

        public RssController(IWebHostEnvironment env)
        {
            _env = env;
        }

        //
        // GET: /Rss/
        public ActionResult Index(string id)
        {
            if (id != null && id.Contains("favicon"))
            {
                return File("favicon.ico", "image/x-icon");
            }
            return File("rss.xml", "text/xml");
        }
    }
}