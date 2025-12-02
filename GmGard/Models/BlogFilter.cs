using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GmGard.Models
{
    public class BlogFilter
    {
        private BlogContext db;
        private readonly List<int> empty = new();
        private IEnumerable<int> _whitelistcategories;
        private IEnumerable<int> _whitelistids;
        private IEnumerable<int> _blacklisttags;
        private IEnumerable<int> _blacklistCategories;
        public IEnumerable<int> Whitelistcategories { private get { return _whitelistcategories; } set { _whitelistcategories = value ?? empty; } }
        public IEnumerable<int> Whitelistids { private get { return _whitelistids; } set { _whitelistids = value ?? empty; } }
        public IEnumerable<int> Blacklisttags { private get { return _blacklisttags; } set { _blacklisttags = value ?? empty; } }
        public IEnumerable<int> BlacklistCategories { private get { return _blacklistCategories; } set { _blacklistCategories = value ?? empty; } }

        public BlogFilter(BlogContext db)
        {
            this.db = db;
            _whitelistcategories = _whitelistids = _blacklisttags = empty;
        }

        public IQueryable<Blog> Filter(IQueryable<Blog> blogquery)
        {
            var blacklistCategories = BlacklistCategories.Except(Whitelistcategories);
            
            // Start with the base query
            var query = blogquery;
            
            // Apply whitelist filter if specified
            if (Whitelistcategories.Any())
            {
                query = query.Where(b => Whitelistcategories.Contains(b.CategoryID) || Whitelistids.Contains(b.BlogID));
            }

            if (blacklistCategories.Any()) 
            {
                // Filter out blogs in blacklisted categories
                query = query.Where(b => !blacklistCategories.Contains(b.CategoryID));
            }
            
            if (Blacklisttags.Any())
            {
                // Filter out blogs that have ANY blacklisted tag using subquery
                query = query.Where(b => !db.TagsInBlogs.Any(t => t.BlogID == b.BlogID && Blacklisttags.Contains(t.TagID)));
            }
            
            return query;
        }

        public async Task UpdateDatabase()
        {
            // Get blogs that should be marked as harmony (approved + in whitelist categories/IDs but NOT tagged with blacklist tags)
            var approvedBlogs = db.Blogs.Where(b => b.isApproved == true);
            
            var whitelistedBlogs = approvedBlogs
                .Where(b => Whitelistcategories.Contains(b.CategoryID) || Whitelistids.Contains(b.BlogID))
                .Select(b => b.BlogID);
            
            var blacklistedBlogs = db.TagsInBlogs
                .Where(t => Blacklisttags.Contains(t.TagID))
                .Join(approvedBlogs.Where(b => Whitelistcategories.Contains(b.CategoryID)),
                    t => t.BlogID,
                    b => b.BlogID,
                    (t, b) => b.BlogID)
                .Distinct();
            
            var harmonyBlogIds = whitelistedBlogs.Except(blacklistedBlogs).ToList();
            
            // Update all blogs
            var allBlogs = await db.Blogs.ToListAsync();
            foreach (var blog in allBlogs)
            {
                blog.isHarmony = harmonyBlogIds.Contains(blog.BlogID);
            }
            await db.SaveChangesAsync();
        }
    }
}