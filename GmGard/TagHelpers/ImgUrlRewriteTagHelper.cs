using GmGard.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System;
using System.Linq;

namespace GmGard.TagHelpers
{
    [HtmlTargetElement("img", Attributes = "src")]
    [HtmlTargetElement(Attributes = "data-src")]
    [HtmlTargetElement(Attributes = "data-img-href")]
    public class ImgUrlRewriteTagHelper : TagHelper
    {
        private readonly string[] TargetPaths = new[] { "//static.gmgard.us/", "//static.gmgard.com" };


        public override int Order => 99;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        private readonly BackgroundSetting _bgSetting;
        private readonly Services.HtmlUtil _htmlUtil;
        private readonly ConstantUtil _constantUtil;

        public ImgUrlRewriteTagHelper(IOptionsSnapshot<BackgroundSetting> bgSetting, Services.HtmlUtil htmlUtil, ConstantUtil constUtil)
        {
            _bgSetting = bgSetting.Value;
            _htmlUtil = htmlUtil;
            _constantUtil = constUtil;
        }

        private string GetRewriteAddr()
        {
            var cookie = ViewContext.HttpContext.Request.Cookies["imgbackup"];
            if (cookie != null && _bgSetting.BackgroundClasses.ContainsKey(cookie))
            {
                return $"//{cookie}/";
            }
            return $"//{_constantUtil.SiteStaticHost}/";
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var name = output.TagName.ToLower();
            if (name == "a" || name == "div" || name == "img")
            {
                var hostAddr = GetRewriteAddr();
                var attrs = output.Attributes.Where(a => a.Name == "href" || a.Name == "data-src" || a.Name == "src").ToList();
                foreach (var attr in attrs)
                {
                    var src = attr.Value.ToString();
                    foreach (var t in TargetPaths)
                    {
                        if (src.Contains(t))
                        {
                            output.Attributes.Remove(attr);
                            output.Attributes.Add(attr.Name, src.Replace(t, hostAddr));
                            break;
                        }
                    }
                    if (ViewContext.HttpContext.Request.IsHttps && src.Contains("http://"))
                    {
                        output.Attributes.Remove(attr);
                        output.Attributes.Add(attr.Name, _htmlUtil.RewriteSrc(src));
                    }
                }
            }
        }
    }
}
