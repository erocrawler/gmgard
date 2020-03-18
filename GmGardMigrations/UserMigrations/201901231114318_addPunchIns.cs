namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPunchIns : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PunchInHistories",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserID, t.TimeStamp })
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PunchInHistories", "UserID", "dbo.UserProfile");
            DropIndex("dbo.PunchInHistories", new[] { "UserID" });
            DropTable("dbo.PunchInHistories");
        }
    }
}
