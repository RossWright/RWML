namespace RossWright.MetalGuardian;

/// <summary>
/// Implement this interface to target a non-MetalNexus authentication backend.
/// MetalGuardian calls these methods to perform login, logout, and token refresh
/// against a custom server API. The connection name passed to each method is never <c>null</c>
/// at call sites.
/// </summary>
public interface IAuthenticationApiService
{
    /// <summary>Authenticates the user with the given credentials and returns tokens on success, or <c>null</c> on failure.</summary>
    Task<AuthenticationTokens?> Login(string userIdentity, string password, 
        string connectionName, CancellationToken cancellationToken = default);

    /// <summary>Logs out the authenticated user and invalidates the provided tokens on the server.</summary>
    Task Logout(AuthenticationTokens tokens,
        string connectionName, CancellationToken cancellationToken = default);

    /// <summary>Refreshes the access token using the provided refresh token and returns new tokens on success, or <c>null</c> if the refresh token is invalid or expired.</summary>
    Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens,
        string connectionName, CancellationToken cancellationToken = default);
}
