using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class UpdateImagePath
    {
        public static void Run()
        {
            BlogContextFactory blogContextFactory = new BlogContextFactory();
            var filenames = System.IO.Directory.EnumerateFiles(@"F:\bak\upload").Aggregate(new Dictionary<string, List<string>>(), (a, k) => {
                k = System.IO.Path.GetFileName(k);
                var key = k;
                if (k.Length > 13)
                {
                    key = k.Substring(k.Length - 13);
                }
                if (a.TryGetValue(key, out var v))
                {
                    v.Add(k);
                }
                else
                {
                    a.Add(key, new List<string>() { k });
                }
                return a;
            });
            using (var db = blogContextFactory.Create())
            {
                var allLegacies = db.Blogs.Where(b => b.IsLocalImg && !b.ImagePath.Contains("static.gmgard")).ToList();
                Console.Out.WriteLine("total legacy: " + allLegacies.Count);
                foreach (var item in allLegacies)
                {
                    var imgs = item.ImagePath.Split(';').Select(s => {
                        if (filenames.TryGetValue(s.Substring(s.Length - 13), out var v))
                        {
                            if (v.Count == 1)
                            {
                                return v[0];
                            }
                            if (v.Contains(s))
                            {
                                return s;
                            }
                            Console.Out.WriteLine("Please choose for " + s);
                            for (int i = 0; i < v.Count; i++)
                            {
                                Console.Out.WriteLine($"{i}: {v[i]}");
                            }
                            var input = Console.ReadLine();
                            return v[int.Parse(input)];
                        }
                        return s;
                    });

                    item.ImagePath = string.Join(";", imgs.Select(s => "//static.gmgard.us/Images/upload/" + s));
                }
                db.SaveChanges();
            }
        }
    }
}
