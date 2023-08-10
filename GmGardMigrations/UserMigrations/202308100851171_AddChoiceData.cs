namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddChoiceData : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ScenarioChoices", "ChoiceData", c => c.String());
            AddColumn("dbo.Games", "GameChapters", c => c.String());
            AddColumn("dbo.Games", "ItemList", c => c.String());
            AddColumn("dbo.UserGameDatas", "Inventory", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserGameDatas", "Inventory");
            DropColumn("dbo.Games", "ItemList");
            DropColumn("dbo.Games", "GameChapters");
            DropColumn("dbo.ScenarioChoices", "ChoiceData");
        }
    }
}
