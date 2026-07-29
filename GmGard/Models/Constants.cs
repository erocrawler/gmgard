using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace GmGard.Models
{
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
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
    public class OAuthConfig
    {
        public string DefaultClientId { get; set; }
        public string DefaultClientSecret { get; set; }
        public string DefaultRedirectUri { get; set; }
        public string[] Scopes { get; set; }
    }
    public class BlazorAppConfig
    {
        /// <summary>Global toggle — when false, App() always redirects to legacy AppHost (Angular). Default false = safe opt-in.</summary>
        public bool Enabled { get; set; } = false;

        /// <summary>Deny list — when Enabled=true, entries with false keep that prefix on legacy Angular. E.g. { "wheel": false }</summary>
        public Dictionary<string, bool> RedirectOverrides { get; set; } = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Legacy allow-list field, kept for backward compat but ignored in deny-list model.</summary>
        public string[] ExtraPrefixes { get; set; } = Array.Empty<string>();
    }

    public class SiteConfig
    {
        public List<SiteInfo> Sites { get; set; }
        public SiteInfo DevSite { get; set; }
        public string[] AppHostOrigins { get; set; }
        public string[] DevAppHostOrigins { get; set; }
        public string[] SmileyPaths { get; set; }
        public OAuthConfig OAuth { get; set; }
        /// <summary>null => treat as disabled (safe default). Must add BlazorApp:{Enabled:true} to opt-in.</summary>
        public BlazorAppConfig? BlazorApp { get; set; }
    }

    public class ConstantUtil
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly SiteConfig _config;
        private SiteInfo _currentSite;

        public ConstantUtil(IHttpContextAccessor contextAccessor, IWebHostEnvironment env, IOptions<SiteConfig> config)
        {
            _contextAccessor = contextAccessor;
            _config = config.Value;
            if (env.IsDevelopment())
            {
                _currentSite = _config.DevSite;
            }
            else
            {
                var host = _contextAccessor.HttpContext.Request.Host.Host;
                _currentSite = _config.Sites.FirstOrDefault(s => s.Host == host) ?? _config.Sites.FirstOrDefault(s => s.Host == "gmgard.com");
            }
        }

        private string EffectiveProtocol
        {
            get
            {
                if (_contextAccessor.HttpContext.Request.Headers.TryGetValue("X-Forwarded-Proto", out Microsoft.Extensions.Primitives.StringValues val) && val.Count > 0)
                {
                    return val[^1];
                }
                return _contextAccessor.HttpContext.Request.IsHttps ? "https" : "http";
            }
        }

        public string SiteName => _currentSite.Name;

        public string SiteHost => _currentSite.Host;

        public string SiteDesc => _currentSite.Desc;

        public string SiteLogo => _currentSite.Logo;

        public string SiteTitle => _currentSite.Title;

        public string SiteStaticHost => _currentSite.StaticHost;

        public string SiteBaseUrl => EffectiveProtocol + "://" + _currentSite.Host;

        public string AppHost => EffectiveProtocol + "://" + _currentSite.AppHost;

        public IHttpContextAccessor HttpContextAccessor() => _contextAccessor;
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