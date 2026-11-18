using System;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GmGard.Tests
{
    /// <summary>
    /// L3:邀请码用 UPDATE ... WHERE UsedBy IS NULL 原子占用。这里直接测控制器里用的那条语句形态(Register 整个流程依赖
    /// SignInManager、验证码等,在夹具里驱动代价太高):并发占用同一个码只有一个成功,占用后能被释放。
    /// L6:LatestAudits 改成数据库内子查询;测试用同一份数据对比"旧的内存版"和"新的 SQL 版"的结果必须一致。
    /// </summary>
    public class InviteCodeAuditTests : IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        private Guid SeedCode(int buyerId)
        {
            using var s = _app.NewScope();
            var udb = s.ServiceProvider.GetRequiredService<UsersContext>();
            var code = Guid.NewGuid();
            udb.UserCodes.Add(new UserCode { Code = code, UserId = buyerId, BuyDate = DateTime.Now });
            udb.SaveChanges();
            return code;
        }

        // 与 AccountController.Register 里相同的占用语句
        private static Task<int> Claim(UsersContext db, Guid code) =>
            db.UserCodes.Where(c => c.Code == code && c.UsedBy == null).ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedBy, c => (int?)c.UserId));

        [SkippableFact]
        public async Task Concurrent_claims_of_the_same_code_only_one_wins()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int buyer = _app.SeedUser("buyer");
            var code = SeedCode(buyer);

            var results = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ => Task.Run(async () =>
            {
                using var s = _app.NewScope();
                return await Claim(s.ServiceProvider.GetRequiredService<UsersContext>(), code);
            })));

            Assert.Equal(1, results.Sum());
            Assert.Equal(buyer, _app.QueryUsers(db => db.UserCodes.Single(c => c.Code == code).UsedBy));
        }

        [SkippableFact]
        public async Task Already_used_code_cannot_be_claimed_and_a_released_code_can_be_claimed_again()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int buyer = _app.SeedUser("buyer");
            var code = SeedCode(buyer);

            using (var s = _app.NewScope())
            {
                var db = s.ServiceProvider.GetRequiredService<UsersContext>();
                Assert.Equal(1, await Claim(db, code));
                Assert.Equal(0, await Claim(db, code));               // 已被占用
                // 创建用户失败时的释放语句
                await db.UserCodes.Where(c => c.Code == code).ExecuteUpdateAsync(x => x.SetProperty(c => c.UsedBy, (int?)null));
                Assert.Equal(1, await Claim(db, code));               // 释放后可以再次使用
            }
        }

        // ---------- L6: LatestAudits ----------

        private void SeedAudit(int blogId, string auditor, int version, BlogAudit.Action action)
        {
            using var s = _app.NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            db.BlogAudits.Add(new BlogAudit { BlogID = blogId, Auditor = auditor, BlogVersion = version, AuditAction = action, AuditDate = DateTime.Now });
            db.SaveChanges();
        }

        // 新实现(与 AuditController.LatestAudits 相同)
        private static IQueryable<BlogAudit> Latest(BlogContext db) =>
            db.BlogAudits.Where(ba => !db.BlogAudits.Any(done => done.BlogID == ba.BlogID
                && (done.AuditAction == BlogAudit.Action.Approve || done.AuditAction == BlogAudit.Action.Deny)
                && done.BlogVersion >= ba.BlogVersion));

        // 旧实现(整张表读进内存再过滤)
        private static System.Collections.Generic.List<BlogAudit> LatestOld(BlogContext db)
        {
            var maxVersionDict = db.BlogAudits
                .Where(ba => ba.AuditAction == BlogAudit.Action.Approve || ba.AuditAction == BlogAudit.Action.Deny)
                .GroupBy(ba => ba.BlogID)
                .Select(g => new { BlogID = g.Key, MaxVersion = g.Max(ba => ba.BlogVersion) })
                .ToList().ToDictionary(x => x.BlogID, x => x.MaxVersion);
            return db.BlogAudits.ToList().Where(ba => !maxVersionDict.ContainsKey(ba.BlogID) || ba.BlogVersion > maxVersionDict[ba.BlogID]).ToList();
        }

        [SkippableFact]
        public void Latest_audits_sql_version_matches_the_old_in_memory_version()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            int cat = _app.SeedCategory();
            int b1 = _app.SeedBlog("owner", cat, approved: null);
            int b2 = _app.SeedBlog("owner", cat, approved: null);
            int b3 = _app.SeedBlog("owner", cat, approved: null);
            // b1:版本 1 被拒(Deny),版本 2 重新提交后两位审核员投了票 -> 只有版本 2 的两条是"进行中"
            SeedAudit(b1, "a1", 1, BlogAudit.Action.VoteDeny);
            SeedAudit(b1, "admin", 1, BlogAudit.Action.Deny);
            SeedAudit(b1, "a1", 2, BlogAudit.Action.VoteApprove);
            SeedAudit(b1, "a2", 2, BlogAudit.Action.VoteDeny);
            // b2:从没被通过/拒绝过 -> 全部都是进行中
            SeedAudit(b2, "a1", 1, BlogAudit.Action.VoteApprove);
            SeedAudit(b2, "a2", 1, BlogAudit.Action.VoteApprove);
            // b3:已经通过(Approve 在版本 3),之前的投票不再算
            SeedAudit(b3, "a1", 3, BlogAudit.Action.VoteApprove);
            SeedAudit(b3, "admin", 3, BlogAudit.Action.Approve);

            var (oldSet, newSet) = _app.QueryData(db => (
                LatestOld(db).Select(a => (a.BlogID, a.Auditor, a.BlogVersion)).OrderBy(x => x).ToList(),
                Latest(db).ToList().Select(a => (a.BlogID, a.Auditor, a.BlogVersion)).OrderBy(x => x).ToList()));

            Assert.Equal(oldSet, newSet);
            Assert.Equal(4, newSet.Count);   // b1 的 2 条版本 2 + b2 的 2 条;b1 版本 1 和 b3 全部被排除
            Assert.DoesNotContain(newSet, x => x.BlogID == b3);
            Assert.DoesNotContain(newSet, x => x.BlogID == b1 && x.BlogVersion == 1);
        }
    }
}
