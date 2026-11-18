using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Xunit;

namespace GmGard.Tests
{
    /// <summary>
    /// 每个测试类一个临时库:gm_test_&lt;guid&gt;。没有设置 GM_TEST_PG 时 Available=false,测试用 SkippableFact 显示为 Skipped。
    /// 连接串主机必须是 127.0.0.1 或 localhost,否则直接报错拒绝运行。
    /// </summary>
    public sealed class PgTestDatabase : IDisposable
    {
        public const string EnvVar = "GM_TEST_PG";

        public bool Available { get; }
        public string DatabaseName { get; }
        public DbContextOptions<UsersContext> Options { get; }

        public PgTestDatabase()
        {
            string prefix = Environment.GetEnvironmentVariable(EnvVar);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            var csb = new NpgsqlConnectionStringBuilder(prefix);
            string host = csb.Host ?? string.Empty;
            if (!host.Equals("127.0.0.1", StringComparison.Ordinal) &&
                !host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"拒绝运行:{EnvVar} 的 Host 必须是 127.0.0.1 或 localhost,当前为 '{host}'。");
            }

            DatabaseName = "gm_test_" + Guid.NewGuid().ToString("N");
            csb.Database = DatabaseName;
            Options = new DbContextOptionsBuilder<UsersContext>()
                .UseNpgsql(csb.ConnectionString)
                .Options;

            using (var ctx = new UsersContext(Options))
            {
                ctx.Database.EnsureCreated();
            }
            Available = true;
        }

