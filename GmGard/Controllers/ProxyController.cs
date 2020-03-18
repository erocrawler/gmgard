using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Extensions.Options;
using GmGard.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GmGard.Controllers
{
    public class ProxyController : Controller
    {
        // GET: /<controller>/
        [RequireHttps]
        public async Task<IActionResult> Index([FromServices]IOptions<AppSettingsModel> settings, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return new EmptyResult();
            }
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
            {
                return NotFound();
            }
            if (!settings.Value.EnableImageProxy)
            {
                return Redirect(url);
            }
            HttpClient client = new HttpClient();
            client.BaseAddress = result;
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.TryAddWithoutValidation(HeaderNames.UserAgent, Request.Headers[HeaderNames.UserAgent].ToString());
            try
            {
                var response = await client.GetAsync(url, HttpContext.RequestAborted);
                return File(await response.Content.ReadAsStreamAsync(), response.Content.Headers.ContentType.ToString());
            }
            catch
            {
                return NotFound();
            }
        }
    }
}
