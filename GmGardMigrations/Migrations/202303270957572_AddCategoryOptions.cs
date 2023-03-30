namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCategoryOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Categories", "LinkOptional", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "DisableRanking", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "DisableRating", c => c.Boolean(nullable: false));
            AddColumn("dbo.Categories", "HideFromHomePage", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Categories", "HideFromHomePage");
            DropColumn("dbo.Categories", "DisableRating");
            DropColumn("dbo.Categories", "DisableRanking");
            DropColumn("dbo.Categories", "LinkOptional");
        }
    }
}
