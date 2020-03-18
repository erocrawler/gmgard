namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostRating : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PostRatings",
                c => new
                    {
                        RatingID = c.Guid(nullable: false, defaultValueSql:"NEWID()"),
                        PostId = c.Int(nullable: false),
                        Rater = c.String(maxLength: 30),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RatingID)
                .ForeignKey("dbo.Posts", t => t.PostId, cascadeDelete: true)
                .Index(t => t.PostId);
            
            AddColumn("dbo.Posts", "Rating", c => c.Int(nullable: false, defaultValue:0));

            SqlFile("../../PostRating_trigger.sql");
        }
        
        public override void Down()
        {
            Sql("DROP TRIGGER dbo.PostRating_trigger ");
            DropForeignKey("dbo.PostRatings", "PostId", "dbo.Posts");
            DropIndex("dbo.PostRatings", new[] { "PostId" });
            DropColumn("dbo.Posts", "Rating");
            DropTable("dbo.PostRatings");
        }
    }
}
