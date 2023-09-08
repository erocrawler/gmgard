namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;

    public partial class AddTitleConfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TitleConfigs",
                c => new
                    {
                        TitleID = c.Int(nullable: false, identity: true),
                        TitleName = c.String(),
                        TitleDescription = c.String(),
                        TitleImage = c.String(),
                    })
                .PrimaryKey(t => t.TitleID);
            
            CreateIndex("dbo.UserQuests", "Title");
            SqlFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../populate_titles.sql"));
            AddForeignKey("dbo.UserQuests", "Title", "dbo.TitleConfigs", "TitleID", cascadeDelete: false);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserQuests", "Title", "dbo.TitleConfigs");
            DropIndex("dbo.UserQuests", new[] { "Title" });
            DropTable("dbo.TitleConfigs");
        }
    }
}
