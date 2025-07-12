namespace GitActDashNet.Services;

/// <summary>
/// Service to retrieve the GitHub OAuth token from cookies.
/// </summary>
/// <param name="httpContextAccessor">
/// The HTTP context accessor to access the current HTTP context.
/// </param>
public sealed class CookieTokenProvider(IHttpContextAccessor httpContextAccessor)
{
    private const string TOKEN_COOKIE_NAME = "github_token";

    /// <summary>
    /// Gets the GitHub OAuth token from the cookies.
    /// </summary>
    /// <returns>
    /// The GitHub OAuth token if it exists in the cookies; otherwise, null.
    /// </returns>
    public string? GetToken()
    {
        var context = httpContextAccessor.HttpContext;

        return context == null
            ? null
            : context.Request.Cookies.TryGetValue(TOKEN_COOKIE_NAME, out var token) ? token : null;
    }
}