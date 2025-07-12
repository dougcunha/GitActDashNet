using GitActDashNet.Components;
using GitActDashNet.Models;
using GitActDashNet.Extensions;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind GitHub OAuth config
builder.Services.Configure<GitHubOAuthOptions>(
    builder.Configuration.GetSection("GitHubOAuth"));

// Bind Server config
builder.Services.Configure<ServerOptions>(
    builder.Configuration.GetSection("Server"));

// Configure server URLs based on appsettings
var serverConfig = builder.Configuration.GetSection("Server");
var port = serverConfig.GetValue("Port", 5000);
var useHttps = serverConfig.GetValue("UseHttps", false);
var host = serverConfig.GetValue<string>("Host", "localhost");

var protocol = useHttps ? "https" : "http";
var serverUrl = $"{protocol}://{host}:{port}";

builder.WebHost.UseUrls(serverUrl);

// Add MudBlazor services
builder.Services.AddMudServices();

// Register TokenStorageService
builder.Services.AddScoped<GitActDashNet.Services.AuthService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<GitActDashNet.Services.CookieTokenProvider>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

app.MapLogout();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Só usar HTTPS redirection se configurado para usar HTTPS
if (useHttps)
    app.UseHttpsRedirection();

app.UseAntiforgery();
app.MapStaticAssets();

// OAuth endpoints
app.MapLogin(host, port);
app.MapCallback(useHttps);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();