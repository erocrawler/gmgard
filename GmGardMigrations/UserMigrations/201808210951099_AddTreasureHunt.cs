namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTreasureHunt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TreasureHuntAttempts",
                c => new
                    {
                        AttemptId = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        IsCorrect = c.Boolean(nullable: false),
                        TargetPuzzle = c.Int(nullable: false),
                        AttemptTime = c.DateTime(nullable: false),
                        AttemptAnswer = c.String(),
                    })
                .PrimaryKey(t => t.AttemptId)
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TreasureHuntAttempts", "UserID", "dbo.UserProfile");
            DropIndex("dbo.TreasureHuntAttempts", new[] { "UserID" });
            DropTable("dbo.TreasureHuntAttempts");
        }
    }
}
