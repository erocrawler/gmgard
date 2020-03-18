using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using System.Data.Entity;
using GmGard.Extensions;

namespace GmGard.Services
{
    public class TagUtil : UtilityService
    {
        public TagUtil(BlogContext db, UsersContext udb, IMemoryCache cache) : base(db, udb, cache)
        {
        }

        public static int CheckBlogTag(string tagstring, int maxcount = 10, int maxlength = 20)
        {
            if (tagstring == null || tagstring.Length == 0)
                return 1;
            else if (tagstring.Length > 100)
                return -1;
            string[] tags = SplitTags(tagstring);
            if (tags.Length > maxcount)
                return 1;
            foreach (var tag in tags)
            {
                if (tag.Length > maxlength)
                    return -1;
            }
            return 0;
        }

        public static string[] SplitTags(string tagstring)
        {
            if (tagstring == null) return new string[0];
            return tagstring.Split(new char[] { '，', ',', ';', '；', ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
        }

        public string[] GetTagNamesInBlog(int blogid)
        {
            return _db.TagsInBlogs.Where(t => t.BlogID == blogid).Select(t => t.tag.TagName).ToArray();
        }

        public Task<List<Tag>> GetTagsInBlogAsync(int blogid)
        {
            return _db.TagsInBlogs.Where(t => t.BlogID == blogid).Select(t => t.tag).ToListAsync();
        }

        public List<Tag> AddTagsForBlog(int blogid, string[] tags, string addby)
        {
            List<Tag> AddedTags = new List<Tag>();
            var tagDict = _db.Tags.Where(tt => tags.Contains(tt.TagName)).ToDictionary(t => t.TagName.ToLower(), Extensions.SqlStringComparer.Instance);
            foreach (string tag in tags)
            {
                Tag t;
                if (!tagDict.TryGetValue(tag, out t))
                {
                    t = new Tag { TagName = tag };
                }
                TagsInBlog tib = new TagsInBlog { BlogID = blogid };
                tib.tag = t;
                tib.AddBy = addby;
                _db.TagsInBlogs.Add(tib);
                AddedTags.Add(t);
            }
            _db.SaveChanges();
            return AddedTags;
        }

        public List<Tag> SetTagsForBlog(int blogid, string[] tags, string user)
        {
            bool hasdelete = false;
            var tagcurrent = _db.TagsInBlogs.Include("tag").Where(i => i.BlogID == blogid).ToList();
            var tagtodel = tagcurrent.Where(a => !tags.Contains(a.tag.TagName, SqlStringComparer.Instance));
            var tagtoadd = tags.Except(tagcurrent.Select(a => a.tag.TagName), SqlStringComparer.Instance).ToList();
            var UpdatedTags = tagcurrent.Except(tagtodel).Select(tib => tib.tag).ToList();
            foreach (var tib in tagtodel)
            {
                _db.TagHistories.Add(new TagHistory { AddBy = tib.AddBy, BlogID = blogid, DeleteBy = user, TagName = tib.tag.TagName, Time = DateTime.Now });
                _db.TagsInBlogs.Remove(tib);
                hasdelete = true;
            }
            var ExistingTags = _db.Tags.Where(t => tagtoadd.Contains(t.TagName)).ToDictionary(t => t.TagName, SqlStringComparer.Instance);
            foreach (var tag in tagtoadd)
            {
                if (!ExistingTags.TryGetValue(tag, out Tag t))
                {
                    t = new Tag { TagName = tag };
                }
                TagsInBlog tib = new TagsInBlog
                {
                    tag = t,
                    BlogID = blogid,
                    AddBy = user
                };
                _db.TagsInBlogs.Add(tib);
                UpdatedTags.Add(tib.tag);
            }
            _db.SaveChanges();
            if (hasdelete)
            {
                _db.Database.ExecuteSqlCommand("Delete From Tags Where TagID not in ((Select TagID from TagsInBlogs) union (Select TagID from Topics))");
            }
            return UpdatedTags;
        }
    }

    public static class TagExtension
    {
        public static bool IsRemovable(this TagsInBlog tib, string Author, string User, bool IsAdmin, IEnumerable<int> BlackListID)
        {
            return !BlackListID.Contains(tib.TagID) && (string.IsNullOrEmpty(tib.AddBy)
                || Author.Equals(User, StringComparison.OrdinalIgnoreCase)
                || !tib.AddBy.Equals(Author, StringComparison.OrdinalIgnoreCase)
                || IsAdmin);
        }
    }
}