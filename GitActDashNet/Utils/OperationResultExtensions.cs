namespace GitActDashNet.Utils;

/// <summary>
/// Extension methods to make working with OperationResult more fluent.
/// </summary>
public static class OperationResultExtensions
{
    /// <summary>
    /// Transforms the value of a successful operation result.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResult">The operation result to transform.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>A new operation result with the transformed value.</returns>
    public static OperationResult<TResult> Map<TSource, TResult>(
        this OperationResult<TSource> operationResult,
        Func<TSource, TResult> transform)
    {
        if (operationResult.IsFailure)
            return OperationResult<TResult>.Failure(operationResult.ErrorMessage);

        if (operationResult.IsWarning)
            return OperationResult<TResult>.Warning(transform(operationResult.Value), operationResult.ErrorMessage);

        return OperationResult<TResult>.Success(transform(operationResult.Value));
    }

    /// <summary>
    /// Transforms the value of a successful operation result asynchronously.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResult">The operation result to transform.</param>
    /// <param name="transform">The asynchronous transformation function.</param>
    /// <returns>A new operation result with the transformed value.</returns>
    public static async Task<OperationResult<TResult>> MapAsync<TSource, TResult>(
        this OperationResult<TSource> operationResult,
        Func<TSource, Task<TResult>> transform)
    {
        if (operationResult.IsFailure)
            return OperationResult<TResult>.Failure(operationResult.ErrorMessage);

        var transformedValue = await transform(operationResult.Value).ConfigureAwait(false);

        if (operationResult.IsWarning)
            return OperationResult<TResult>.Warning(transformedValue, operationResult.ErrorMessage);

        return OperationResult<TResult>.Success(transformedValue);
    }

