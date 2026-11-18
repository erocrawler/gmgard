using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Npgsql;

namespace GmGard.Tests
{
    /// <summary>
    /// 控制器测试夹具("迷你启动器"):真实 PostgreSQL(两个临时库,和生产一样分成 Data/User)+ 站里真实的服务类,
    /// 外部依赖(S3、搜索推荐、访问计数)换成假的。直接用 ActivatorUtilities 创建控制器并调用 action。
    /// 注意:直接调用 action 不会执行 [Authorize] 等过滤器——这类特性用反射测试覆盖;这里测的是 action 方法体里的逻辑。
    /// 需要环境变量 GM_TEST_PG(见 ExpUtilPointsTests);没设置时 Available=false,测试应当跳过。
    /// </summary>
    public sealed class TestApp : IDisposable
    {
        public bool Available { get; }
        public IServiceProvider Root { get; }
        private readonly string _adminConn;
        private readonly string _dataDb, _userDb;

        public TestApp()
        {
            string prefix = Environment.GetEnvironmentVariable("GM_TEST_PG");
            if (string.IsNullOrWhiteSpace(prefix)) return;
            var csb = new NpgsqlConnectionStringBuilder(prefix);
            string host = csb.Host ?? "";
            if (host != "127.0.0.1" && !host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"拒绝运行:GM_TEST_PG 的 Host 必须是 127.0.0.1 或 localhost,当前为 '{host}'。");
            _adminConn = csb.ConnectionString;
            string id = Guid.NewGuid().ToString("N");
            _dataDb = "gm_test_data_" + id;
            _userDb = "gm_test_user_" + id;
            csb.Database = _dataDb; string dataCs = csb.ConnectionString;
            csb.Database = _userDb; string userCs = csb.ConnectionString;

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddDbContext<BlogContext>(o => o.UseNpgsql(dataCs));
            services.AddDbContext<UsersContext>(o => o.UseNpgsql(userCs));
            services.AddIdentityCore<UserProfile>().AddRoles<IdentityRole<int>>().AddEntityFrameworkStores<UsersContext>();

            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ApplicationSettings:ListPageSize"] = "20",
                ["ApplicationSettings:ReplyPageSize"] = "20",
                ["ApplicationSettings:MsgPageSize"] = "20",
                ["ApplicationSettings:RateCredentialType"] = "A",
                ["ApplicationSettings:UploadBackendType"] = "Fake",
            }).Build();
            services.AddSingleton<IConfiguration>(config);
            services.Configure<AppSettingsModel>(config.GetSection("ApplicationSettings"));
            services.Configure<DataSettingsModel>(config.GetSection("DataSettings"));

            // 站里真实的服务(与 Startup.cs 一致)
            services.AddSingleton<BackgroundTaskQueue>();
            services.AddScoped<JobTaskRunner>();
            services.AddSingleton<CacheService>();
            services.AddSingleton(_ => HtmlSanitizerService.CreateInstance());
            services.AddScoped<ContextlessBlogUtil>();
            services.AddScoped<BlogUtil>();
            services.AddScoped<AdminUtil>();
            services.AddScoped<CategoryUtil>();
            services.AddScoped<ConstantUtil>();
            services.AddScoped<ExpUtil>();
            services.AddScoped<ImageUtil>();
            services.AddScoped<MessageUtil>();
            services.AddScoped<RatingUtil>();
            services.AddScoped<TagUtil>();
            services.AddScoped<TopicUtil>();
            services.AddScoped<UploadUtil>();
            services.AddScoped<WidgetUtil>();
            services.AddSingleton<QuestService>();
            services.AddSingleton<TitleService>();
            services.AddTransient<HtmlUtil>();
            services.AddTransient<GachaBonusService>();
            services.AddScoped<DbBlogSearchProvider>();
            services.AddScoped<ISearchProvider, DbBlogSearchProvider>();
            services.AddTransient<INickNameProvider, TitleNickNameProvider>();
            // 假的外部依赖
            services.AddScoped<IUpload, FakeUpload>();
            services.AddScoped<IRecommendationProvider, FakeRecommendation>();
            services.AddSingleton<IVisitCounter, FakeVisitCounter>();
            services.AddScoped<IUrlHelper>(_ => new FakeUrlHelper());

            Root = services.BuildServiceProvider(validateScopes: false);

