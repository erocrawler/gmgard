using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace GmGardMigrations.OneOffTasks
{
    class FillBlogPass
    {
        class BlogLink
        {
            public string name { get; set; }
            public string url { get; set; }
            public string pass { get; set; }
        }

        const int LAST_BLOG_ID = 81169;
        const int BATCH_SIZE = 5000;

        public static void Run()
        {
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            using (var db = blogContextFactory.Create())
            {
                var totalBlogs = db.Blogs.Where(b => b.BlogID > 0).Count();
                Console.WriteLine($"total blogs: {totalBlogs}");
                int lastblog = LAST_BLOG_ID;
                for (int i = 0; i < totalBlogs; i += BATCH_SIZE)
                {
                    var blogs = db.Blogs.Where(b => b.BlogID > LAST_BLOG_ID && b.Links.Length > 0).OrderBy(b => b.BlogID).Skip(i).Take(BATCH_SIZE);
                    foreach (var blog in blogs)
                    {
                        var link = Newtonsoft.Json.JsonConvert.DeserializeObject<BlogLink[]>(blog.Links);
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(blog.Content);
                        var ns = doc.DocumentNode.SelectNodes("//span[@class=\"label label-inverse\"]");
                        if (ns == null) {
                            continue;
                        }
                        var nodes = ns.Where(n => n.InnerText.Length == 4);
                        if (nodes.Count() > 0 && link.Count() > 0)
                        {
                            if (link.Count() != nodes.Count())
                            {
                                if (link.Count() == 1)
                                {
                                    Console.WriteLine("multiple pass for single link, using last one: " + nodes.Last().InnerText);
                                    link.ElementAt(0).pass = nodes.Last().InnerText;
                                }
                                else if (nodes.Count() == 1 && link.Count(b => b.url.Contains("pan.baidu.com")) == 1)
                                {
                                    link.Single(b => b.url.Contains("pan.baidu.com")).pass = nodes.First().InnerText;
                                }
                                else
                                {
                                    Console.WriteLine(blog.BlogID + " Manual select: ");
                                    for (int j = 0; j < link.Count(); j++)
                                    {
                                        Console.WriteLine(j + ": " + Newtonsoft.Json.JsonConvert.SerializeObject(link.ElementAt(j), Newtonsoft.Json.Formatting.Indented));
                                    }
                                    Console.WriteLine(nodes.ElementAt(0).ParentNode.InnerHtml);
                                    for (int j = 0; j < nodes.Count(); j++)
                                    {
                                        Console.WriteLine($"{j}: {nodes.ElementAt(j).InnerText} Enter index [0-{link.Count() - 1}]:");
                                        int k;
                                        string input;
                                        do
                                        {
                                            input = Console.ReadLine();
                                        }
                                        while (!int.TryParse(input, out k));
                                        if (k >= 0 && k < nodes.Count() && k < link.Count())
                                        {
                                            link.ElementAt(k).pass = nodes.ElementAt(j).InnerText;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < nodes.Count(); j++)
                                {
                                    link.ElementAt(j).pass = nodes.ElementAt(j).InnerText;
                                }
                            }
                            blog.Links = Newtonsoft.Json.JsonConvert.SerializeObject(link);
                        }
                        lastblog = blog.BlogID;
                    }
                    db.SaveChanges();
                    Console.WriteLine($"{blogs.Count()} blogs saved. last saved blog: {lastblog}");
                }
            }
        }
    }
}