        public void Dispose()
        {
            if (!Available)
            {
                return;
            }
            using var ctx = new UsersContext(Options);
            ctx.Database.EnsureDeleted();
        }
    }

    internal sealed class FakeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = new DefaultHttpContext();
    }

    public class ExpUtilPointsTests : IClassFixture<PgTestDatabase>
    {
        private readonly PgTestDatabase _db;

        public ExpUtilPointsTests(PgTestDatabase db)
        {
            _db = db;
        }

        // ---------- helpers ----------

        private UsersContext NewUsersContext() => new UsersContext(_db.Options);

        private static ExpUtil NewExpUtil(UsersContext udb, IMemoryCache cache) =>
            new ExpUtil(null, udb, cache, new FakeHttpContextAccessor(), null, null);

        private static IMemoryCache NewCache() => new MemoryCache(new MemoryCacheOptions());

        /// <summary>插入一个用户,返回 (Id, UserName)。用户名唯一,同一个库里多个测试互不干扰。</summary>
        private (int Id, string Name) SeedUser(int points, int level = 1)
        {
            string name = "u" + Guid.NewGuid().ToString("N").Substring(0, 12);
            string email = name + "@test.invalid";
            using var ctx = NewUsersContext();
            var user = new UserProfile
            {
                UserName = name,
                NormalizedUserName = name.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                Points = points,
                Level = level,
                CreateDate = new DateTime(2024, 1, 1),
                LastLoginDate = new DateTime(2024, 1, 1),
                LastSignDate = new DateTime(2024, 1, 1),
                SecurityStamp = Guid.NewGuid().ToString("N"),
            };
            ctx.Users.Add(user);
            ctx.SaveChanges();
            return (user.Id, name);
        }

        private int ReadPoints(int id)
        {
            using var ctx = NewUsersContext();
            return ctx.Users.AsNoTracking().Single(u => u.Id == id).Points;
        }

        // ---------- 1. 并发扣费 ----------

        [SkippableFact]
        public async Task Concurrent_deductions_succeed_exactly_10_times_and_never_go_negative()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (id, _) = SeedUser(points: 100);
            var observed = new ConcurrentBag<int>();
            int successes = 0;

            var tasks = Enumerable.Range(0, 30).Select(_ => Task.Run(() =>
            {
                // 每个任务独立的上下文和 ExpUtil,模拟不同请求
                using var udb = NewUsersContext();
                using var cache = NewCache();
                var expUtil = NewExpUtil(udb, cache);
                var user = udb.Users.Single(u => u.Id == id);

                if (expUtil.TryChangePoints(user, -10))
                {
                    Interlocked.Increment(ref successes);
                    observed.Add(user.Points);
                }
            }));
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(60));

            Assert.Equal(10, successes);
            Assert.Equal(0, ReadPoints(id));
            // 任何成功扣费之后看到的余额都不能为负
            Assert.All(observed, p => Assert.InRange(p, 0, 100));
            Assert.Equal(10, observed.Count);
        }

        // ---------- 2. 余额不足 ----------

        [SkippableFact]
        public void Insufficient_balance_returns_false_and_keeps_balance()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (id, _) = SeedUser(points: 5);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);
            var user = udb.Users.Single(u => u.Id == id);

            bool ok = expUtil.TryChangePoints(user, -10);

            Assert.False(ok);
            Assert.Equal(5, ReadPoints(id));
        }

        // ---------- 3. 事务回滚 ----------

        [SkippableFact]
        public async Task Deduction_inside_transaction_is_rolled_back()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (id, _) = SeedUser(points: 100);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);
            var user = udb.Users.Single(u => u.Id == id);

            await using (var tx = await udb.Database.BeginTransactionAsync())
            {
                Assert.True(expUtil.TryChangePoints(user, -30));
                await tx.RollbackAsync();
            }

            Assert.Equal(100, ReadPoints(id));
        }

        // ---------- 4. 实体同步,不回写旧值 ----------

        [SkippableFact]
        public void Tracked_entity_does_not_overwrite_concurrent_change_on_save()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (id, _) = SeedUser(points: 100);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);
            var user = udb.Users.Single(u => u.Id == id);     // 原上下文里 Points == 100

            // 另一个上下文直接改库:100 + 50
            using (var other = NewUsersContext())
            {
                int rows = other.Users.Where(u => u.Id == id)
                    .ExecuteUpdate(s => s.SetProperty(u => u.Points, u => u.Points + 50));
                Assert.Equal(1, rows);
            }

            Assert.True(expUtil.TryChangePoints(user, -20));
            Assert.Equal(130, user.Points);

            string newNick = "nick" + Guid.NewGuid().ToString("N").Substring(0, 8);
            user.NickName = newNick;
            udb.SaveChanges();

            Assert.Equal(130, user.Points);
            Assert.Equal(130, ReadPoints(id));
            using (var check = NewUsersContext())
            {
                Assert.Equal(newNick, check.Users.AsNoTracking().Single(u => u.Id == id).NickName);
            }
        }

        // ---------- 5. 允许透支路径 ----------

        [SkippableFact]
        public void AddPoint_allows_overdraft_but_TryChangePoints_does_not()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (id, _) = SeedUser(points: 100);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);
            var user = udb.Users.Single(u => u.Id == id);

            // 旧语义:AddPoint 允许透支(故意保留)
            expUtil.AddPoint(user, -500);
            Assert.Equal(-400, ReadPoints(id));
            Assert.Equal(-400, user.Points);

            // 默认 TryChangePoints 不允许透支
            Assert.False(expUtil.TryChangePoints(user, -500));
            Assert.Equal(-400, ReadPoints(id));
        }

        // ---------- 6. 新实体(Id==0)只改内存 ----------

        [SkippableFact]
        public void New_entity_is_changed_in_memory_without_touching_database()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            // 指向一个必然连不上的地址:如果代码访问了数据库,这里会抛异常
            var unreachable = new DbContextOptionsBuilder<UsersContext>()
                .UseNpgsql("Host=127.0.0.1;Port=1;Database=never;Username=x;Password=x;Timeout=1")
                .Options;
            using var udb = new UsersContext(unreachable);
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);
            var newUser = new UserProfile { UserName = "new-entity-user", Points = 0 };

            bool ok = expUtil.TryChangePoints(newUser, 10);

            Assert.True(ok);
            Assert.Equal(10, newUser.Points);
        }

        // ---------- 7. trySpendPointAsync ----------

        [SkippableFact]
        public async Task trySpendPointAsync_spends_when_enough_and_reports_balance_otherwise()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (id, name) = SeedUser(points: 100);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);

            var ok = await expUtil.trySpendPointAsync(name, 30);
            Assert.True(ok.Item1);
            Assert.Equal(70, ok.Item2);

            var fail = await expUtil.trySpendPointAsync(name, 200);
            Assert.False(fail.Item1);
            Assert.Equal(70, fail.Item2);

            Assert.Equal(70, ReadPoints(id));
        }

        // ---------- 8. TryBeginPost(评论冷却) ----------

        [SkippableFact]
        public void TryBeginPost_low_level_user_is_throttled_after_first_post()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (_, name) = SeedUser(points: 0, level: 1);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);

            Assert.Equal(0, expUtil.TryBeginPost(name));

            int wait = expUtil.TryBeginPost(name);
            Assert.InRange(wait, 1, 30);
        }

        [SkippableFact]
        public void TryBeginPost_level_10_and_above_is_never_throttled()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (_, name10) = SeedUser(points: 0, level: 10);
            var (_, name20) = SeedUser(points: 0, level: 20);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);

            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(0, expUtil.TryBeginPost(name10));
                Assert.Equal(0, expUtil.TryBeginPost(name20));
            }
        }

        [SkippableFact]
        public void TryBeginPost_cooldown_is_per_user()
        {
            Skip.IfNot(_db.Available, $"未设置 {PgTestDatabase.EnvVar},跳过 PostgreSQL 集成测试");

            var (_, nameA) = SeedUser(points: 0, level: 1);
            var (_, nameB) = SeedUser(points: 0, level: 1);
            using var udb = NewUsersContext();
            using var cache = NewCache();
            var expUtil = NewExpUtil(udb, cache);

            Assert.Equal(0, expUtil.TryBeginPost(nameA));
            Assert.True(expUtil.TryBeginPost(nameA) > 0);

            // A 在冷却中,B 不受影响
            Assert.Equal(0, expUtil.TryBeginPost(nameB));
        }
    }
}
