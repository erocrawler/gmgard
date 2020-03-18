namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRatePostDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "LastRatePostDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "LastRatePostDate");
        }
    }
}
