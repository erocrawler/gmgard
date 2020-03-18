namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addScore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AuditExamSubmissions", "Score", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AuditExamSubmissions", "Score");
        }
    }
}
