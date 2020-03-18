using System.Diagnostics;
using Microsoft.Extensions.Logging;
using CodeFirstStoreFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace GmGard.Models
{
    [DbConfigurationType(typeof(DbCodeConfiguration))]
    public class BlogContext : DbContext
    {
        public BlogContext(string nameOrConnectionString, ILoggerFactory logger)
            :base(nameOrConnectionString)
        {
            var log = logger.CreateLogger<BlogContext>();
            Database.Log = l =>
            {
                log.LogInformation(l);
            };
        }

        public BlogContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogOption> BlogOptions { get; set; }
        public DbSet<BlogAudit> BlogAudits { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostRating> PostRatings { get; set; }
        public DbSet<BlogRating> BlogRatings { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagsInBlog> TagsInBlogs { get; set; }
        public DbSet<TagHistory> TagHistories { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<BlogsInTopic> BlogsInTopics { get; set; }
        public DbSet<Advertisment> Advertisments { get; set; }
        public DbSet<HanGroup> HanGroups { get; set; }
        public DbSet<HanGroupBlog> HanGroupBlogs { get; set; }
        public DbSet<HanGroupMember> HanGroupMembers { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<HistoryRanking> HistoryRankings { get; set; }
        public DbSet<Bounty> Bounties { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ComplexType<RankedBlogId>();
            modelBuilder.Conventions.Add(new FunctionsConvention<BlogContext>("dbo"));
            modelBuilder.Entity<Blog>()
                        .HasRequired(e => e.category)
                        .WithMany(c => c.Blogs)
                        .HasForeignKey(e => e.CategoryID)
                        .WillCascadeOnDelete(false);
        }

        [DbFunction("BlogContext", "FreeTextSearchBlog")]
        public IQueryable<RankedBlogId> FreeTextSearchBlog(string SearchTitle)
        {
            var SearchTitleParameter = SearchTitle != null ?
                new ObjectParameter("SearchTitle", SearchTitle) :
                new ObjectParameter("SearchTitle", typeof(string));

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<RankedBlogId>(
                    string.Format("[{0}].{1}", GetType().Name,
                        "[FreeTextSearchBlog](@SearchTitle)"), SearchTitleParameter);
        }

        [DbFunction("BlogContext", "ContainsSearchBlog")]
        public IQueryable<RankedBlogId> ContainsSearchBlog(string SearchTitle)
        {
            var SearchTitleParameter = SearchTitle != null ?
                new ObjectParameter("SearchTitle", SearchTitle) :
                new ObjectParameter("SearchTitle", typeof(string));

            return ((IObjectContextAdapter)this).ObjectContext
                .CreateQuery<RankedBlogId>(
                    string.Format("[{0}].{1}", GetType().Name,
                        "[ContainsSearchBlog](@SearchTitle)"), SearchTitleParameter);
        }
    }

    public class RankedBlogId
    {
        public int BlogID { get; set; }
        public int SearchRank { get; set; }
    }

    public class Blog
    {
        [ScaffoldColumn(false), DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int BlogID { get; set; }

        [Required(ErrorMessage = "请输入标题"), StringLength(120, ErrorMessage = "标题不得超过120个字符"), Display(Name = "标题")]
        public string BlogTitle { get; set; }

        [Required(ErrorMessage = "请输入内容"), StringLength(int.MaxValue), Display(Name = "内容"), DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [MaxLength(1024)]
        public string ImagePath { get; set; }

        // If true, the image files in ImagePath is owned by this blog.
        public bool IsLocalImg { get; set; }

        [Display(Name = "日期")]
        public DateTime BlogDate { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category category { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(30)]
        public string Author { get; set; }

        public bool? isApproved { get; set; }
        public bool isHarmony { get; set; }

        [DefaultValue(0)]
        public long BlogVisit { get; set; }

        public string Links { get; set; }

        public int? Rating { get; set; }

        public virtual BlogOption option { get; set; }

        public virtual List<BlogAudit> blogAudits { get; set; }
    }

    public class BlogOption
    {
        [Key, ForeignKey("blog")]
        public int BlogID { get; set; }

        public bool LockTags { get; set; }

        [MaxLength(200, ErrorMessage = "签名不得超过200字")]
        public string LockDesc { get; set; }

        public bool NoRate { get; set; }

        public bool NoComment { get; set; }

        public bool NoApprove { get; set; }

        public virtual Blog blog { get; set; }

        public bool IsDefault()
        {
            return !LockTags && string.IsNullOrEmpty(LockDesc) && !NoRate && !NoComment && !NoApprove;
        }
    }

    public class BlogAudit
    {
        public enum Action
        {
            None, VoteApprove, VoteDeny, Approve, Deny,
        }

        [Key, ForeignKey("blog"), Column(Order = 1)]
        public int BlogID { get; set; }

        [Key, Column(Order = 2), MaxLength(30)]
        public string Auditor { get; set; }

        [Key, Column(Order = 3)]
        public int BlogVersion { get; set; }

        public DateTime AuditDate { get; set; }

        public Action AuditAction { get; set; }

        public string Reason { get; set; }

        public virtual Blog blog { get; set; }
    }

    public enum ItemType
    {
        Unknown = 0,
        Blog = 1,
        Topic = 2,
        Bounty = 3,
        Answer = 4
    }

    public class Post
    {
        public int PostId { get; set; }
        public DateTime PostDate { get; set; }

        [MaxLength(30)]
        public string Author { get; set; }

        [Required(ErrorMessage = "请输入内容"), StringLength(int.MaxValue), DataType(DataType.MultilineText)]
        public string Content { get; set; }

        public ItemType IdType { get; set; }
        public int ItemId { get; set; }

        public int Rating { get; set; }

        public virtual List<Reply> Replies { get; set; }

        public virtual IList<PostRating> Ratings { get; set; }
    }

    public class PostRating
    {
        [Key]
        public Guid RatingID { get; set; }

        [ForeignKey("Post")]
        public int PostId { get; set; }

        [MaxLength(30)]
        public string Rater { get; set; }

        public int Value { get; set; }

        public virtual Post Post { get; set; }
    }

    public class Reply
    {
        public int ReplyId { get; set; }
        public DateTime ReplyDate { get; set; }

        [MaxLength(30)]
        public string Author { get; set; }

        [Required(ErrorMessage = "请输入内容"), StringLength(int.MaxValue), DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [ForeignKey("PostId")]
        public virtual Post post { get; set; }

        public int PostId { get; set; }
    }

    [Table("Ratings")]
    public class BlogRating
    {
        [Key]
        public Guid RatingID { get; set; }

        public int BlogID { get; set; }

        [ForeignKey("BlogID")]
        public Blog blog { get; set; }

        [Required]
        public int value { get; set; }

        [MaxLength(50)]
        public string credential { get; set; }

        public DateTime ratetime { get; set; }
        
        [ForeignKey("PostId")]
        public virtual Post post { get; set; }

        public int? PostId { get; set; }
    }

    public class Tag
    {
        [Key]
        public int TagID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [MaxLength(30)]
        public string TagName { get; set; }

        [DefaultValue(0)]
        public long TagVisit { get; set; }
    }

    public class TagsInBlog
    {
        [Key, Column(Order = 1)]
        public int BlogID { get; set; }

        [ForeignKey("BlogID")]
        public virtual Blog blog { get; set; }

        [Key, Column(Order = 2)]
        public int TagID { get; set; }

        [ForeignKey("TagID")]
        public virtual Tag tag { get; set; }

        [MaxLength(30)]
        public string AddBy { get; set; }
    }

    public class TagHistory
    {
        [Key]
        public int HistoryID { get; set; }

        [ForeignKey("blog")]
        public int BlogID { get; set; }

        public DateTime Time { get; set; }

        [MaxLength(30)]
        public string TagName { get; set; }

        [MaxLength(30)]
        public string AddBy { get; set; }

        [MaxLength(30)]
        public string DeleteBy { get; set; }

        public virtual Blog blog { get; set; }
    }

    public class Topic
    {
        [Key]
        public int TopicID { get; set; }

        [Required(ErrorMessage = "请输入标题"), StringLength(80, ErrorMessage = "标题不得超过80个字符")]
        public string TopicTitle { get; set; }

        [Required(ErrorMessage = "请输入内容"), DataType(DataType.MultilineText)]
        public string Content { get; set; }

        [MaxLength(512)]
        public string ImagePath { get; set; }

        [MaxLength(512)]
        public string BannerPath { get; set; }

        public bool isLocalImg { get; set; }

        [Display(Name = "创建日期")]
        public DateTime CreateDate { get; set; }

        [Display(Name = "更新日期")]
        public DateTime UpdateDate { get; set; }

        [Required]
        [MaxLength(30)]
        public string Author { get; set; }

        [DefaultValue(0)]
        public long TopicVisit { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        public int TagID { get; set; }

        [ForeignKey("TagID")]
        public virtual Tag tag { get; set; }

        public IQueryable<Post> GetPosts(BlogContext db) => db.Posts.Where(p => p.IdType == ItemType.Topic && p.ItemId == TopicID);
    }

    public class BlogsInTopic
    {
        [Key, Column(Order = 1)]
        public int TopicID { get; set; }

        [ForeignKey("TopicID")]
        public virtual Topic topic { get; set; }

        [Key, Column(Order = 2)]
        public int BlogID { get; set; }

        [ForeignKey("BlogID")]
        public virtual Blog blog { get; set; }

        public int BlogOrder { get; set; }
    }

    public enum AdvertisementType
    {
        /// <summary>
        /// 正文与评论间的轮播
        /// </summary>
        [Description("正文与评论间的轮播")]
        Carousel = 1,

        /// <summary>
        /// 侧边栏广告
        /// </summary>
        [Description("侧边栏广告")]
        Sidebar = 2,

        /// <summary>
        /// 正文横幅广告1
        /// </summary>
        [Description("正文横幅广告1")]
        Banner = 4,

        /// <summary>
        /// 正文与评论间的轮播横幅
        /// </summary>
        [Description("正文与评论间的轮播横幅")]
        CarouselBanner = 5,

        /// <summary>
        /// 侧边栏固定横幅（上）
        /// </summary>
        [Description("侧边栏固定横幅（上）")]
        SidebarBanner1 = 6,

        /// <summary>
        /// 侧边栏固定横幅（下）
        /// </summary>
        [Description("侧边栏固定横幅（下）")]
        SidebarBanner2 = 7,
    }

    public class Advertisment
    {
        [Key]
        public int AdID { get; set; }

        [MaxLength(1000), DataType(DataType.Url), Required]
        public string AdUrl { get; set; }

        [MaxLength(1000), DataType(DataType.ImageUrl)]
        public string ImgUrl { get; set; }

        [MaxLength(100)]
        public string AdTitle { get; set; }

        public int? AdOrder { get; set; }
        public int ClickCount { get; set; }
        public AdvertisementType AdType { get; set; }
    }

    public class Favorite
    {
        [Key, Column(Order = 1)]
        public string Username { get; set; }

        [Key, Column(Order = 2)]
        public int BlogID { get; set; }

        [ForeignKey("BlogID")]
        public virtual Blog blog { get; set; }

        public DateTime AddDate { get; set; }
    }

    public class HistoryRanking
    {
        public enum Type
        {
            Unknown,
            /// <summary>
            /// 最近24小时排行（不同于日榜）
            /// </summary>
            Rank24h,
            /// <summary>
            /// 日榜（每日20点结算）
            /// </summary>
            RankDaily,
            /// <summary>
            /// 周榜（每周日24点结算）
            /// </summary>
            RankWeekly,
            /// <summary>
            /// 月榜（每月最后一天24点结算）
            /// </summary>
            RankMonthly,
        }

        [Key, Column(Order = 1)]
        public DateTime RankDate { get; set; }

        [Key, Column(Order = 2)]
        public int BlogID { get; set; }
        
        [Key, Column(Order = 3)]
        public Type RankType { get; set; }

        public int Rating { get; set; }
        public string BlogTitle { get; set; }
        public string BlogThumb { get; set; }
        public long BlogVisit { get; set; }
        public int PostCount { get; set; }
        public string Author { get; set; }
        public DateTime BlogDate { get; set; }
    }
}