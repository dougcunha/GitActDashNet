using System.Net.Http.Headers;
using System.Text.Json;

namespace GitActDashNet.Extensions;

public static class ApiExtensions
{
    public static WebApplication MapLogout(this WebApplication app)
    {
        app.MapGet("/api/auth/logout", (HttpContext http) =>
        {
            http.Response.Cookies.Delete("github_token");

            return Results.Redirect("/login");
        });

        return app;
    }

    public static WebApplication MapLogin(this WebApplication app, string host, int port)
    {
        app.MapGet("/api/auth/login", (IConfiguration config, HttpContext _) =>
        {
            var clientId = config["GitHubOAuth:ClientId"] ?? string.Empty;

            if (string.IsNullOrEmpty(clientId))
                return Results.BadRequest("GitHub OAuth ClientId or ClientSecret are not configured. Please configure appsettings.json.");

            var redirectUri = $"http://{host}:{port}/api/auth/callback";
            var githubAuthUrl = string.Concat(
                "https://github.com/login/oauth/authorize?client_id=", clientId,
                "&redirect_uri=", Uri.EscapeDataString(redirectUri),
                "&scope=repo%20read:user%20workflow"
            );

            return Results.Redirect(githubAuthUrl);
        });

        return app;
    }

    public static WebApplication MapCallback(this WebApplication app, bool useHttps)
    {
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

        return app;
    }
}