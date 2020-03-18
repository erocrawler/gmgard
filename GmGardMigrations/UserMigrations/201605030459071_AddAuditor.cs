namespace MyMVCWeb.UserMigrations
{
    using System.Data.Entity.Migrations;

    public partial class AddAuditor : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Auditors",
                c => new
                {
                    UserID = c.Int(nullable: false),
                    AuditCount = c.Int(nullable: false, defaultValue: 0),
                    CorrectCount = c.Int(nullable: false, defaultValue: 0),
                })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.UserProfile", t => t.UserID)
                .Index(t => t.UserID);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Auditors", "UserID", "dbo.UserProfile");
            DropIndex("dbo.Auditors", new[] { "UserID" });
            DropTable("dbo.Auditors");
        }
    }
}