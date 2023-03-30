using System.Linq;
using Microsoft.AspNetCore.Mvc;
using GmGard.Models;
using GmGard.Services;
using GmGard.Models.App;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.Collections.Generic;
using GmGard.Extensions;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Net.Http;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Search/[action]")]
    [EnableCors("GmAppOrigin")]
    [ApiController]
    public class SearchController : AppControllerBase
    {
        private readonly ISearchProvider searchProvider_;
        private readonly BlogUtil blogUtil_;
        private readonly HarmonySettingsModel harmonySettings_;
        private readonly System.Uri proxyAddress_;

        public SearchController(ISearchProvider searchProvider, BlogUtil blogUtil, IOptions<AppSettingsModel> harmonySettings)
        {
            searchProvider_ = searchProvider;
            blogUtil_ = blogUtil;
            harmonySettings_ = harmonySettings.Value.HarmonySettings ?? new HarmonySettingsModel();
            if (System.Uri.TryCreate(harmonySettings.Value.HttpRequestProxy, System.UriKind.Absolute, out System.Uri result))
            {
                proxyAddress_ = result;
            }
        }

        private bool IsHarmony => harmonySettings_.Harmony && !User.Identity.IsAuthenticated;

        [HttpPost, ActionName("Blog")]
        public Task<JsonResult> PostBlog([FromBody]SearchModel model, int limit = 10, int skip = 0)
        {
            return Blog(model, limit, skip);
        }

        [HttpGet]
        public async Task<JsonResult> Blog([FromQuery]SearchModel model, int limit = 10, int skip = 0)
        {
            limit = System.Math.Min(System.Math.Max(limit, 1), 100);
            int page = skip <= 0 ? 1 : (skip / limit + 1);
            if (IsHarmony)
            {
                model.Harmony = true;
            }
            var blogs = await searchProvider_.SearchBlogAsync(model, page, limit);
            var items = new X.PagedList.StaticPagedList<BlogPreview>(blogs.Blogs.Select(b => 
                new BlogPreview {
                    Brief = blogUtil_.GetFirstLine(b, true).ToString(),
                    Id = b.BlogID,
                    ThumbUrl = BlogHelper.firstImgPath(b, true),
                    ImageUrl = BlogHelper.firstImgPath(b),
                    Title = b.BlogTitle,
                    CreateDate = b.BlogDate,
                    Url = Url.Action("Details", "Blog", new { Area = "", id = b.BlogID }, Request.Scheme),
            }), blogs.Blogs.GetMetaData());
            return Json(new Paged<BlogPreview>(items));
        }
        
        [HttpPost]
        [Authorize(Policy = "Harmony")]
        public async Task<JsonResult> Dlsite([FromBody] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(null);
            var url = string.Format(@"http://www.dlsite.com/maniax/fsr/=/language/jp/sex_category%5B0%5D/male/keyword/{0}/per_page/30/show_type/1", WebUtility.UrlEncode(q));
            var handler = new HttpClientHandler();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (proxyAddress_ != null)
            {
                handler.Proxy = new WebProxy(proxyAddress_, true);
                handler.UseProxy = true;
            }
            var cookie = new Cookie("adultchecked", "1", "/", "dlsite.com");
            var cookieHeader = new CookieHeaderValue(cookie.Name, cookie.Value);
            request.Headers.Add(HeaderNames.Cookie, cookieHeader.ToString());
            request.Headers.Add(HeaderNames.UserAgent, Request.Headers[HeaderNames.UserAgent].ToString());
            request.Headers.Add(HeaderNames.Accept, Request.Headers[HeaderNames.Accept].ToString());
            var client = new HttpClient(handler);
            try
            {
                var resp = await client.SendAsync(request);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.Load(resp.Content.ReadAsStream(), System.Text.Encoding.UTF8);
                var node = doc.GetElementbyId("search_result_list");
                if (node == null)
                {
                    return Json(new { success = true });
                }
                var entryList = new List<DLsite>();
                foreach(var dl in node.SelectNodes(".//dl[@class='work_1col']"))
                {
                    var entry = new DLsite();
                    var titleLink = dl.SelectSingleNode("dt/a");
                    if (titleLink != null)
                    {
                        entry.Title = titleLink.InnerText;
                        entry.Url = titleLink.GetAttributeValue("href", "");
                        int spos = entry.Url.LastIndexOf("product_id/") + 11;
                        int epos = entry.Url.LastIndexOf(".html");
                        if (spos > 11 && epos > spos)
                        {
                            entry.RjCode = entry.Url.Substring(spos, epos - spos);
                        }
                    }
                    var circleLink = dl.SelectSingleNode("dd[@class='maker_name']/a");
                    if (circleLink != null)
                    {
                        entry.Circle = circleLink.InnerText;
                        entry.CircleUrl = circleLink.GetAttributeValue("href", "");
                    }
                    entry.Description = dl.SelectSingleNode("dd[@class='work_text']")?.InnerText;
                    entryList.Add(entry);
                }
                return Json(new { success = true, entries = entryList });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }
}