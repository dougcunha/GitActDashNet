
# Project Specification: GitActDash for .NET

## 1. Project Overview

The goal of this project is to port the existing GitActDash application (currently in Node.js/React) to the .NET platform, using .NET 9 and Blazor. The new application must replicate all the features and resources of the original, providing an identical user experience.

GitActDash is a dashboard that allows users to monitor the status of their GitHub Actions runs for multiple repositories in a single interface.

## 2. Architecture and Technologies

- **Main Framework:** .NET 9
- **User Interface (UI):** Blazor Web App
  - **Rendering Mode:** Interactive Server will be the primary mode for all dynamic components.
- **Authentication:** OAuth 2.0 authentication via GitHub, managed by the ASP.NET Core authentication framework.
- **GitHub API Communication:** The Octokit.NET library will be used for all interactions with the GitHub REST API, providing a type-safe and optimized interface.
- **Client Persistence:** User repository selection and other UI preferences will be persisted in the browser's `localStorage` using Blazor's JavaScript Interop (JS Interop).
- **Logging:** Serilog will be used for structured logging with enrichers for contextual information (environment, machine name, operation context). Logs will be written to both console and daily rolling files.
- **Styling:** The application should use a CSS framework (such as Bootstrap 5, the default Blazor template, or Tailwind CSS) to replicate the look and feel of the original application, including a dark mode.

## 3. Functional Requirements (FR)

### FR-01: GitHub Authentication
- The system must allow users to log in using their GitHub accounts via the OAuth 2.0 flow.
- The application must request the following GitHub scopes: `repo`, `read:org`, `read:user`.
- The user's access token must be securely stored in the server-side session and never exposed to the client.

### FR-02: Logout
- The system must provide a logout functionality that ends the user's session and clears authentication cookies.

### FR-03: Session Management
- When accessing the application, the system must check if the user has an active authentication session.
- If the user is authenticated, they must be redirected to the dashboard page (`/dashboard`).
- If the user is not authenticated, they must be presented with the login page.

### FR-04: Login Page
- A simple landing page must be displayed for unauthenticated users.
- This page must contain a single "Login with GitHub" button that initiates the authentication flow (FR-01).

### FR-05: Repository Fetching
- After successful login, the application must use the Octokit client to fetch all repositories the user has access to.
- The fetch must include personal repositories and repositories from all organizations the user is a member of.
- The repository list must be combined and deduplicated using Octokit's `GetAllForCurrent()` and `GetAllForOrg()` methods.

### FR-06: Repository Filter and Selection Panel
- A side panel (or modal) must be displayed, listing all fetched repositories (FR-05).
- Each repository in the list must have a checkbox so the user can select it for monitoring.

### FR-07: Selection Persistence
- The list of selected repository IDs must be saved in the browser's `localStorage`.
- When reloading the application, the repository selection must be restored from `localStorage`.

### FR-08: Repository Filtering and Search
- The filter panel must include a text field to search repositories by name.
- There must be buttons to filter the repository list by type: "All", "Personal", "Organization".

### FR-09: Repository Sorting
- The filter panel must allow the user to sort the repository list by:
  - Name (ascending/descending)
  - Last updated date (most recent first)

### FR-10: Workflow and Run Fetching
- For each repository selected by the user in the panel, the application must use the Octokit client to fetch:
  1. The list of all workflows for the repository using `Actions.Workflows.List()`.
  2. The latest run for each of these workflows using `Actions.Workflows.Runs.List()` with appropriate filters.

### FR-11: Status Panel Display
- The main panel area must display the selected repositories in a multi-column (masonry) layout.
- Each column represents a selected repository.

### FR-12: Workflow Card
- Inside each repository column, its workflows must be displayed as individual "cards".
- Each workflow card must display:
  - The workflow name.
  - A colored visual indicator representing the status of the latest run (e.g., green for `success`, red for `failure`, yellow for `in_progress`, gray for others).
  - The textual status of the latest run (e.g., "success", "failure").
  - The relative time of the latest run (e.g., "5 minutes ago").
- If a workflow has no runs, this must be indicated.

### FR-13: GitHub Link
- Clicking a workflow card must open the main page of that workflow on the GitHub website in a new browser tab.

### FR-14: Auto-Refresh
- The panel must have controls that allow the user to:
  - Enable or disable automatic refresh of workflow statuses.
  - Select the refresh interval (e.g., 30s, 1min, 5min).
  - A visual countdown must show the time until the next refresh.
  - A button to trigger a manual refresh at any time.

### FR-15: Fullscreen Mode
- A button in the interface must allow the user to enter and exit a fullscreen mode, expanding the panel area to occupy the entire screen and hiding other UI elements.

### FR-16: Dark/Light Theme
- The application must support both a light and a dark theme.
- A toggle button must allow the user to switch between themes.
- The user's theme preference must be saved in `localStorage`.

