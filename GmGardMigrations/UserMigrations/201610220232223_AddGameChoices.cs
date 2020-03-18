namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGameChoices : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "DeathCount", c => c.Int(nullable: false, defaultValue:0));
            AddColumn("dbo.UserQuests", "GameChoices", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "GameChoices");
            DropColumn("dbo.UserQuests", "DeathCount");
        }
    }
}
