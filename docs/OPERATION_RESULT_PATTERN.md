# OperationResult Pattern Documentation

## Overview

The GitActDash project uses the **monad pattern** with `OperationResult<T>` to handle operations that can succeed, fail, or succeed with warnings. This approach provides several benefits over traditional exception-based error handling:

- **Explicit error handling**: All possible outcomes are represented in the type system
- **Composable operations**: Operations can be chained together with predictable behavior
- **No hidden exceptions**: Expected failures (like API rate limits) don't throw exceptions
- **Better performance**: Avoiding exception throwing and catching improves performance
- **Clearer intent**: Code explicitly shows what can go wrong and how it's handled

## Core Components

### OperationResult (Non-Generic)

Used for operations that don't return a value but can succeed or fail:

```csharp
public sealed class OperationResult
{
    public OperationStatus Status { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public bool IsWarning { get; }
}
```

### OperationResult\<T> (Generic)

Used for operations that return a value:

```csharp
public sealed class OperationResult<T>
{
    public OperationStatus Status { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public bool IsWarning { get; }
}
```

### OperationStatus Enum

```csharp
public enum OperationStatus
{
    Success,  // Operation completed successfully
    Warning,  // Operation completed but with warnings
    Failure   // Operation failed
}
```

## Creating Results

### Success Results

```csharp
// Non-generic
var result = OperationResult.Success();

// Generic
var result = OperationResult<Repository[]>.Success(repositories);

// Implicit conversion from value
OperationResult<string> result = "Hello World";
```

### Failure Results

```csharp
// Non-generic
var result = OperationResult.Failure("Something went wrong");

// Generic
var result = OperationResult<Repository[]>.Failure("GitHub API rate limit exceeded");

// Implicit conversion from error message
OperationResult<Repository[]> result = "API call failed";
```

### Warning Results

```csharp
// Non-generic
var result = OperationResult.Warning("Operation completed with warnings");

// Generic
var result = OperationResult<Repository[]>.Warning(repositories, "Some repositories were inaccessible");
```

## Extension Methods for Fluent Operations

### Map - Transform Values

Transform the value of a successful operation:

```csharp
var result = await gitHubService.GetUserRepositoriesAsync()
    .Map(repos => repos.Where(r => r.Private).ToArray())
    .Map(privateRepos => privateRepos.Length);

// Result type: OperationResult<int>
```

### Bind - Chain Operations

Chain operations that can fail:

```csharp
var result = await gitHubService.GetUserRepositoriesAsync()
    .BindAsync(async repos => 
    {
        if (repos.Count == 0)
            return OperationResult<string>.Failure("No repositories found");
        
        return OperationResult<string>.Success($"Found {repos.Count} repositories");
    });
```

### Side Effect Methods

Execute actions based on the result state:

```csharp
await gitHubService.GetUserRepositoriesAsync()
    .OnSuccess(repos => logger.LogInformation("Found {Count} repositories", repos.Count))
    .OnWarning(warning => logger.LogWarning("Warning: {Warning}", warning))
    .OnFailure(error => logger.LogError("Error: {Error}", error));
```

### Safe Value Extraction

Get values with fallbacks:

```csharp
var repositoryCount = await gitHubService.GetUserRepositoriesAsync()
    .Map(repos => repos.Count)
    .ValueOrDefault(0); // Returns 0 if operation failed
```

## Usage Patterns in Services

### GitHubService Example

