using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Data.Entity;
using System;
using Microsoft.AspNetCore.Mvc;
using GmGard.Filters;
using GmGard.Models.Binding;
using Newtonsoft.Json;

namespace GmGard.Models
{
    public class AppSettingsModel
    {
        public int UpdateInterval { get; set; }
        public int ExpAddOnPass { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> ExpAddOnDay { get; set; }
        public int ListPageSize { get; set; }
        public int HomePageSize { get; set; }
        public int UserPageSize { get; set; }
        public int AuditPageSize { get; set; }
        public int ReplyPageSize { get; set; }
        public int MsgPageSize { get; set; }
        public int TopicCarouselSize { get; set; }
        public int RankingSize { get; set; }
        public HarmonySettingsModel HarmonySettings { get; set; }
        public string RateCredentialType { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> NoLinkCategories { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> DailyReward { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> WeeklyReward { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> MonthlyReward { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> NoRankCategories { get; set; }
        public bool EnableImageProxy { get; set; }
        [ReadOnlySetting, JsonIgnore]
        public string HttpsPort { get; set; } = "443";
        [ReadOnlySetting, JsonIgnore]
        public string HttpPort { get; set; } = "80";
        [ReadOnlySetting, JsonIgnore]
        public string UploadSecret { get; set; }
        [ReadOnlySetting, JsonIgnore]
        public string SearchBackendType  { get; set; }
    }

    public class DataSettingsModel
    {
        public string FeaturedBlogIds
        {
            get { return featuredBlogIds_; }
            set
            {
                featuredBlogIds_ = value;
                featuredBlogIdList_ = BlogHelper.ParseIntListFromString(featuredBlogIds_);
            }
        }

        public string BannerTopicIds
        {
            get { return bannerTopicIds_; }
            set
            {
                bannerTopicIds_ = value;
                bannerTopicIdList_ = BlogHelper.ParseIntListFromString(bannerTopicIds_);
            }
        }

        public string BannerBlogIds
        {
            get { return bannerBlogIds_; }
            set
            {
                bannerBlogIds_ = value;
                bannerBlogIdList_ = BlogHelper.ParseIntListFromString(bannerBlogIds_);
            }
        }

        public string chuncaiNotice { get; set; }
        public string QQGroupNumber { get; set; }
        public string QQGroupUrl { get; set; }
        public float BlogApproveThreshold { get; set; }
        public float BlogDenyThreshold { get; set; }
        public int JoinAuditorLevel { get; set; }
        public List<FriendLink> FriendLinks { get; set; }

        public List<int> FeaturedBlogIdList() => featuredBlogIdList_;
        public List<int> BannerTopicIdList() => bannerTopicIdList_;
        public List<int> BannerBlogIdList() => bannerBlogIdList_;

        private string featuredBlogIds_;
        private string bannerTopicIds_;
        private string bannerBlogIds_;
        private List<int> featuredBlogIdList_;
        private List<int> bannerTopicIdList_;
        private List<int> bannerBlogIdList_;
    }

    public class FriendLink
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Img { get; set; }
    }

    public class RegisterQuestion
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

    public class RegisterSettingsModel
    {
        public bool RegisterQuestionEnabled { get; set; }
        public bool AllowRegister { get; set; }
        public int AllowMemberRegisterLevel { get; set; }
        public bool AllowBackdoor { get; set; }
        public List<RegisterQuestion> RegisterQuestions { get; set; }
        public bool RequireCode { get; set; }
        public int CodeCost { get; set; } = 300;
        public int CodeCostIncrement { get; set; } = 100;
        public const int CodeCoolDownDays = 7;
    }

    public class HarmonySettingsModel
    {
        public bool Harmony { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> WhitelistCategories { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> WhitelistTags { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> BlacklistTags { get; set; }
        // Hide display of these categories.
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> BlacklistCategories { get; set; }
        [ModelBinder(BinderType = typeof(StringSplitIntListModelBinder))]
        public List<int> WhitelistIds { get; set; }
    }

    public class AdminViewModel
    {
        public List<UserProfile> Admins { get; set; }
        public List<UserProfile> Writers { get; set; }
        public List<UserProfile> Auditors { get; set; }
        public List<UserProfile> BannedUsers { get; set; }
        public List<Category> allCategory { get; set; }
        public List<HanGroup> allHanGroup { get; set; }
        public List<Advertisment> AllAds { get; set; }
        public int totalauditcount { get; set; }
        public int auditcount { get; set; }
        public int totalusercount { get; set; }
        public int todaynewitem { get; set; }
        public int yesterdaynewitem { get; set; }
        public int bannedusercount { get; set; }
        public int harmonyblogcount { get; set; }
        public AppSettingsModel appsettings { get; set; }
        public DataSettingsModel datasettings { get; set; }
        public RegisterSettingsModel registersettings { get; set; }
        public BackgroundSetting backgroundsetting { get; set; }
    }

    public class BackgroundCss
    {
        public enum BackgroundType
        {
            Normal,
            Fixed,
        }

        [Required, RegularExpression("^[a-zA-Z0-9_\\-]+$", ErrorMessage = "无效的背景名")]
        public string Name { get; set; }

        [Required]
        public string BackgroundUrl { get; set; }

        [Required]
        public string BannerUrl { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        public string Position1440 { get; set; }

        public BackgroundType Type { get; set; }

        public string BackgroundUrl1440 { get { return BackgroundUrl.Replace(".jpg", "_1440.jpg"); } }
        public string BackgroundUrl1024 { get { return BackgroundUrl.Replace(".jpg", "_1024.jpg"); } }
        public string BannerUrl1440 { get { return BannerUrl.Replace(".jpg", "_1440.jpg"); } }
        public string BannerUrl1024 { get { return BannerUrl.Replace(".jpg", "_1024.jpg"); } }
    }

    public class BackgroundSetting
    {
        public const string DefaultHost = "static.gmgard.com";
        public const string AltDefaultHost = "static.hggard.com";
        public List<BackgroundCss> Backgrounds { get; set; }

        /// <summary>
        /// Cookie value (host name) to class name mapping
        /// </summary>
        public Dictionary<string, string> BackgroundClasses { get; set; }

        private int hash;

        public BackgroundSetting()
        {
            Backgrounds = new List<BackgroundCss>();
            BackgroundClasses = new Dictionary<string, string>();
        }

        public override int GetHashCode()
        {
            if (hash == 0)
            {
                hash = JsonConvert.SerializeObject(this).GetHashCode();
            }
            return hash;
        }

        public string ClassList()
        {
            return string.Join(",", Backgrounds.Select(b => b.Name).Reverse().Concat(new[] { "old" }));
        }

        public UserBackground GetUserBackground(HttpRequest Request)
        {
            string BgTypeCookie = Request.Cookies["bgtype"];
            string ImgBackupCookie = Request.Cookies["imgbackup"];
            var ImgBackupCookieValue = ImgBackupCookie == null ? string.Empty : ImgBackupCookie.Trim();
            var IsImgBackup = !ImgBackupCookieValue.Contains(DefaultHost);
            return new UserBackground
            {
                BgType = BgTypeCookie ?? "new",
                BgClassName = IsImgBackup && BackgroundClasses.ContainsKey(ImgBackupCookieValue)
                    ? BackgroundClasses[ImgBackupCookieValue]
                    : string.Empty
            };
        }

        public struct UserBackground
        {
            public string BgClasses
            {
                get
                {
                    return BgClassName != string.Empty ? BgType + " " + BgClassName : BgType;
                }
            }

            public string BgType;
            public string BgClassName;
        }
    }
    
    public class AdViewModel
    {
        public IEnumerable<Advertisment> Ads { get; set; }
        public AdvertisementType Type { get; set; }
    }
}