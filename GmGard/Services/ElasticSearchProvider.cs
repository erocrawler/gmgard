using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GmGard.Models;
using Nest;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Entity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GmGard.Services
{
    public class ElasticSearchProvider : IRecommendationProvider, ISearchProvider
    {
        public class BlogIndexed
        {
            public int Id { get; set; }
            [Text]
            public string Title { get; set; }
            [Text]
            public string Content { get; set; }
            [Keyword]
            public IEnumerable<string> Tags { get; set; }
            public int CategoryId { get; set; }
            public DateTime CreateDate { get; set; }
            [Keyword]
            public string Author { get; set; }
            public bool IsHarmony { get; set; }
            public bool? IsApproved { get; set; }
            public long BlogVisit { get; set; }
            public int PostCount { get; set; }
            public int Rating { get; set; }
            [Keyword(Index = false)]
            public string ImagePath { get; set; }
            [Boolean(Index = false)]
            public bool IsLocalImg { get; set; }

            public Blog ToBlog() => new Blog
            {
                BlogID = Id,
                BlogTitle = Title,
                Content = Content,
                Author = Author,
                BlogDate = CreateDate,
                CategoryID = CategoryId,
                isHarmony = IsHarmony,
                isApproved = IsApproved,
                BlogVisit = BlogVisit,
                Rating = Rating,
                ImagePath = ImagePath,
                IsLocalImg = IsLocalImg,
            };

            public static BlogIndexed FromBlogTag(Blog b, IEnumerable<string> tags, int postcount)
            {
                return new BlogIndexed
                {
                    Id = b.BlogID,
                    Title = b.BlogTitle,
                    Content = b.Content,
                    Tags = tags,
                    CategoryId = b.CategoryID,
                    CreateDate = b.BlogDate,
                    Author = b.Author,
                    IsHarmony = b.isHarmony,
                    BlogVisit = b.BlogVisit,
                    PostCount = postcount,
                    Rating = b.Rating.GetValueOrDefault(0),
                    IsApproved = b.isApproved,
                    ImagePath = b.ImagePath,
                    IsLocalImg = b.IsLocalImg,
                };
            }
        }

        public class ElasticSearchSettings
        {
            public string EndPoint { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public static ElasticClient CreateClient(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetService<IOptions<ElasticSearchSettings>>();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            ElasticClient elasticClient = null;
            if (!string.IsNullOrEmpty(settings.Value.EndPoint))
            {
                var conn = new ConnectionSettings(new Uri(settings.Value.EndPoint))
                    .DefaultIndex("blogs")
                    .BasicAuthentication(settings.Value.UserName, settings.Value.Password);
                if (env.IsDevelopment())
                {
                    var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
                    conn.DisableDirectStreaming().EnableDebugMode((d) =>
                    {
                        var log = logger.CreateLogger("ES Debug");
                        log.LogInformation(System.Text.Encoding.UTF8.GetString(d.RequestBodyInBytes));
                    });
                }
                elasticClient = new ElasticClient(conn);
            }
            return elasticClient;
        }


        private readonly ElasticClient _client;
        private readonly ILogger _logger;
        private readonly HttpContext _httpContext;
        private readonly BlogContext _db;
        private readonly CategoryUtil _categoryUtil;
        private readonly BlogUtil _blogUtil;
        private readonly DbBlogSearchProvider _dbBlogSearch;

        public ElasticSearchProvider(
            ElasticClient elasticClient,
            ILoggerFactory logger,
            IHttpContextAccessor httpContextAccessor,
            BlogContext blogContext,
            BlogUtil blogUtil,
            CategoryUtil categoryUtil,
            DbBlogSearchProvider dbBlogSearch)
        {
            _client = elasticClient;
            _logger = logger.CreateLogger<ElasticSearchProvider>();
            _httpContext = httpContextAccessor.HttpContext;
            _db = blogContext;
            _blogUtil = blogUtil;
            _categoryUtil = categoryUtil;
            _dbBlogSearch = dbBlogSearch;
            if (!IsValid())
            {
                _logger.LogError("Invalid elastic search client.");
            }
        }

        public async Task<IEnumerable<Blog>> GetRecommendationAsync(Blog blog, IEnumerable<string> tags, int count)
        {
            if (!IsValid())
            {
                return Enumerable.Empty<Blog>();
            }
            var result = await _client.SearchAsync<BlogIndexed>(
                s => s.Query(q => q.Bool(b => b.Should(
                    bs => bs.MoreLikeThis(mlt =>
                        mlt.Like(l => l.Document(d => d.Document(BlogIndexed.FromBlogTag(blog, tags, 0)).Id(null)))
                           .MinTermFrequency(1).Fields(new string[] { "title", "tags" })),
                    bs => bs.Term("categoryId", blog.CategoryID))
                    .MustNot(
                        bs => bs.Ids(i => i.Values(blog.BlogID)),
                        bs => bs.Term("isApproved", false)))).Size(count),
                _httpContext.RequestAborted);
            if (result.IsValid)
            {
                return result.Documents.Select(d => d.ToBlog());
            }
            return Enumerable.Empty<Blog>();
        }

        public bool IsValid()
        {
            return _client != null;
        }

        public async Task<SearchBlogResult> SearchBlogAsync(SearchModel m, int pageNumber, int pageSize)
        {
            SearchBlogResult searchBlogResult = new SearchBlogResult { SearchModel = m };
            if (!IsValid())
            {
                // Fallback
                return await _dbBlogSearch.SearchBlogAsync(m, pageNumber, pageSize);
            }
            double? minScore = null;
            QueryContainer query(QueryContainerDescriptor<BlogIndexed> q)
            {
                var queries = new List<QueryContainer>();
                if (!string.IsNullOrWhiteSpace(m.Title))
                {
                    queries.Add(q.Bool(b => b.MustNot(q.Term("isApproved", false))));
                }
                else
                {
                    queries.Add(q.Term("isApproved", true));
                }
                if (m.StartDate.HasValue)
                {
                    queries.Add(q.DateRange(dr => dr.Field(f => f.CreateDate).GreaterThanOrEquals(m.StartDate)));
                }
                if (m.EndDate.HasValue)
                {
                    var enddate = new DateTime(m.EndDate.Value.Year, m.EndDate.Value.Month, m.EndDate.Value.Day, 23, 59, 59);
                    queries.Add(q.DateRange(dr => dr.Field(f => f.CreateDate).LessThanOrEquals(enddate)));
                }
                if (!string.IsNullOrWhiteSpace(m.Author))
                {
                    queries.Add(q.Term("author", m.Author));
                }
                IEnumerable<int> flatCategories = _categoryUtil.GetCategoryList().Where(c => !c.HideFromHomePage).Select(c => c.CategoryID);
                if (m.CurrentCategory.HasValue)
                {
                    flatCategories = _categoryUtil.GetCategoryWithSubcategories(m.CurrentCategory.Value);
                }
                if (m.CategoryIds != null && m.CategoryIds.Any())
                {
                    var userCategories = m.CategoryIds.Aggregate(new List<int>(), (l, id) => { l.AddRange(_categoryUtil.GetCategoryWithSubcategories(id)); return l; });
                    if (m.CurrentCategory.HasValue)
                    {
                        flatCategories = flatCategories.Intersect(userCategories);
                    }
                    else
                    {
                        flatCategories = userCategories;
                    }
                }
                if (flatCategories.Any())
                {
                    queries.Add(q.Terms(ts => ts.Field("categoryId").Terms(flatCategories)));
                }
                if (!string.IsNullOrWhiteSpace(m.Tags))
                {
                    var tags = TagUtil.SplitTags(m.Tags);
                    var tagQueries = tags.Select(t => q.Bool(b => b.Should(
                        q.Match(mm => mm.Field("tags").Query(t).Operator(Operator.And)),
                        q.Match(mm => mm.Field("tags.ngram_lc").Query(t).Operator(Operator.And))))).ToArray();
                    if (m.TagsMatchAny)
                    {
                        queries.Add(q.Bool(b => b.Should(tagQueries)));
                    }
                    else
                    {
                        queries.Add(q.Bool(b => b.Must(tagQueries)));
                    }
                }
                if (!string.IsNullOrWhiteSpace(m.Title))
                {
                    var titles = m.TitleMatchAny ? m.Title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new[] { m.Title };
                    var field = m.Title.Length > 30 ? "title" : "title.ngram_lc";
                    queries.Add(q.Bool(b => b.Should(
                               titles.Select(t => new QueryContainer(new MatchQuery
                               {
                                   Field = field,
                                   Query = t,
                                   Operator = Operator.And
                               })).ToArray())));
                    minScore = 5;
                }
                if (!string.IsNullOrWhiteSpace(m.Query))
                {
                    queries.Add(q.MultiMatch(mm => mm.Query(m.Query).Operator(Operator.And).Fields(new[] { "tags", "title", "title.ngram_lc", "content" })));
                    minScore = 5;
                }
                if (m.Harmony == true)
                {
                    queries.Add(q.Term("isHarmony", true));
                }
                return q.Bool(bs => bs.Must(queries.ToArray()));
            }
            SortDescriptor<BlogIndexed> selector(SortDescriptor<BlogIndexed> s)
            {
                switch (m.Sort)
                {
                    case "Date":
                        return s.Ascending(b => b.CreateDate);

                    case "Date_desc":
                        return s.Descending(b => b.CreateDate);

                    case "Visit_desc":
                        return s.Descending(q => q.BlogVisit);

                    case "Visit":
                        return s.Ascending(q => q.BlogVisit);

                    case "Post":
                        return s.Ascending(q => q.PostCount);

                    case "Post_desc":
                        return s.Descending(q => q.PostCount);

                    case "Rate":
                        return s.Ascending(q => q.Rating);

                    case "Rate_desc":
                        return s.Descending(q => q.Rating);

                    case "Score":
                        return s.Ascending(SortSpecialField.Score);

                    case "Score_desc":
                        return s.Descending(SortSpecialField.Score);

                    // TODO: AddDate, AddDate_desc for search in favorite.
                    case null:
                    default:
                        if (string.IsNullOrWhiteSpace(m.Query))
                        {
                            return s.Descending(b => b.CreateDate);
                        }
                        return s.Descending(SortSpecialField.Score);
                }
            }
            
            var result = await _client.SearchAsync<BlogIndexed>(s => {
                s = s.Query(query)
                    .TrackTotalHits(true)
                    .MinScore(minScore)
                    .Size(pageSize)
                    .Skip((pageNumber - 1) * pageSize)
                    .Sort(selector);
                if (new[] { m.Tags, m.Title, m.Query }.Any(v => !string.IsNullOrWhiteSpace(v)))
                {
                    s = s.Aggregations(agg => agg
                        .Terms("distinct_tags", tg => tg.Field("tags.kw").Size(10))
                        .Terms("categories", tg => tg.Field(b => b.CategoryId).Size(_categoryUtil.GetCategoryList().Count)));
                }
                return s;
            }, _httpContext.RequestAborted);
            if (result.IsValid)
            {
                searchBlogResult.Blogs = new X.PagedList.StaticPagedList<Blog>(result.Documents.Select(d => d.ToBlog()), pageNumber, pageSize, (int)result.Total);
                if (result.Aggregations.ContainsKey("distinct_tags"))
                {
                    var tags = result.Aggregations.Terms("distinct_tags").Buckets.Select(b => b.Key);
                    searchBlogResult.TagsSearched = await _db.Tags.Where(t => tags.Contains(t.TagName)).ToListAsync();
                }
                if (result.Aggregations.ContainsKey("categories"))
                {
                    var counts = result.Aggregations.Terms<int>("categories").Buckets.ToDictionary(k => k.Key, k => k.DocCount.GetValueOrDefault(0));
                    long CalculateTotalItems(Category c)
                    {
                        if (c == null)
                        {
                            return 0;
                        }
                        counts.TryGetValue(c.CategoryID, out long count);
                        if (c.SubCategories != null)
                        {
                            foreach (var subcat in c.SubCategories)
                            {
                                count += CalculateTotalItems(subcat);
                            }
                            counts[c.CategoryID] = count;
                        }
                        return count;
                    }
                    long total = 0;
                    foreach (var main in _categoryUtil.GetCategoryList().Where(h => !h.ParentCategoryID.HasValue))
                    {
                        total += CalculateTotalItems(main);
                    }
                    counts[0] = total;
                    searchBlogResult.SearchModel.CategoryItemCount = counts.Where(v => v.Value > 0).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }
            }
            else
            {
                _logger.LogError(result.DebugInformation);
                searchBlogResult.Blogs = new X.PagedList.PagedList<Blog>(Enumerable.Empty<Blog>(), pageNumber, pageSize);
            }
            return searchBlogResult;
        }
    }
}
