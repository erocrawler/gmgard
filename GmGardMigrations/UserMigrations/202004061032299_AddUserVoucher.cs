namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserVoucher : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserVouchers",
                c => new
                    {
                        VoucherID = c.Guid(nullable: false),
                        UserID = c.Int(),
                        IssueTime = c.DateTime(nullable: false),
                        UseTime = c.DateTime(),
                        RedeemItem = c.String(),
                        RowVersion = c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"),
                        VoucherKind = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.VoucherID)
                .ForeignKey("dbo.UserProfile", t => t.UserID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserVouchers", "UserID", "dbo.UserProfile");
            DropIndex("dbo.UserVouchers", new[] { "UserID" });
            DropTable("dbo.UserVouchers");
        }
    }
}
