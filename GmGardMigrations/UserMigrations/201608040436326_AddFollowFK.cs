namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFollowFK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Follows", "FollowID", "dbo.UserProfile");
            AddForeignKey("dbo.Follows", "FollowID", "dbo.UserProfile", "UserId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Follows", "FollowID", "dbo.UserProfile");
            AddForeignKey("dbo.Follows", "FollowID", "dbo.UserProfile", "UserId", cascadeDelete: false);
        }
    }
}
