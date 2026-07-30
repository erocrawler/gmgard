using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using GmGard.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GmGard.Services
{
    public class ElasticSearchProvider : IRecommendationProvider, ISearchProvider
    {
        public class BlogIndexed
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Content { get; set; }
            public IEnumerable<string> Tags { get; set; }
            public int CategoryId { get; set; }
            public DateTime CreateDate { get; set; }
            public string Author { get; set; }
            public bool IsHarmony { get; set; }
            public bool? IsApproved { get; set; }
            public long BlogVisit { get; set; }
            public int PostCount { get; set; }
            public int Rating { get; set; }
            public string ImagePath { get; set; }
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

        // Separate doc type for bounties - just title, question content, and answer content per FR
        public class BountyIndexed
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty; // question body, stripped of HTML?
            public string AnswerContent { get; set; } = string.Empty; // concatenated answers
            public string Author { get; set; } = string.Empty;
            public DateTime CreateDate { get; set; }
            public bool IsDeleted { get; set; }
            public bool IsAccepted { get; set; }
            public int Prize { get; set; }
            public int AnswerCount { get; set; }

            private static string StripHtml(string html)
            {
                if (string.IsNullOrEmpty(html)) return "";
                // quick strip - full sanitizer not needed for index
                var plain = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
                plain = System.Net.WebUtility.HtmlDecode(plain);
                return plain;
            }

            public static BountyIndexed FromBounty(Bounty bounty, IEnumerable<string>? answerContents = null)
            {
                var aContents = answerContents != null ? string.Join("\n", answerContents.Where(s => !string.IsNullOrWhiteSpace(s)).Select(StripHtml)) : "";
                return new BountyIndexed
                {
                    Id = bounty.BountyId,
                    Title = StripHtml(bounty.Title ?? ""),
                    Content = StripHtml(bounty.Content ?? ""),
                    AnswerContent = aContents,
                    Author = bounty.Author ?? "",
                    CreateDate = bounty.CreateDate,
                    IsDeleted = bounty.IsDeleted,
                    IsAccepted = bounty.IsAccepted,
                    Prize = bounty.Prize,
                    AnswerCount = bounty.Answers?.Count ?? (answerContents?.Count() ?? 0)
                };
            }

            public static BountyIndexed FromBountyWithAnswers(Bounty bounty, IEnumerable<Answer> answers)
            {
                return FromBounty(bounty, answers?.Select(a => a.Content));
            }
        }

        public class ElasticSearchSettings
        {
            public string EndPoint { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public static ElasticsearchClient CreateClient(IServiceProvider serviceProvider)
        {
            var settings = serviceProvider.GetService<IOptions<ElasticSearchSettings>>();
            var env = serviceProvider.GetRequiredService<IWebHostEnvironment>();
            ElasticsearchClient elasticClient = null;
            if (!string.IsNullOrEmpty(settings.Value.EndPoint))
            {
                var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Value.EndPoint))
                    .DefaultIndex("blogs")
                    .Authentication(new BasicAuthentication(settings.Value.UserName, settings.Value.Password))
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
                // No direct EnableDebugMode / DisableDirectStreaming in 9.x settings. Keep basic.
                elasticClient = new ElasticsearchClient(clientSettings);
            }
            return elasticClient;
        }

        private readonly ElasticsearchClient _client;
        private readonly ILogger _logger;
        private readonly HttpContext _httpContext;
        private readonly BlogContext _db;
        private readonly CategoryUtil _categoryUtil;
        private readonly BlogUtil _blogUtil;
        private readonly DbBlogSearchProvider _dbBlogSearch;

        public ElasticSearchProvider(
            ElasticsearchClient elasticClient,
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
            try
            {
                var tagList = tags?.ToList() ?? new List<string>();
                var likeTexts = new List<Like>();
                if (!string.IsNullOrWhiteSpace(blog.BlogTitle))
                {
                    likeTexts.Add(new Like(blog.BlogTitle));
                }
                if (tagList.Any())
                {
                    likeTexts.Add(new Like(string.Join(" ", tagList)));
                    // Also add combined title + tags as a like item for better relevance
                    if (!string.IsNullOrWhiteSpace(blog.BlogTitle))
                        likeTexts.Add(new Like(blog.BlogTitle + " " + string.Join(" ", tagList)));
                }
                if (!likeTexts.Any())
                {
                    likeTexts.Add(new Like(blog.BlogTitle ?? ""));
                }

                var result = await _client.SearchAsync<BlogIndexed>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                sh => sh.MoreLikeThis(m =>
                                {
                                    m.Fields(new[] { "title", "tags" });
                                    m.Like(likeTexts);
                                    m.MinTermFreq(1);
                                }),
                                sh => sh.Term(t => t.Field("categoryId").Value(blog.CategoryID))
                            )
                            .MustNot(
                                mn => mn.Ids(i => i.Values(new Ids(new[] { blog.BlogID.ToString() }))),
                                mn => mn.Term(t => t.Field("isApproved").Value(false))
                            )
                        )
                    )
                    .Size(count),
                    _httpContext?.RequestAborted ?? default);

                if (result.IsValidResponse)
                {
                    return result.Documents.Select(d => d.ToBlog());
                }
                else
                {
                    _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason ?? "ES recommendation failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetRecommendationAsync failed");
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
                return await _dbBlogSearch.SearchBlogAsync(m, pageNumber, pageSize);
            }

            double? minScore = null;
            var mustQueries = new List<Query>();

            // isApproved logic
            if (!string.IsNullOrWhiteSpace(m.Title))
            {
                // when searching title, include non-approved? original added MustNot false
                mustQueries.Add(new BoolQuery { MustNot = new Query[] { new TermQuery { Field = "isApproved", Value = false } } });
            }
            else
            {
                mustQueries.Add(new TermQuery { Field = "isApproved", Value = true });
            }

            if (m.StartDate.HasValue)
            {
                mustQueries.Add(new DateRangeQuery
                {
                    Field = "createDate",
                    Gte = m.StartDate.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                });
            }
            if (m.EndDate.HasValue)
            {
                var enddate = new DateTime(m.EndDate.Value.Year, m.EndDate.Value.Month, m.EndDate.Value.Day, 23, 59, 59);
                mustQueries.Add(new DateRangeQuery
                {
                    Field = "createDate",
                    Lte = enddate.ToString("yyyy-MM-ddTHH:mm:ss")
                });
            }
            if (!string.IsNullOrWhiteSpace(m.Author))
            {
                mustQueries.Add(new TermQuery { Field = "author", Value = m.Author });
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
                var values = flatCategories.Select(c => FieldValue.Long(c)).ToArray();
                mustQueries.Add(new TermsQuery
                {
                    Field = "categoryId",
                    Terms = new TermsQueryField(values)
                });
            }

            if (!string.IsNullOrWhiteSpace(m.Tags))
            {
                var tags = TagUtil.SplitTags(m.Tags);
                var tagBoolQueries = tags.Select(t =>
                    (Query)new BoolQuery
                    {
                        Should = new Query[]
                        {
                            new MatchQuery { Field = "tags", Query = t, Operator = Operator.And },
                            new MatchQuery { Field = "tags.ngram_lc", Query = t, Operator = Operator.And }
                        }
                    }).ToArray();

                if (m.TagsMatchAny)
                {
                    mustQueries.Add(new BoolQuery { Should = tagBoolQueries });
                }
                else
                {
                    mustQueries.Add(new BoolQuery { Must = tagBoolQueries });
                }
            }

            if (!string.IsNullOrWhiteSpace(m.Title))
            {
                var titles = m.TitleMatchAny ? m.Title.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) : new[] { m.Title };
                var field = m.Title.Length > 30 ? "title" : "title.ngram_lc";
                var titleShould = titles.Select(t => (Query)new MatchQuery
                {
                    Field = field,
                    Query = t,
                    Operator = Operator.And
                }).ToArray();
                mustQueries.Add(new BoolQuery { Should = titleShould });
                minScore = 5;
            }

            if (!string.IsNullOrWhiteSpace(m.Query))
            {
                mustQueries.Add(new MultiMatchQuery
                {
                    Query = m.Query,
                    Operator = Operator.And,
                    Fields = new[] { "tags", "title", "title.ngram_lc", "content" }
                });
                minScore = 5;
            }

            if (m.Harmony == true)
            {
                mustQueries.Add(new TermQuery { Field = "isHarmony", Value = true });
            }

            var finalQuery = new BoolQuery
            {
                Must = mustQueries
            };

            try
            {
                var searchRequest = new Action<SearchRequestDescriptor<BlogIndexed>>(s =>
                {
                    s.Query(finalQuery)
                        .TrackTotalHits(new TrackHits(true))
                        .From((pageNumber - 1) * pageSize)
                        .Size(pageSize);

                    if (minScore.HasValue)
                        s.MinScore((float)minScore.Value);

                    // Sorting
                    s.Sort(sort =>
                    {
                        switch (m.Sort)
                        {
                            case "Date":
                                sort.Field(f => f.Field("createDate").Order(SortOrder.Asc));
                                break;
                            case "Date_desc":
                                sort.Field(f => f.Field("createDate").Order(SortOrder.Desc));
                                break;
                            case "Visit_desc":
                                sort.Field(f => f.Field("blogVisit").Order(SortOrder.Desc));
                                break;
                            case "Visit":
                                sort.Field(f => f.Field("blogVisit").Order(SortOrder.Asc));
                                break;
                            case "Post":
                                sort.Field(f => f.Field("postCount").Order(SortOrder.Asc));
                                break;
                            case "Post_desc":
                                sort.Field(f => f.Field("postCount").Order(SortOrder.Desc));
                                break;
                            case "Rate":
                                sort.Field(f => f.Field("rating").Order(SortOrder.Asc));
                                break;
                            case "Rate_desc":
                                sort.Field(f => f.Field("rating").Order(SortOrder.Desc));
                                break;
                            case "Score":
                                sort.Score(sc => sc.Order(SortOrder.Asc));
                                break;
                            case "Score_desc":
                                sort.Score(sc => sc.Order(SortOrder.Desc));
                                break;
                            default:
                                if (string.IsNullOrWhiteSpace(m.Query))
                                    sort.Field(f => f.Field("createDate").Order(SortOrder.Desc));
                                else
                                    sort.Score(sc => sc.Order(SortOrder.Desc));
                                break;
                        }
                    });

                    if (new[] { m.Tags, m.Title, m.Query }.Any(v => !string.IsNullOrWhiteSpace(v)))
                    {
                        s.Aggregations(aggs => aggs
                            .Add("distinct_tags", agg => agg.Terms(t => t.Field("tags.kw").Size(10)))
                            .Add("categories", agg => agg.Terms(t => t.Field("categoryId").Size(_categoryUtil.GetCategoryList().Count)))
                        );
                    }
                });

                var result = await _client.SearchAsync<BlogIndexed>(searchRequest, _httpContext?.RequestAborted ?? default);

                if (result.IsValidResponse)
                {
                    searchBlogResult.Blogs = new X.PagedList.StaticPagedList<Blog>(result.Documents.Select(d => d.ToBlog()), pageNumber, pageSize, (int)result.Total);

                    if (result.Aggregations != null)
                    {
                        if (result.Aggregations.TryGetAggregate("distinct_tags", out StringTermsAggregate tagAgg) && tagAgg != null)
                        {
                            var tagKeys = tagAgg.Buckets.Select(b =>
                            {
                                // Key is FieldValue
                                if (b.Key.TryGetString(out var str)) return str;
                                return b.Key.ToString();
                            }).Where(k => !string.IsNullOrEmpty(k)).ToList();

                            if (tagKeys.Any())
                            {
                                searchBlogResult.TagsSearched = await _db.Tags.Where(t => tagKeys.Contains(t.TagName)).ToListAsync();
                            }
                        }

                        if (result.Aggregations.TryGetAggregate("categories", out LongTermsAggregate catAgg) && catAgg != null)
                        {
                            var counts = catAgg.Buckets.ToDictionary(k => (int)k.Key, k => k.DocCount);

                            long CalculateTotalItems(Category c)
                            {
                                if (c == null) return 0;
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
                        else if (result.Aggregations.TryGetAggregate("categories", out StringTermsAggregate catStrAgg) && catStrAgg != null)
                        {
                            // Fallback if categories stored as string
                            var counts = new Dictionary<int, long>();
                            foreach (var b in catStrAgg.Buckets)
                            {
                                if (b.Key.TryGetString(out var s) && int.TryParse(s, out var key))
                                {
                                    counts[key] = b.DocCount;
                                }
                                else if (b.Key.TryGetLong(out var l))
                                {
                                    counts[(int)l] = b.DocCount;
                                }
                            }

                            long CalculateTotalItems(Category c)
                            {
                                if (c == null) return 0;
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
                }
                else
                {
                    _logger.LogError(result.DebugInformation ?? result.ElasticsearchServerError?.Error?.Reason ?? "Search failed");
                    searchBlogResult.Blogs = new X.PagedList.PagedList<Blog>(Enumerable.Empty<Blog>(), pageNumber, pageSize);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchBlogAsync exception");
                searchBlogResult.Blogs = new X.PagedList.PagedList<Blog>(Enumerable.Empty<Blog>(), pageNumber, pageSize);
            }

            return searchBlogResult;
        }

        // Bounty search - separate index "bounties"
        public class BountySearchResult
        {
            public List<int> Ids { get; set; } = new();
            public long Total { get; set; }
        }

        public async Task<BountySearchResult> SearchBountyAsync(string query, int pageNumber, int pageSize)
        {
            var result = new BountySearchResult();
            if (!IsValid() || string.IsNullOrWhiteSpace(query))
                return result;

            try
            {
                var q = query.Trim();
                var esResult = await _client.SearchAsync<BountyIndexed>(s => s
                    .Indices("bounties")
                    .Query(qd => qd
                        .Bool(b => b
                            .MustNot(mn => mn.Term(t => t.Field("isDeleted").Value(true)))
                            .Must(m => m
                                .MultiMatch(mm => mm
                                    .Query(q)
                                    .Fields(new[] { "title^3", "title.ngram_lc^2", "content", "answerContent" })
                                    .Operator(Operator.Or)
                                )
                            )
                        )
                    )
                    .TrackTotalHits(new TrackHits(true))
                    .From((pageNumber - 1) * pageSize)
                    .Size(pageSize)
                    .Sort(sort => sort.Score(sc => sc.Order(SortOrder.Desc)).Field(f => f.Field("createDate").Order(SortOrder.Desc))),
                    _httpContext?.RequestAborted ?? default);

                if (esResult.IsValidResponse)
                {
                    result.Ids = esResult.Hits.Select(h => h.Source?.Id ?? 0).Where(id => id != 0).ToList();
                    // fallback if Source null but Id parsed from hit
                    if (result.Ids.Count == 0)
                    {
                        result.Ids = esResult.Hits.Select(h => int.TryParse(h.Id, out var i) ? i : 0).Where(i => i != 0).ToList();
                    }
                    result.Total = esResult.Total;
                }
                else
                {
                    _logger.LogError(esResult.DebugInformation ?? esResult.ElasticsearchServerError?.Error?.Reason ?? "Bounty search failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchBountyAsync exception");
            }
            return result;
        }
    }
}
