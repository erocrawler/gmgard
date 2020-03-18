namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddGachaPool : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GachaPools",
                c => new
                    {
                        Name = c.Int(nullable: false),
                        ItemId = c.Int(nullable: false),
                        Weight = c.Int(nullable: false, defaultValue: 1),
                    })
                .PrimaryKey(t => new { t.Name, t.ItemId })
                .ForeignKey("dbo.GachaItems", t => t.ItemId, cascadeDelete: true)
                .Index(t => t.ItemId);

            Sql(@"Insert Into GachaPools (Name, ItemId) 
                  Select 1, Id From GachaItems");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GachaPools", "ItemId", "dbo.GachaItems");
            DropIndex("dbo.GachaPools", new[] { "ItemId" });
            DropTable("dbo.GachaPools");
        }
    }
}
