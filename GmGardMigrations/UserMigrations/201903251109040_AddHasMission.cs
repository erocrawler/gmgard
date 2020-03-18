namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHasMission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GachaItems", "HasMission", c => c.Boolean(nullable: false, defaultValue: false));
            Sql("update GachaItems set HasMission = 1 where Rarity >= 3 ");
        }
        
        public override void Down()
        {
            DropColumn("dbo.GachaItems", "HasMission");
        }
    }
}