## 4. Non-Functional Requirements (NFR)

- **NFR-01 (Security):** The GitHub access token must not be stored on the client or in logs. It must be managed by the ASP.NET Core server session.
- **NFR-02 (Performance):** All Octokit calls must be asynchronous. The UI must display loading indicators (spinners/skeletons) while data is being fetched.
- **NFR-03 (Usability):** The interface must be responsive and work well on different screen sizes, from desktops to mobile devices.
- **NFR-04 (Maintainability):** The code must be well-structured, with a clear separation of concerns (e.g., Blazor components for UI, services for business logic and data access). The use of Octokit.NET ensures a robust abstraction layer for the GitHub API.
- **NFR-05 (Error Handling):** All service methods must use the monad pattern with `OperationResult<T>` to handle success, failure, and warning states without throwing exceptions. This ensures predictable error handling and better composability of operations.
- **NFR-06 (Observability):** All operations must be properly logged using Serilog with structured logging and contextual enrichers. This includes operation timing, GitHub API calls, error details, and user actions for debugging and monitoring purposes.

## 5. Data Structure (C# Models)

Octokit.NET already provides robust C# models for all GitHub API entities. The main types to be used are:

- `Octokit.Repository` - Represents repositories
- `Octokit.Workflow` - Represents GitHub Actions workflows
- `Octokit.WorkflowRun` - Represents workflow runs
- `Octokit.Organization` - Represents organizations

For internal application use, auxiliary models can be created if necessary:

```csharp
// Combined model for UI usage
public sealed record WorkflowWithLatestRun(
    long WorkflowId,
    string WorkflowName,
    string WorkflowPath,
    string WorkflowState,
    string WorkflowUrl,
    Octokit.WorkflowRun? LatestRun
);

// Model for repository filters
public sealed record RepositoryFilter(
    string SearchText = "",
    RepositoryType Type = RepositoryType.All,
    RepositorySortBy SortBy = RepositorySortBy.Name,
    bool Ascending = true
);

public enum RepositoryType
{
    All,
    Personal,
    Organization
}

public enum RepositorySortBy
{
    Name,
    UpdatedAt
}
```

### 5.1 Monad Pattern with OperationResult

All service operations use the monad pattern with `OperationResult<T>` for predictable error handling:

```csharp
// Success result
OperationResult<Repository[]> result = OperationResult<Repository[]>.Success(repositories);

// Failure result
OperationResult<Repository[]> result = OperationResult<Repository[]>.Failure("GitHub API rate limit exceeded");

// Warning result (operation succeeded but with warnings)
OperationResult<Repository[]> result = OperationResult<Repository[]>.Warning(repositories, "Some repositories were inaccessible");

// Fluent operations using extension methods
var result = await gitHubService.GetUserRepositoriesAsync()
    .Map(repos => repos.Where(r => r.Owner.Type == AccountType.User).ToArray())
    .BindAsync(repos => someOtherOperation(repos))
    .OnFailure(error => logger.LogError(error))
    .OnSuccess(repos => logger.LogInformation($"Found {repos.Length} repositories"));
```

## 6. Project Structure (Suggestion)

```
/GitActDash.Blazor/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor         (Login Page)
│   │   └── Dashboard.razor    (Main Dashboard)
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Shared/
│       ├── FilterPanel.razor
│       ├── RepositoryColumn.razor
│       ├── WorkflowCard.razor
│       ├── RefreshControls.razor
│       └── ThemeToggle.razor
├── Services/
│   ├── GitHubService.cs       (Wrapper for Octokit.NET with OperationResult pattern)
│   └── LocalStorageService.cs (Wrapper for JS Interop with localStorage using OperationResult)
├── Utils/
│   ├── OperationResult.cs     (Monad pattern implementation for error handling)
│   ├── OperationResultExtensions.cs (Extension methods for fluent operations)
│   └── LoggingExtensions.cs   (Serilog extensions for contextual logging)
├── Data/
│   └── GitHubModels.cs        (Auxiliary models and enums specific to the application)
├── wwwroot/
│   ├── css/
│   │   └── app.css
│   └── js/
│       └── interop.js         (JS functions for fullscreen, theme, etc.)
├── Program.cs                 (Application, services, and authentication configuration)
└── appsettings.json           (GitHub Client ID and Client Secret configuration)
```

## 8. Octokit.NET Configuration and Usage

### 8.1 Configuration in Program.cs
```csharp
// Octokit.NET configuration
builder.Services.AddScoped<IGitHubClient>(provider =>
{
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var accessToken = httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
    
    var client = new GitHubClient(new ProductHeaderValue("GitActDash"));
    if (!string.IsNullOrEmpty(accessToken))
    {
        client.Credentials = new Credentials(accessToken);
    }
    
    return client;
});
```

