using GitActDashNet.Extensions;
using Serilog;

// Configure Serilog early for application startup
Log.Logger = LoggingExtensions.CreateBootstrapLogger();

try
{
    Log.Information("Starting GitActDash application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from configuration
    builder.Host.ConfigureSerilog(builder.Configuration);

    // Configure services
    builder.Services
        .AddBlazorServices()
        .AddGitHubAuthentication(builder.Configuration)
        .AddGitHubServices();

    var app = builder.Build();

    // Configure pipeline and endpoints
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