using GitActDashNet.Utils;
using Microsoft.JSInterop;
using System.Text.Json;

namespace GitActDashNet.Services;

/// <summary>
/// Service to interact with browser's localStorage using JavaScript Interop.
/// </summary>
public sealed class LocalStorageService(IJSRuntime jsRuntime, ILogger<LocalStorageService> logger)
{
    private const string SET_ITEM_FUNCTION = "localStorage.setItem";
    private const string GET_ITEM_FUNCTION = "localStorage.getItem";
    private const string REMOVE_ITEM_FUNCTION = "localStorage.removeItem";
    private const string CLEAR_FUNCTION = "localStorage.clear";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false // LocalStorage typically doesn't need indented JSON
    };

    /// <summary>
    /// Checks if JavaScript interop is available (not during prerendering).
    /// </summary>
    private async Task<bool> IsJavaScriptAvailableAsync()
    {
        try
        {
            // Try a simple, non-invasive JS call to test availability
            await jsRuntime.InvokeAsync<bool>("Boolean", true);
            return true;
        }
        catch (InvalidOperationException)
        {
            // This is the expected exception during prerendering
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Sets a value in localStorage.
    /// </summary>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public async Task<OperationResult> SetItemAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        using var _ = logger.ForServiceOperation(nameof(LocalStorageService), nameof(SetItemAsync));

        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Attempted to set localStorage item with null or empty key");

            return OperationResult.Failure("Key cannot be null or empty.");
        }

        if (!await IsJavaScriptAvailableAsync())
        {
            logger.LogWarning("JavaScript interop not available (prerendering). Cannot set localStorage item with key: {Key}", key);

            return OperationResult.Failure("JavaScript interop is not available during prerendering. Please call this method after the component has been rendered.");
        }

        try
        {
            logger.LogDebug("Setting localStorage item with key: {Key}", key);
            await jsRuntime.InvokeVoidAsync(SET_ITEM_FUNCTION, cancellationToken, key, value).ConfigureAwait(false);
            logger.LogDebug("Successfully set localStorage item with key: {Key}", key);

            return OperationResult.Success();
        }
        catch (JSException ex)
        {
            logger.LogError(ex, "JavaScript error while setting localStorage item with key: {Key}", key);

            return OperationResult.Failure($"JavaScript error while setting localStorage item '{key}': {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            logger.LogWarning("Operation was cancelled while setting localStorage item with key: {Key}", key);

            return OperationResult.Failure($"Operation was cancelled while setting localStorage item '{key}'.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while setting localStorage item with key: {Key}", key);

            return OperationResult.Failure($"Unexpected error while setting localStorage item '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Sets an object in localStorage as JSON.
    /// </summary>
    /// <typeparam name="T">The type of the object to store.</typeparam>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The object to store.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public async Task<OperationResult> SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);

            return await SetItemAsync(key, json, cancellationToken).ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            return OperationResult.Failure($"Failed to serialize object for localStorage key '{key}': {ex.Message}");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Unexpected error while serializing object for localStorage key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a value from localStorage.
    /// </summary>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result containing the value, or null if not found.</returns>
    public async Task<OperationResult<string?>> GetItemAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return OperationResult<string?>.Failure("Key cannot be null or empty.");

        if (!await IsJavaScriptAvailableAsync())
        {
            logger.LogWarning("JavaScript interop not available (prerendering). Cannot get localStorage item with key: {Key}", key);

            return OperationResult<string?>.Failure("JavaScript interop is not available during prerendering. Please call this method after the component has been rendered.");
        }

        try
        {
            var value = await jsRuntime.InvokeAsync<string?>(GET_ITEM_FUNCTION, cancellationToken, key).ConfigureAwait(false);

            return OperationResult<string?>.Success(value);
        }
        catch (JSException ex)
        {
            return OperationResult<string?>.Failure($"JavaScript error while getting localStorage item '{key}': {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return OperationResult<string?>.Failure($"Operation was cancelled while getting localStorage item '{key}'.");
        }
        catch (Exception ex)
        {
            return OperationResult<string?>.Failure($"Unexpected error while getting localStorage item '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Gets an object from localStorage by deserializing JSON.
    /// </summary>
    /// <typeparam name="T">The type of the object to retrieve.</typeparam>
    /// <param name="key">The key to retrieve the value for.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result containing the deserialized object, or null if not found.</returns>
    public async Task<OperationResult<T?>> GetItemAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var jsonResult = await GetItemAsync(key, cancellationToken).ConfigureAwait(false);

        if (jsonResult.IsFailure)
            return OperationResult<T?>.Failure(jsonResult.ErrorMessage);

        if (jsonResult.Value is null)
            return OperationResult<T?>.Success(default);

        try
        {
            var value = JsonSerializer.Deserialize<T>(jsonResult.Value, _jsonOptions);

            return OperationResult<T?>.Success(value);
        }
        catch (JsonException ex)
        {
            return OperationResult<T?>.Failure($"Failed to deserialize JSON for localStorage key '{key}': {ex.Message}");
        }
        catch (Exception ex)
        {
            return OperationResult<T?>.Failure($"Unexpected error while deserializing JSON for localStorage key '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Removes an item from localStorage.
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public async Task<OperationResult> RemoveItemAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return OperationResult.Failure("Key cannot be null or empty.");

        if (!await IsJavaScriptAvailableAsync())
        {
            logger.LogWarning("JavaScript interop not available (prerendering). Cannot remove localStorage item with key: {Key}", key);

            return OperationResult.Failure("JavaScript interop is not available during prerendering. Please call this method after the component has been rendered.");
        }

        try
        {
            await jsRuntime.InvokeVoidAsync(REMOVE_ITEM_FUNCTION, cancellationToken, key).ConfigureAwait(false);

            return OperationResult.Success();
        }
        catch (JSException ex)
        {
            return OperationResult.Failure($"JavaScript error while removing localStorage item '{key}': {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return OperationResult.Failure($"Operation was cancelled while removing localStorage item '{key}'.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Unexpected error while removing localStorage item '{key}': {ex.Message}");
        }
    }

    /// <summary>
    /// Clears all items from localStorage.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result indicating success or failure.</returns>
    public async Task<OperationResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsJavaScriptAvailableAsync())
        {
            logger.LogWarning("JavaScript interop not available (prerendering). Cannot clear localStorage");

            return OperationResult.Failure("JavaScript interop is not available during prerendering. Please call this method after the component has been rendered.");
        }

        try
        {
            await jsRuntime.InvokeVoidAsync(CLEAR_FUNCTION, cancellationToken).ConfigureAwait(false);

            return OperationResult.Success();
        }
        catch (JSException ex)
        {
            return OperationResult.Failure($"JavaScript error while clearing localStorage: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return OperationResult.Failure("Operation was cancelled while clearing localStorage.");
        }
        catch (Exception ex)
        {
            return OperationResult.Failure($"Unexpected error while clearing localStorage: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the LocalStorage service is available for use (JavaScript interop is ready).
    /// </summary>
    /// <returns>True if LocalStorage operations can be performed, false if still in prerendering phase.</returns>
    public async Task<bool> IsAvailableAsync()
        => await IsJavaScriptAvailableAsync();
}
