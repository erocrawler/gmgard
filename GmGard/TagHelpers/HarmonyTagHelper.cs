using GmGard.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace GmGard.TagHelpers
{
    /// <summary>
    /// Content wraped in <harmony></harmony> is only displayed when Harmony is turned on and user is not logged in.
    /// </summary>
    [HtmlTargetElement("harmony")]
    public class HarmonyTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// If true, display content when harmony. Otherwise hide content when harmony.
        /// </summary>
        public bool Show { get; set; }

        private IOptions<AppSettingsModel> _appSetting;

        public HarmonyTagHelper(IOptions<AppSettingsModel> appSetting)
        {
            _appSetting = appSetting;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "";
            bool harmony = _appSetting.Value.HarmonySettings.Harmony && !ViewContext.HttpContext.User.Identity.IsAuthenticated;
            if ((Show && !harmony) || (!Show && harmony))
            {
                output.SuppressOutput();
            }
        }
    }
}
