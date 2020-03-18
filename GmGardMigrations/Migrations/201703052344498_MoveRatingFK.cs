namespace MyMVCWeb.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MoveRatingFK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Posts", "RatingId", "dbo.Ratings");
            AddColumn("dbo.Ratings", "PostId", c => c.Int(nullable: true));
            Sql("ALTER TABLE dbo.Ratings ADD CONSTRAINT [FK_dbo.Ratings_dbo.Posts_PostId] FOREIGN KEY([PostId]) REFERENCES [dbo].[Posts] ([PostId]) ON DELETE SET NULL");
            Sql("alter table dbo.Ratings alter column PostId int sparse");
            Sql(@"Update r set r.PostId = p.PostId
                  From Ratings r Inner Join Posts p On
                    r.RatingId = p.RatingId");
            CreateIndex("dbo.Ratings", "PostId");
            DropIndex("dbo.Posts", new[] { "RatingId" });
            DropColumn("dbo.Posts", "RatingId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Posts", "RatingId", c => c.Guid());
            Sql(@"Update p set p.RatingId = r.RatingId
                  From Ratings r Inner Join Posts p On
                    r.PostId = p.PostId");
            DropForeignKey("dbo.Ratings", "PostId", "dbo.Posts");
            DropIndex("dbo.Ratings", new[] { "PostId" });
            DropColumn("dbo.Ratings", "PostId");
            CreateIndex("dbo.Posts", "RatingId");
            AddForeignKey("dbo.Posts", "RatingId", "dbo.Ratings", "RatingID");
        }
    }
}
