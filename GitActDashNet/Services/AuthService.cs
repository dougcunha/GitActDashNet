using Microsoft.AspNetCore.Components;

namespace GitActDashNet.Services;

/// <summary>
/// Service to manage authentication state in the application.
/// </summary>
/// <param name="cookieTokenProvider">
/// The provider for managing cookie-based tokens.
/// </param>
/// <param name="navigationManager">
/// The navigation manager to redirect users when authentication is required.
/// </param>
public sealed class AuthService(CookieTokenProvider cookieTokenProvider, NavigationManager navigationManager)
{
    /// <summary>
    /// Gets if the user is authenticated by checking if a valid token exists in cookies.
    /// </summary>
    /// <returns>
    /// true if the user is authenticated, false otherwise.
    /// </returns>
    public Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var token = cookieTokenProvider.GetToken();

            return Task.FromResult(!string.IsNullOrWhiteSpace(token));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Redirects the user to the login page if they are not authenticated.
    /// </summary>
    public async Task RequireAuthenticationAsync()
    {
        if (!await IsAuthenticatedAsync())
            navigationManager.NavigateTo("/login", replace: true);
    }
}
