namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGacha : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GachaItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 30),
                        Title = c.String(),
                        Description = c.String(),
                        Rarity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UserGachas",
                c => new
                    {
                        GachaId = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        GachaTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.GachaId)
                .ForeignKey("dbo.GachaItems", t => t.ItemId, cascadeDelete: true)
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID)
                .Index(t => t.ItemId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserGachas", "UserID", "dbo.UserProfile");
            DropForeignKey("dbo.UserGachas", "ItemId", "dbo.GachaItems");
            DropIndex("dbo.UserGachas", new[] { "ItemId" });
            DropIndex("dbo.UserGachas", new[] { "UserID" });
            DropTable("dbo.UserGachas");
            DropTable("dbo.GachaItems");
        }
    }
}
