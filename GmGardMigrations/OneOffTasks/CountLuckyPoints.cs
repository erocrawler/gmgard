using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GmGardMigrations.OneOffTasks
{
    class CountLuckyPoints
    {
        public static void Run()
        {
            UsersContextFactory usersContextFactory = new UsersContextFactory();

            using (var f = File.CreateText("luckypoints.csv"))
            using (var udb = usersContextFactory.Create())
            {
                f.WriteLine("用户名,当前积分,累计积分");
                var lps = udb.UserVouchers.Where(uv => uv.VoucherKind == GmGard.Models.UserVoucher.Kind.LuckyPoint)
                    .GroupBy(g => g.User.UserName).ToList();
                foreach(var user in lps)
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
