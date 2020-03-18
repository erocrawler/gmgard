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
    [HtmlTargetElement("bg-style", TagStructure = TagStructure.WithoutEndTag)]
    public class BackgroundCssTagHelper : TagHelper {
        private BackgroundSetting _bgSetting;
        private IUrlHelper _urlHelper;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// The background class name
        /// </summary>
        public string BgName { get; set; }

        public BackgroundCssTagHelper(IUrlHelper urlHelper, IOptionsSnapshot<BackgroundSetting> bgSetting)
        {
            _urlHelper = urlHelper;
            _bgSetting = bgSetting.Value;
        }

        public override int Order => base.Order - 1;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string bgName = BgName;
            output.TagName = "link";
            output.Attributes.Add("rel", "stylesheet");
            output.Attributes.Add("href", _urlHelper.Action("Index", "Css", new { bg = bgName, v = _bgSetting.GetHashCode() }));
        }
    }

    [HtmlTargetElement("bg-user", TagStructure = TagStructure.WithoutEndTag)]
    public class UserBackgroundCssTagHelper : TagHelper
    {
        private BackgroundSetting _bgSetting;
        private IUrlHelper _urlHelper;

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public UserBackgroundCssTagHelper(IUrlHelper urlHelper, IOptionsSnapshot<BackgroundSetting> bgSetting)
        {
            _urlHelper = urlHelper;
            _bgSetting = bgSetting.Value;
        }

        public override int Order => base.Order - 1;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var bg = _bgSetting.GetUserBackground(ViewContext.HttpContext.Request);
            output.TagName = "link";
            output.Attributes.Add("rel", "stylesheet");
            output.Attributes.Add("href", _urlHelper.Action("UserBg", "Css", new { bg = bg.BgClassName, v = UserQuest.AllTitleBackground.Count }));
        }
    }
}
