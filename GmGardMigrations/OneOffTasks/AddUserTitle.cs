using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class AddUserTitle
    {
        public static void AddTitle(List<int> ids, GmGard.Models.UserQuest.UserProfession title)
        {
            UsersContextFactory usersContextFactory = new UsersContextFactory();

            using (var udb = usersContextFactory.Create())
            {
                var quests = udb.UserQuests.Include("user").Where(uq => ids.Contains(uq.UserId));
                Console.Out.WriteLine("user count: " + quests.Count());
                foreach (var q in quests)
                {
                    q.AddTitle(title);
                }
                udb.SaveChanges();
            }
        }

        public static void ListTitle(List<int> ids, GmGard.Models.UserQuest.UserProfession title)
        {
            UsersContextFactory usersContextFactory = new UsersContextFactory();

            using (var udb = usersContextFactory.Create())
            {
                var users = udb.UserQuests.Include("user").Where(u => ids.Contains(u.UserId)).ToList();
                foreach (var id in ids)
                {
                    var user = users.Find(u => u.UserId == id);
                    if (user.HasTitle(title))
                    {
                        Console.WriteLine($"{user.UserId} {user.user.UserName}");
                    }
                }
            }
        }
    }
}
