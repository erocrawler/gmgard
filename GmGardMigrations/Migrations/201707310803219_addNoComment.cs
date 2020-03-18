namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addNoComment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogOptions", "NoComment", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BlogOptions", "NoComment");
        }
    }
}
