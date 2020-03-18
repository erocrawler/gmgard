namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addHomePageBlacklistTags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserOptions", "homepageTagBlacklist", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserOptions", "homepageTagBlacklist");
        }
    }
}
