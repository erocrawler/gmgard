using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;

namespace GmGard.TagHelpers
{
    /// <summary>
    /// Content wraped in <user-option></user-option> is only displayed when the given user option is true.
    /// </summary>
    [HtmlTargetElement("user-option")]
    public class UserOptionTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Selects which option to use to decide to display.
        /// </summary>
        public Func<UserOption, bool> Option { get; set; }

        private readonly BlogUtil _blogUtil;

        public UserOptionTagHelper(BlogUtil blogUtil)
        {
            _blogUtil = blogUtil;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "";
            bool show = ViewContext.HttpContext.User.Identity.IsAuthenticated && _blogUtil.GetUserOption(ViewContext.HttpContext.User.Identity.Name, Option);
            if (!show)
            {
                output.SuppressOutput();
            }
        }
    }
}
