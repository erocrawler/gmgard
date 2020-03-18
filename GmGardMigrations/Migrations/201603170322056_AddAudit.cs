namespace GmGard.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddAudit : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BlogAudits",
                c => new
                {
                    BlogID = c.Int(nullable: false),
                    Auditor = c.String(nullable: false, maxLength: 30),
                    AuditAction = c.Int(nullable: false),
                    Reason = c.String(),
                })
                .PrimaryKey(t => new { t.BlogID, t.Auditor })
                .ForeignKey("dbo.Blogs", t => t.BlogID, cascadeDelete: true)
                .Index(t => t.BlogID).Index(t => new { t.BlogID, t.Auditor });
        }

        public override void Down()
        {
            DropForeignKey("dbo.BlogAudits", "BlogID", "dbo.Blogs");
            DropIndex("dbo.BlogAudits", new[] { "BlogID" });
            DropIndex("dbo.BlogAudits", new[] { "BlogID", "Auditor" });
            DropTable("dbo.BlogAudits");
        }
    }
}