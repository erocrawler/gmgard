namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addUserPool : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserGachas", "PoolName", c => c.Int(nullable: false, defaultValue:0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserGachas", "PoolName");
        }
    }
}
