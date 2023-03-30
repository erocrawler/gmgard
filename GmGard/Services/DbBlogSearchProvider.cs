using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Extensions;
using GmGard.Models;
using LinqKit;

namespace GmGard.Services
{
    public class DbBlogSearchProvider : ISearchProvider
    {
        private readonly BlogContext _db;
        private readonly CategoryUtil _categoryUtil;

        public DbBlogSearchProvider(BlogContext blogContext, CategoryUtil categoryUtil)
        {
            _db = blogContext;
            _categoryUtil = categoryUtil;
        }

        public async Task<SearchBlogResult> SearchBlogAsync(SearchModel m, int pageNumber, int pageSize)
        {
            SearchBlogResult result = new SearchBlogResult { SearchModel = m };
            var predicate = PredicateBuilder.New<Blog>(true);
            if (!string.IsNullOrWhiteSpace(m.FavUser))
            {
                predicate = predicate.And(b => _db.Favorites.Where(f => f.Username == m.FavUser).Any(f => f.BlogID == b.BlogID));
            }
            // If searching by title, include isApproved == null
            if (!string.IsNullOrWhiteSpace(m.Title))
            {
                predicate = predicate.And(b => b.isApproved != false);
            }
            else
            {
                predicate = predicate.And(b => b.isApproved == true);
            }
            if (m.StartDate.HasValue)
            {
                predicate = predicate.And(b => b.BlogDate >= m.StartDate.Value);
            }
            if (m.EndDate.HasValue)
            {
                var enddate = new DateTime(m.EndDate.Value.Year, m.EndDate.Value.Month, m.EndDate.Value.Day, 23, 59, 59);
                predicate = predicate.And(b => b.BlogDate <= enddate);
            }
            if (!string.IsNullOrWhiteSpace(m.Author))
            {
                predicate = predicate.And(b => b.Author == m.Author);
            }
            IEnumerable<int> flatCategories = _categoryUtil.GetCategoryList().Where(c => !c.HideFromHomePage).Select(c => c.CategoryID);
            if (m.CurrentCategory.HasValue)
            {
                flatCategories = _categoryUtil.GetCategoryWithSubcategories(m.CurrentCategory.Value);
                predicate = predicate.And(b => flatCategories.Contains(b.CategoryID));
            }
            if (m.CategoryIds != null && m.CategoryIds.Count() > 0)
            {
                flatCategories = m.CategoryIds.Aggregate(new List<int>(), (l, id) => { l.AddRange(_categoryUtil.GetCategoryWithSubcategories(id)); return l; });
                predicate = predicate.And(b => flatCategories.Contains(b.CategoryID));
            }
            if (!string.IsNullOrWhiteSpace(m.Tags))
            {
                var tags = TagUtil.SplitTags(m.Tags);
                IQueryable<TagsInBlog> tagsInBlog;
                if (!m.TagsMatchAny)
                {
                    tagsInBlog = tags.Aggregate(_db.TagsInBlogs.AsExpandable(), (r, name) => r.Join(
                        _db.TagsInBlogs.Where(tt => tt.tag.TagName.ToLower().Contains(name.ToLower())),
                        rr => rr.BlogID, t => t.BlogID, (rr, t) => rr));
                }
                else
                {
                    tagsInBlog = tags.Aggregate(_db.TagsInBlogs.AsExpandable().Where(_ => false), (r, name) => r.Union(
                        _db.TagsInBlogs.AsExpandable().Where(tt => tt.tag.TagName.ToLower().Contains(name.ToLower()))).Distinct());
                }
                var tagResult = await tagsInBlog.Select(tib => new { blogid = tib.BlogID, tag = tib.tag }).Distinct().ToListAsync();
                if (!m.TagsMatchAny)
                {
                    result.TagsSearched = tagResult.Where(tib =>
                        tags.Any(name =>
                            tib.tag.TagName.ToSingleByteCharacterString().ToLower().Contains(
                                name.ToSingleByteCharacterString().ToLower())))
                        .Select(tib => tib.tag).Distinct();
                }
                else
                {
                    result.TagsSearched = tagResult.Select(tib => tib.tag).Distinct();
                }
                var blogIds = tagResult.Select(tib => tib.blogid).Distinct();
                predicate = predicate.And(b => blogIds.Contains(b.BlogID));
            }
            if (!string.IsNullOrWhiteSpace(m.Title))
            {
                var keywords = m.Title.Replace('(', ' ').Replace(')', ' ').Replace('"', ' ').Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                // var search = _db.ContainsSearchBlog(string.Join(m.TitleMatchAny ? " OR " : " AND ", keywords.Select(s => '"' + s + '"')));
                var TitlePredicate = PredicateBuilder.New<Blog>(true);
                if (!m.TitleMatchAny)
                {
                    TitlePredicate = TitlePredicate.And(keywords.Aggregate(PredicateBuilder.New<Blog>(true), (p, word) => p.And(b => b.BlogTitle.Contains(word))));
                }
                else
                {
                    TitlePredicate = TitlePredicate.And(keywords.Aggregate(PredicateBuilder.New<Blog>(false), (p, word) => p.Or(b => b.BlogTitle.Contains(word))));
                }
                // TitlePredicate = TitlePredicate.Or(b => search.Count(r => r.BlogID == b.BlogID) > 0);
                predicate = predicate.And(TitlePredicate);
            }
            if (m.Harmony.HasValue)
            {
                predicate = predicate.And(b => b.isHarmony == m.Harmony.Value);
            }
            result.Blogs = await BlogHelper.getSortedQuery(_db, _db.Blogs.AsExpandable().Where(predicate), m.Sort).ToPagedListAsync(pageNumber, pageSize);
            return result;
        }
    }
}
