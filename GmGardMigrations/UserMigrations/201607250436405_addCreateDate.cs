namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCreateDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserProfile", "CreateDate", c => c.DateTime(nullable: false, defaultValueSql:"GETDATE()"));
        //    Sql(@"Update [dbo].[UserProfile] set [CreateDate] = m.CreateDate
        //        FROM UserProfile as u
        //        LEFT OUTER JOIN webpages_Membership as m ON m.UserId = u.UserId");
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserProfile", "CreateDate");
        }
    }
}
