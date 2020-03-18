namespace MyMVCWeb.UserMigrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminLogs",
                c => new
                {
                    LogID = c.Int(nullable: false, identity: true),
                    Actor = c.String(maxLength: 20),
                    Action = c.String(maxLength: 20),
                    Target = c.String(maxLength: 100),
                    Reason = c.String(maxLength: 100),
                    LogTime = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.LogID);

            CreateTable(
                "dbo.Pictures",
                c => new
                {
                    PicID = c.Int(nullable: false, identity: true),
                    PicUserName = c.String(nullable: false, maxLength: 50),
                    PicType = c.String(nullable: false, maxLength: 50),
                    PicName = c.String(nullable: false, maxLength: 256),
                    PicDate = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.PicID);

            CreateTable(
                "dbo.ExperienceTables",
                c => new
                {
                    Level = c.Int(nullable: false),
                    ExperienceStart = c.Int(nullable: false),
                    ExperienceEnd = c.Int(nullable: false),
                    Title = c.String(maxLength: 10),
                })
                .PrimaryKey(t => t.Level);

            CreateTable(
                "dbo.Follows",
                c => new
                {
                    UserID = c.Int(nullable: false),
                    FollowID = c.Int(nullable: false),
                    FollowTime = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => new { t.UserID, t.FollowID })
                .ForeignKey("dbo.UserProfile", t => t.FollowID, cascadeDelete: false)
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID)
                .Index(t => t.FollowID);

            CreateTable(
                "dbo.UserProfile",
                c => new
                {
                    UserId = c.Int(nullable: false, identity: true),
                    UserName = c.String(nullable: false, maxLength: 20),
                    NickName = c.String(maxLength: 20),
                    UserComment = c.String(maxLength: 200),
                    Email = c.String(nullable: false, maxLength: 50),
                    LastLoginDate = c.DateTime(nullable: false),
                    LastLoginIP = c.String(maxLength: 80),
                    Points = c.Int(nullable: false),
                    Experience = c.Int(nullable: false),
                    Level = c.Int(nullable: false),
                    LastSignDate = c.DateTime(nullable: false),
                    ConsecutiveSign = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.UserId);

            CreateTable(
                "dbo.UserOptions",
                c => new
                {
                    UserId = c.Int(nullable: false),
                    sendNoticeForNewReply = c.Boolean(nullable: false),
                    sendNoticeForNewPostReply = c.Boolean(nullable: false),
                    addFavFlameEffect = c.Boolean(nullable: false),
                    homepageHideHarmony = c.Boolean(nullable: false),
                    homepageCategories = c.String(maxLength: 200),
                })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.UserProfile", t => t.UserId)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.UserQuests",
                c => new
                {
                    UserId = c.Int(nullable: false),
                    LastRateDate = c.DateTime(),
                    LastPostDate = c.DateTime(),
                    LastBlogDate = c.DateTime(),
                    DayBlogCount = c.Int(nullable: false),
                    WeekBlogCount = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.UserProfile", t => t.UserId)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.Messages",
                c => new
                {
                    MsgId = c.Int(nullable: false, identity: true),
                    Sender = c.String(nullable: false, maxLength: 30),
                    Recipient = c.String(nullable: false, maxLength: 30),
                    MsgDate = c.DateTime(nullable: false),
                    MsgContent = c.String(nullable: false),
                    MsgTitle = c.String(maxLength: 80),
                    IsRead = c.Boolean(nullable: false),
                    IsSenderDelete = c.Boolean(nullable: false),
                    IsRecipientDelete = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.MsgId);

            CreateTable(
                "dbo.UserCodes",
                c => new
                {
                    Code = c.Guid(nullable: false),
                    UserId = c.Int(nullable: false),
                    UsedBy = c.Int(),
                })
                .PrimaryKey(t => t.Code)
                .ForeignKey("dbo.UserProfile", t => t.UsedBy)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.UsedBy);
        }

        public override void Down()
        {
            DropForeignKey("dbo.UserCodes", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserCodes", "UsedBy", "dbo.UserProfile");
            DropForeignKey("dbo.Follows", "UserID", "dbo.UserProfile");
            DropForeignKey("dbo.Follows", "FollowID", "dbo.UserProfile");
            DropForeignKey("dbo.UserQuests", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.UserOptions", "UserId", "dbo.UserProfile");
            DropIndex("dbo.UserCodes", new[] { "UsedBy" });
            DropIndex("dbo.UserCodes", new[] { "UserId" });
            DropIndex("dbo.UserQuests", new[] { "UserId" });
            DropIndex("dbo.UserOptions", new[] { "UserId" });
            DropIndex("dbo.Follows", new[] { "FollowID" });
            DropIndex("dbo.Follows", new[] { "UserID" });
            DropTable("dbo.UserCodes");
            DropTable("dbo.Messages");
            DropTable("dbo.UserQuests");
            DropTable("dbo.UserOptions");
            DropTable("dbo.UserProfile");
            DropTable("dbo.Follows");
            DropTable("dbo.ExperienceTables");
            DropTable("dbo.Pictures");
            DropTable("dbo.AdminLogs");
        }
    }
}