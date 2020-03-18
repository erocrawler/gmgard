using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;

namespace GmGard.Models
{
    public static class SiteConstant
    {
        public static readonly string SiteName = "紳士の庭";
        public static readonly string AltSiteName = "北紳の庭";
        public static readonly string AltHost = "hggard.com";
        public static readonly string Host = "gmgard.com";
        public static readonly string Description = "gmgard.com♢紳士の庭♢ 绅士们的二次元资源分享交流平台";
        public static readonly string AltDescription = "hggard.com♢北紳の庭♢ 二次元资源分享交流平台";
        public static readonly string AppHost = "app.gmgard.com";
        public static readonly string AltAppHost = "app.hggard.com";
        public static readonly string DevAppHost = "localhost:4200";
        public static readonly string SiteLogo = "//static.gmgard.com/Images/sinsi2.png";
        public static readonly string AltSiteLogo = "//static.hggard.com/Images/hopo.png";
        public static readonly string[] AppHostOrigins = new[] { "http://app.gmgard.com", "https://app.gmgard.com", "http://app.hggard.com", "https://app.hggard.com" };
        public static readonly string[] DevAppHostOrigins = new[] { "http://localhost:4200" };
        public static readonly string[] SmileyPaths = { "gmgard.us/smiley", "hggard.us/smiley", "gmgard.com/smiley", "hggard.com/smiley" };
    }

    public class ConstantUtil
    {
        private IHttpContextAccessor _contextAccessor;
        private bool isDev;

        public ConstantUtil(IHttpContextAccessor contextAccessor, IHostingEnvironment env)
        {
            _contextAccessor = contextAccessor;
            isDev = env.IsDevelopment();
        }

        public bool IsAltSite => _contextAccessor.HttpContext.Request.Host.ToUriComponent().Contains(SiteConstant.AltHost);

        public string SiteName => IsAltSite ? SiteConstant.AltSiteName : SiteConstant.SiteName;

        public string SiteHost => IsAltSite ? SiteConstant.AltHost : SiteConstant.Host;

        public string SiteDesc => IsAltSite ? SiteConstant.AltDescription : SiteConstant.Description;

        public string SiteLogo => IsAltSite ? SiteConstant.AltSiteLogo : SiteConstant.SiteLogo;

        public string AppHost {
            get {
                var host = isDev ? SiteConstant.DevAppHost: (IsAltSite ? SiteConstant.AltAppHost : SiteConstant.AppHost);
                return (_contextAccessor.HttpContext.Request.IsHttps ? "https://" : "http://") + host;
            }
        }
    }

    public static class PostConstant
    {
        public static readonly int Suggestions = 0;
        public static readonly int Problems = -1;
        public static readonly int HistoryRankings = -2;

        public static bool PostRatingEventActive => DateTime.Now < new DateTime(2019, 2, 11);
    }

    public static class JsConstant
    {
        public static readonly string CKEditor = "/ckeditor/ckeditor.js?v4.7.3";
    }
}