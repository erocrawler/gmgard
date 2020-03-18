using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.Drawing;
using System.IO;

namespace GmGard.Controllers
{
    public class UploadController : Controller
    {
        private const string UploadFolder = @"Images\upload\";
        private IHostingEnvironment _env;

        public UploadController(IHostingEnvironment env)
        {
            _env = env;
        }

        //
        // GET: /Image/
        [ResponseCache(Duration = 864000, Location = ResponseCacheLocation.Any)]
        public ActionResult Index(string img)
        {
            if (string.IsNullOrEmpty(img))
                return NotFound();
            string path = Path.Combine(_env.WebRootPath, UploadFolder);
            if (!System.IO.File.Exists(path + img))
                return NotFound();
            return File(UploadFolder + img, "image/jpeg");
        }
    }
}