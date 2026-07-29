using Ganss.Xss;
using GmGard.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class HtmlSanitizerService
    {
        private readonly HtmlSanitizer sanitizer_;

        public static HtmlSanitizerService CreateInstance()
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("id");
            sanitizer.AllowedAttributes.Add("data-mention");
            sanitizer.AllowedAttributes.Add("controls");
            sanitizer.AllowedAttributes.Add("preload");
            sanitizer.AllowedAttributes.Add("autoplay");
            sanitizer.AllowedAttributes.Add("loop");
            sanitizer.AllowedAttributes.Add("muted");
            sanitizer.AllowedAttributes.Add("poster");
            sanitizer.AllowedAttributes.Add("playsinline");
            sanitizer.AllowedAttributes.Add("webkit-playsinline");
            sanitizer.AllowedAttributes.Add("type");
            sanitizer.AllowedAttributes.Add("src");
            sanitizer.AllowedAttributes.Add("width");
            sanitizer.AllowedAttributes.Add("height");
            sanitizer.AllowedAttributes.Add("style");
            sanitizer.AllowedAttributes.Add("crossorigin");

            return new HtmlSanitizerService(sanitizer);
        }

        public HtmlSanitizerService(HtmlSanitizer sanitizer)
        {
            sanitizer_ = sanitizer;
        }

        public string Sanitize(string content)
        {
            return sanitizer_.Sanitize(content);
        }
    }
}
