using GitActDashNet.Data;
using GitActDashNet.Utils;
using Octokit;

namespace GitActDashNet.Services;

/// <summary>
/// Service to interact with the GitHub API using Octokit.NET.
/// </summary>
public sealed class GitHubService
(
    IGitHubClient gitHubClient,
    ILogger<GitHubService> logger
)
{
    /// <summary>
    /// Gets all repositories for the current user, including personal and organization repositories.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result containing a read-only list of repositories.</returns>
    public async Task<OperationResult<IReadOnlyList<Repository>>> GetUserRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        using var _ = logger.ForServiceOperation(nameof(GitHubService), nameof(GetUserRepositoriesAsync));
        using var timer = logger.TimeOperation("GetUserRepositories");

        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Operation was cancelled before starting");

            return OperationResult<IReadOnlyList<Repository>>.Success([]);
        }

        try
        {
            logger.LogInformation("Starting to fetch user repositories");
            var allRepos = new List<Repository>();

            var userRepos = await gitHubClient.Repository.GetAllForCurrent().ConfigureAwait(false);
            allRepos.AddRange(userRepos);
            logger.LogDebug("Fetched {Count} personal repositories", userRepos.Count);

            var organizations = await gitHubClient.Organization.GetAllForCurrent().ConfigureAwait(false);
            logger.LogDebug("Found {Count} organizations", organizations.Count);

            foreach (var org in organizations)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    logger.LogWarning("Operation was cancelled while fetching organization repositories");

                    break;
                }

                using var orgContext = logger.ForGitHubOperation
                (
                    "GetOrgRepositories",
                    organization: org.Login
                );

                var orgRepos = await gitHubClient.Repository.GetAllForOrg(org.Login).ConfigureAwait(false);
                allRepos.AddRange(orgRepos);

                logger.LogDebug
                (
                    "Fetched {Count} repositories from organization {Organization}",
                    orgRepos.Count,
                    org.Login
                );
            }

            var distinctRepos = allRepos.DistinctBy(r => r.Id).ToArray();

            logger.LogInformation
            (
                "Successfully fetched {TotalCount} repositories ({UniqueCount} unique)",
                allRepos.Count,
                distinctRepos.Length
            );

            return OperationResult<IReadOnlyList<Repository>>.Success
            (
                distinctRepos
            );
        }
        catch (RateLimitExceededException ex)
        {
            logger.LogWarning("GitHub API rate limit exceeded. Reset at: {ResetTime}", ex.Reset);

            return OperationResult<IReadOnlyList<Repository>>.Failure($"GitHub API rate limit exceeded. Reset at: {ex.Reset}");
        }
        catch (AuthorizationException ex)
        {
            logger.LogError(ex, "Authorization failed while fetching repositories");

            return OperationResult<IReadOnlyList<Repository>>.Failure("Authorization failed. Please check your GitHub access token.");
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "GitHub API error while fetching repositories: {StatusCode}", ex.HttpResponse?.StatusCode);

            return OperationResult<IReadOnlyList<Repository>>.Failure($"GitHub API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while fetching repositories");

            return OperationResult<IReadOnlyList<Repository>>.Failure($"Unexpected error while fetching repositories: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all workflows for a given repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result containing a read-only list of workflows.</returns>
    public async Task<OperationResult<IReadOnlyList<Workflow>>> GetWorkflowsAsync
    (
        string owner,
        string repoName,
        CancellationToken cancellationToken = default
    )
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<IReadOnlyList<Workflow>>.Success([]);

        if (string.IsNullOrWhiteSpace(owner))
            return OperationResult<IReadOnlyList<Workflow>>.Failure("Repository owner cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(repoName))
            return OperationResult<IReadOnlyList<Workflow>>.Failure("Repository name cannot be null or empty.");

        try
        {
            var workflows = await gitHubClient.Actions.Workflows.List(owner, repoName).ConfigureAwait(false);

            return OperationResult<IReadOnlyList<Workflow>>.Success(workflows.Workflows);
        }
        catch (NotFoundException)
        {
            return OperationResult<IReadOnlyList<Workflow>>.Failure($"Repository '{owner}/{repoName}' not found or you don't have access to it.");
        }
        catch (RateLimitExceededException ex)
        {
            return OperationResult<IReadOnlyList<Workflow>>.Failure($"GitHub API rate limit exceeded. Reset at: {ex.Reset}");
        }
        catch (AuthorizationException)
        {
            return OperationResult<IReadOnlyList<Workflow>>.Failure("Authorization failed. Please check your GitHub access token.");
        }
        catch (ApiException ex)
        {
            return OperationResult<IReadOnlyList<Workflow>>.Failure($"GitHub API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return OperationResult<IReadOnlyList<Workflow>>.Failure($"Unexpected error while fetching workflows for '{owner}/{repoName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all workflows for a repository, including their latest run if available.
    /// </summary>
    /// <param name="repository">The repository to get workflows for.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result containing a read-only list of workflows with their latest run.</returns>
    public async Task<OperationResult<IReadOnlyList<WorkflowWithLatestRun>>> GetWorkflowsWithLatestRunsAsync
    (
        Repository repository,
        CancellationToken cancellationToken = default
    )
    {
        if (repository.Owner?.Login is null)
            return OperationResult<IReadOnlyList<WorkflowWithLatestRun>>.Failure("Repository or repository owner cannot be null.");

        var workflowsResult = await GetWorkflowsAsync(repository.Owner.Login, repository.Name, cancellationToken).ConfigureAwait(false);

        if (workflowsResult.IsFailure)
            return OperationResult<IReadOnlyList<WorkflowWithLatestRun>>.Failure(workflowsResult.ErrorMessage);

        var workflowsWithRuns = new List<WorkflowWithLatestRun>();
        var warnings = new List<string>();

        foreach (var workflow in workflowsResult.Value.TakeWhile(_ => !cancellationToken.IsCancellationRequested))
        {
            var latestRunResult = await GetLatestWorkflowRunAsync
            (
                repository.Owner.Login,
                repository.Name,
                workflow.Id,
                cancellationToken
            ).ConfigureAwait(false);

            WorkflowRun? latestRun = null;

            if (latestRunResult.IsFailure)
                warnings.Add($"Failed to get latest run for workflow '{workflow.Name}': {latestRunResult.ErrorMessage}");
            else
                latestRun = latestRunResult.Value;

            var workflowWithRun = new WorkflowWithLatestRun
            (
                workflow.Id,
                workflow.Name,
                workflow.Path,
                workflow.State.ToString(),
                workflow.HtmlUrl,
                latestRun
            );

            workflowsWithRuns.Add(workflowWithRun);
        }

        return warnings.Count > 0
            ? OperationResult<IReadOnlyList<WorkflowWithLatestRun>>.Warning(workflowsWithRuns, [.. warnings])
            : OperationResult<IReadOnlyList<WorkflowWithLatestRun>>.Success(workflowsWithRuns);
    }

    /// <summary>
    /// Gets the latest run for a specific workflow in a repository.
    /// </summary>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="workflowId">The ID of the workflow.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An operation result containing the latest workflow run, or null if not found.</returns>
    public async Task<OperationResult<WorkflowRun?>> GetLatestWorkflowRunAsync
    (
        string owner,
        string repoName,
        long workflowId,
        CancellationToken cancellationToken = default
    )
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<WorkflowRun?>.Success(null);

        if (string.IsNullOrWhiteSpace(owner))
            return OperationResult<WorkflowRun?>.Failure("Repository owner cannot be null or empty.");

        if (string.IsNullOrWhiteSpace(repoName))
            return OperationResult<WorkflowRun?>.Failure("Repository name cannot be null or empty.");

        try
        {
            var runs = await gitHubClient.Actions.Workflows.Runs.ListByWorkflow
            (
                owner,
                repoName,
                workflowId
            ).ConfigureAwait(false);

            var latestRun = runs.WorkflowRuns.Count > 0
                ? runs.WorkflowRuns[0]
                : null;

            return OperationResult<WorkflowRun?>.Success
            (
                latestRun
            );
        }
        catch (NotFoundException)
        {
            return OperationResult<WorkflowRun?>.Failure($"Workflow with ID '{workflowId}' not found in repository '{owner}/{repoName}'.");
        }
        catch (RateLimitExceededException ex)
        {
            return OperationResult<WorkflowRun?>.Failure($"GitHub API rate limit exceeded. Reset at: {ex.Reset}");
        }
        catch (AuthorizationException)
        {
            return OperationResult<WorkflowRun?>.Failure("Authorization failed. Please check your GitHub access token.");
        }
        catch (ApiException ex)
        {
            return OperationResult<WorkflowRun?>.Failure($"GitHub API error: {ex.Message}");
        }
        catch (Exception ex)
        {
            return OperationResult<WorkflowRun?>.Failure($"Unexpected error while fetching latest run for workflow '{workflowId}' in '{owner}/{repoName}': {ex.Message}");
        }
    }
}
