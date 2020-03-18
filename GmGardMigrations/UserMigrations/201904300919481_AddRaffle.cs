namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRaffle : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserRaffles",
                c => new
                    {
                        RaffleID = c.Guid(nullable: false),
                        UserID = c.Int(nullable: false),
                        TimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.RaffleID)
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserRaffles", "UserID", "dbo.UserProfile");
            DropIndex("dbo.UserRaffles", new[] { "UserID" });
            DropTable("dbo.UserRaffles");
        }
    }
}
