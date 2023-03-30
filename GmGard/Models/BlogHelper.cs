using GmGard.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Collections.Concurrent;

namespace GmGard.Models
{
    public static class BlogHelper
    {
        public static bool BlogIsHarmony(BlogContext db, Blog blog, HarmonySettingsModel settings)
        {
            bool authorize;
            if (blog == null || settings == null)
            {
                authorize = false;
            }
            else if ((settings.BlacklistTags != null && settings.BlacklistTags.Count > 0) &&
                (db.TagsInBlogs.Any(t => t.BlogID == blog.BlogID && settings.BlacklistTags.Contains(t.TagID)))) //If list contains tag
            {
                authorize = false;
            }
            else if (settings.WhitelistIds != null && settings.WhitelistIds.Contains(blog.BlogID))
            {
                authorize = true;
            }
            else if ((settings.WhitelistCategories != null && settings.WhitelistCategories.Count > 0) &&
                settings.WhitelistCategories.Contains(blog.CategoryID))
            {
                authorize = true;
            }
            else
            {
                authorize = false;
            }
            return authorize;
        }


        public static HtmlString getFirstLine(string content, int maxlength, bool removetags = false)
        {
            content = Regex.Replace(content, "<[/]?img[^>]*>", string.Empty);
            content = Regex.Replace(content, @"\[img[^\]]+\]", string.Empty);
            content = Regex.Replace(content, @"</?[ ]*(table|tr|td|tbody|thead|tfoot)(.|\n)*?>", string.Empty);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            string firstline = null;
            string firstlinetext = null;
            var nodes = doc.DocumentNode.ChildNodes;
            if (nodes != null)
                foreach (HtmlNode node in nodes)
                {
                    firstlinetext = node.InnerText;
                    if (!string.IsNullOrWhiteSpace(firstlinetext.Replace("&nbsp;", string.Empty)))
                    {
                        firstline = node.InnerHtml;
                        break;
                    }
                }
            if (firstline == null)
            {
                string s = doc.DocumentNode.InnerText;
                int i = s.Length < maxlength ? s.Length : maxlength;
                return new HtmlString(s.Substring(0, i));
            }
            else
            {
                if (firstlinetext.Length <= maxlength)
                {
                    content = firstline;
                    if (removetags)
                        content = firstlinetext;
                }
                else
                {
                    firstline = firstlinetext;
                    content = firstline.Length >= maxlength ? firstline.Substring(0, maxlength) : firstline;
                }

                return new HtmlString(content);
            }
        }

        public static void RemoveComments(HtmlDocument objHTMLdoc)
        {
            var nodes = objHTMLdoc.DocumentNode.SelectNodes("//comment()");
            if (nodes != null)
            {
                foreach (HtmlNode comment in nodes)
                {
                    comment.ParentNode.RemoveChild(comment);
                }
            }
        }

        public static string RemoveComments(string content)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            RemoveComments(doc);
            return doc.DocumentNode.InnerHtml;
        }

        public static string removeAllTags(string content)
        {
            if (content == null)
            {
                return null;
            }
            return Regex.Replace(content, "<[^>]*>", string.Empty);
        }

        public static BlogLink[] GetBlogLink(string links)
        {
            return links == null ? null : Newtonsoft.Json.JsonConvert.DeserializeObject<BlogLink[]>(links);
        }

        //get nth url from content
        public static string getNthLink(int n, string content, out string text)
        {
            n--;
            string result = null;
            text = null;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            var nodes = doc.DocumentNode.SelectNodes("//a");
            if (nodes != null && nodes.Count > n && n >= 0)
            {
                var node = nodes[n];
                result = node.GetAttributeValue("href", null);
                text = node.InnerText;
            }
            return result;
        }

