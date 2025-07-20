using Serilog.Context;

namespace GitActDashNet.Utils;

/// <summary>
/// Extension methods for enhanced logging throughout the application.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Creates a context-aware logger for GitHub operations.
    /// </summary>
    /// <param name="logger">The base logger.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <param name="repository">Optional repository context.</param>
    /// <param name="organization">Optional organization context.</param>
    /// <returns>A disposable logger context.</returns>
    public static IDisposable ForGitHubOperation<T>
    (
        // ReSharper disable once UnusedParameter.Global
        this ILogger<T> logger,
        string operation,
        string? repository = null,
        string? organization = null
    )
    {
        var context = LogContext.PushProperty("Operation", operation);

        if (!string.IsNullOrEmpty(repository))
            context = LogContext.PushProperty("Repository", repository);

        if (!string.IsNullOrEmpty(organization))
            context = LogContext.PushProperty("Organization", organization);

        return context;
    }

    /// <summary>
    /// Creates a context-aware logger for service operations.
    /// </summary>
    /// <param name="logger">The base logger.</param>
    /// <param name="serviceName">The name of the service.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <returns>A disposable logger context.</returns>
    public static IDisposable ForServiceOperation<T>
    (
        // ReSharper disable once UnusedParameter.Global
        this ILogger<T> logger,
        string serviceName,
        string operation
    )
    {
        var serviceContext = LogContext.PushProperty("Service", serviceName);
        var operationContext = LogContext.PushProperty("Operation", operation);

        return new CompositeDisposable(serviceContext, operationContext);
    }

    /// <summary>
    /// Creates a context-aware logger for component operations.
    /// </summary>
    /// <param name="logger">The base logger.</param>
    /// <param name="componentName">The name of the component.</param>
    /// <param name="operation">The operation being performed.</param>
    /// <returns>A disposable logger context.</returns>
    public static IDisposable ForComponentOperation<T>
    (
        // ReSharper disable once UnusedParameter.Global
        this ILogger<T> logger,
        string componentName,
        string operation
    )
    {
        var componentContext = LogContext.PushProperty("Component", componentName);
        var operationContext = LogContext.PushProperty("Operation", operation);

        return new CompositeDisposable(componentContext, operationContext);
    }

    /// <summary>
    /// Logs the execution time of an operation.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="operation">The operation being timed.</param>
    /// <returns>A disposable that logs the execution time when disposed.</returns>
    public static IDisposable TimeOperation<T>(this ILogger<T> logger, string operation)
        => new OperationTimer<T>(logger, operation);
}

/// <summary>
/// Combines multiple disposables into one.
/// </summary>
internal sealed class CompositeDisposable : IDisposable
{
    private readonly IDisposable[] _disposables;
    private bool _disposed;

    public CompositeDisposable(params IDisposable[] disposables)
        => _disposables = disposables;

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var disposable in _disposables)
            disposable.Dispose();

        _disposed = true;
    }
}

/// <summary>
/// Times an operation and logs the execution time when disposed.
/// </summary>
internal sealed class OperationTimer<T> : IDisposable
{
    private readonly ILogger<T> _logger;
    private readonly string _operation;
    private readonly DateTime _startTime;
    private bool _disposed;

    public OperationTimer(ILogger<T> logger, string operation)
    {
        _logger = logger;
        _operation = operation;
        _startTime = DateTime.UtcNow;

        _logger.LogDebug("Starting operation: {Operation}", _operation);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        var duration = DateTime.UtcNow - _startTime;
        _logger.LogInformation("Completed operation: {Operation} in {Duration}ms",
            _operation, duration.TotalMilliseconds);

        _disposed = true;
    }
}