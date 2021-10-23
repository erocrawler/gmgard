using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using AspNetCore.Identity.EntityFramework6;
using System.Data.Entity.Infrastructure.Annotations;

namespace GmGard.Models
{
    [DbConfigurationType(typeof(DbCodeConfiguration))]
    public class UsersContext : IdentityDbContext<UserProfile>
    {
        public UsersContext(string nameOrConnectionString, ILoggerFactory logger)
            : base(nameOrConnectionString)
        {
            var log = logger.CreateLogger<UsersContext>();
            Database.Log = l =>
            {
                log.LogInformation(l);
            };
        }

        public UsersContext(string connectionString)
            : base(connectionString)
        {
        }

        public DbSet<UserOption> UserOptions { get; set; }
        public DbSet<UserQuest> UserQuests { get; set; }
        public DbSet<UserCode> UserCodes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Pictures> Avatars { get; set; }
        public DbSet<ExperienceTable> ExpTable { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Auditor> Auditors { get; set; }
        public DbSet<AuditExamSubmission> AuditExamSubmissions { get; set; }
        public DbSet<UserGacha> UserGachas { get; set; }
        public DbSet<GachaItem> GachaItems { get; set; }
        public DbSet<GachaPool> GachaPools { get; set; }
        public DbSet<TreasureHuntAttempt> TreasureHuntAttempts { get; set; }
        public DbSet<PunchInHistory> PunchInHistories { get; set; }
        public DbSet<UserRaffle> UserRaffles { get; set; }

        public DbSet<UserVoucher> UserVouchers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Needed to ensure subclasses share the same table
            var user = modelBuilder.Entity<UserProfile>()
                .ToTable("UserProfile");
            user.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserNameIndex") { IsUnique = true }));
            user.Property(u => u.NormalizedUserName)
                .HasMaxLength(20)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("NormalizedUserNameIndex") { IsUnique = true }));
            user.Property(u => u.Email)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("UserEmailIndex") { IsUnique = true }));
            user.Property(u => u.NormalizedEmail)
                .HasMaxLength(50)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("NormalizedEmailIndex") { IsUnique = true }));
            user.HasMany(u => u.Roles).WithRequired().HasForeignKey(ur => ur.UserId);
            user.HasMany(u => u.Claims).WithRequired().HasForeignKey(uc => uc.UserId);
            user.HasMany(u => u.Logins).WithRequired().HasForeignKey(ul => ul.UserId);
            user.HasMany(u => u.Tokens).WithRequired().HasForeignKey(ut => ut.UserId);

            modelBuilder.Entity<IdentityUserRole>()
                .HasKey(r => new { r.UserId, r.RoleId })
                .ToTable("AspNetUserRoles");

            modelBuilder.Entity<IdentityUserLogin>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId })
                .ToTable("AspNetUserLogins");

            modelBuilder.Entity<IdentityUserClaim>()
                .ToTable("AspNetUserClaims");

            var role = modelBuilder.Entity<IdentityRole>()
                .ToTable("AspNetRoles");
            role.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("RoleNameIndex") { IsUnique = true }));

            role.Property(r => r.NormalizedName)
                .IsRequired()
                .HasMaxLength(256)
                .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute("RoleNormalizedNameIndex") { IsUnique = true }));
            role.HasMany(r => r.Users).WithRequired().HasForeignKey(ur => ur.RoleId);
            role.HasMany(r => r.Claims).WithRequired().HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<IdentityRoleClaim>()
                .ToTable("AspNetRoleClaims");

            var tokens = modelBuilder.Entity<IdentityUserToken>()
                .ToTable("AspNetUserTokens");
            tokens.HasKey(t => new { t.UserId, t.LoginProvider, t.Name })
                .Property(t => t.LoginProvider)
                .HasMaxLength(128);
            tokens.Property(t => t.Name).HasMaxLength(128);

            var follow = modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.UserID, f.FollowID });
            follow.HasRequired(f => f.user)
                .WithMany(u => u.follows)
                .HasForeignKey(f => f.UserID)
                .WillCascadeOnDelete(true);
            follow.HasRequired(f => f.follow)
                .WithMany()
                .HasForeignKey(f => f.FollowID)
                .WillCascadeOnDelete(false);
        }
    }

    public class Message
    {
        [Key, ScaffoldColumn(false)]
        public int MsgId { get; set; }

        [Required]
        [MaxLength(30)]
        public string Sender { get; set; }

        [Required(ErrorMessage = "请输入收件人")]
        [RegularExpression("^[a-zA-Z0-9_\u2E80-\u9FFF]{1,20}$", ErrorMessage = "用户名包含无效字符")]
        [MaxLength(30)]
        public string Recipient { get; set; }

        public DateTime MsgDate { get; set; }

        [Required(ErrorMessage = "请输入正文"), DataType(DataType.MultilineText)]
        public string MsgContent { get; set; }

        [MaxLength(80, ErrorMessage = "标题不得超过80字符")]
        public string MsgTitle { get; set; }

        [DefaultValue(false)]
        public bool IsRead { get; set; }

        [DefaultValue(false)]
        public bool IsSenderDelete { get; set; }

        [DefaultValue(false)]
        public bool IsRecipientDelete { get; set; }
    }

    public class Pictures
    {
        [ScaffoldColumn(false), Key]
        public int PicID { get; set; }

        [Required]
        [MaxLength(50)]
        public string PicUserName { get; set; }

        [Required]
        [MaxLength(50)]
        public string PicType { get; set; }

        [Required]
        [MaxLength(256)]
        public string PicName { get; set; }

        [DataType(DataType.DateTime), Required]
        public DateTime PicDate { get; set; }
    }

    public class UserProfile : IdentityUser
    {
        [Key, Column("UserId")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public override string UserName { get; set; }

        [MaxLength(20)]
        public string NickName { get; set; }

        [MaxLength(200, ErrorMessage = "签名不得超过200字符")]
        public string UserComment { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [MaxLength(50)]
        public override string Email { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastLoginDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreateDate { get; set; }

        [DataType(DataType.Text)]
        [MaxLength(80)]
        public string LastLoginIP { get; set; }

        [Required]
        [DefaultValue(0)]
        public int Points { get; set; }

        public int Experience { get; set; }
        public int Level { get; set; }

        [DataType(DataType.Date)]
        public DateTime LastSignDate { get; set; }

        public int ConsecutiveSign { get; set; }
        public int HistoryConsecutiveSign { get; set; }

        public virtual UserOption option { get; set; }
        public virtual UserQuest quest { get; set; }
        public virtual ICollection<Follow> follows { get; set; }
        public virtual Auditor auditor { get; set; }
        public virtual IList<PunchInHistory> PunchIns { get; set; }
    }

    public class UserOption
    {
        public virtual UserProfile user { get; set; }

        [Key, ForeignKey("user")]
        public int UserId { get; set; }

        public bool sendNoticeForNewReply { get; set; }
        public bool sendNoticeForNewPostReply { get; set; }
        public bool addFavFlameEffect { get; set; }
        public bool homepageHideHarmony { get; set; }
        public bool ShowBlogDateOnLists { get; set; }

        [MaxLength(200)]
        public string homepageCategories { get; set; }

        public string homepageTagBlacklist { get; set; }

        public UserOption()
        {
            sendNoticeForNewReply = sendNoticeForNewPostReply = true;
            addFavFlameEffect = homepageHideHarmony = false;
            homepageCategories = string.Empty;
        }
    }

    public class UserCode
    {
        public virtual UserProfile User { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Key]
        public Guid Code { get; set; }

        [ForeignKey("UsedByUser")]
        public int? UsedBy { get; set; }

        public DateTime? BuyDate { get; set; }

        public virtual UserProfile UsedByUser { get; set; }

        public UserCode()
        {
            Code = Guid.NewGuid();
        }
    }

    public partial class UserQuest
    {
        public virtual UserProfile user { get; set; }

        [Key, ForeignKey("user")]
        public int UserId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastRateDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastPostDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastBlogDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? LastRatePostDate { get; set; }

        public int PunchInTicket { get; set; }

        public int DayBlogCount { get; set; }
        public int WeekBlogCount { get; set; }
        public UserProfession Title { get; set; }
        public string PersonalBackground { get; set; }
        public byte[] Titles { get; set; }

        #region Game Related Fields

        public UserProfession Profession { get; set; }
        public GameProgress Progress { get; set; }

        public bool HasGotReward { get; set; }
        public bool IsDead { get; set; }
        public int DeathCount { get; set; }
        public byte[] GameChoices { get; set; }

        public void SetDead()
        {
            IsDead = true;
            DeathCount++;
        }

        public enum GameProgress
        {
            None = 0,
            Act1BeforeChoose,
            Act1AfterChoose,
            Act2Start,
            Act2AfterChoose,
            Act3Start,
            Act3AfterChoose,
            Act4Start,
            Act4AfterChoose,
            Act5StartRight,
            Act5Start,
            Act5BeforeChoose,
            Act5AfterChoose,
            Act5bStart,
            Act5bAfterChoose,
            Act6Start,
            Act6AfterChoose,
            Act6Q1,
            Act6Q2,
            Act6Q3,
            Act6AfterQ,
            Act6Leave,
            Act6Stay,
        }

        public int EternalCircleRetryCount { get; set; }
        public ECGameProgress EternalCircleProgress { get;set; }

        public enum ECGameProgress
        {
            Prologue = 0,
            Act1Start,
            Act1After,
            ACT1End,
            Act2Start,
            Act2After,
            Act3,
            Act3_5,
            Act4,
            Act5,
            ActEX,
            GE,
            BE1,
            BE2,
            BE3,
            BE4,
            BE5,
            BE6,
            BE7,
            RE,
        }

        #endregion Game Related Fields
    }

    public class Follow
    {
        [Key, Column(Order = 1), ForeignKey("user")]
        public int UserID { get; set; }

        [Key, Column(Order = 2), ForeignKey("follow")]
        public int FollowID { get; set; }

        public DateTime FollowTime { get; set; }

        [InverseProperty("follows")]
        public virtual UserProfile user { get; set; }

        public virtual UserProfile follow { get; set; }
    }

    public class Auditor
    {
        [Key, ForeignKey("User")]
        public int UserID { get; set; }

        public int AuditCount { get; set; }

        public int CorrectCount { get; set; }

        [NotMapped]
        public float Accuracy { get { return AuditCount > 0 ? CorrectCount / (float)AuditCount : 1; } }

        public virtual UserProfile User { get; set; }
    }

    public class ExperienceTable
    {
        public int ExperienceStart { get; set; }
        public int ExperienceEnd { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Level { get; set; }

        [MaxLength(10)]
        public string Title { get; set; }
    }

    public class AdminLog
    {
        [Key]
        public int LogID { get; set; }

        [MaxLength(20)]
        public string Actor { get; set; }

        [MaxLength(20)]
        public string Action { get; set; }

        [MaxLength(100)]
        public string Target { get; set; }

        [MaxLength(100)]
        public string Reason { get; set; }

        public DateTime LogTime { get; set; }
    }

    public class AuditExamSubmission
    {
        [Key, Column(Order = 1), ForeignKey("User")]
        public int UserID { get; set; }

        [Key, Column(Order = 2), MaxLength(20)]
        public string Version { get; set; }

        public string RawSubmission { get; set; }

        public string RawResult { get; set; }

        public bool HasPassed { get; set; }

        public decimal Score { get; set; }

        public bool IsSubmitted { get; set; }

        public DateTime SubmitTime { get; set; }

        public virtual UserProfile User { get; set; }
    }

    public class GachaItem
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public int Rarity { get; set; }
        public bool HasMission { get; set; }

        public virtual IEnumerable<GachaPool> Pools { get; set; }
    }

    public class GachaPool
    {
        public enum PoolName
        {
            Common = 0,
            FourthAnniversary = 1,
            Touhou1st = 2,
            April2018 = 3,
            June2018 = 4,
            April2019 = 5,
            August2020 = 6,
        }

        [Key, Column(Order = 1)]
        public PoolName Name { get; set; }
        [Key, Column(Order = 2), ForeignKey("Item")]
        public int ItemId { get; set; }

        // Likelyhood of appearing among the same rarity items in the pool.
        [DefaultValue(1)]
        public int Weight { get; set; }

        public virtual GachaItem Item { get; set; }
    }

    public class UserGacha
    {
        [Key]
        public int GachaId { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("Item")]
        public int ItemId { get; set; }

        public GachaPool.PoolName PoolName { get; set; }

        public DateTime GachaTime { get; set; }

        public virtual UserProfile User { get; set; }
        public virtual GachaItem Item { get; set; }
    }

    public class TreasureHuntAttempt
    {
        [Key]
        public int AttemptId { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }

        public bool IsCorrect { get; set; }
        public int TargetPuzzle { get; set; }
        public DateTime AttemptTime { get; set; }
        public string AttemptAnswer { get; set; }

        public virtual UserProfile User { get; set; }
    }

    public class PunchInHistory
    {
        [Key, ForeignKey("User"), Column(Order = 0)]
        public int UserID { get; set; }
        [Key, Column(Order = 1)]
        public DateTime TimeStamp { get; set; }

        public bool IsMakeup { get; set; }

        public virtual UserProfile User { get; set; }
    }

    public class UserRaffle
    {
        [Key]
        public Guid RaffleID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        public DateTime TimeStamp { get; set; }

        public virtual UserProfile User { get; set; }
    }

    public class UserVoucher
    {
        [Key]
        public Guid VoucherID { get; set; }
        [ForeignKey("User")]
        public int? UserID { get; set; }
        public virtual UserProfile User { get; set; }
        public DateTime IssueTime { get; set; }
        public DateTime? UseTime { get; set; }
        public string RedeemItem { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }

        public Kind VoucherKind { get; set; }

        public enum Kind
        {
            Default = 0,
            WheelA,
            WheelB,
            LuckyPoint,
            Prize,
            CeilingPrize,
            Coupon,
            WheelC,
        }
    }
}