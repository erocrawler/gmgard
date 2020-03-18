using System.Collections.Generic;
using System.Linq;

namespace GmGard.Models
{
    public class BlogFilter
    {
        private BlogContext db;
        private readonly List<int> empty = new List<int>();
        private List<int> _whitelistcategories;
        private List<int> _whitelistids;
        private List<int> _blacklisttags;
        public List<int> Whitelistcategories { private get { return _whitelistcategories; } set { _whitelistcategories = value ?? empty; } }
        public List<int> Whitelistids { private get { return _whitelistids; } set { _whitelistids = value ?? empty; } }
        public List<int> Blacklisttags { private get { return _blacklisttags; } set { _blacklisttags = value ?? empty; } }

        public BlogFilter(BlogContext db)
        {
            this.db = db;
            _whitelistcategories = _whitelistids = _blacklisttags = empty;
        }

        public IQueryable<Blog> Filter(IQueryable<Blog> blogquery)
        {
            var query = blogquery.GroupJoin(db.TagsInBlogs, b => b.BlogID, t => t.BlogID, (b, t) => new { blog = b, tag = t }).SelectMany(bt => bt.tag.DefaultIfEmpty(), (b, t) => new { blog = b.blog, tag = t.TagID });
            if (Whitelistcategories.Count != 0)
            {
                query = query.Where(a => Whitelistcategories.Contains(a.blog.CategoryID) || Whitelistids.Contains(a.blog.BlogID));
            }
            return query.Select(a => a.blog).Except(query.Where(a => Blacklisttags.Contains(a.tag)).Select(a => a.blog)).Distinct();
        }

        public void UpdateDatabase()
        {
            db.Database.ExecuteSqlCommand(string.Format(@"Update Blogs SET isHarmony = CASE WHEN BlogID IN (
            SELECT
                [Except1].[BlogID]

                FROM  (SELECT
                    [Extent1].[BlogID] AS [BlogID]
                    FROM  [dbo].[Blogs] AS [Extent1]
                    LEFT OUTER JOIN [dbo].[TagsInBlogs] AS [Extent2] ON [Extent1].[BlogID] = [Extent2].[BlogID]
                    WHERE (1 = [Extent1].[isApproved]) AND (([Extent1].[CategoryID] IN ({0}) OR [Extent1].[BlogID] IN ({2})))
                EXCEPT
                    SELECT
                    [Extent3].[BlogID] AS [BlogID]
                    FROM  [dbo].[Blogs] AS [Extent3]
                    INNER JOIN [dbo].[TagsInBlogs] AS [Extent4] ON [Extent3].[BlogID] = [Extent4].[BlogID]
                    WHERE (1 = [Extent3].[isApproved]) AND ((CASE WHEN ([Extent3].[CategoryID] IN ({0})) THEN cast(1 as bit) ELSE cast(0 as bit) END) = 1) AND ([Extent4].[TagID] IN ({1}))) AS [Except1]
		            )
            THEN 1 ELSE 0 END", Whitelistcategories.Count > 0 ? string.Join(",", Whitelistcategories) : "''", Blacklisttags.Count > 0 ? string.Join(",", Blacklisttags) : "''", Whitelistids.Count > 0 ? string.Join(",", Whitelistids) : "''"));
        }
    }
}