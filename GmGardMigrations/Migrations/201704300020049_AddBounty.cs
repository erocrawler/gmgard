namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBounty : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Answers",
                c => new
                    {
                        AnswerId = c.Int(nullable: false, identity: true),
                        BountyId = c.Int(nullable: false),
                        Author = c.String(),
                        Content = c.String(),
                        ImageUrl = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.AnswerId)
                .ForeignKey("dbo.Bounties", t => t.BountyId, cascadeDelete: true)
                .Index(t => t.BountyId);
            
            CreateTable(
                "dbo.Bounties",
                c => new
                    {
                        BountyId = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Content = c.String(),
                        Author = c.String(),
                        ImageUrls = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Prize = c.Int(nullable: false),
                        IsAccepted = c.Boolean(nullable: false),
                        ViewCount = c.Int(nullable: false, defaultValue: 0),
                        AcceptedAnswer_AnswerId = c.Int(),
                    })
                .PrimaryKey(t => t.BountyId)
                .ForeignKey("dbo.Answers", t => t.AcceptedAnswer_AnswerId)
                .Index(t => t.AcceptedAnswer_AnswerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Answers", "BountyId", "dbo.Bounties");
            DropForeignKey("dbo.Bounties", "AcceptedAnswer_AnswerId", "dbo.Answers");
            DropIndex("dbo.Bounties", new[] { "AcceptedAnswer_AnswerId" });
            DropIndex("dbo.Answers", new[] { "BountyId" });
            DropTable("dbo.Bounties");
            DropTable("dbo.Answers");
        }
    }
}
