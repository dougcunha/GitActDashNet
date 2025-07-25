using Serilog;
using Serilog.Events;

namespace GitActDashNet.Extensions;

/// <summary>
/// Extension methods for configuring logging infrastructure.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog as the application logger.
    /// </summary>
    /// <param name="hostBuilder">The host builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The host builder for chaining.</returns>
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder, IConfiguration configuration)
        => hostBuilder.UseSerilog((_, _, loggerConfiguration) =>
        {
            loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName();
        });

    /// <summary>
    /// Creates and configures the initial Serilog logger for early application startup.
    /// </summary>
    /// <returns>The configured Serilog logger.</returns>
    public static Serilog.ILogger CreateBootstrapLogger()
        => new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .WriteTo.Console
        (
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        )
        .WriteTo.File
        (
            path: "logs/gitactdash-.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}",
            shared: true,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 10)
        .CreateLogger();
}
