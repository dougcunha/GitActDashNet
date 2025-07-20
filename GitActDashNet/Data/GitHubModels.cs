using Octokit;

namespace GitActDashNet.Data;

/// <summary>
/// Model combined for UI use that combines Octokit data
/// </summary>
public sealed record WorkflowWithLatestRun
(
    long WorkflowId,
    string WorkflowName,
    string WorkflowPath,
    string WorkflowState,
    string WorkflowUrl,
    WorkflowRun? LatestRun
);

/// <summary>
/// Model for repository filters
/// </summary>
public sealed record RepositoryFilter
(
    string SearchText = "",
    RepositoryType Type = RepositoryType.All,
    RepositorySortBy SortBy = RepositorySortBy.Name,
    bool Ascending = true
);

/// <summary>
/// Repositories types for filtering
/// </summary>
public enum RepositoryType
{
    All,
    Personal,
    Organization
}

/// <summary>
/// Repositories sorting criteria
/// </summary>
public enum RepositorySortBy
{
    Name,
    UpdatedAt
}

/// <summary>
/// Extensions for working with Octokit types
/// </summary>
public static class OctokitExtensions
{
    /// <summary>
    /// Converts an Octokit WorkflowRun to a simplified display status
    /// </summary>
    public static string GetDisplayStatus(this WorkflowRun workflowRun)
        => workflowRun.Status.Value switch
        {
            WorkflowRunStatus.Completed when workflowRun.Conclusion?.Value == WorkflowRunConclusion.Success => "success",
            WorkflowRunStatus.Completed when workflowRun.Conclusion?.Value == WorkflowRunConclusion.Failure => "failure",
            WorkflowRunStatus.Completed when workflowRun.Conclusion?.Value == WorkflowRunConclusion.Cancelled => "cancelled",
            WorkflowRunStatus.InProgress => "in_progress",
            WorkflowRunStatus.Queued => "queued",
            _ => "unknown"
        };

    /// <summary>
    /// Determines if a repository is personal or organization
    /// </summary>
    public static RepositoryType GetRepositoryType(this Repository repository)
        => repository.Owner.Type == AccountType.Organization
            ? RepositoryType.Organization
            : RepositoryType.Personal;
}
