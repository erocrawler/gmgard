namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRaffleConfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RaffleConfigs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        EventStart = c.DateTime(nullable: false),
                        EventEnd = c.DateTime(nullable: false),
                        RaffleCost = c.Int(nullable: false),
                        Image = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.UserRaffles", "Config_Id", c => c.Int());
            CreateIndex("dbo.UserRaffles", "Config_Id");
            AddForeignKey("dbo.UserRaffles", "Config_Id", "dbo.RaffleConfigs", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRaffles", "Config_Id", "dbo.RaffleConfigs");
            DropIndex("dbo.UserRaffles", new[] { "Config_Id" });
            DropColumn("dbo.UserRaffles", "Config_Id");
            DropTable("dbo.RaffleConfigs");
        }
    }
}
