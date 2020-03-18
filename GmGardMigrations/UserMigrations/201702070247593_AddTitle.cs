namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "Title", c => c.Int(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "Title");
        }
    }
}
