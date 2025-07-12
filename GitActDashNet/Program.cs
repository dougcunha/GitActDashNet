using System.Net.Http.Headers;
using System.Text.Json;
using GitActDashNet.Components;
using GitActDashNet.Models;
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

app.MapGet("/api/auth/logout", (HttpContext http) =>
{
    http.Response.Cookies.Delete("github_token");
    return Results.Redirect("/login");
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Só usar HTTPS redirection se configurado para usar HTTPS
if (useHttps)
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();
app.MapStaticAssets();

// OAuth endpoints
app.MapGet("/api/auth/login", (IConfiguration config, HttpContext _) =>
{
    var clientId = config["GitHubOAuth:ClientId"] ?? string.Empty;

    if (string.IsNullOrEmpty(clientId))
        return Results.BadRequest("GitHub OAuth ClientId or ClientSecret are not configured. Please configure appsettings.json.");

    var redirectUri = $"http://{host}:{port}/api/auth/callback";
    var githubAuthUrl = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope=repo%20read:user%20workflow";

    return Results.Redirect(githubAuthUrl);
});

app.MapGet("/api/auth/callback", async (IConfiguration config, HttpContext http) =>
{
    var code = http.Request.Query["code"].ToString();

    if (string.IsNullOrEmpty(code))
        return Results.BadRequest("Missing code");

    var clientId = config["GitHubOAuth:ClientId"] ?? string.Empty;
    var clientSecret = config["GitHubOAuth:ClientSecret"] ?? string.Empty;

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        return Results.BadRequest("GitHub OAuth ClientId or ClientSecret are not configured. Please configure appsettings.json.");

    var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
    {
        Content = new FormUrlEncodedContent
        (
            [
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", code)
            ]
        )
    };

    tokenRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    using var client = new HttpClient();
    var response = await client.SendAsync(tokenRequest);
    var json = await response.Content.ReadAsStringAsync();
    var obj = JsonDocument.Parse(json).RootElement;

    if (!obj.TryGetProperty("access_token", out var accessTokenProp))
        return Results.BadRequest("No access_token returned");

    var accessToken = accessTokenProp.GetString();

    http.Response.Cookies.Append
    (
        "github_token",
        accessToken!,
        new CookieOptions
        {
            HttpOnly = true,
            Secure = useHttps,
            SameSite = SameSiteMode.Lax
        }
    );

    return Results.Redirect("/dashboard");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();