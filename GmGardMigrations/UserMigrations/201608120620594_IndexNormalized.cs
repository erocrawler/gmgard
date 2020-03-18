namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IndexNormalized : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserProfile", "NormalizedUserName", c => c.String(maxLength: 20));
            AlterColumn("dbo.UserProfile", "NormalizedEmail", c => c.String(maxLength: 50));
            AlterColumn("dbo.AspNetRoles", "NormalizedName", c => c.String(nullable: false, maxLength: 256));
            CreateIndex("dbo.UserProfile", "NormalizedUserName", unique: true, name: "NormalizedUserNameIndex");
            CreateIndex("dbo.UserProfile", "NormalizedEmail", unique: true, name: "NormalizedEmailIndex");
            CreateIndex("dbo.AspNetRoles", "NormalizedName", unique: true, name: "RoleNormalizedNameIndex");
        }
        
        public override void Down()
        {
            DropIndex("dbo.AspNetRoles", "RoleNormalizedNameIndex");
            DropIndex("dbo.UserProfile", "NormalizedEmailIndex");
            DropIndex("dbo.UserProfile", "NormalizedUserNameIndex");
            AlterColumn("dbo.AspNetRoles", "NormalizedName", c => c.String());
            AlterColumn("dbo.UserProfile", "NormalizedEmail", c => c.String());
            AlterColumn("dbo.UserProfile", "NormalizedUserName", c => c.String());
        }
    }
}
