using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace GmGard.Models
{
    public class BlogDBinit : CreateDatabaseIfNotExists<BlogContext>
    {
        protected override void Seed(BlogContext context)
        {
            GetCategories().ForEach(c => context.Categories.Add(c));
            GetBlogs().ForEach(p => context.Blogs.Add(p));
            GetPosts().ForEach(p => context.Posts.Add(p));
            context.SaveChanges();
            context.Database.ExecuteSqlCommand(@"set Identity_insert Blogs on;
                                                 Insert into dbo.Blogs (blogid, BlogTitle, Content, ImagePath, isLocalImg, BlogDate,CategoryID, Author, isApproved, BlogVisit, isHarmony)
                                                 values (0, 'V0.01', '版本历史', null, 'false', GETDATE(), 1, 'admin', 'false', 0, 'false'),
                                                        (-1, '举报消息', '举报消息', null, 'false', GETDATE(), 1, 'admin', 'false', 0, 'false');
                                                 set Identity_insert Blogs off;");
            context.Database.ExecuteSqlCommand(@"create trigger Rating_trigger
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
                    BlogVisit = 0,
                    IsLocalImg = false,
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