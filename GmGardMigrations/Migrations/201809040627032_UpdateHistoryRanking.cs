namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateHistoryRanking : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.HistoryRankings", "BlogID", "dbo.Blogs");
            DropIndex("dbo.HistoryRankings", new[] { "BlogID" });
            DropPrimaryKey("dbo.HistoryRankings");
            AddColumn("dbo.HistoryRankings", "RankType", c => c.Int(nullable: false, defaultValue: 2));
            AddColumn("dbo.HistoryRankings", "BlogTitle", c => c.String());
            AddColumn("dbo.HistoryRankings", "BlogThumb", c => c.String());
            AddColumn("dbo.HistoryRankings", "BlogVisit", c => c.Long(nullable: false));
            AddColumn("dbo.HistoryRankings", "PostCount", c => c.Int(nullable: false));
            AddColumn("dbo.HistoryRankings", "Author", c => c.String());
            AddColumn("dbo.HistoryRankings", "BlogDate", c => c.DateTime(nullable: false));
            AddPrimaryKey("dbo.HistoryRankings", new[] { "RankDate", "BlogID", "RankType" });
            CreateIndex("dbo.HistoryRankings", new[] { "RankDate", "BlogID", "RankType" });
        }
        
        public override void Down()
        {
            DropIndex("dbo.HistoryRankings", new[] { "RankDate", "BlogID", "RankType" });
            DropPrimaryKey("dbo.HistoryRankings");
            DropColumn("dbo.HistoryRankings", "BlogDate");
            DropColumn("dbo.HistoryRankings", "Author");
            DropColumn("dbo.HistoryRankings", "PostCount");
            DropColumn("dbo.HistoryRankings", "BlogVisit");
            DropColumn("dbo.HistoryRankings", "BlogThumb");
            DropColumn("dbo.HistoryRankings", "BlogTitle");
            DropColumn("dbo.HistoryRankings", "RankType");
            AddPrimaryKey("dbo.HistoryRankings", new[] { "RankDate", "BlogID" });
            CreateIndex("dbo.HistoryRankings", "BlogID");
            AddForeignKey("dbo.HistoryRankings", "BlogID", "dbo.Blogs", "BlogID", cascadeDelete: true);
        }
    }
}
