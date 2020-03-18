namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPunchTicket : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "PunchInTicket", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "PunchInTicket");
        }
    }
}