            using var scope = Root.CreateScope();
            scope.ServiceProvider.GetRequiredService<BlogContext>().Database.EnsureCreated();
            scope.ServiceProvider.GetRequiredService<UsersContext>().Database.EnsureCreated();
            Available = true;
        }

        /// <summary>每次调用创建一个独立的请求作用域(新的 DbContext)。</summary>
        public IServiceScope NewScope() => Root.CreateScope();

        /// <summary>创建控制器并设置当前用户(username 为 null 表示匿名)。roles 例如 "Administrator"、"Moderator"。</summary>
        public T Controller<T>(string username, params string[] roles) where T : ControllerBase
        {
            var scope = Root.CreateScope();
            var sp = scope.ServiceProvider;
            var ctx = new DefaultHttpContext { RequestServices = sp };
            ctx.Session = new MemorySession();
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            ctx.Request.Form = new FormCollection(new Dictionary<string, StringValues>(), new FormFileCollection());
            if (username != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test", ClaimTypes.Name, ClaimTypes.Role));
            }
            sp.GetRequiredService<IHttpContextAccessor>().HttpContext = ctx;
            var c = ActivatorUtilities.CreateInstance<T>(sp);
            c.ControllerContext = new ControllerContext(new ActionContext(ctx, new RouteData(), new ControllerActionDescriptor()));
            if (c is Controller mvc)
            {
                mvc.Url = new FakeUrlHelper();
                mvc.TempData = new TempDataDictionary(ctx, new NullTempDataProvider());
            }
            else
            {
                c.Url = new FakeUrlHelper();
            }
            return c;
        }

        // ---- 种子数据与查询辅助 ----
        public int SeedUser(string name, int points = 100, int level = 1)
        {
            using var s = NewScope();
            var udb = s.ServiceProvider.GetRequiredService<UsersContext>();
            var u = new UserProfile
            {
                UserName = name, NormalizedUserName = name.ToUpperInvariant(),
                Email = name + "@t.test", NormalizedEmail = (name + "@t.test").ToUpperInvariant(),
                Points = points, Level = level,
                CreateDate = DateTime.Now.AddYears(-1), LastLoginDate = DateTime.Now,
                SecurityStamp = Guid.NewGuid().ToString("N"),
            };
            udb.Users.Add(u);
            udb.SaveChanges();
            return u.Id;
        }

        public int SeedCategory(bool linkOptional = false, string name = null)
        {
            using var s = NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            var c = new Category { CategoryName = name ?? ("cat" + Guid.NewGuid().ToString("N").Substring(0, 8)), LinkOptional = linkOptional };
            db.Categories.Add(c);
            db.SaveChanges();
            return c.CategoryID;
        }

        public int SeedBlog(string author, int categoryId, string title = "原标题", string content = "<p>原始正文内容,足够长。</p><img src=\"http://example.com/cover.jpg\" />", bool? approved = true, string linksJson = null)
        {
            using var s = NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            var b = new Blog
            {
                BlogTitle = title, Content = content, Author = author, CategoryID = categoryId,
                BlogDate = DateTime.Now, isApproved = approved, Links = linksJson,
            };
            db.Blogs.Add(b);
            db.SaveChanges();
            return b.BlogID;
        }

        /// <summary>用全新的上下文读数据(避免读到测试里控制器上下文的缓存)。</summary>
        public T QueryData<T>(Func<BlogContext, T> f) { using var s = NewScope(); return f(s.ServiceProvider.GetRequiredService<BlogContext>()); }
        public T QueryUsers<T>(Func<UsersContext, T> f) { using var s = NewScope(); return f(s.ServiceProvider.GetRequiredService<UsersContext>()); }

        public void Dispose()
        {
            if (!Available) return;
            // 不要释放 Root:站里有静态事件(OnEditBlog 等),上一个夹具的服务实例仍挂在上面,
            // 释放它们的 MemoryCache 会让后面测试触发事件时抛 ObjectDisposedException。生产环境只有一个实例,没有这个问题。
            NpgsqlConnection.ClearAllPools();
            using var conn = new NpgsqlConnection(_adminConn);
            conn.Open();
            foreach (var db in new[] { _dataDb, _userDb })
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"DROP DATABASE IF EXISTS \"{db}\" WITH (FORCE)";
                cmd.ExecuteNonQuery();
            }
        }
    }

    public class FakeUpload : IUpload
    {
        public string BucketName => "test-bucket";
        public Task<bool> PutObjectAsync(string filepath, string contentType, Stream data) => Task.FromResult(true);
        public Task DeleteObjectsAsync(List<string> filepath) => Task.CompletedTask;
        public Task<Stream> GetObjectAsync(string filepath) => Task.FromResult<Stream>(new MemoryStream());
        public void Dispose() { }
    }

    public class FakeRecommendation : IRecommendationProvider
    {
        public bool IsValid() => false;
        public Task<IEnumerable<Blog>> GetRecommendationAsync(Blog blog, IEnumerable<string> tags, int count) => Task.FromResult<IEnumerable<Blog>>(new List<Blog>());
    }

    public class FakeVisitCounter : IVisitCounter
    {
        public long GetBlogVisit(int id, bool increment = false) => 0;
        public long GetTopicVisit(int id, bool increment = false) => 0;
        public long GetBountyVisit(int id, bool increment = false) => 0;
        public void PrepareBlogVisits(IEnumerable<int> ids) { }
        public void PrepareBountyVisits(IEnumerable<int> ids) { }
        public Task SaveVisits() => Task.CompletedTask;
    }

    public class FakeUrlHelper : IUrlHelper
    {
        public ActionContext ActionContext => null;
        public string Action(UrlActionContext c) => "/" + c.Controller + "/" + c.Action + (c.Values == null ? "" : "?" + string.Join("&", c.Values.GetType().GetProperties().Select(p => p.Name + "=" + p.GetValue(c.Values))));
        public string Content(string contentPath) => contentPath;
        public bool IsLocalUrl(string url) => url != null && url.StartsWith("/");
        public string RouteUrl(UrlRouteContext routeContext) => "/route";
        public string Link(string routeName, object values) => "/link";
    }

    public class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }

    public class MemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> _d = new Dictionary<string, byte[]>();
        public bool IsAvailable => true;
        public string Id { get; } = Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _d.Keys;
        public void Clear() => _d.Clear();
        public Task CommitAsync(System.Threading.CancellationToken ct = default) => Task.CompletedTask;
        public Task LoadAsync(System.Threading.CancellationToken ct = default) => Task.CompletedTask;
        public void Remove(string key) => _d.Remove(key);
        public void Set(string key, byte[] value) => _d[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _d.TryGetValue(key, out value);
    }
}