```csharp
public async Task<OperationResult<IReadOnlyList<Repository>>> GetUserRepositoriesAsync(CancellationToken cancellationToken = default)
{
    if (cancellationToken.IsCancellationRequested)
        return OperationResult<IReadOnlyList<Repository>>.Success([]);

    try
    {
        var userRepos = await gitHubClient.Repository.GetAllForCurrent().ConfigureAwait(false);
        var organizations = await gitHubClient.Organization.GetAllForCurrent().ConfigureAwait(false);

        var allRepos = new List<Repository>(userRepos);
        
        foreach (var org in organizations)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var orgRepos = await gitHubClient.Repository.GetAllForOrg(org.Login).ConfigureAwait(false);
            allRepos.AddRange(orgRepos);
        }

        var distinctRepos = allRepos.DistinctBy(r => r.Id).ToArray();
        return OperationResult<IReadOnlyList<Repository>>.Success(distinctRepos);
    }
    catch (RateLimitExceededException ex)
    {
        return OperationResult<IReadOnlyList<Repository>>.Failure($"GitHub API rate limit exceeded. Reset at: {ex.Reset}");
    }
    catch (AuthorizationException)
    {
        return OperationResult<IReadOnlyList<Repository>>.Failure("Authorization failed. Please check your GitHub access token.");
    }
    catch (ApiException ex)
    {
        return OperationResult<IReadOnlyList<Repository>>.Failure($"GitHub API error: {ex.Message}");
    }
    catch (Exception ex)
    {
        return OperationResult<IReadOnlyList<Repository>>.Failure($"Unexpected error while fetching repositories: {ex.Message}");
    }
}
```

## Usage Patterns in Blazor Components

### Basic Pattern

```csharp
private async Task LoadData()
{
    var result = await gitHubService.GetUserRepositoriesAsync();
    
    if (result.IsFailure)
    {
        errorMessage = result.ErrorMessage;
        logger.LogError("Failed to load repositories: {Error}", result.ErrorMessage);
    }
    else if (result.IsWarning)
    {
        warningMessage = result.ErrorMessage;
        repositories = result.Value.ToList();
        logger.LogWarning("Repositories loaded with warnings: {Warning}", result.ErrorMessage);
    }
    else
    {
        repositories = result.Value.ToList();
        logger.LogInformation("Successfully loaded {Count} repositories", repositories.Count);
    }
}
```

### Fluent Pattern

```csharp
private async Task LoadDataFluent()
{
    await gitHubService.GetUserRepositoriesAsync()
        .OnSuccess(repos => 
        {
            repositories = repos.ToList();
            errorMessage = null;
            warningMessage = null;
        })
        .OnWarning(warning => 
        {
            warningMessage = warning;
            errorMessage = null;
        })
        .OnFailure(error => 
        {
            errorMessage = error;
            repositories.Clear();
        });
}
```

### Complex Transformation

```csharp
private async Task LoadPersonalRepositories()
{
    var result = await gitHubService.GetUserRepositoriesAsync()
        .Map(repos => repos.Where(r => r.Owner.Type == AccountType.User))
        .Map(personalRepos => personalRepos.OrderByDescending(r => r.UpdatedAt))
        .Map(sortedRepos => sortedRepos.Take(10).ToArray());

    repositories = result.ValueOrDefault([]);
}
```

## Error Handling Strategy

### GitHub API Specific Errors

The services handle GitHub API-specific errors gracefully:

- **RateLimitExceededException**: Returns failure with reset time information
- **AuthorizationException**: Returns failure with authentication guidance
- **NotFoundException**: Returns failure with specific resource information
- **ApiException**: Returns failure with GitHub API error details

### Generic Error Handling

Unexpected exceptions are caught and wrapped in failure results with context:

```csharp
catch (Exception ex)
{
    return OperationResult<T>.Failure($"Unexpected error while {operationDescription}: {ex.Message}");
}
```

## Benefits in Practice

1. **No Surprises**: Methods never throw unexpected exceptions for API failures
2. **Explicit Contracts**: Method signatures clearly show what can go wrong
3. **Composable**: Operations can be chained together with predictable behavior
4. **Better Testing**: Easy to test success, warning, and failure scenarios
5. **Performance**: Avoids expensive exception throwing for expected failures
6. **Logging**: Centralized error handling with consistent logging patterns

## Conversion Guidelines

When converting existing code to use OperationResult:

1. **Replace thrown exceptions** with failure results for expected errors
2. **Keep exceptions** for truly unexpected errors (programming errors)
3. **Use warning results** when operations succeed but with caveats
4. **Chain operations** using Bind/BindAsync for dependent operations
5. **Transform values** using Map for simple transformations
6. **Handle all states** explicitly in UI components

This pattern makes the codebase more robust, predictable, and easier to maintain while providing better error handling and user experience.
