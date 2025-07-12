namespace GitActDashNet.Models;

/// <summary>
/// Represents the configuration options for GitHub OAuth authentication.
/// </summary>
public sealed class GitHubOAuthOptions
{
    /// <summary>
    /// The client ID for the GitHub OAuth application.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret for the GitHub OAuth application.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
}