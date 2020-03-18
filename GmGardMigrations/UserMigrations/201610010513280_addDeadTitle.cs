namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addDeadTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "IsDead", c => c.Boolean(nullable: false, defaultValue:false));
            AddColumn("dbo.UserQuests", "Titles", c => c.Binary());
            Sql("Update dbo.UserQuests set Titles = Power(2, Profession - 1)");
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "Titles");
            DropColumn("dbo.UserQuests", "IsDead");
        }
    }
}
