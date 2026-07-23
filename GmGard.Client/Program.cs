using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using GmGard.Client;
using GmGard.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BlogSearchService>();
builder.Services.AddScoped<DLsiteSearchService>();
builder.Services.AddScoped<PunchInService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<RaffleService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<ExamService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<TitleHelperService>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();

await builder.Build().RunAsync();
