namespace GmGard.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Advertisments",
                c => new
                    {
                        AdID = c.Int(nullable: false, identity: true),
                        AdUrl = c.String(nullable: false, maxLength: 1000),
                        ImgUrl = c.String(maxLength: 1000),
                        AdTitle = c.String(maxLength: 100),
                        AdOrder = c.Int(),
                        ClickCount = c.Int(nullable: false),
                        AdType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AdID);
            
            CreateTable(
                "dbo.BlogOptions",
                c => new
                    {
                        BlogID = c.Int(nullable: false),
                        LockTags = c.Boolean(nullable: false),
                        LockDesc = c.String(maxLength: 200),
                        NoRate = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.BlogID)
                .ForeignKey("dbo.Blogs", t => t.BlogID)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        BlogID = c.Int(nullable: false, identity: true),
                        BlogTitle = c.String(nullable: false, maxLength: 120),
                        Content = c.String(nullable: false),
                        ImagePath = c.String(maxLength: 1024),
                        isLocalImg = c.Boolean(nullable: false),
                        BlogDate = c.DateTime(nullable: false),
                        CategoryID = c.Int(nullable: false),
                        Author = c.String(nullable: false, maxLength: 30),
                        isApproved = c.Boolean(),
                        isHarmony = c.Boolean(nullable: false),
                        BlogVisit = c.Long(nullable: false),
                        Links = c.String(),
                        Rating = c.Int(),
                    })
                .PrimaryKey(t => t.BlogID)
                .ForeignKey("dbo.Categories", t => t.CategoryID)
                .Index(t => t.CategoryID);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 256),
                        ParentCategoryID = c.Int(),
                    })
                .PrimaryKey(t => t.CategoryID)
                .ForeignKey("dbo.Categories", t => t.ParentCategoryID)
                .Index(t => t.ParentCategoryID);
            
            CreateTable(
                "dbo.Topics",
                c => new
                    {
                        TopicID = c.Int(nullable: false, identity: true),
                        TopicTitle = c.String(nullable: false, maxLength: 80),
                        Content = c.String(nullable: false),
                        ImagePath = c.String(maxLength: 512),
                        BannerPath = c.String(maxLength: 512),
                        isLocalImg = c.Boolean(nullable: false),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        Author = c.String(nullable: false, maxLength: 30),
                        TopicVisit = c.Long(nullable: false),
                        CategoryID = c.Int(nullable: false),
                        TagID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.TopicID)
                .ForeignKey("dbo.Tags", t => t.TagID, cascadeDelete: true)
                .ForeignKey("dbo.Categories", t => t.CategoryID, cascadeDelete: true)
                .Index(t => t.CategoryID)
                .Index(t => t.TagID);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        TagID = c.Int(nullable: false, identity: true),
                        TagName = c.String(nullable: false, maxLength: 30),
                        TagVisit = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.TagID);
            
            CreateTable(
                "dbo.BlogsInTopics",
                c => new
                    {
                        TopicID = c.Int(nullable: false),
                        BlogID = c.Int(nullable: false),
                        BlogOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.TopicID, t.BlogID })
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .ForeignKey("dbo.Topics", t => t.TopicID, cascadeDelete: true)
                .Index(t => t.TopicID)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.Favorites",
                c => new
                    {
                        Username = c.String(nullable: false, maxLength: 128),
                        BlogID = c.Int(nullable: false),
                        AddDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.Username, t.BlogID })
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.HanGroupBlogs",
                c => new
                    {
                        HanGroupID = c.Int(nullable: false),
                        BlogID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.HanGroupID, t.BlogID })
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .ForeignKey("dbo.HanGroups", t => t.HanGroupID, cascadeDelete: true)
                .Index(t => t.HanGroupID)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.HanGroups",
                c => new
                    {
                        HanGroupID = c.Int(nullable: false, identity: true),
                        GroupUri = c.String(maxLength: 20),
                        Title = c.String(maxLength: 50),
                        Logo = c.String(maxLength: 512),
                        Intro = c.String(maxLength: 2048),
                    })
                .PrimaryKey(t => t.HanGroupID);
            
            CreateTable(
                "dbo.HanGroupMembers",
                c => new
                    {
                        HanGroupID = c.Int(nullable: false),
                        Username = c.String(nullable: false, maxLength: 20),
                        GroupLvl = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.HanGroupID, t.Username })
                .ForeignKey("dbo.HanGroups", t => t.HanGroupID, cascadeDelete: true)
                .Index(t => t.HanGroupID);
            
            CreateTable(
                "dbo.HistoryRankings",
                c => new
                    {
                        RankDate = c.DateTime(nullable: false),
                        BlogID = c.Int(nullable: false),
                        Rating = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.RankDate, t.BlogID })
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        PostId = c.Int(nullable: false, identity: true),
                        PostDate = c.DateTime(nullable: false),
                        Author = c.String(maxLength: 30),
                        Content = c.String(nullable: false),
                        IdType = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        RatingId = c.Guid(),
                    })
                .PrimaryKey(t => t.PostId)
                .ForeignKey("dbo.Ratings", t => t.RatingId)
                .Index(t => t.RatingId);
            
            CreateTable(
                "dbo.Ratings",
                c => new
                    {
                        RatingID = c.Guid(nullable: false),
                        BlogID = c.Int(nullable: false),
                        value = c.Int(nullable: false),
                        credential = c.String(maxLength: 50),
                        ratetime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RatingID)
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.Replies",
                c => new
                    {
                        ReplyId = c.Int(nullable: false, identity: true),
                        ReplyDate = c.DateTime(nullable: false),
                        Author = c.String(maxLength: 30),
                        Content = c.String(nullable: false),
                        PostId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ReplyId)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId);
            
            CreateTable(
                "dbo.TagHistories",
                c => new
                    {
                        HistoryID = c.Int(nullable: false, identity: true),
                        BlogID = c.Int(nullable: false),
                        Time = c.DateTime(nullable: false),
                        TagName = c.String(maxLength: 30),
                        AddBy = c.String(maxLength: 30),
                        DeleteBy = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => t.HistoryID)
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .Index(t => t.BlogID);
            
            CreateTable(
                "dbo.TagsInBlogs",
                c => new
                    {
                        BlogID = c.Int(nullable: false),
                        TagID = c.Int(nullable: false),
                        AddBy = c.String(maxLength: 30),
                    })
                .PrimaryKey(t => new { t.BlogID, t.TagID })
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .ForeignKey("dbo.Tags", t => t.TagID, cascadeDelete: true)
                .Index(t => t.BlogID)
                .Index(t => t.TagID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TagsInBlogs", "TagID", "dbo.Tags");
            DropForeignKey("dbo.TagsInBlogs", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.TagHistories", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.Replies", "PostId", "dbo.Posts");
            DropForeignKey("dbo.Posts", "RatingId", "dbo.Ratings");
            DropForeignKey("dbo.Ratings", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.HistoryRankings", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.HanGroupMembers", "HanGroupID", "dbo.HanGroups");
            DropForeignKey("dbo.HanGroupBlogs", "HanGroupID", "dbo.HanGroups");
            DropForeignKey("dbo.HanGroupBlogs", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.Favorites", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.BlogsInTopics", "TopicID", "dbo.Topics");
            DropForeignKey("dbo.BlogsInTopics", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.BlogOptions", "BlogID", "dbo.Blogs");
            DropForeignKey("dbo.Blogs", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Topics", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.Topics", "TagID", "dbo.Tags");
            DropForeignKey("dbo.Categories", "ParentCategoryID", "dbo.Categories");
            DropIndex("dbo.TagsInBlogs", new[] { "TagID" });
            DropIndex("dbo.TagsInBlogs", new[] { "BlogID" });
            DropIndex("dbo.TagHistories", new[] { "BlogID" });
            DropIndex("dbo.Replies", new[] { "PostId" });
            DropIndex("dbo.Ratings", new[] { "BlogID" });
            DropIndex("dbo.Posts", new[] { "RatingId" });
            DropIndex("dbo.HistoryRankings", new[] { "BlogID" });
            DropIndex("dbo.HanGroupMembers", new[] { "HanGroupID" });
            DropIndex("dbo.HanGroupBlogs", new[] { "BlogID" });
            DropIndex("dbo.HanGroupBlogs", new[] { "HanGroupID" });
            DropIndex("dbo.Favorites", new[] { "BlogID" });
            DropIndex("dbo.BlogsInTopics", new[] { "BlogID" });
            DropIndex("dbo.BlogsInTopics", new[] { "TopicID" });
            DropIndex("dbo.Topics", new[] { "TagID" });
            DropIndex("dbo.Topics", new[] { "CategoryID" });
            DropIndex("dbo.Categories", new[] { "ParentCategoryID" });
            DropIndex("dbo.Blogs", new[] { "CategoryID" });
            DropIndex("dbo.BlogOptions", new[] { "BlogID" });
            DropTable("dbo.TagsInBlogs");
            DropTable("dbo.TagHistories");
            DropTable("dbo.Replies");
            DropTable("dbo.Ratings");
            DropTable("dbo.Posts");
            DropTable("dbo.HistoryRankings");
            DropTable("dbo.HanGroupMembers");
            DropTable("dbo.HanGroups");
            DropTable("dbo.HanGroupBlogs");
            DropTable("dbo.Favorites");
            DropTable("dbo.BlogsInTopics");
            DropTable("dbo.Tags");
            DropTable("dbo.Topics");
            DropTable("dbo.Categories");
            DropTable("dbo.Blogs");
            DropTable("dbo.BlogOptions");
            DropTable("dbo.Advertisments");
        }
    }
}
