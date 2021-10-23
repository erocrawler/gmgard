namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddShowDateOnListOption : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserOptions", "ShowBlogDateOnLists", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserOptions", "ShowBlogDateOnLists");
        }
    }
}
