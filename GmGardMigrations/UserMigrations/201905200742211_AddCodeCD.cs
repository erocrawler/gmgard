namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCodeCD : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserCodes", "BuyDate", c => c.DateTime(nullable:true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserCodes", "BuyDate");
        }
    }
}
