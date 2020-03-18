namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIdentityUserToken : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetUserTokens",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 128),
                        Value = c.String(),
                    })
                .PrimaryKey(t => new { t.UserId, t.LoginProvider, t.Name })
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserTokens", "UserId", "dbo.UserProfile");
            DropIndex("dbo.AspNetUserTokens", new[] { "UserId" });
            DropTable("dbo.AspNetUserTokens");
        }
    }
}
