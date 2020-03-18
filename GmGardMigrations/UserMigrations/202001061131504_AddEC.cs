namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEC : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "EternalCircleRetryCount", c => c.Int(nullable: false));
            AddColumn("dbo.UserQuests", "EternalCircleProgress", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "EternalCircleProgress");
            DropColumn("dbo.UserQuests", "EternalCircleRetryCount");
        }
    }
}
