using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using GmGard.Models;
using GmGard.Filters;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.StaticFiles;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.Hosting;
using Serilog.Filters;
using Microsoft.AspNetCore.Http;

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
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                })
                .AddSessionStateTempDataProvider();

            // Add Blazor WebAssembly services
            services.AddRazorPages();

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

            // Configure Data Protection to persist keys
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(_basePath, "App_Data", "DataProtection-Keys")));

            // Determine database provider from connection string
            var usePostgreSQL = _dataDbConnectionString.Contains("Host=") || _dataDbConnectionString.Contains("Server=") && _dataDbConnectionString.Contains("Username=");
            
            if (usePostgreSQL)
            {
                // Configure Npgsql to use timestamp without time zone globally
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                
                services.AddDbContext<BlogContext>(options => 
                    options.UseNpgsql(_dataDbConnectionString, b => b.MigrationsAssembly("GmGard")));
                services.AddDbContext<UsersContext>(options => 
                    options.UseNpgsql(_userDbConnectionString, b => b.MigrationsAssembly("GmGard")));
            }
            else
            {
                services.AddDbContext<BlogContext>(options => 
                    options.UseSqlServer(_dataDbConnectionString, b => b.MigrationsAssembly("GmGard")));
                services.AddDbContext<UsersContext>(options => 
                    options.UseSqlServer(_userDbConnectionString, b => b.MigrationsAssembly("GmGard")));
            }

            services.AddIdentity<UserProfile, IdentityRole<int>>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = string.Empty;
            })
                .AddEntityFrameworkStores<UsersContext>()
                .AddErrorDescriber<GmIdentityErrorDescriber>()
                .AddDefaultTokenProviders();

            services.AddScoped<UserManager<UserProfile>, UserManager>();

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
            services.AddScoped<JobTaskRunner>();
            services.AddSingleton<BackgroundTaskQueue>();
            services.AddHostedService<BackgroundJobService>();
            services.AddSingleton<QuestService>();
            services.AddSingleton<TitleService>();
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
            services.AddHttpContextAccessor();
            services.AddScoped(provider =>
            {
                var factory = provider.GetService<IUrlHelperFactory>();
                var httpContextAccessor = provider.GetService<IHttpContextAccessor>();
                var endpoint = httpContextAccessor.HttpContext.GetEndpoint();
                var actionDescriptor = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor>() 
                    ?? new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor();
                var actionContext = new ActionContext(httpContextAccessor.HttpContext, httpContextAccessor.HttpContext.GetRouteData(), actionDescriptor);
                return factory.GetUrlHelper(actionContext);
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
            services.GetRequiredService<SchedulerService>();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
                //app.UseBrowserLink();
                
                // Seed data in development environment only
                // SeedDevelopmentData(services).Wait();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");
            
            app.UseStaticFiles(new StaticFileOptions {
                ContentTypeProvider = ConfigureFileExtensionProvider(),
                OnPrepareResponse = ctx => {
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", IsDev ? SiteConstant.DevAppHostOrigins : SiteConstant.AppHostOrigins);
                    ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                },
            });

            // Enable Blazor WebAssembly static files at /app path
            app.UseBlazorFrameworkFiles("/app");
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("Avatar", "Avatar/{name?}",
                    defaults: new { controller = "Avatar", action = "Show" }
                );
                endpoints.MapControllerRoute("Rss", "Rss/{*id}",
                    defaults: new { controller = "Rss", action = "Index" }
                );
                endpoints.MapControllerRoute(
                    name: "UserInfo",
                    pattern: "User/{name?}",
                    defaults: new { controller = "Home", action = "UserInfo" }
                );
                endpoints.MapControllerRoute(
                    name: "Follows",
                    pattern: "Follow/{action}/{name?}",
                    defaults: new { controller = "Follow", action = "Index" }
                );
                endpoints.MapControllerRoute(
                    name: "Favorite",
                    pattern: "Favorite/{name?}",
                    defaults: new { controller = "Home", action = "Favorite" }
                );
                endpoints.MapControllerRoute(
                    name: "UserRank",
                    pattern: "Rank",
                    defaults: new { controller = "Home", action = "UserRanking" }
                );
                endpoints.MapControllerRoute(
                    name: "AdminManage",
                    pattern: "Admin/Manage/{context?}",
                    defaults: new { controller = "Admin", action = "Manage" }
                );
                endpoints.MapControllerRoute(
                    name: "Upload",
                    pattern: "Upload/{*img}",
                    defaults: new { controller = "Upload", action = "Index" }
                );
                endpoints.MapControllerRoute(
                    name: "HanGroups",
                    pattern: "H/{name}",
                    defaults: new { controller = "Han", action = "Index" }
                );
                endpoints.MapControllerRoute(
                    name: "Topics",
                    pattern: "gmt{id:decimal}",
                    defaults: new { controller = "Topic", action = "Details" }
                );
                endpoints.MapControllerRoute(
                    name: "Blogs",
                    pattern: "gm{id:decimal}",
                    defaults: new { controller = "Blog", action = "Details" }
                );
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                
                // Map Blazor app to /app route
                endpoints.MapFallbackToFile("/app/{*path:nonfile}", "index.html");
            });
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

        private async Task SeedDevelopmentData(IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var usersContext = serviceProvider.GetRequiredService<UsersContext>();
                var blogContext = serviceProvider.GetRequiredService<BlogContext>();
                var userManager = serviceProvider.GetRequiredService<UserManager<UserProfile>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

                await DataSeeder.SeedUsersAsync(usersContext, userManager, roleManager);
                DataSeeder.SeedBlog(blogContext);
            }
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
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((opt) =>
                    {
                        opt.AllowSynchronousIO = true;
                    })
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>();
                });
    }
}
