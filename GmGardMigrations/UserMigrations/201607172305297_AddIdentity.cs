namespace MyMVCWeb.UserMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;

    public partial class AddIdentity : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.Int(nullable: false),
                        ProviderDisplayName = c.String(),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.UserProfile", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoleClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 256),
                        NormalizedName = c.String(),
                        ConcurrencyStamp = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            AddColumn("dbo.UserProfile", "NormalizedUserName", c => c.String());
            AddColumn("dbo.UserProfile", "NormalizedEmail", c => c.String());
            AddColumn("dbo.UserProfile", "EmailConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserProfile", "PasswordHash", c => c.String());
            AddColumn("dbo.UserProfile", "SecurityStamp", c => c.String());
            AddColumn("dbo.UserProfile", "ConcurrencyStamp", c => c.String());
            AddColumn("dbo.UserProfile", "PhoneNumber", c => c.String());
            AddColumn("dbo.UserProfile", "PhoneNumberConfirmed", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserProfile", "TwoFactorEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserProfile", "LockoutEnd", c => c.DateTimeOffset(precision: 7));
            AddColumn("dbo.UserProfile", "LockoutEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserProfile", "AccessFailedCount", c => c.Int(nullable: false));
            CreateIndex("dbo.UserProfile", "UserName", unique: true, name: "UserNameIndex");
            CreateIndex("dbo.UserProfile", "Email", unique: true, name: "UserEmailIndex");

            //SqlFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../migrate_users.sql"));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetRoleClaims", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.UserProfile");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.UserProfile");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetRoleClaims", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.UserProfile", "UserEmailIndex");
            DropIndex("dbo.UserProfile", "UserNameIndex");
            DropColumn("dbo.UserProfile", "AccessFailedCount");
            DropColumn("dbo.UserProfile", "LockoutEnabled");
            DropColumn("dbo.UserProfile", "LockoutEnd");
            DropColumn("dbo.UserProfile", "TwoFactorEnabled");
            DropColumn("dbo.UserProfile", "PhoneNumberConfirmed");
            DropColumn("dbo.UserProfile", "PhoneNumber");
            DropColumn("dbo.UserProfile", "ConcurrencyStamp");
            DropColumn("dbo.UserProfile", "SecurityStamp");
            DropColumn("dbo.UserProfile", "PasswordHash");
            DropColumn("dbo.UserProfile", "EmailConfirmed");
            DropColumn("dbo.UserProfile", "NormalizedEmail");
            DropColumn("dbo.UserProfile", "NormalizedUserName");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetRoleClaims");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
        }
    }
}
