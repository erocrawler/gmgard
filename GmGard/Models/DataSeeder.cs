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
                // Add special records with specific IDs
                context.ExpTable.Add(new ExperienceTable { Level = 0, ExperienceStart = 0, ExperienceEnd = 0, Title = "缺省" });
                context.ExpTable.Add(new ExperienceTable { Level = -1, ExperienceStart = -1, ExperienceEnd = -1, Title = "小黑屋" });
                context.ExpTable.Add(new ExperienceTable { Level = 99, ExperienceStart = 999, ExperienceEnd = 999, Title = "管理员" });
                
                // Add sample data
                GetExpTableSample().ForEach(e => context.ExpTable.Add(e));
                
                await context.SaveChangesAsync();
            }
        }

        public static void SeedBlog(BlogContext context)
        {
            var isPostgreSQL = context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

            if (!context.Categories.Any())
            {
                GetCategories().ForEach(c => context.Categories.Add(c));
                context.SaveChanges();
                
                // Reset sequence for PostgreSQL after inserting with explicit IDs
                if (isPostgreSQL)
                {
                    context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Categories\"', 'CategoryID'), COALESCE(MAX(\"CategoryID\"), 1)) FROM \"Categories\";");
                }
            }
            
            if (!context.Blogs.Any())
            {
                // Add special system blogs with specific IDs
            
                // For PostgreSQL, insert directly with raw SQL to preserve explicit IDs
                context.Database.ExecuteSqlRaw(@"
                    INSERT INTO ""Blogs"" (""BlogID"", ""BlogTitle"", ""Content"", ""BlogDate"", ""CategoryID"", ""Author"", ""isApproved"", ""BlogVisit"", ""IsLocalImg"")
                    VALUES (-1, '举报消息', '举报消息', NOW(), 1, 'admin', false, 0, false);
                ");
                context.Database.ExecuteSqlRaw(@"
                    INSERT INTO ""Blogs"" (""BlogID"", ""BlogTitle"", ""Content"", ""BlogDate"", ""CategoryID"", ""Author"", ""isApproved"", ""BlogVisit"", ""IsLocalImg"")
                    VALUES (0, 'V0.01', '版本历史', NOW(), 1, 'admin', false, 0, false);
                ");
                
                GetBlogs().ForEach(p => context.Blogs.Add(p));
                
                context.SaveChanges();
                
                // Reset sequence for PostgreSQL after inserting with explicit IDs
                if (isPostgreSQL)
                {
                    context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Blogs\"', 'BlogID'), GREATEST(COALESCE(MAX(\"BlogID\"), 1), 1)) FROM \"Blogs\";");
                }
            }

            if (!context.Posts.Any())
            {
                GetPosts().ForEach(p => context.Posts.Add(p));
                context.SaveChanges();
                
                // Reset sequence for PostgreSQL after inserting with explicit IDs
                if (isPostgreSQL)
                {
                    context.Database.ExecuteSqlRaw("SELECT setval(pg_get_serial_sequence('\"Posts\"', 'PostId'), COALESCE(MAX(\"PostId\"), 1)) FROM \"Posts\";");
                }
            }

            // Create database triggers if they don't exist
            CreateTriggersIfNotExist(context);
        }

        private static void CreateTriggersIfNotExist(BlogContext context)
        {
            var isPostgreSQL = context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";
            
            try
            {
                if (isPostgreSQL)
                {
                    // PostgreSQL triggers
                    context.Database.ExecuteSqlRaw(@"
                        CREATE OR REPLACE FUNCTION update_blog_rating()
                        RETURNS TRIGGER AS $$
                        BEGIN
                            UPDATE ""Blogs""
                            SET ""Rating"" = COALESCE((
                                SELECT SUM(""value"")
                                FROM ""Ratings""
                                WHERE ""BlogID"" = COALESCE(NEW.""BlogID"", OLD.""BlogID"")
                            ), 0)
                            WHERE ""BlogID"" = COALESCE(NEW.""BlogID"", OLD.""BlogID"");
                            RETURN NULL;
                        END;
                        $$ LANGUAGE plpgsql;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        DO $$
                        BEGIN
                            IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'rating_trigger') THEN
                                CREATE TRIGGER rating_trigger
                                AFTER INSERT OR UPDATE OR DELETE ON ""Ratings""
                                FOR EACH ROW
                                EXECUTE FUNCTION update_blog_rating();
                            END IF;
                        END $$;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        CREATE OR REPLACE FUNCTION update_post_rating()
                        RETURNS TRIGGER AS $$
                        BEGIN
                            UPDATE ""Posts""
                            SET ""Rating"" = COALESCE((
                                SELECT SUM(""Value"")
                                FROM ""PostRatings""
                                WHERE ""PostId"" = COALESCE(NEW.""PostId"", OLD.""PostId"")
                            ), 0)
                            WHERE ""PostId"" = COALESCE(NEW.""PostId"", OLD.""PostId"");
                            RETURN NULL;
                        END;
                        $$ LANGUAGE plpgsql;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        DO $$
                        BEGIN
                            IF NOT EXISTS (SELECT 1 FROM pg_trigger WHERE tgname = 'postrating_trigger') THEN
                                CREATE TRIGGER postrating_trigger
                                AFTER INSERT OR UPDATE OR DELETE ON ""PostRatings""
                                FOR EACH ROW
                                EXECUTE FUNCTION update_post_rating();
                            END IF;
                        END $$;
                    ");
                }
                else
                {
                    // SQL Server triggers
                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'Rating_trigger')
                        BEGIN
                            EXEC('
                                CREATE TRIGGER Rating_trigger
                                ON Ratings
                                AFTER INSERT, UPDATE, DELETE
                                AS
                                BEGIN
                                    UPDATE b
                                    SET b.Rating = (
                                        SELECT ISNULL(SUM(ratings.value), 0) 
                                        FROM Ratings 
                                        WHERE BlogID = i.BlogID
                                    )
                                    FROM dbo.Blogs AS b 
                                    INNER JOIN (
                                        SELECT BlogID FROM inserted 
                                        UNION 
                                        SELECT BlogID FROM deleted
                                    ) AS i ON i.BlogID = b.BlogID
                                END
                            ')
                        END
                    ");

                    // PostRating_trigger is not needed - PostRatings are for Post entities, not Blogs
                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.triggers WHERE name = 'PostRating_trigger')
                        BEGIN
                            EXEC('
                                CREATE TRIGGER PostRating_trigger
                                ON PostRatings
                                AFTER INSERT, UPDATE, DELETE
                                AS
                                BEGIN
                                    UPDATE p
                                    SET p.Rating = (
                                        SELECT ISNULL(SUM(PostRatings.[Value]), 0) 
                                        FROM PostRatings 
                                        WHERE PostID = i.PostID
                                    )
                                    FROM dbo.Posts AS p 
                                    INNER JOIN (
                                        SELECT PostID FROM inserted 
                                        UNION 
                                        SELECT PostID FROM deleted
                                    ) AS i ON i.PostID = p.PostID
                                END
                            ')
                        END
                    ");
                }
            }
            catch
            {
                // Triggers might already exist or there might be permission issues
                // This is not critical for initial setup
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