        //get first img from content
        public static string getFirstImg(string content)
        {
            string result = null;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            var nodes = doc.DocumentNode.SelectNodes("//img");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var src = node.GetAttributeValue("src", string.Empty);
                    if (!string.IsNullOrWhiteSpace(src) && !SiteConstant.SmileyPaths.Any(p => src.Contains(p)))
                    {
                        result = src;
                        break;
                    }
                }
            }

            return result;
        }

        public static string firstImgPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            int s = path.IndexOf(';');
            if (s > 0)
            {
                path = path.Substring(0, s);
            }
            return path;
        }

        //get first img path from a blog
        public static string firstImgPath(Blog blog, bool isthumb = false)
        {
            if (blog == null || blog.ImagePath == null)
                return null;
            string path = blog.ImagePath;
            if (blog.IsLocalImg)
            {
                path = firstImgPath(path);
                if (isthumb)
                {
                    path = path.Replace("/upload/", "/thumbs/");
                }
            }
            return path;
        }

        public static string removeImgPlaceholder(string content, int id, int maxid = 3)
        {
            content = content.Replace("[img" + id + "]", string.Empty);
            for (++id; id <= maxid; id++)
            {
                content = content.Replace("[img" + id + "]", "[img" + (id - 1) + "]");
            }
            return content;
        }

        public static string InsertImgPlaceholder(string content, int maxid = 3)
        {
            for (int id = maxid - 1; id >= 0; id--)
            {
                content = content.Replace("[img" + id + "]", "[img" + (id + 1) + "]");
            }
            return content.Replace("[nimg0]", "[img0]");
        }

        public static string ReplaceNewImgPlaceholder(string content, int newid, int currentid, bool[] uploadpos, int maxid = 3)
        {
            for (; newid <= maxid; newid++)
            {
                if (uploadpos.Length > newid && uploadpos[newid])
                {
                    content = content.Replace("[nimg" + newid + "]", "[img" + currentid++ + "]");
                }
                else
                {
                    content = content.Replace("[nimg" + newid + "]", string.Empty);
                }
            }
            return content;
        }

        public static bool checkBlogLinks(BlogLink[] blogLink)
        {
            if (blogLink == null || blogLink.Length == 0)
                return false;
            foreach (var link in blogLink)
            {
                if (string.IsNullOrEmpty(link.url))
                    return false;
                else if (link.url.ToUpperInvariant().Contains("JAVASCRIPT"))
                {
                    return false;
                }
                try
                {
                    link.url = new UriBuilder(link.url).Uri.ToString();
                }
                catch { }
            }
            return true;
        }

        public static string appendPassToContent(string content, BlogLink[] password)
        {
            if (password != null && password.Length > 0)
            {
                string pass = string.Empty;
                foreach (var item in password)
                {
                    if (!string.IsNullOrWhiteSpace(item.url))
                    {
                        pass += string.Format("{0}：<span class='label label-inverse'>{1}</span><br />", (string.IsNullOrWhiteSpace(item.name) ? "密码" : item.name), item.url);
                    }
                }
                content += string.Format("<p>{0}</p>", pass);
            }
            return content;
        }

        public static IQueryable<Blog> getSortedQuery(BlogContext db, IQueryable<Blog> query, string sort)
        {
            switch (sort)
            {
                case "Date":
                    query = query.OrderBy(q => q.BlogDate);
                    break;

                case "Visit_desc":
                    query = query.OrderByDescending(q => q.BlogVisit);
                    break;

                case "Visit":
                    query = query.OrderBy(q => q.BlogVisit);
                    break;

                case "Post":
                    query = query.OrderBy(q => db.Posts.Count(p => p.ItemId == q.BlogID && p.IdType == ItemType.Blog));
                    break;

                case "Post_desc":
                    query = query.OrderByDescending(q => db.Posts.Count(p => p.ItemId == q.BlogID && p.IdType == ItemType.Blog));
                    break;

                case "Rate":
                    query = query.OrderBy(q => q.Rating);
                    break;

                case "Rate_desc":
                    query = query.OrderByDescending(q => q.Rating);
                    break;

                case null:
                    query = query.OrderByDescending(q => q.BlogDate);
                    break;

                default:
                    // Special logic for favorite, need to read db.Favorites for order. username is added to sort type.
                    if (sort.StartsWith("AddDate_desc:"))
                    {
                        var username = sort.Split(':')[1];
                        query = query.Join(db.Favorites.Where(f => f.Username == username), b => b.BlogID, f => f.BlogID, (b, f) => new { date = f.AddDate, blog = b }).OrderByDescending(f => f.date).Select(f => f.blog);
                    }
                    else if (sort.StartsWith("AddDate:"))
                    {
                        var username = sort.Split(':')[1];
                        query = query.Join(db.Favorites.Where(f => f.Username == username), b => b.BlogID, f => f.BlogID, (b, f) => new { date = f.AddDate, blog = b }).OrderBy(f => f.date).Select(f => f.blog);
                    }
                    else
                    {
                        query = query.OrderByDescending(q => q.BlogDate);
                    }
                    break;
            }
            return query;
        }

        public static IQueryable<Topic> getSortedQuery(BlogContext db, IQueryable<Topic> query, string sort)
        {
            switch (sort)
            {
                case "Date":
                    query = query.OrderBy(q => q.CreateDate);
                    break;

                case "Update_desc":
                    query = query.OrderByDescending(q => q.UpdateDate);
                    break;

                case "Update":
                    query = query.OrderBy(q => q.UpdateDate);
                    break;

                case "Visit_desc":
                    query = query.OrderByDescending(q => q.TopicVisit);
                    break;

                case "Visit":
                    query = query.OrderBy(q => q.TopicVisit);
                    break;

                case "Post":
                    query = query.OrderBy(q => db.Posts.Count(p => p.ItemId == q.TopicID && p.IdType == ItemType.Topic));
                    break;

                case "Post_desc":
                    query = query.OrderByDescending(q => db.Posts.Count(p => p.ItemId == q.TopicID && p.IdType == ItemType.Topic));
                    break;

                default:
                    query = query.OrderByDescending(q => q.CreateDate);
                    break;
            }
            return query;
        }

        public static DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
        }

        static public List<int> ParseIntListFromString(string input, List<int> defaultlist = null)
        {
            List<int> result = null;
            bool isgood = true;
            if (!string.IsNullOrEmpty(input))
            {
                string[] stringlist = input.Split(',');
                result = new List<int>(stringlist.Length);
                foreach (var id in stringlist)
                {
                    int val;
                    if (!int.TryParse(id, out val))
                    {
                        isgood = false;
                        break;
                    }
                    result.Add(val);
                }
            }
            if (result == null || isgood == false)
            {
                result = defaultlist;
            }
            return result;
        }

        static public string ReplaceContentImage(Blog blog)
        {
            var content = blog.Content;
            if (blog.ImagePath != null)
            {
                var paths = blog.ImagePath.Split(';');
                var regex = new Regex("\\[img(\\d)\\]");
                content = regex.Replace(content, (Match m) =>
                {
                    if (int.TryParse(m.Groups[1].Value, out int idx) && idx < paths.Length)
                    {
                        return $"<img src='{paths[idx]}' data-index='{idx}' />";
                    }
                    return m.Value;
                });
            }
            return content;
        }
    }
}
