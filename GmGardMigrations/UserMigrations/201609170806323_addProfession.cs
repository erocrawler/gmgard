namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProfession : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "Profession", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.UserQuests", "Progress", c => c.Int(nullable: false, defaultValue: 0));
            CreateIndex("dbo.UserQuests", "Profession");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UserQuests", new[] { "Profession" });
            DropColumn("dbo.UserQuests", "Progress");
            DropColumn("dbo.UserQuests", "Profession");
        }
    }
}
