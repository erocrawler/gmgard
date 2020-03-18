namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addHasGotReward : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "HasGotReward", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "HasGotReward");
        }
    }
}
