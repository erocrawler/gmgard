using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using GmGard.Models;
using AspNetCore.Identity.EntityFramework6;
using GmGard.Filters;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Serilog;
using System.IO;
using FluentScheduler;
using System.Data.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Hosting;
using Serilog.Filters;

namespace GmGard
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddMvc(option =>
            {
                option.EnableEndpointRouting = false;
                option.CacheProfiles.Add("Never", new CacheProfile
                {
                    Location = ResponseCacheLocation.None,
                    NoStore = true
                });
                option.Filters.Add(typeof(GlobalExceptionFilter));
                var SslPort = Configuration.GetSection("ApplicationSettings").GetValue<string>("HttpsPort");
                if (!string.IsNullOrEmpty(SslPort))
                {
                    option.SslPort = int.Parse(SslPort);
                }
            })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                })
                .AddSessionStateTempDataProvider();

            if (IsDev)
            {
                builder.AddRazorRuntimeCompilation();
            }

            services.AddOptions();

            services.AddSingleton(Configuration);
            services.Configure<AppSettingsModel>(Configuration.GetSection("ApplicationSettings"));
            services.Configure<EmailSender.EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.Configure<ElasticSearchProvider.ElasticSearchSettings>(Configuration.GetSection("ElasticSearchSettings"));
            services.Configure<LinodeS3Config>(Configuration.GetSection("LinodeS3Config"));
            services.Configure<RegisterSettingsModel>(ConfigFromDataFile("App_Data/RegisterSettings.json"));
            services.Configure<DataSettingsModel>(ConfigFromDataFile("App_Data/DataSettings.json"));
            services.Configure<BackgroundSetting>(ConfigFromDataFile("App_Data/BackgroundSetting.json"));
            services.Configure<Models.App.AuditExamConfig>(ConfigFromDataFile("App_Data/AuditExam.json"));
            services.Configure<Models.App.WheelConfig>(ConfigFromDataFile("App_Data/WheelConfig.json"));

            services.AddMemoryCache();
            services.AddSession();

            services.AddScoped(p => new BlogContext(_dataDbConnectionString, p.GetRequiredService<ILoggerFactory>()));
            services.AddScoped(p => new UsersContext(_userDbConnectionString, p.GetRequiredService<ILoggerFactory>()));

            services.AddIdentity<UserProfile, AspNetCore.Identity.EntityFramework6.IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = string.Empty;
            })
                .AddUserStore<UserStore<UserProfile, UsersContext>>()
                .AddRoleStore<RoleStore<UsersContext>>()
                .AddErrorDescriber<GmIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
                options.AccessDeniedPath = "/403.html";
                options.Events.OnRedirectToLogin = ctx =>
                {
                    if (ctx.Request.Path.StartsWithSegments("/api") &&
                        ctx.Response.StatusCode == (int)System.Net.HttpStatusCode.OK)
                    {
                        ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    }
                    else
                    {
                        ctx.Response.Redirect(ctx.RedirectUri);
                    }
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization(option =>
            {
                option.AddPolicy("Harmony", policy => policy.AddRequirements(new HarmonyRequirement()));
                option.AddPolicy("AdminAccess", policy => policy.AddRequirements(new AdminAccessRequirement()));
            });

            services.AddCors(option =>
            {
                option.AddPolicy("GmAppOrigin",
                    builder => builder.WithOrigins(IsDev ? SiteConstant.DevAppHostOrigins : SiteConstant.AppHostOrigins)
                                      .AllowAnyHeader()
                                      .AllowCredentials()
                                      .AllowAnyMethod());
            });

            services.AddAntiforgery(a => a.HeaderName = "X-CSRF-TOKEN");

            services.AddSingleton<IAuthorizationHandler, HarmonyHandler>();
            services.AddSingleton<IAuthorizationHandler, AdminAccessHandler>();

            if (_env.IsProduction())
            {
                services.AddSingleton(provider =>
                {
                    var scheduler = new SchedulerService(
                        provider.GetRequiredService<IServiceScopeFactory>(),
                        provider.GetRequiredService<IWebHostEnvironment>(),
                        provider.GetRequiredService<IMemoryCache>(),
                        provider.GetRequiredService<IOptions<AppSettingsModel>>(),
                        provider.GetRequiredService<ILoggerFactory>());
                    JobManager.Initialize(scheduler);
                    return scheduler;
                });
            }
            services.AddScoped<JobTaskRunner>();
            services.AddSingleton<BackgroundTaskQueue>();
            services.AddHostedService<BackgroundJobService>();
            services.AddSingleton<QuestService>();
            services.AddSingleton<IVisitCounter>(s => 
            {
                if (_env.IsStaging()) {
                    return new ReadonlyVisitCounter(s.GetRequiredService<IServiceScopeFactory>());
                }
                else {
                    return new VisitCounter(s.GetRequiredService<IServiceScopeFactory>());
                }
            });
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
            services.AddScoped<IRecommendationProvider, ElasticSearchProvider>();
            bool enabelS3 = Configuration.GetSection("ApplicationSettings").GetValue<string>("UploadBackendType") == "LinodeS3";
            if (enabelS3)
            {
                services.AddScoped<IUpload, LinodeS3>();
            }
            else
            {
                services.AddScoped<IUpload, LinodeUtil>();
            }
            bool enabelES = Configuration.GetSection("ApplicationSettings").GetValue<string>("SearchBackendType") == "ElasticSearch";
            if (enabelES)
            {
                services.AddScoped<ISearchProvider, ElasticSearchProvider>();
            }
            else
            {
                services.AddScoped<ISearchProvider, DbBlogSearchProvider>();
            }
            services.AddScoped<DbBlogSearchProvider>();
            services.AddTransient<HtmlUtil>();
            services.AddTransient<GachaBonusService>();
            services.AddTransient<INickNameProvider, TitleNickNameProvider>();

            services.AddSingleton(ElasticSearchProvider.CreateClient);
            if (enabelES && !Configuration.GetSection("ElasticSearchSettings").GetValue<bool>("Readonly"))
            {
                services.AddSingleton<ElasticSearchUpdateService>();
            }
            services.AddSingleton<EmailSender>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped(provider =>
            {
                var factory = provider.GetService<IUrlHelperFactory>();
                var actionContext = provider.GetService<IActionContextAccessor>();
                return factory.GetUrlHelper(actionContext.ActionContext);
            });

            services.Configure<WebEncoderOptions>(options =>
            {
                options.TextEncoderSettings = new TextEncoderSettings(System.Text.Unicode.UnicodeRanges.All);
            });

            services.AddLogging(builder => {
                if (IsDev)
                {
                    builder.AddConsole();
                    builder.AddDebug();
                    builder.AddEventSourceLogger();
                }
                builder.AddSerilog();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, QuestService questService, IServiceProvider services)
        {
            services.GetService<ElasticSearchUpdateService>();
            if (env.IsProduction())
            {
                services.GetRequiredService<SchedulerService>();
            }
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
                //app.UseBrowserLink();

                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    Database.SetInitializer(new BlogDBinit());
                    Database.SetInitializer(new UserDBinit(
                        serviceScope.ServiceProvider.GetService<IPasswordHasher<UserProfile>>()));
                    serviceScope.ServiceProvider.GetService<BlogContext>().Database.Initialize(false);
                    serviceScope.ServiceProvider.GetService<UsersContext>().Database.Initialize(false);
                }
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");
            
            app.UseStaticFiles(new StaticFileOptions {
                ContentTypeProvider = ConfigureFileExtensionProvider(),
                OnPrepareResponse = ctx => {
                    ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", IsDev ? SiteConstant.DevAppHostOrigins : SiteConstant.AppHostOrigins);
                    ctx.Context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                },
            });

            app.UseAuthentication();

            app.UseSession();

            app.UseMvc(RouteConfig);
        }

        private IConfigurationRoot ConfigFromDataFile(string datafile)
        {
            return new ConfigurationBuilder().SetBasePath(_basePath).AddJsonFile(datafile, true, true).Build();
        }

        private FileExtensionContentTypeProvider ConfigureFileExtensionProvider()
        {
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".ks"] = "text/plain";
            provider.Mappings[".tjs"] = "text/plain";
            return provider;
        }

        private void RouteConfig(IRouteBuilder routes)
        {
            routes.MapRoute("Avatar", "Avatar/{name?}",
                defaults: new { controller = "Avatar", action = "Show" }
            );
            routes.MapRoute("Rss", "Rss/{*id}",
                defaults: new { controller = "Rss", action = "Index" }
            );
            routes.MapRoute(
                name: "UserInfo",
                template: "User/{name?}",
                defaults: new { controller = "Home", action = "UserInfo" }
            );
            routes.MapRoute(
                name: "Follows",
                template: "Follow/{action}/{name?}",
                defaults: new { controller = "Follow", action = "Index" }
            );
            routes.MapRoute(
                name: "Favorite",
                template: "Favorite/{name?}",
                defaults: new { controller = "Home", action = "Favorite" }
            );
            routes.MapRoute(
                name: "UserRank",
                template: "Rank",
                defaults: new { controller = "Home", action = "UserRanking" }
            );
            routes.MapRoute(
                name: "AdminManage",
                template: "Admin/Manage/{context?}",
                defaults: new { controller = "Admin", action = "Manage" }
            );
            routes.MapRoute(
                name: "Upload",
                template: "Upload/{*img}",
                defaults: new { controller = "Upload", action = "Index" }
            );
            routes.MapRoute(
                name: "HanGroups",
                template: "H/{name}",
                defaults: new { controller = "Han", action = "Index" }
            );
            routes.MapRoute(
                name: "Topics",
                template: "gmt{id:decimal}",
                defaults: new { controller = "Topic", action = "Details" }
            );
            routes.MapRoute(
                name: "Blogs",
                template: "gm{id:decimal}",
                defaults: new { controller = "Blog", action = "Details" }
            );
            routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
        }

        public Startup(IWebHostEnvironment env)
        {
            _basePath = env.ContentRootPath;
            _env = env;
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(_basePath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("appsettings.override.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(env.IsProduction() ? Serilog.Events.LogEventLevel.Error : Serilog.Events.LogEventLevel.Debug)
                .WriteTo.File(Path.Combine(_basePath, "Log/log-.txt"), rollingInterval: RollingInterval.Day)
                .WriteTo.Logger(lc => lc.Filter.ByIncludingOnly(le => Matching.FromSource<BackgroundJobService>()(le) || Matching.FromSource<JobTaskRunner>()(le) || Matching.FromSource<ElasticSearchUpdateService>()(le))
                    .WriteTo.File(Path.Combine(_basePath, "Log/job-.txt"), rollingInterval: RollingInterval.Day))
                .CreateLogger();
            if (!env.IsProduction())
            {
                Log.Logger.Error("Non production build!");
            }
            _dataDbConnectionString = Configuration.GetConnectionString("GmGardData").Replace("|DataDirectory|", Path.Combine(_basePath, "App_Data"));
            _userDbConnectionString = Configuration.GetConnectionString("GmGardUser").Replace("|DataDirectory|", Path.Combine(_basePath, "App_Data"));
        }

        private readonly string _basePath;
        private readonly string _userDbConnectionString;
        private readonly string _dataDbConnectionString;
        private readonly IWebHostEnvironment _env;
        private bool IsDev => _env.IsDevelopment();

        public IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();
    }
}
