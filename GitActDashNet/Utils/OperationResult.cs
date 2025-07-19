using System.Diagnostics.CodeAnalysis;

namespace GitActDashNet.Utils;

/// <summary>
/// Represents the result of an operation that can succeed, fail, or succeed with warnings.
/// </summary>
public sealed class OperationResult
{
    /// <summary>
    /// Gets the operation status.
    /// </summary>
    public OperationStatus Status { get; }

    /// <summary>
    /// Gets the message with details or errors.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets whether the result represents a successful operation.
    /// </summary>
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess
        => Status is OperationStatus.Success;

    /// <summary>
    /// Gets whether the result represents a failure.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool IsFailure
        => Status is OperationStatus.Failure;

    /// <summary>
    /// Gets whether the result represents a warning.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool IsWarning
        => Status is OperationStatus.Warning;

    private OperationResult(OperationStatus status, string? message = null)
    {
        Status = status;
        ErrorMessage = message;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static OperationResult Success()
        => new(OperationStatus.Success);

    /// <summary>
    /// Creates a result with warnings.
    /// </summary>
    public static OperationResult Warning(params string[] messages)
        => new(OperationStatus.Warning, string.Join("\n", messages));

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static OperationResult Failure(params string[] messages)
        => new(OperationStatus.Failure, string.Join("\n", messages));
}

/// <summary>
/// Represents the result of an operation that can succeed, fail, or succeed with warnings.
/// </summary>
/// <typeparam name="T">Type of the value returned by the operation.</typeparam>
public sealed class OperationResult<T>
{
    /// <summary>
    /// Gets the operation status.
    /// </summary>
    public OperationStatus Status { get; }

    /// <summary>
    /// Gets the returned value of the operation, if any.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets the message with details or errors.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Gets whether the result represents a successful operation.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool IsSuccess
        => Status is OperationStatus.Success;

    /// <summary>
    /// Gets whether the result represents a failure.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool IsFailure
        => Status is OperationStatus.Failure;

    /// <summary>
    /// Gets whether the result represents a warning.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(true, nameof(ErrorMessage))]
    public bool IsWarning
        => Status is OperationStatus.Warning;

    private OperationResult(OperationStatus status, T? value = default, string? message = null)
    {
        Status = status;
        Value = value;
        ErrorMessage = message;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The value of the result.</param>
    /// <returns>An OperationResult instance representing success.</returns>
    public static OperationResult<T> Success(T value)
        => new(OperationStatus.Success, value);

    /// <summary>
    /// Creates a result with warnings.
    /// </summary>
    /// <param name="value">The value of the result.</param>
    /// <param name="messages">Warning message(s).</param>
    /// <returns>An OperationResult instance representing a warning.</returns>
    public static OperationResult<T> Warning(T value, params string[] messages)
        => new(OperationStatus.Warning, value, string.Join("\n", messages));

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="messages">Failure message(s).</param>
    /// <returns>An OperationResult instance representing failure.</returns>
    public static OperationResult<T> Failure(params string[] messages)
        => new(OperationStatus.Failure, default, string.Join("\n", messages));

    /// <summary>
    /// Implicitly converts a value to a successful result.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    public static implicit operator OperationResult<T>(T value)
        => Success(value);

    /// <summary>
    /// Implicitly converts a string message to a failed result.
    /// </summary>
    /// <param name="message">The failure message.</param>
    public static implicit operator OperationResult<T>(string message)
        => Failure(message);
}

/// <summary>
/// Represents the possible states of an operation.
/// </summary>
public enum OperationStatus
{
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success,

    /// <summary>
    /// The operation completed with warnings.
    /// </summary>
    Warning,

    /// <summary>
    /// The operation failed.
    /// </summary>
    Failure
}
