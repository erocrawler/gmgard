namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;

    public partial class AddGachaTitleConditionConfig : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GachaTitleConditionConfigs",
                c => new
                    {
                        ConditionID = c.Int(nullable: false, identity: true),
                        ConditionType = c.String(),
                        TitleID = c.Int(nullable: false),
                        ConditionRequirements = c.String(),
                    })
                .PrimaryKey(t => t.ConditionID)
                .ForeignKey("dbo.TitleConfigs", t => t.TitleID, cascadeDelete: true)
                .Index(t => t.TitleID, unique: true);

            SqlFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../populate_gacha_conditions.sql"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GachaTitleConditionConfigs", "TitleID", "dbo.TitleConfigs");
            DropIndex("dbo.GachaTitleConditionConfigs", new[] { "TitleID" });
            DropTable("dbo.GachaTitleConditionConfigs");
        }
    }
}
