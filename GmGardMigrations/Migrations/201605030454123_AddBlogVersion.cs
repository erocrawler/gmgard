namespace GmGard.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddBlogVersion : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.BlogAudits");
            AddColumn("dbo.BlogAudits", "BlogVersion", c => c.Int(nullable: false));
            AddColumn("dbo.BlogAudits", "AuditDate", c => c.DateTime(nullable: false));
            AddPrimaryKey("dbo.BlogAudits", new[] { "BlogID", "Auditor", "BlogVersion" });
            CreateIndex("dbo.BlogAudits", new[] { "BlogID", "Auditor", "BlogVersion" });
        }

        public override void Down()
        {
            DropIndex("dbo.BlogAudits", new[] { "BlogID", "Auditor", "BlogVersion" });
            DropPrimaryKey("dbo.BlogAudits");
            DropColumn("dbo.BlogAudits", "AuditDate");
            DropColumn("dbo.BlogAudits", "BlogVersion");
            AddPrimaryKey("dbo.BlogAudits", new[] { "BlogID", "Auditor" });
        }
    }
}