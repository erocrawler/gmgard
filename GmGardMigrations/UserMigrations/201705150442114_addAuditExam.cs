namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAuditExam : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AuditExamSubmissions",
                c => new
                    {
                        UserID = c.Int(nullable: false),
                        Version = c.String(nullable: false, maxLength: 20),
                        RawSubmission = c.String(),
                        RawResult = c.String(),
                        HasPassed = c.Boolean(nullable: false),
                        IsSubmitted = c.Boolean(nullable: false),
                        SubmitTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserID, t.Version })
                .ForeignKey("dbo.UserProfile", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AuditExamSubmissions", "UserID", "dbo.UserProfile");
            DropIndex("dbo.AuditExamSubmissions", new[] { "UserID" });
            DropTable("dbo.AuditExamSubmissions");
        }
    }
}
