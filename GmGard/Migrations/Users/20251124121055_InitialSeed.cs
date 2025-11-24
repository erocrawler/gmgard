using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmGard.Migrations.Users
{
    /// <inheritdoc />
    public partial class InitialSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminLogs",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Actor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Target = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminLogs", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Avatars",
                columns: table => new
                {
                    PicID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PicUserName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PicType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PicName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PicDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avatars", x => x.PicID);
                });

            migrationBuilder.CreateTable(
                name: "ExpTable",
                columns: table => new
                {
                    Level = table.Column<int>(type: "integer", nullable: false),
                    ExperienceStart = table.Column<int>(type: "integer", nullable: false),
                    ExperienceEnd = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpTable", x => x.Level);
                });

            migrationBuilder.CreateTable(
                name: "GachaItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    HasMission = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    GameID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameName = table.Column<string>(type: "text", nullable: true),
                    GameChapters = table.Column<string>(type: "text", nullable: true),
                    ItemList = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.GameID);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MsgId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Sender = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Recipient = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    MsgDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MsgContent = table.Column<string>(type: "text", nullable: false),
                    MsgTitle = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    IsSenderDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsRecipientDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MsgId);
                });

            migrationBuilder.CreateTable(
                name: "RaffleConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    EventStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RaffleCost = table.Column<int>(type: "integer", nullable: false),
                    Image = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaffleConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TitleConfigs",
                columns: table => new
                {
                    TitleID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TitleName = table.Column<string>(type: "text", nullable: true),
                    TitleDescription = table.Column<string>(type: "text", nullable: true),
                    TitleImage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleConfigs", x => x.TitleID);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NickName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    UserComment = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginIP = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    LastSignDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConsecutiveSign = table.Column<int>(type: "integer", nullable: false),
                    HistoryConsecutiveSign = table.Column<int>(type: "integer", nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GachaPools",
                columns: table => new
                {
                    Name = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaPools", x => new { x.Name, x.ItemId });
                    table.ForeignKey(
                        name: "FK_GachaPools_GachaItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "GachaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameScenarios",
                columns: table => new
                {
                    ScenarioID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameID = table.Column<int>(type: "integer", nullable: false),
                    Dialogs = table.Column<string>(type: "text", nullable: true),
                    Narrators = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameScenarios", x => x.ScenarioID);
                    table.ForeignKey(
                        name: "FK_GameScenarios_Games_GameID",
                        column: x => x.GameID,
                        principalTable: "Games",
                        principalColumn: "GameID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GachaTitleConditionConfigs",
                columns: table => new
                {
                    ConditionID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConditionType = table.Column<string>(type: "text", nullable: true),
                    TitleID = table.Column<int>(type: "integer", nullable: false),
                    ConditionRequirements = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GachaTitleConditionConfigs", x => x.ConditionID);
                    table.ForeignKey(
                        name: "FK_GachaTitleConditionConfigs_TitleConfigs_TitleID",
                        column: x => x.TitleID,
                        principalTable: "TitleConfigs",
                        principalColumn: "TitleID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey, x.UserId });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditExamSubmissions",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RawSubmission = table.Column<string>(type: "text", nullable: true),
                    RawResult = table.Column<string>(type: "text", nullable: true),
                    HasPassed = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<decimal>(type: "numeric", nullable: false),
                    IsSubmitted = table.Column<bool>(type: "boolean", nullable: false),
                    SubmitTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditExamSubmissions", x => new { x.UserID, x.Version });
                    table.ForeignKey(
                        name: "FK_AuditExamSubmissions_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Auditors",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    AuditCount = table.Column<int>(type: "integer", nullable: false),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditors", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Auditors_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Follows",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    FollowID = table.Column<int>(type: "integer", nullable: false),
                    FollowTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => new { x.UserID, x.FollowID });
                    table.ForeignKey(
                        name: "FK_Follows_UserProfile_FollowID",
                        column: x => x.FollowID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Follows_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PunchInHistories",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsMakeup = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PunchInHistories", x => new { x.UserID, x.TimeStamp });
                    table.ForeignKey(
                        name: "FK_PunchInHistories_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreasureHuntAttempts",
                columns: table => new
                {
                    AttemptId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false),
                    TargetPuzzle = table.Column<int>(type: "integer", nullable: false),
                    AttemptTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AttemptAnswer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasureHuntAttempts", x => x.AttemptId);
                    table.ForeignKey(
                        name: "FK_TreasureHuntAttempts_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCodes",
                columns: table => new
                {
                    Code = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UsedBy = table.Column<int>(type: "integer", nullable: true),
                    BuyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCodes", x => x.Code);
                    table.ForeignKey(
                        name: "FK_UserCodes_UserProfile_UsedBy",
                        column: x => x.UsedBy,
                        principalTable: "UserProfile",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_UserCodes_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGachas",
                columns: table => new
                {
                    GachaId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    PoolName = table.Column<int>(type: "integer", nullable: false),
                    GachaTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGachas", x => x.GachaId);
                    table.ForeignKey(
                        name: "FK_UserGachas_GachaItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "GachaItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGachas_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOptions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    sendNoticeForNewReply = table.Column<bool>(type: "boolean", nullable: false),
                    sendNoticeForNewPostReply = table.Column<bool>(type: "boolean", nullable: false),
                    addFavFlameEffect = table.Column<bool>(type: "boolean", nullable: false),
                    homepageHideHarmony = table.Column<bool>(type: "boolean", nullable: false),
                    ShowBlogDateOnLists = table.Column<bool>(type: "boolean", nullable: false),
                    homepageCategories = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    homepageTagBlacklist = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOptions", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserOptions_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserQuests",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    LastRateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastPostDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastBlogDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastRatePostDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PunchInTicket = table.Column<int>(type: "integer", nullable: false),
                    DayBlogCount = table.Column<int>(type: "integer", nullable: false),
                    WeekBlogCount = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<int>(type: "integer", nullable: false),
                    PersonalBackground = table.Column<string>(type: "text", nullable: true),
                    Titles = table.Column<byte[]>(type: "bytea", nullable: true),
                    Profession = table.Column<int>(type: "integer", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    HasGotReward = table.Column<bool>(type: "boolean", nullable: false),
                    IsDead = table.Column<bool>(type: "boolean", nullable: false),
                    DeathCount = table.Column<int>(type: "integer", nullable: false),
                    GameChoices = table.Column<byte[]>(type: "bytea", nullable: true),
                    EternalCircleRetryCount = table.Column<int>(type: "integer", nullable: false),
                    EternalCircleProgress = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuests", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserQuests_TitleConfigs_Title",
                        column: x => x.Title,
                        principalTable: "TitleConfigs",
                        principalColumn: "TitleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserQuests_UserProfile_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRaffles",
                columns: table => new
                {
                    RaffleID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfigId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRaffles", x => x.RaffleID);
                    table.ForeignKey(
                        name: "FK_UserRaffles_RaffleConfigs_ConfigId",
                        column: x => x.ConfigId,
                        principalTable: "RaffleConfigs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRaffles_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVouchers",
                columns: table => new
                {
                    VoucherID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<int>(type: "integer", nullable: true),
                    IssueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UseTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RedeemItem = table.Column<string>(type: "text", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    VoucherKind = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVouchers", x => x.VoucherID);
                    table.ForeignKey(
                        name: "FK_UserVouchers_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "ScenarioChoices",
                columns: table => new
                {
                    ScenarioID = table.Column<int>(type: "integer", nullable: false),
                    NextScenarioID = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ChoiceData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioChoices", x => new { x.ScenarioID, x.NextScenarioID });
                    table.ForeignKey(
                        name: "FK_ScenarioChoices_GameScenarios_NextScenarioID",
                        column: x => x.NextScenarioID,
                        principalTable: "GameScenarios",
                        principalColumn: "ScenarioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScenarioChoices_GameScenarios_ScenarioID",
                        column: x => x.ScenarioID,
                        principalTable: "GameScenarios",
                        principalColumn: "ScenarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserGameDatas",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    GameID = table.Column<int>(type: "integer", nullable: false),
                    CurrentScenarioID = table.Column<int>(type: "integer", nullable: false),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    Inventory = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGameDatas", x => new { x.UserID, x.GameID });
                    table.ForeignKey(
                        name: "FK_UserGameDatas_GameScenarios_CurrentScenarioID",
                        column: x => x.CurrentScenarioID,
                        principalTable: "GameScenarios",
                        principalColumn: "ScenarioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGameDatas_Games_GameID",
                        column: x => x.GameID,
                        principalTable: "Games",
                        principalColumn: "GameID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserGameDatas_UserProfile_UserID",
                        column: x => x.UserID,
                        principalTable: "UserProfile",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserVisitedScenarios",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "integer", nullable: false),
                    GameID = table.Column<int>(type: "integer", nullable: false),
                    ScenarioID = table.Column<int>(type: "integer", nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    VisitDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVisitedScenarios", x => new { x.UserID, x.GameID, x.ScenarioID, x.Attempt });
                    table.ForeignKey(
                        name: "FK_UserVisitedScenarios_GameScenarios_ScenarioID",
                        column: x => x.ScenarioID,
                        principalTable: "GameScenarios",
                        principalColumn: "ScenarioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserVisitedScenarios_UserGameDatas_UserID_GameID",
                        columns: x => new { x.UserID, x.GameID },
                        principalTable: "UserGameDatas",
                        principalColumns: new[] { "UserID", "GameID" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNormalizedNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowID",
                table: "Follows",
                column: "FollowID");

            migrationBuilder.CreateIndex(
                name: "IX_GachaPools_ItemId",
                table: "GachaPools",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GachaTitleConditionConfigs_TitleID",
                table: "GachaTitleConditionConfigs",
                column: "TitleID");

            migrationBuilder.CreateIndex(
                name: "IX_GameScenarios_GameID",
                table: "GameScenarios",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioChoices_NextScenarioID",
                table: "ScenarioChoices",
                column: "NextScenarioID");

            migrationBuilder.CreateIndex(
                name: "IX_TreasureHuntAttempts_UserID",
                table: "TreasureHuntAttempts",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserCodes_UsedBy",
                table: "UserCodes",
                column: "UsedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserCodes_UserId",
                table: "UserCodes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGachas_ItemId",
                table: "UserGachas",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGachas_UserID",
                table: "UserGachas",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGameDatas_CurrentScenarioID",
                table: "UserGameDatas",
                column: "CurrentScenarioID");

            migrationBuilder.CreateIndex(
                name: "IX_UserGameDatas_GameID",
                table: "UserGameDatas",
                column: "GameID");

            migrationBuilder.CreateIndex(
                name: "NormalizedEmailIndex",
                table: "UserProfile",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "NormalizedUserNameIndex",
                table: "UserProfile",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserEmailIndex",
                table: "UserProfile",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "UserProfile",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserQuests_Title",
                table: "UserQuests",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_UserRaffles_ConfigId",
                table: "UserRaffles",
                column: "ConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRaffles_UserID",
                table: "UserRaffles",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVisitedScenarios_ScenarioID",
                table: "UserVisitedScenarios",
                column: "ScenarioID");

            migrationBuilder.CreateIndex(
                name: "IX_UserVouchers_UserID",
                table: "UserVouchers",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminLogs");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditExamSubmissions");

            migrationBuilder.DropTable(
                name: "Auditors");

            migrationBuilder.DropTable(
                name: "Avatars");

            migrationBuilder.DropTable(
                name: "ExpTable");

            migrationBuilder.DropTable(
                name: "Follows");

            migrationBuilder.DropTable(
                name: "GachaPools");

            migrationBuilder.DropTable(
                name: "GachaTitleConditionConfigs");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "PunchInHistories");

            migrationBuilder.DropTable(
                name: "ScenarioChoices");

            migrationBuilder.DropTable(
                name: "TreasureHuntAttempts");

            migrationBuilder.DropTable(
                name: "UserCodes");

            migrationBuilder.DropTable(
                name: "UserGachas");

            migrationBuilder.DropTable(
                name: "UserOptions");

            migrationBuilder.DropTable(
                name: "UserQuests");

            migrationBuilder.DropTable(
                name: "UserRaffles");

            migrationBuilder.DropTable(
                name: "UserVisitedScenarios");

            migrationBuilder.DropTable(
                name: "UserVouchers");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "GachaItems");

            migrationBuilder.DropTable(
                name: "TitleConfigs");

            migrationBuilder.DropTable(
                name: "RaffleConfigs");

            migrationBuilder.DropTable(
                name: "UserGameDatas");

            migrationBuilder.DropTable(
                name: "GameScenarios");

            migrationBuilder.DropTable(
                name: "UserProfile");

            migrationBuilder.DropTable(
                name: "Games");
        }
    }
}
