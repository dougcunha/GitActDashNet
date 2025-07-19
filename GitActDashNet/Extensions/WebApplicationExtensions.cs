using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GitActDashNet.Extensions;

/// <summary>
/// Extension methods for configuring the application pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the HTTP request pipeline with standard middleware.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseAntiforgery();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapStaticAssets();
        app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    /// <summary>
    /// Configures authentication endpoints for login and logout.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication ConfigureAuthenticationEndpoints(this WebApplication app)
    {
        // Minimal API endpoints for login and logout
        app.MapGet("/login", (string? returnUrl = "/")
            => Results.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, ["GitHub"]));

        app.MapPost("/logout", async context =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            context.Response.Redirect("/");
        });

        return app;
    }
}
