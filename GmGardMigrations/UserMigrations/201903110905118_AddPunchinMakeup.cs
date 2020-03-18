namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPunchinMakeup : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "HistoryConsecutiveSign", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.PunchInHistories", "IsMakeup", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PunchInHistories", "IsMakeup");
            DropColumn("dbo.UserProfile", "HistoryConsecutiveSign");
        }
    }
}