### 8.2 Service Methods with OperationResult Pattern

All service methods now return `OperationResult<T>` instead of throwing exceptions:

```csharp
// GitHubService methods
public async Task<OperationResult<IReadOnlyList<Repository>>> GetUserRepositoriesAsync(CancellationToken cancellationToken = default);
public async Task<OperationResult<IReadOnlyList<Workflow>>> GetWorkflowsAsync(string owner, string repoName, CancellationToken cancellationToken = default);
public async Task<OperationResult<IReadOnlyList<WorkflowWithLatestRun>>> GetWorkflowsWithLatestRunsAsync(Repository repository, CancellationToken cancellationToken = default);
public async Task<OperationResult<WorkflowRun?>> GetLatestWorkflowRunAsync(string owner, string repoName, long workflowId, CancellationToken cancellationToken = default);

// LocalStorageService methods
public async Task<OperationResult> SetItemAsync<T>(string key, T value, CancellationToken cancellationToken = default);
public async Task<OperationResult<T?>> GetItemAsync<T>(string key, CancellationToken cancellationToken = default);
public async Task<OperationResult> RemoveItemAsync(string key, CancellationToken cancellationToken = default);
```

### 8.3 Error Handling Strategy

The OperationResult pattern provides three states:
- **Success**: Operation completed successfully with a value
- **Warning**: Operation completed with a value but has warnings to report
- **Failure**: Operation failed with an error message

Common GitHub API errors are handled specifically:
- **RateLimitExceededException**: Rate limit exceeded with reset time
- **AuthorizationException**: Authentication/authorization failures
- **NotFoundException**: Resource not found errors
- **ApiException**: General GitHub API errors
- **Generic exceptions**: Unexpected errors with context

### 8.4 Logging Configuration

Serilog is configured with the following features:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/gitactdash-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 31,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();
```

### 8.5 Contextual Logging

Services use contextual logging for better observability:

```csharp
// Service operation context
using var _ = logger.ForServiceOperation(nameof(GitHubService), nameof(GetUserRepositoriesAsync));

// GitHub operation context with repository/organization details
using var orgContext = logger.ForGitHubOperation("GetOrgRepositories", organization: org.Login);

// Operation timing
using var timer = logger.TimeOperation("GetUserRepositories");

// Component operation context
using var componentContext = logger.ForComponentOperation("FilterPanel", "LoadRepositories");
```

### 8.6 Log Levels and Content

- **Debug**: Detailed flow information for development
- **Information**: General application flow and successful operations
- **Warning**: Unexpected situations that don't prevent operation (e.g., API warnings)
- **Error**: Exceptions and operation failures with context
- **Fatal**: Critical errors that cause application termination

## 9. Suggested Implementation Plan

1.  **Initial Setup:**
    - Create a new Blazor Web App project (`dotnet new blazor`).
    - Configure GitHub authentication in `Program.cs` using `AddAuthentication().AddGitHub()`. Add the Client ID and Client Secret in `appsettings.json`.
    - Configure Octokit.NET as a service registered in the DI container.

2.  **API Service:**
    - Create auxiliary models in `Data/GitHubModels.cs`.
    - Create `GitHubService.cs` as a wrapper for Octokit's `IGitHubClient`.
    - Implement the method to fetch repositories (`GetUserRepositoriesAsync`) using `Repository.GetAllForCurrent()` and `Repository.GetAllForOrg()`.

3.  **Basic Pages and Authentication Flow:**
    - Create the `Home.razor` page with the login button.
    - Create the `Dashboard.razor` page (initially empty) and protect it with the `[Authorize]` attribute.
    - Implement the logout logic.

4.  **Filter and Selection Panel:**
    - In `Dashboard.razor`, call `GitHubService` to get the repositories.
    - Create the `FilterPanel.razor` component to display repositories with checkboxes, filters, and sorting.
    - Implement selection and persistence in `localStorage` via JS Interop.

5.  **Workflow Display:**
    - In `GitHubService`, add the method to fetch workflows and their latest runs (`GetWorkflowsWithLatestRunsAsync`) using `Actions.Workflows.List()` and `Actions.Workflows.Runs.List()`.
    - In `Dashboard.razor`, when the repository selection changes, call the new service method.
    - Create the `RepositoryColumn.razor` and `WorkflowCard.razor` components to display workflow data.

6.  **Additional Features:**
    - Implement the auto-refresh feature (`RefreshControls.razor`) using a `System.Threading.Timer`.
    - Implement theme toggling (light/dark) and fullscreen mode using JS Interop.

7.  **Polishing:**
    - Add loading indicators throughout the UI.
    - Refine the CSS to match the original application's design.
    - Perform comprehensive testing of all features.
