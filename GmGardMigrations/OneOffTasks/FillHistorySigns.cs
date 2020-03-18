using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class FillHistorySigns
    {
        const string DB_CONNECTION_STRING_0125 = @"Data Source=.\SQLEXPRESS;Initial Catalog=MyMVCWebUser20190125;Integrated Security=SSPI;";

        const int BATCH_SIZE = 5000;

        public static void FillConsecutiveSign()
        {
            var factory = new UsersContextFactory();
            var oldDb = new UsersContext(DB_CONNECTION_STRING_0125);
            var newDb = factory.Create();
            int i = 0;
            int hasConsecutiveSign = 0, noConsecutiveSign = 0;
            while (true)
            {
                var dict = oldDb.Users.Where(p => p.Id >= i && p.Id < (i + BATCH_SIZE)).ToDictionary(p => p.Id);
                var list = newDb.Users.Where(p => p.Id >= i && p.Id < (i + BATCH_SIZE)).Select(p => p.Id).ToList();
                if (dict.Count == 0 || list.Count == 0)
                {
                    break;
                }
                Dictionary<int, int> hcs = new Dictionary<int, int>();
                foreach (var up in list)
                {
                    if (!dict.ContainsKey(up))
                    {
                        Console.WriteLine("Id not found: " + up);
                        continue;
                    }
                    var u = dict[up];
                    if (u.LastSignDate >= new DateTime(2019, 1, 24))
                    {
                        hcs[up] = u.ConsecutiveSign;
                        hasConsecutiveSign++;
                    }
                    else
                    {
                        hcs[up] = 0;
                        noConsecutiveSign++;
                    }
                }
                var ValueString = hcs.Select(v => string.Format("({0},{1})", v.Key, v.Value));
                newDb.Database.ExecuteSqlCommand(
                        @"UPDATE b SET HistoryConsecutiveSign = t.s
                    FROM UserProfile b
                    JOIN (
	                    VALUES " + string.Join(",", ValueString) +
                        ") t (id, s) ON b.UserId = t.id"
                    );
                i += BATCH_SIZE;
            }
            Console.WriteLine("Last batch i: " + i);
            Console.WriteLine(string.Format("Has Sign: {0}, No Sign: {1}", hasConsecutiveSign, noConsecutiveSign));
        }

        public static void RecalculateConsecutiveSign()
        {
            var factory = new UsersContextFactory();
            var db = factory.Create();
            int i = 0;
            var minHistoryDate = new DateTime(2019, 1, 25);
            while (true)
            {
                Console.WriteLine($"Handling {i} to {i + BATCH_SIZE}");
                var userMissingSigns = new Dictionary<int, int>();
                var list = db.Users.Include("PunchIns").Where(u => u.LastSignDate >= new DateTime(2019, 5, 26)).Where(p => p.Id >= i && p.Id < (i + BATCH_SIZE)).Select(p => new
                {
                    p.Id,
                    PunchIns = p.PunchIns.OrderByDescending(pp => pp.TimeStamp),
                    Signs = p.ConsecutiveSign,
                    p.HistoryConsecutiveSign,
                    p.LastSignDate
                }).ToList();
                if (list.Count == 0)
                {
                    break;
                }
                var userWrongSignDate = new Dictionary<int, DateTime>();
                foreach (var up in list)
                {
                    var dset = up.PunchIns.ToLookup(p => p.TimeStamp.Date);
                    int consecutive = 0;
                    var lastDay = dset.Max(g => g.Key);
                    if (up.LastSignDate < lastDay)
                    {
                        var d = up.PunchIns.Max(p => p.TimeStamp);
                        Console.Out.WriteLine($"User {up.Id} last sign incorrect according to timestamp. Want {d}, Actual {up.LastSignDate}");
                        userWrongSignDate.Add(up.Id, d);
                    }
                    foreach(var t in dset.Select(g => g.Key).OrderByDescending(k => k))
                    {
                        if (t.Date > lastDay)
                        {
                            continue;
                        }
                        if (t.Date != lastDay)
                        {
                            break;
                        }
                        consecutive++;
                        lastDay = lastDay.AddDays(-1);
                    }
                    if (lastDay < minHistoryDate)
                    {
                        consecutive += up.HistoryConsecutiveSign;
                    }
                    if (consecutive != up.Signs)
                    {
                        Console.Out.WriteLine($"User {up.Id} sign incorrect according to timestamp. Want {consecutive}, Actual {up.Signs}");
                        userMissingSigns.Add(up.Id, consecutive);
                    }
                }

                if (userWrongSignDate.Count > 0)
                {
                    var ValueString = userWrongSignDate.Select(v => string.Format("({0},'{1}')", v.Key, v.Value.ToString("yyyy-MM-dd HH:mm:ss")));
                    db.Database.ExecuteSqlCommand(
                                @"UPDATE b SET LastSignDate = t.s
                    FROM UserProfile b
                    JOIN (
	                    VALUES " + string.Join(",", ValueString) +
                                ") t (id, s) ON b.UserId = t.id"
                            );
                }
                if (userMissingSigns.Count > 0)
                {
                    var ValueString = userMissingSigns.Select(v => string.Format("({0},{1})", v.Key, v.Value));
                    db.Database.ExecuteSqlCommand(
                                @"UPDATE b SET ConsecutiveSign = t.s
                    FROM UserProfile b
                    JOIN (
	                    VALUES " + string.Join(",", ValueString) +
                                ") t (id, s) ON b.UserId = t.id"
                            );
                }

                i += BATCH_SIZE;
            }
        }
    }
}
