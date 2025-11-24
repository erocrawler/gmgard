using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmGard.Migrations
{
    /// <inheritdoc />
    public partial class InitialSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Advertisments",
                columns: table => new
                {
                    AdID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ImgUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdOrder = table.Column<int>(type: "integer", nullable: true),
                    ClickCount = table.Column<int>(type: "integer", nullable: false),
                    AdType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertisments", x => x.AdID);
                });

            migrationBuilder.CreateTable(
                name: "Bounties",
                columns: table => new
                {
                    BountyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Author = table.Column<string>(type: "text", nullable: true),
                    ImageUrls = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Prize = table.Column<int>(type: "integer", nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bounties", x => x.BountyId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LinkOptional = table.Column<bool>(type: "boolean", nullable: false),
                    DisableRanking = table.Column<bool>(type: "boolean", nullable: false),
                    DisableRating = table.Column<bool>(type: "boolean", nullable: false),
                    HideFromHomePage = table.Column<bool>(type: "boolean", nullable: false),
                    ParentCategoryID = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryID);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryID",
                        column: x => x.ParentCategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID");
                });

            migrationBuilder.CreateTable(
                name: "HanGroups",
                columns: table => new
                {
                    HanGroupID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupUri = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Logo = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Intro = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HanGroups", x => x.HanGroupID);
                });

            migrationBuilder.CreateTable(
                name: "HistoryRankings",
                columns: table => new
                {
                    RankDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    RankType = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    BlogTitle = table.Column<string>(type: "text", nullable: true),
                    BlogThumb = table.Column<string>(type: "text", nullable: true),
                    BlogVisit = table.Column<long>(type: "bigint", nullable: false),
                    PostCount = table.Column<int>(type: "integer", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: true),
                    BlogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryRankings", x => new { x.RankDate, x.BlogID, x.RankType });
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    PostId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Author = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Content = table.Column<string>(type: "text", maxLength: 2147483647, nullable: false),
                    IdType = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.PostId);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TagName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TagVisit = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagID);
                });

            migrationBuilder.CreateTable(
                name: "Answers",
                columns: table => new
                {
                    AnswerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BountyId = table.Column<int>(type: "integer", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BountyId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Answers", x => x.AnswerId);
                    table.ForeignKey(
                        name: "FK_Answers_Bounties_BountyId",
                        column: x => x.BountyId,
                        principalTable: "Bounties",
                        principalColumn: "BountyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Answers_Bounties_BountyId1",
                        column: x => x.BountyId1,
                        principalTable: "Bounties",
                        principalColumn: "BountyId");
                });

            migrationBuilder.CreateTable(
                name: "Blogs",
                columns: table => new
                {
                    BlogID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlogTitle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Content = table.Column<string>(type: "text", maxLength: 2147483647, nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsLocalImg = table.Column<bool>(type: "boolean", nullable: false),
                    BlogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CategoryID = table.Column<int>(type: "integer", nullable: false),
                    Author = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: true),
                    isHarmony = table.Column<bool>(type: "boolean", nullable: false),
                    BlogVisit = table.Column<long>(type: "bigint", nullable: false),
                    Links = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blogs", x => x.BlogID);
                    table.ForeignKey(
                        name: "FK_Blogs_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HanGroupMembers",
                columns: table => new
                {
                    HanGroupID = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GroupLvl = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HanGroupMembers", x => new { x.HanGroupID, x.Username });
                    table.ForeignKey(
                        name: "FK_HanGroupMembers_HanGroups_HanGroupID",
                        column: x => x.HanGroupID,
                        principalTable: "HanGroups",
                        principalColumn: "HanGroupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostRatings",
                columns: table => new
                {
                    RatingID = table.Column<Guid>(type: "uuid", nullable: false),
                    PostId = table.Column<int>(type: "integer", nullable: false),
                    Rater = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostRatings", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_PostRatings_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Replies",
                columns: table => new
                {
                    ReplyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReplyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Author = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Content = table.Column<string>(type: "text", maxLength: 2147483647, nullable: false),
                    PostId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replies", x => x.ReplyId);
                    table.ForeignKey(
                        name: "FK_Replies_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    TopicID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicTitle = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    BannerPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    isLocalImg = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Author = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TopicVisit = table.Column<long>(type: "bigint", nullable: false),
                    CategoryID = table.Column<int>(type: "integer", nullable: false),
                    TagID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.TopicID);
                    table.ForeignKey(
                        name: "FK_Topics_Categories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "Categories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Topics_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "TagID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogAudits",
                columns: table => new
                {
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    Auditor = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    BlogVersion = table.Column<int>(type: "integer", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuditAction = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogAudits", x => new { x.BlogID, x.Auditor, x.BlogVersion });
                    table.ForeignKey(
                        name: "FK_BlogAudits_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogOptions",
                columns: table => new
                {
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    LockTags = table.Column<bool>(type: "boolean", nullable: false),
                    LockDesc = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    NoRate = table.Column<bool>(type: "boolean", nullable: false),
                    NoComment = table.Column<bool>(type: "boolean", nullable: false),
                    NoApprove = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogOptions", x => x.BlogID);
                    table.ForeignKey(
                        name: "FK_BlogOptions_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Username = table.Column<string>(type: "text", nullable: false),
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    AddDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => new { x.Username, x.BlogID });
                    table.ForeignKey(
                        name: "FK_Favorites_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HanGroupBlogs",
                columns: table => new
                {
                    HanGroupID = table.Column<int>(type: "integer", nullable: false),
                    BlogID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HanGroupBlogs", x => new { x.HanGroupID, x.BlogID });
                    table.ForeignKey(
                        name: "FK_HanGroupBlogs_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HanGroupBlogs_HanGroups_HanGroupID",
                        column: x => x.HanGroupID,
                        principalTable: "HanGroups",
                        principalColumn: "HanGroupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingID = table.Column<Guid>(type: "uuid", nullable: false),
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false),
                    credential = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ratetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_Ratings_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ratings_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "PostId");
                });

            migrationBuilder.CreateTable(
                name: "TagHistories",
                columns: table => new
                {
                    HistoryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TagName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    AddBy = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    DeleteBy = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagHistories", x => x.HistoryID);
                    table.ForeignKey(
                        name: "FK_TagHistories_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TagsInBlogs",
                columns: table => new
                {
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    TagID = table.Column<int>(type: "integer", nullable: false),
                    AddBy = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagsInBlogs", x => new { x.BlogID, x.TagID });
                    table.ForeignKey(
                        name: "FK_TagsInBlogs_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagsInBlogs_Tags_TagID",
                        column: x => x.TagID,
                        principalTable: "Tags",
                        principalColumn: "TagID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlogsInTopics",
                columns: table => new
                {
                    TopicID = table.Column<int>(type: "integer", nullable: false),
                    BlogID = table.Column<int>(type: "integer", nullable: false),
                    BlogOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogsInTopics", x => new { x.TopicID, x.BlogID });
                    table.ForeignKey(
                        name: "FK_BlogsInTopics_Blogs_BlogID",
                        column: x => x.BlogID,
                        principalTable: "Blogs",
                        principalColumn: "BlogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogsInTopics_Topics_TopicID",
                        column: x => x.TopicID,
                        principalTable: "Topics",
                        principalColumn: "TopicID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_BountyId",
                table: "Answers",
                column: "BountyId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_BountyId1",
                table: "Answers",
                column: "BountyId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Blogs_CategoryID",
                table: "Blogs",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_BlogsInTopics_BlogID",
                table: "BlogsInTopics",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryID",
                table: "Categories",
                column: "ParentCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_BlogID",
                table: "Favorites",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_HanGroupBlogs_BlogID",
                table: "HanGroupBlogs",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_PostRatings_PostId",
                table: "PostRatings",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_BlogID",
                table: "Ratings",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_PostId",
                table: "Ratings",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Replies_PostId",
                table: "Replies",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_TagHistories_BlogID",
                table: "TagHistories",
                column: "BlogID");

            migrationBuilder.CreateIndex(
                name: "IX_TagsInBlogs_TagID",
                table: "TagsInBlogs",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_CategoryID",
                table: "Topics",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_TagID",
                table: "Topics",
                column: "TagID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Advertisments");

            migrationBuilder.DropTable(
                name: "Answers");

            migrationBuilder.DropTable(
                name: "BlogAudits");

            migrationBuilder.DropTable(
                name: "BlogOptions");

            migrationBuilder.DropTable(
                name: "BlogsInTopics");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "HanGroupBlogs");

            migrationBuilder.DropTable(
                name: "HanGroupMembers");

            migrationBuilder.DropTable(
                name: "HistoryRankings");

            migrationBuilder.DropTable(
                name: "PostRatings");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Replies");

            migrationBuilder.DropTable(
                name: "TagHistories");

            migrationBuilder.DropTable(
                name: "TagsInBlogs");

            migrationBuilder.DropTable(
                name: "Bounties");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "HanGroups");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Blogs");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
