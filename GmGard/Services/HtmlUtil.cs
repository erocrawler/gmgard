using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using GmGard.Models;
using Microsoft.Extensions.Hosting;

namespace GmGard.Services
{
    public class HtmlUtil
    {
        private static readonly string[] OwnedSites = new[] { "hggard.com/", "gmgard.com/", "gmgard.us/", "//localhost" };
        private static readonly string[] NoSslOwnedSites = new[] { "cnmobchishi.hggard.com", "tu2.gmgard.com" };

        private readonly IWebHostEnvironment _env;
        private readonly HttpContext _httpContext;
        private readonly IHtmlHelper _htmlHelper;
        private readonly AppSettingsModel _appSettings;

        public HtmlUtil(IHttpContextAccessor httpContextAccessor, IHtmlHelper htmlHelper, IWebHostEnvironment env, IOptions<AppSettingsModel> settings)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _htmlHelper = htmlHelper;
            _env = env;
            _appSettings = settings.Value;
        }


        public IHtmlContent ContentWithRewrite(string content)
        {
            if (!_httpContext.Request.IsHttps || string.IsNullOrWhiteSpace(content))
            {
                return _htmlHelper.Raw(content);
            }
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);
            var nodes = document.DocumentNode.SelectNodes("//img");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var src = node.GetAttributeValue("src", "");
                    if (src.StartsWith("http://"))
                    {
                        node.SetAttributeValue("src", RewriteSrc(src));
                    }
                }
            }
            return _htmlHelper.Raw(document.DocumentNode.OuterHtml);
        }

        public IHtmlContent RewriteBlogImage(Blog blog)
        {
            return ContentWithRewrite(BlogHelper.ReplaceContentImage(blog));
        }

        public string RewriteSrc(string src)
        {
            if (OwnedSites.Any(s => src.Contains(s)))
            {
                return src.Replace("http://", "https://");
            }
            return src;
        }
    }
}
