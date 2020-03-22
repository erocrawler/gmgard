using AspNetCore.Identity.EntityFramework6;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models
{
    public class UserDBinit : CreateDatabaseIfNotExists<UsersContext>
    {
        private readonly IPasswordHasher<UserProfile> _hasher;

        public UserDBinit(IPasswordHasher<UserProfile> hasher)
        {
            _hasher = hasher;
        }

        protected override void Seed(UsersContext context)
        {
            InitRoles(context);
            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var admin = new UserProfile {
                    UserName = "admin",
                    Email = "admin@gmgard.com",
                    Points = 100,
                    LastLoginDate = DateTime.Now,
                    Level = 99,
                    ConsecutiveSign = 0,
                    Experience = -2,
                    LastSignDate = new DateTime(1900, 1, 1),
                    CreateDate = DateTime.Now,
                    SecurityStamp = Guid.NewGuid().ToString()
                };
                admin.NormalizedEmail = admin.Email.ToUpper();
                admin.NormalizedUserName = admin.UserName.ToUpper();
                admin.PasswordHash = _hasher.HashPassword(admin, "gmgard");
                context.Users.Add(admin);
                context.SaveChanges();
                var store = new UserStore<UsersContext>(context);
                store.AddToRoleAsync(admin, "Administrator").Wait();
            }

            GetExpTableSample().ForEach(e => context.ExpTable.Add(e));
            context.Database.ExecuteSqlCommand("Insert Into dbo.ExperienceTables (Level, ExperienceStart,ExperienceEnd,Title) values (0,0,0,'缺省')");
            context.Database.ExecuteSqlCommand("Insert Into dbo.ExperienceTables (Level, ExperienceStart,ExperienceEnd,Title) values (-1,-1,-1,'小黑屋')");
            context.Database.ExecuteSqlCommand("Insert Into dbo.ExperienceTables (Level, ExperienceStart,ExperienceEnd,Title) values (99,999,999,'管理员')");
            context.SaveChanges();
        }

        public static void InitRoles(UsersContext db)
        {
            RoleStore<UsersContext> store = new RoleStore<UsersContext>(db);
            var roles = db.Roles.ToList();
            foreach(var role in new[] { "Administrator", "Writers", "Moderator", "Banned", "AdManager", "Auditor" })
            {
                if (!roles.Any(r => r.Name.Equals(role, StringComparison.OrdinalIgnoreCase)))
                {
                    store.CreateAsync(new AspNetCore.Identity.EntityFramework6.IdentityRole { Name = role, NormalizedName = role.ToUpper() }).Wait();
                }
            }
        }

        private List<ExperienceTable> GetExpTableSample()
        {
            return new List<ExperienceTable>(){
                new ExperienceTable{ ExperienceStart=21, ExperienceEnd=100, Level=2, Title="绅士"},
                new ExperienceTable{ ExperienceStart=1, ExperienceEnd=20, Level=1, Title="路人"},
            };
        }
    }
}