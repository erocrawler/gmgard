using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace GmGard.Models
{
    public static class SiteConstant
    {
        public class SiteInfo
        {
            public string Name { get; set; }
            public string Host { get; set; }
            public string StaticHost { get; set; }
            public string Desc { get; set; }
            public string Logo { get; set; }
            public string Title { get; set; }
            public string AppHost { get; set; }
        }
        public static readonly SiteInfo DefaultSite = new()
        {
            Name = "紳士の庭",
            Host = "gmgard.com",
            Desc = "gmgard.com♢紳士の庭♢ 绅士们的二次元资源分享交流平台",
            Logo = "//static.gmgard.com/Images/sinsi2.png",
            Title = "GMgard",
            StaticHost = "static.gmgard.com",
            AppHost = "app.gmgard.com",
        };
        public static readonly SiteInfo HggardSite = new()
        {
            Name = "北紳の庭",
            Host = "hggard.com",
            Desc = "hggard.com♢北紳の庭♢ 二次元资源分享交流平台",
            Logo = "//static.hggard.com/Images/hopo.png",
            StaticHost = "static.hggard.com",
            Title = "HGgard",
            AppHost = "app.hggard.com",
        };
        public static readonly SiteInfo GmGardMoeSite = new()
        {
            Name = "紳士の庭",
            Host = "gmgard.moe",
            Desc = "gmgard.moe♢紳士の庭♢ 绅士们的二次元资源分享交流平台",
            Logo = "//static.gmgard.moe/Images/sinsi2.png",
            StaticHost = "static.gmgard.moe",
            Title = "GMgard",
            AppHost = "app.gmgard.moe",
        };
        public static readonly SiteInfo DevSite = new()
        {
            Name = "紳士の庭（DEV版）",
            Host = "localhost:59144",
            Desc = "gmgard.com♢紳士の庭♢ 绅士们的二次元资源分享交流平台",
            Logo = "//static.gmgard.com/Images/sinsi2.png",
            Title = "GMgard",
            StaticHost = "static.gmgard.com",
            AppHost = "localhost:4200",
        };
        public static readonly IDictionary<string, SiteInfo> Sites = new Dictionary<string, SiteInfo>()
        {
            { DefaultSite.Host, DefaultSite },
            { HggardSite.Host, HggardSite },
            { GmGardMoeSite.Host, GmGardMoeSite },
        };
        public static readonly string[] AppHostOrigins = new[] { 
            "http://app.gmgard.com", 
            "https://app.gmgard.com", 
            "http://app.hggard.com", 
            "https://app.hggard.com",
            "http://app.gmgard.moe",
            "https://app.gmgard.moe",
        };
        public static readonly string[] DevAppHostOrigins = new[] { "http://localhost:4200" };
        public static readonly string[] SmileyPaths = { 
            "gmgard.us/smiley",
            "gmgard.com/smiley", 
            "hggard.com/smiley",
            "gmgard.moe/smiley",
        };
    }

    public class ConstantUtil
    {
        private IHttpContextAccessor _contextAccessor;
        private SiteConstant.SiteInfo _currentSite;

        public ConstantUtil(IHttpContextAccessor contextAccessor, IWebHostEnvironment env)
        {
            _contextAccessor = contextAccessor;
            if (env.IsDevelopment())
            {
                _currentSite = SiteConstant.DevSite;
            }
            else if (!SiteConstant.Sites.TryGetValue(_contextAccessor.HttpContext.Request.Host.Host, out _currentSite))
            {
                _currentSite = SiteConstant.DefaultSite;
            }
        }

        public string SiteName => _currentSite.Name;

        public string SiteHost => _currentSite.Host;

        public string SiteDesc => _currentSite.Desc;

        public string SiteLogo => _currentSite.Logo;

        public string SiteTitle => _currentSite.Title;

        public string SiteStaticHost => _currentSite.StaticHost;

        public string AppHost => (_contextAccessor.HttpContext.Request.IsHttps ? "https://" : "http://") + _currentSite.AppHost;
    }

    public static class PostConstant
    {
        public static readonly int Suggestions = 0;
        public static readonly int Problems = -1;
        public static readonly int HistoryRankings = -2;
        public static readonly int SiteRules = -1;
        public static readonly int SiteVersionNotes = 0;

        public static bool PostRatingEventActive => DateTime.Now < new DateTime(2019, 2, 11);
    }

    public static class JsConstant
    {
        public static readonly string CKEditor = "/ckeditor/ckeditor.js?v4.7.3";
    }
}