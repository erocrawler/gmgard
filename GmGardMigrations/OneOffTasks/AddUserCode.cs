using System;
using System.Collections.Generic;
using System.Text;

namespace GmGardMigrations.OneOffTasks
{
    class AddUserCode
    {
        public static void AddCode(int userid, int count)
        {
            UsersContextFactory usersContextFactory = new UsersContextFactory();
            using (var udb = usersContextFactory.Create())
            {
                for (int i = 0; i <count; i++)
                {
                    udb.UserCodes.Add(new GmGard.Models.UserCode
                    {
                        UserId = userid,
                        BuyDate = DateTime.Now,
                    });
                }
                udb.SaveChanges();
            }
        }
    }
}
