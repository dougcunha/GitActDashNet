using GitActDashNet.Extensions;
using Serilog;

// Configure Serilog early for application startup
Log.Logger = LoggingExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting GitActDash application");
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureSerilog(builder.Configuration);

    // Configure URLs from appsettings
    var urls = builder.Configuration["Urls"];

    if (!string.IsNullOrEmpty(urls))
    {
        builder.WebHost.UseUrls(urls);
        Log.Information("Using configured URLs: {Urls}", urls);
    }

    builder.Services
        .AddBlazorServices()
        .AddGitHubAuthentication(builder.Configuration)
        .AddGitHubServices();

    var app = builder.Build();

    app.ConfigurePipeline()
        .ConfigureAuthenticationEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}