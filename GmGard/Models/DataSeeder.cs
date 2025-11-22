using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models
{
    public static class DataSeeder
    {
        public static async Task SeedUsersAsync(UsersContext context, UserManager<UserProfile> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            // Init Roles
            var roles = new[] { "Administrator", "Writers", "Moderator", "Banned", "AdManager", "Auditor" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role, NormalizedName = role.ToUpper() });
                }
            }

            // Init Admin
            if (!context.Users.Any(u => u.UserName == "admin"))
            {
                var admin = new UserProfile
                {
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
                
                var result = await userManager.CreateAsync(admin, "gmgard");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrator");
                }
            }

            // Init ExpTable
            if (!context.ExpTable.Any())
            {
                GetExpTableSample().ForEach(e => context.ExpTable.Add(e));
                try
                {
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ExperienceTables ON; Insert Into dbo.ExperienceTables (Level, ExperienceStart,ExperienceEnd,Title) values (0,0,0,'缺省'); SET IDENTITY_INSERT ExperienceTables OFF;");
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ExperienceTables ON; Insert Into dbo.ExperienceTables (Level, ExperienceStart,ExperienceEnd,Title) values (-1,-1,-1,'小黑屋'); SET IDENTITY_INSERT ExperienceTables OFF;");
                    context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT ExperienceTables ON; Insert Into dbo.ExperienceTables (Level, ExperienceStart,ExperienceEnd,Title) values (99,999,999,'管理员'); SET IDENTITY_INSERT ExperienceTables OFF;");
                }
                catch { }
                await context.SaveChangesAsync();
            }
        }

        public static void SeedBlog(BlogContext context)
        {
            if (!context.Categories.Any())
            {
                GetCategories().ForEach(c => context.Categories.Add(c));
            }
            
            if (!context.Blogs.Any())
            {
                GetBlogs().ForEach(p => context.Blogs.Add(p));
                context.SaveChanges();
                
                try
                {
                    // Using ExecuteSqlRaw for identity insert and triggers
                    context.Database.ExecuteSqlRaw(@"set Identity_insert Blogs on;
                                                     Insert into dbo.Blogs (blogid, BlogTitle, Content, ImagePath, isLocalImg, BlogDate,CategoryID, Author, isApproved, BlogVisit, isHarmony)
                                                     values (0, 'V0.01', '版本历史', null, 'false', GETDATE(), 1, 'admin', 'false', 0, 'false'),
                                                            (-1, '举报消息', '举报消息', null, 'false', GETDATE(), 1, 'admin', 'false', 0, 'false');
                                                     set Identity_insert Blogs off;");
                }
                catch { }
            }

            if (!context.Posts.Any())
            {
                GetPosts().ForEach(p => context.Posts.Add(p));
                context.SaveChanges();
            }

            // Trigger creation should probably be in a migration, but if we keep it here:
            try 
            {
                context.Database.ExecuteSqlRaw(@"create trigger Rating_trigger
                    on ratings
                    after insert,update,delete
                    as
                    begin
                    update b
                    set b.Rating = (
                        select isnull(SUM(ratings.value), 0) from Ratings where
                        BlogID = i.BlogID
                        )
                    from dbo.Blogs as b inner join 
                    (select BlogID from inserted union select BlogID from deleted) as i 
                    on i.BlogID = b.BlogID
                    end");
            }
            catch
            {
                // Trigger might already exist
            }
        }

        private static List<ExperienceTable> GetExpTableSample()
        {
            return new List<ExperienceTable>(){
                new ExperienceTable{ ExperienceStart=21, ExperienceEnd=100, Level=2, Title="绅士"},
                new ExperienceTable{ ExperienceStart=1, ExperienceEnd=20, Level=1, Title="路人"},
            };
        }

        private static List<Category> GetCategories()
        {
            var categories = new List<Category> {
            new Category
            {
                CategoryID = 1,
                CategoryName = "资讯"
            },
            new Category
            {
                CategoryID = 2,
                CategoryName = "动漫"
            },
            new Category
            {
                CategoryID = 3,
                CategoryName = "CG"
            },
            new Category
            {
                CategoryID = 4,
                CategoryName = "游戏"
            },
            new Category
            {
                CategoryID = 5,
                CategoryName = "漫画"
            },
            new Category
            {
                CategoryID = 6,
                CategoryName = "画集"
            }
        };

            return categories;
        }

        private static List<Blog> GetBlogs()
        {
            var blogs = new List<Blog> {
                new Blog
                {
                    BlogID = 1,
                    BlogTitle = "Hello World",
                    Content = "This is the first blog in this web! Cheers!!!",
                    ImagePath="/images/upload/hello.png",
                    BlogDate=DateTime.Now,
                    CategoryID = 1,
                    Author = "admin",
                    isApproved = true,
                    IsLocalImg = false,
                    BlogVisit = 0,
                },
                new Blog
                {
                    BlogID = 2,
                    BlogTitle = "Please add more contents",
                    Content = "Please...",
                    ImagePath="/images/upload/hello.png",
                    BlogDate=DateTime.Now,
                    CategoryID = 1,
                    Author = "admin",
                    isApproved = true,
                    IsLocalImg = true,
                    BlogVisit = 0
                },
                new Blog
                {
                    BlogID = 3,
                    BlogTitle = "鬼父",
                    Content = "鬼父～爱娘强制発情・ブルーゲイル",
                    ImagePath="/images/upload/oni.png",
                    BlogDate=DateTime.Now,
                    CategoryID = 2,
                    Author = "admin",
                    isApproved = true,
                    BlogVisit = 0,
                    IsLocalImg = false,
                },
                new Blog
                {
                    BlogID = 5,
                    BlogTitle = "Comic LO 2013-05",
                    Content = "哦也",
                    ImagePath="/images/upload/lo.png",
                    BlogDate=DateTime.Now,
                    CategoryID = 5,
                    Author = "admin",
                    isApproved = true,
                    IsLocalImg = false,
                    BlogVisit = 0
                }
            };
            return blogs;
        }

        private static List<Post> GetPosts()
        {
            return new List<Post>{
                new Post{
                    Author = "admin",
                    Content = "<p>LZSB</p>",
                    PostDate = DateTime.Now,
                    PostId = 1,
                    IdType = ItemType.Blog,
                    ItemId = 1
                }
            };
        }
    }
}
