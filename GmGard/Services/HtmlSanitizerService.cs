using Ganss.XSS;
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
