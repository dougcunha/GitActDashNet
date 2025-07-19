using System.Text.Json.Serialization;
using Octokit;

namespace GitActDashNet.Data;

/// <summary>
/// Modelo combinado para uso na UI que combina dados do Octokit
/// </summary>
public sealed record WorkflowWithLatestRun(
    long WorkflowId,
    string WorkflowName,
    string WorkflowPath,
    string WorkflowState,
    string WorkflowUrl,
    WorkflowRun? LatestRun
);

/// <summary>
/// Modelo para filtros de repositório
/// </summary>
public sealed record RepositoryFilter(
    string SearchText = "",
    RepositoryType Type = RepositoryType.All,
    RepositorySortBy SortBy = RepositorySortBy.Name,
    bool Ascending = true
);

/// <summary>
/// Tipos de repositório para filtro
/// </summary>
public enum RepositoryType
{
    All,
    Personal,
    Organization
}

/// <summary>
/// Critérios de ordenação de repositórios
/// </summary>
public enum RepositorySortBy
{
    Name,
    UpdatedAt
}

/// <summary>
/// Extensões para trabalhar com tipos do Octokit
/// </summary>
public static class OctokitExtensions
{

    /// <summary>
    /// Converte um WorkflowRun do Octokit para um status de exibição simplificado
    /// </summary>
    public static string GetDisplayStatus(this WorkflowRun workflowRun)
    {

        return workflowRun.Status.Value switch
        {
            WorkflowRunStatus.Completed when workflowRun.Conclusion?.Value == WorkflowRunConclusion.Success => "success",
            WorkflowRunStatus.Completed when workflowRun.Conclusion?.Value == WorkflowRunConclusion.Failure => "failure",
            WorkflowRunStatus.Completed when workflowRun.Conclusion?.Value == WorkflowRunConclusion.Cancelled => "cancelled",
            WorkflowRunStatus.InProgress => "in_progress",
            WorkflowRunStatus.Queued => "queued",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Determina se um repositório é pessoal ou de organização
    /// </summary>
    public static RepositoryType GetRepositoryType(this Repository repository)
    {

        return repository.Owner.Type == AccountType.Organization 
            ? RepositoryType.Organization 
            : RepositoryType.Personal;
    }
}
