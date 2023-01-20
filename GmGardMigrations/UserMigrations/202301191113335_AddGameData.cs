namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameData : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GameScenarios",
                c => new
                    {
                        ScenarioID = c.Int(nullable: false, identity: true),
                        GameID = c.Int(nullable: false),
                        Dialogs = c.String(),
                        Narrators = c.String(),
                        Data = c.String(),
                    })
                .PrimaryKey(t => t.ScenarioID)
                .ForeignKey("dbo.Games", t => t.GameID, cascadeDelete: true)
                .Index(t => t.GameID);
            
            CreateTable(
                "dbo.ScenarioChoices",
                c => new
                    {
                        ScenarioID = c.Int(nullable: false),
                        NextScenarioID = c.Int(nullable: false),
                        Title = c.String(),
                    })
                .PrimaryKey(t => new { t.ScenarioID, t.NextScenarioID })
                .ForeignKey("dbo.GameScenarios", t => t.NextScenarioID, cascadeDelete: true)
                .ForeignKey("dbo.GameScenarios", t => t.ScenarioID)
                .Index(t => t.ScenarioID)
                .Index(t => t.NextScenarioID);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameID = c.Int(nullable: false, identity: true),
                        GameName = c.String(),
                    })
                .PrimaryKey(t => t.GameID);
            
            CreateTable(
                "dbo.UserGameDatas",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        GameID = c.Int(nullable: false),
                        CurrentScenarioID = c.Int(nullable: false),
                        RetryCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserID, t.GameID })
                .ForeignKey("dbo.GameScenarios", t => t.CurrentScenarioID, cascadeDelete: true)
                .ForeignKey("dbo.Games", t => t.GameID)
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID)
                .Index(t => t.GameID)
                .Index(t => t.CurrentScenarioID);
            
            CreateTable(
                "dbo.UserVisitedScenarios",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        GameID = c.Int(nullable: false),
                        ScenarioID = c.Int(nullable: false),
                        Attempt = c.Int(nullable: false),
                        VisitDate = c.DateTimeOffset(nullable: false, precision: 7, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    })
                .PrimaryKey(t => new { t.UserID, t.GameID, t.ScenarioID, t.Attempt })
                .ForeignKey("dbo.GameScenarios", t => t.ScenarioID, cascadeDelete: true)
                .ForeignKey("dbo.UserGameDatas", t => new { t.UserID, t.GameID })
                .Index(t => new { t.UserID, t.GameID })
                .Index(t => t.ScenarioID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserVisitedScenarios", new[] { "UserID", "GameID" }, "dbo.UserGameDatas");
            DropForeignKey("dbo.UserVisitedScenarios", "ScenarioID", "dbo.GameScenarios");
            DropForeignKey("dbo.UserGameDatas", "UserID", "dbo.UserProfile");
            DropForeignKey("dbo.UserGameDatas", "GameID", "dbo.Games");
            DropForeignKey("dbo.UserGameDatas", "CurrentScenarioID", "dbo.GameScenarios");
            DropForeignKey("dbo.GameScenarios", "GameID", "dbo.Games");
            DropForeignKey("dbo.ScenarioChoices", "ScenarioID", "dbo.GameScenarios");
            DropForeignKey("dbo.ScenarioChoices", "NextScenarioID", "dbo.GameScenarios");
            DropIndex("dbo.UserVisitedScenarios", new[] { "ScenarioID" });
            DropIndex("dbo.UserVisitedScenarios", new[] { "UserID", "GameID" });
            DropIndex("dbo.UserGameDatas", new[] { "CurrentScenarioID" });
            DropIndex("dbo.UserGameDatas", new[] { "GameID" });
            DropIndex("dbo.UserGameDatas", new[] { "UserID" });
            DropIndex("dbo.ScenarioChoices", new[] { "NextScenarioID" });
            DropIndex("dbo.ScenarioChoices", new[] { "ScenarioID" });
            DropIndex("dbo.GameScenarios", new[] { "GameID" });
            DropTable("dbo.UserVisitedScenarios");
            DropTable("dbo.UserGameDatas");
            DropTable("dbo.Games");
            DropTable("dbo.ScenarioChoices");
            DropTable("dbo.GameScenarios");
        }
    }
}
