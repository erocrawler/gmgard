namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPersonalBg : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserQuests", "PersonalBackground", c => c.String());
            Sql(@"Update UserQuests set PersonalBackground = CASE 
                WHEN Title = 30 THEN N'乐园的巫女'
                WHEN Title = 31 THEN N'偷心的魔法使'
                WHEN Title = 32 THEN N'完美潇洒的从者'
                WHEN Title = 33 THEN N'永远鲜红的幼月'
                WHEN Title = 34 THEN N'风之祭祀'
                WHEN Title = 35 THEN N'魔理沙的后宫'
                WHEN Title = 36 THEN N'七色的人偶使'
                WHEN Title = 37 THEN N'七曜的大法师'
                WHEN Title = 38 THEN N'恶魔之妹'
                WHEN Title = 39 THEN N'半灵庭师'
                WHEN Title = 40 THEN N'幽冥公主'
                WHEN Title = 41 THEN N'十七岁的妖怪贤者'
                WHEN Title >= 42 AND Title <= 47 THEN N'路人女主1'
                WHEN Title = 50 THEN N'路人女主1'
                WHEN Title = 48 THEN N'路人女主2'
            END");
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserQuests", "PersonalBackground");
        }
    }
}
