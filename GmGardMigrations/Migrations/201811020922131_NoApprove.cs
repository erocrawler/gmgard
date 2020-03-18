namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoApprove : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogOptions", "NoApprove", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BlogOptions", "NoApprove");
        }
    }
}