    /// <summary>
    /// Transforms the value of a successful operation result from a Task.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResultTask">The task containing the operation result to transform.</param>
    /// <param name="transform">The transformation function.</param>
    /// <returns>A new operation result with the transformed value.</returns>
    public static async Task<OperationResult<TResult>> Map<TSource, TResult>(
        this Task<OperationResult<TSource>> operationResultTask,
        Func<TSource, TResult> transform)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return operationResult.Map(transform);
    }

    /// <summary>
    /// Transforms the value of a successful operation result from a Task asynchronously.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResultTask">The task containing the operation result to transform.</param>
    /// <param name="transform">The asynchronous transformation function.</param>
    /// <returns>A new operation result with the transformed value.</returns>
    public static async Task<OperationResult<TResult>> MapAsync<TSource, TResult>(
        this Task<OperationResult<TSource>> operationResultTask,
        Func<TSource, Task<TResult>> transform)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return await operationResult.MapAsync(transform).ConfigureAwait(false);
    }

    /// <summary>
    /// Chains two operations together, where the second operation depends on the first.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResult">The first operation result.</param>
    /// <param name="next">Function that returns the next operation result.</param>
    /// <returns>The result of the second operation, or the failure of the first.</returns>
    public static OperationResult<TResult> Bind<TSource, TResult>(
        this OperationResult<TSource> operationResult,
        Func<TSource, OperationResult<TResult>> next)
    {
        if (operationResult.IsFailure)
            return OperationResult<TResult>.Failure(operationResult.ErrorMessage);

        var nextResult = next(operationResult.Value);

        if (operationResult.IsWarning && nextResult.IsSuccess)
            return OperationResult<TResult>.Warning(nextResult.Value, operationResult.ErrorMessage);

        if (operationResult.IsWarning && nextResult.IsWarning)
        {
            var combinedWarnings = $"{operationResult.ErrorMessage}\n{nextResult.ErrorMessage}";
            return OperationResult<TResult>.Warning(nextResult.Value, combinedWarnings);
        }

        return nextResult;
    }

    /// <summary>
    /// Chains two asynchronous operations together, where the second operation depends on the first.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResult">The first operation result.</param>
    /// <param name="next">Function that returns the next operation result asynchronously.</param>
    /// <returns>The result of the second operation, or the failure of the first.</returns>
    public static async Task<OperationResult<TResult>> BindAsync<TSource, TResult>(
        this OperationResult<TSource> operationResult,
        Func<TSource, Task<OperationResult<TResult>>> next)
    {
        if (operationResult.IsFailure)
            return OperationResult<TResult>.Failure(operationResult.ErrorMessage);

        var nextResult = await next(operationResult.Value).ConfigureAwait(false);

        if (operationResult.IsWarning && nextResult.IsSuccess)
            return OperationResult<TResult>.Warning(nextResult.Value, operationResult.ErrorMessage);

        if (operationResult.IsWarning && nextResult.IsWarning)
        {
            var combinedWarnings = $"{operationResult.ErrorMessage}\n{nextResult.ErrorMessage}";
            return OperationResult<TResult>.Warning(nextResult.Value, combinedWarnings);
        }

        return nextResult;
    }

    /// <summary>
    /// Chains two operations together from a Task, where the second operation depends on the first.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResultTask">The task containing the first operation result.</param>
    /// <param name="next">Function that returns the next operation result.</param>
    /// <returns>The result of the second operation, or the failure of the first.</returns>
    public static async Task<OperationResult<TResult>> Bind<TSource, TResult>(
        this Task<OperationResult<TSource>> operationResultTask,
        Func<TSource, OperationResult<TResult>> next)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return operationResult.Bind(next);
    }

    /// <summary>
    /// Chains two asynchronous operations together from a Task, where the second operation depends on the first.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="operationResultTask">The task containing the first operation result.</param>
    /// <param name="next">Function that returns the next operation result asynchronously.</param>
    /// <returns>The result of the second operation, or the failure of the first.</returns>
    public static async Task<OperationResult<TResult>> BindAsync<TSource, TResult>(
        this Task<OperationResult<TSource>> operationResultTask,
        Func<TSource, Task<OperationResult<TResult>>> next)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return await operationResult.BindAsync(next).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an action on the value if the operation was successful.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The original operation result.</returns>
    public static OperationResult<T> OnSuccess<T>(
        this OperationResult<T> operationResult,
        Action<T> action)
    {
        if (operationResult.IsSuccess || operationResult.IsWarning)
            action(operationResult.Value);

        return operationResult;
    }

    /// <summary>
    /// Executes an action on the value if the operation was successful from a Task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResultTask">The task containing the operation result.</param>
    /// <param name="action">The action to execute on success.</param>
    /// <returns>The original operation result.</returns>
    public static async Task<OperationResult<T>> OnSuccess<T>(
        this Task<OperationResult<T>> operationResultTask,
        Action<T> action)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return operationResult.OnSuccess(action);
    }

    /// <summary>
    /// Executes an action on the error message if the operation failed.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="action">The action to execute on failure.</param>
    /// <returns>The original operation result.</returns>
    public static OperationResult<T> OnFailure<T>(
        this OperationResult<T> operationResult,
        Action<string> action)
    {
        if (operationResult.IsFailure)
            action(operationResult.ErrorMessage);

        return operationResult;
    }

    /// <summary>
    /// Executes an action on the error message if the operation failed from a Task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResultTask">The task containing the operation result.</param>
    /// <param name="action">The action to execute on failure.</param>
    /// <returns>The original operation result.</returns>
    public static async Task<OperationResult<T>> OnFailure<T>(
        this Task<OperationResult<T>> operationResultTask,
        Action<string> action)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return operationResult.OnFailure(action);
    }

    /// <summary>
    /// Executes an action on the warning message if the operation succeeded with warnings.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="action">The action to execute on warning.</param>
    /// <returns>The original operation result.</returns>
    public static OperationResult<T> OnWarning<T>(
        this OperationResult<T> operationResult,
        Action<string> action)
    {
        if (operationResult.IsWarning)
            action(operationResult.ErrorMessage);

        return operationResult;
    }

    /// <summary>
    /// Executes an action on the warning message if the operation succeeded with warnings from a Task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResultTask">The task containing the operation result.</param>
    /// <param name="action">The action to execute on warning.</param>
    /// <returns>The original operation result.</returns>
    public static async Task<OperationResult<T>> OnWarning<T>(
        this Task<OperationResult<T>> operationResultTask,
        Action<string> action)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return operationResult.OnWarning(action);
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResult">The operation result.</param>
    /// <param name="defaultValue">The default value to return on failure.</param>
    /// <returns>The value or default value.</returns>
    public static T ValueOrDefault<T>(
        this OperationResult<T> operationResult,
        T defaultValue = default!)
        => operationResult.IsSuccess || operationResult.IsWarning
            ? operationResult.Value
            : defaultValue;

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value from a Task.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="operationResultTask">The task containing the operation result.</param>
    /// <param name="defaultValue">The default value to return on failure.</param>
    /// <returns>The value or default value.</returns>
    public static async Task<T> ValueOrDefault<T>(
        this Task<OperationResult<T>> operationResultTask,
        T defaultValue = default!)
    {
        var operationResult = await operationResultTask.ConfigureAwait(false);
        return operationResult.ValueOrDefault(defaultValue);
    }

    /// <summary>
    /// Converts a nullable value to an OperationResult.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="errorMessage">The error message if the value is null.</param>
    /// <returns>An operation result.</returns>
    public static OperationResult<T> ToOperationResult<T>(
        this T? value,
        string errorMessage = "Value cannot be null")
        where T : class
        => value is not null
            ? OperationResult<T>.Success(value)
            : OperationResult<T>.Failure(errorMessage);

    /// <summary>
    /// Converts a nullable struct to an OperationResult.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <param name="errorMessage">The error message if the value is null.</param>
    /// <returns>An operation result.</returns>
    public static OperationResult<T> ToOperationResult<T>(
        this T? value,
        string errorMessage = "Value cannot be null")
        where T : struct
        => value.HasValue
            ? OperationResult<T>.Success(value.Value)
            : OperationResult<T>.Failure(errorMessage);
}
