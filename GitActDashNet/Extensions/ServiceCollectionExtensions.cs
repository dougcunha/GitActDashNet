using GitActDashNet.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Octokit;

namespace GitActDashNet.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Blazor components and related services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlazorServices(this IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents();

        services.AddCascadingAuthenticationState();
        services.AddAuthorizationCore();

        return services;
    }

    /// <summary>
    /// Configures GitHub OAuth authentication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGitHubAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "GitHub";
            })
            .AddCookie()
            .AddGitHub("GitHub", options =>
            {
                options.ClientId = configuration["GitHub:ClientId"]!;
                options.ClientSecret = configuration["GitHub:ClientSecret"]!;
                options.CallbackPath = "/signin-github";

                // Requesting the necessary scopes as per spec
                options.Scope.Add("repo");
                options.Scope.Add("read:org");
                options.Scope.Add("read:user");

                options.SaveTokens = true;
            });

        return services;
    }

    /// <summary>
    /// Configures GitHub API services using Octokit.NET.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddGitHubServices(this IServiceCollection services)
    {
        // Add HttpContextAccessor to access HttpContext from services
        services.AddHttpContextAccessor();

        // Configure Octokit.NET
        services.AddScoped<IGitHubClient>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;

            var client = new GitHubClient(new ProductHeaderValue("GitActDashNet", "1.0"));

            if (httpContext == null)
                return client;

            var accessToken = httpContext.GetTokenAsync("access_token").GetAwaiter().GetResult();

            if (!string.IsNullOrEmpty(accessToken))
                client.Credentials = new Credentials(accessToken);

            return client;
        });

        // Register application services
        services.AddScoped<GitHubService>();
        services.AddScoped<LocalStorageService>();

        return services;
    }
}
