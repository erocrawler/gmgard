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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
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
            services.Configure<SiteConfig>(o =>
            {
                var site = new SiteInfo { Name = "test", Host = "gmgard.com", StaticHost = "static.gmgard.com", AppHost = "app.gmgard.com", Title = "test", Desc = "test", Logo = "" };
                o.Sites = new List<SiteInfo> { site };
                o.DevSite = site;
                o.AppHostOrigins = new string[0];
                o.DevAppHostOrigins = new string[0];
                o.SmileyPaths = new string[0];
            });
            services.Configure<GmGard.Models.App.BountyConfig>(config.GetSection("BountyConfig"));   // 缺省值 = 生产默认值

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
            services.AddSingleton<IWebHostEnvironment>(new FakeWebHostEnvironment());

            Root = services.BuildServiceProvider(validateScopes: false);

            using var scope = Root.CreateScope();
            scope.ServiceProvider.GetRequiredService<BlogContext>().Database.EnsureCreated();
            var udbInit = scope.ServiceProvider.GetRequiredService<UsersContext>();
            udbInit.Database.EnsureCreated();
            // UserQuest.Title 外键默认指向 TitleID=0(生产库里有这条默认称号)
            udbInit.Database.ExecuteSqlRaw("INSERT INTO \"TitleConfigs\" (\"TitleID\", \"TitleName\") SELECT 0, 'default' WHERE NOT EXISTS (SELECT 1 FROM \"TitleConfigs\" WHERE \"TitleID\" = 0)");
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
            // 默认按"空表单"处理(BlogController.Create 会读 Request.Form.Files)。不要直接给 Request.Form 赋值:
            // 那样 HasFormContentType 会永远为 true,JSON 请求就被当成表单。需要 JSON 的测试自己设 ContentType 和 Body。
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            ctx.Request.Body = new MemoryStream();
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

        /// <summary>直接写入一条求物悬赏(绕过 Create,用于构造各种状态)。closed=true 表示已 Close(CloseDate 有值)。</summary>
        public int SeedBounty(string author, int prize = 40, int deposit = 40, int helpfulReward = 20, bool closed = false, bool accepted = false, bool deleted = false)
        {
            using var s = NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            var b = new Bounty
            {
                Title = "求物", Content = "<p>求一个东西,内容足够长。</p>", Author = author, ImageUrls = "[]",
                CreateDate = DateTime.Now, IsDeleted = deleted, Prize = prize, Deposit = deposit, HelpfulReward = helpfulReward,
                IsAccepted = accepted, ViewCount = 0, ExpiresAt = DateTime.Now.AddDays(30),
                CloseDate = closed ? DateTime.Now : (DateTime?)null, Answers = new List<Answer>(),
            };
            db.Bounties.Add(b);
            db.SaveChanges();
            return b.BountyId;
        }

        /// <summary>直接写入一条悬赏回答。</summary>
        public int SeedAnswer(int bountyId, string author, string content = "<p>这是回答内容</p>")
        {
            using var s = NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            var a = new Answer { BountyId = bountyId, Author = author, Content = content, ImageUrl = "[]", CreateDate = DateTime.Now };
            db.Answers.Add(a);
            db.SaveChanges();
            return a.AnswerId;
        }

        /// <summary>直接写入一条评论/留言帖(ItemType.Blog 且 itemId 为 0/-1 时即意见反馈/问题举报留言板)。</summary>
        public int SeedPost(ItemType type, int itemId, string author, string content = "<p>评论内容</p>")
        {
            using var s = NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            var p = new Post { Author = author, Content = content, PostDate = DateTime.Now, IdType = type, ItemId = itemId, Rating = 0 };
            db.Posts.Add(p);
            db.SaveChanges();
            return p.PostId;
        }

        /// <summary>为已存在的用户写入任务记录(签到补签券等)。</summary>
        public void SeedQuest(string username, int punchInTickets = 0)
        {
            using var s = NewScope();
            var udb = s.ServiceProvider.GetRequiredService<UsersContext>();
            int uid = udb.Users.Single(u => u.UserName == username).Id;
            // UserQuest.Title 外键默认指向 TitleID=0(生产库里有这条默认称号)
            udb.Database.ExecuteSqlRaw("INSERT INTO \"TitleConfigs\" (\"TitleID\", \"TitleName\") SELECT 0, 'default' WHERE NOT EXISTS (SELECT 1 FROM \"TitleConfigs\" WHERE \"TitleID\" = 0)");
            udb.UserQuests.Add(new UserQuest { UserId = uid, PunchInTicket = punchInTickets });
            udb.SaveChanges();
        }

        /// <summary>直接写入一个抽奖活动配置,返回 Id。</summary>
        public int SeedRaffleConfig(int cost, DateTime start, DateTime end)
        {
            using var s = NewScope();
            var udb = s.ServiceProvider.GetRequiredService<UsersContext>();
            var cfg = new RaffleConfig { Title = "测试抽奖", EventStart = start, EventEnd = end, RaffleCost = cost, Image = "" };
            udb.RaffleConfigs.Add(cfg);
            udb.SaveChanges();
            return cfg.Id;
        }

        /// <summary>
        /// 与 Controller&lt;T&gt; 相同,但额外带上 NameIdentifier 声明(值为用户 Id)。
        /// UserManager.GetUserAsync(User) 依赖该声明;需要它的控制器(如 RaffleController)用这个方法创建。用户必须已存在。
        /// </summary>
        public T ControllerWithId<T>(string username, params string[] roles) where T : ControllerBase
        {
            var c = Controller<T>(username, roles);
            if (username != null)
            {
                int uid = QueryUsers(db => db.Users.Single(u => u.UserName == username).Id);
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username), new Claim(ClaimTypes.NameIdentifier, uid.ToString()) };
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
                c.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test", ClaimTypes.Name, ClaimTypes.Role));
            }
            return c;
        }

        /// <summary>
        /// 与 Controller&lt;T&gt; 相同的上下文设置,但由调用方用 factory 显式构造控制器。
        /// 用于构造函数里有夹具未注册依赖(如 SignInManager、EmailSender)的控制器:factory 传 null 占位,被测 action 不用到它们。
        /// </summary>
        public T ControllerWith<T>(string username, System.Func<IServiceProvider, T> factory, params string[] roles) where T : Controller
        {
            var scope = Root.CreateScope();
            var sp = scope.ServiceProvider;
            var ctx = new DefaultHttpContext { RequestServices = sp };
            ctx.Session = new MemorySession();
            // 默认按"空表单"处理(BlogController.Create 会读 Request.Form.Files)。不要直接给 Request.Form 赋值:
            // 那样 HasFormContentType 会永远为 true,JSON 请求就被当成表单。需要 JSON 的测试自己设 ContentType 和 Body。
            ctx.Request.ContentType = "application/x-www-form-urlencoded";
            ctx.Request.Body = new MemoryStream();
            if (username != null)
            {
                var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
                claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
                ctx.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test", ClaimTypes.Name, ClaimTypes.Role));
            }
            sp.GetRequiredService<IHttpContextAccessor>().HttpContext = ctx;
            var c = factory(sp);
            c.ControllerContext = new ControllerContext(new ActionContext(ctx, new RouteData(), new ControllerActionDescriptor()));
            c.Url = new FakeUrlHelper();
            c.TempData = new TempDataDictionary(ctx, new NullTempDataProvider());
            return c;
        }

        /// <summary>写入一个卡池:为每个稀有度写一个 GachaItem,并把它们加入指定卡池(权重 1)。</summary>
        public void SeedGachaPool(GachaPool.PoolName pool, params int[] rarities)
        {
            using var s = NewScope();
            var udb = s.ServiceProvider.GetRequiredService<UsersContext>();
            foreach (int r in rarities)
            {
                var item = new GachaItem { Name = "item" + r + "_" + Guid.NewGuid().ToString("N").Substring(0, 6), Title = "t", Description = "d", Rarity = r };
                udb.GachaItems.Add(item);
                udb.SaveChanges();
                udb.GachaPools.Add(new GachaPool { Name = pool, ItemId = item.Id, Weight = 1 });
                udb.SaveChanges();
            }
        }

        /// <summary>直接写入一条站内信。</summary>
        public int SeedMessage(string sender, string recipient, string title = "标题", string content = "正文", bool isRead = false)
        {
            using var s = NewScope();
            var udb = s.ServiceProvider.GetRequiredService<UsersContext>();
            var m = new Message { Sender = sender, Recipient = recipient, MsgTitle = title, MsgContent = content, MsgDate = DateTime.Now, IsRead = isRead };
            udb.Messages.Add(m);
            udb.SaveChanges();
            return m.MsgId;
        }

        /// <summary>直接写入一条评论下的回复。</summary>
        public int SeedReply(int postId, string author, string content = "<p>回复内容</p>")
        {
            using var s = NewScope();
            var db = s.ServiceProvider.GetRequiredService<BlogContext>();
            var r = new Reply { PostId = postId, Author = author, Content = content, ReplyDate = DateTime.Now };
            db.Replies.Add(r);
            db.SaveChanges();
            return r.ReplyId;
        }

        /// <summary>生产库里存在的三个占位博客:0=意见建议、-1=问题举报(也是站点规则页)、-2=历史排行。评论/举报板的帖子挂在它们上面。</summary>
        public void SeedPlaceholderBlogs()
        {
            int cat = SeedCategory(name: "占位");
            foreach (int id in new[] { 0, -1, -2 })
            {
                int tmp = SeedBlog("admin", cat, title: "占位" + id);
                using var s = NewScope();
                var db = s.ServiceProvider.GetRequiredService<BlogContext>();
                db.Database.ExecuteSqlRaw("UPDATE \"Blogs\" SET \"BlogID\" = {0} WHERE \"BlogID\" = {1}", id, tmp);
            }
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

    public class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "GmGard.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public string EnvironmentName { get; set; } = "Testing";
        public string WebRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
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
