using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GmGardMigrations.OneOffTasks
{
    class CountLuckyPoints
    {
        const int BATCH_SIZE = 1000;
        public static void Run()
        {
            DateTime reduceTime = new DateTime(2020, 6, 2, 17, 30, 0);
            UsersContextFactory usersContextFactory = new UsersContextFactory();
            using (var udb = usersContextFactory.Create())
            {
                var lps = udb.UserVouchers.Where(uv => uv.VoucherKind == GmGard.Models.UserVoucher.Kind.WheelA).ToList();
                var dict = new Dictionary<int, int>();
                foreach (var v in lps)
                {
                    int points = (v.IssueTime < reduceTime) ? 100 : 30;
                    if (dict.ContainsKey(v.UserID.Value))
                    {
                        dict[v.UserID.Value] += points; 
                    } 
                    else
                    {
                        dict[v.UserID.Value] = points;
                    }
                }
                Console.Out.WriteLine($"total users: {dict.Count}");
                Console.Out.WriteLine($"max points added: {dict.Max(v => v.Value)}");
                var ValueString = dict.Select(v => string.Format("({0},{1})", v.Key, v.Value));
                var sql = @"update u set u.Points = u.Points + t.p
                    FROM UserProfile u
                    JOIN (
	                    VALUES " + string.Join(",", ValueString) +
                        ") t (id, p) ON u.UserId = t.id";
                Console.Out.WriteLine(sql);
                udb.Database.ExecuteSqlCommand(sql);
            }
        }

        public static void Count()
        {
            UsersContextFactory usersContextFactory = new UsersContextFactory();

            using (var f = File.CreateText("luckypoints.csv"))
            using (var udb = usersContextFactory.Create())
            {
                f.WriteLine("用户名,当前积分,累计积分");
                var lps = udb.UserVouchers.Where(uv => uv.VoucherKind == GmGard.Models.UserVoucher.Kind.LuckyPoint)
                    .GroupBy(g => g.User.UserName).ToList();
                foreach (var user in lps)
                {
                    var points = user.Aggregate(new { cp = 0, tp = 0 }, (acc, uv) =>
                    {
                        var tokens = uv.RedeemItem.Split("/");
                        return new { cp = acc.cp + int.Parse(tokens[0]), tp = acc.tp + int.Parse(tokens[1]) };
                    });
                    f.WriteLine($"{user.Key},{points.cp},{points.tp}");
                }
            }
        }
    }
}
