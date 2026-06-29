namespace RossWright.MetalGuardian;

/// <summary>
/// Manages client-side MetalGuardian authentication state, including login, token refresh,
/// logout, and authentication-change notifications for named connections.
/// </summary>
public interface IMetalGuardianAuthenticationClient
{
    /// <summary>
    /// Login to the server using the user identity and password provided
    /// </summary>
    /// <returns>null if the login failed or authentication information if successful</returns>
    Task<IAuthenticationInformation?> Login(string userIdentity, string password, string? connectionName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the status to authenticated using the authentication tokens provided
    /// </summary>
    /// <returns>authentication information from the tokens provided</returns>
    Task<IAuthenticationInformation?> Login(AuthenticationTokens tokens, string? connectionName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempt to authenticate with the server. This will only connect to the server if the current authentication
    /// has expired or <paramref name="forceRefesh"/> is <c>true</c>.
    /// </summary>
    /// <returns>null if a token was unable to be acquired or authentication information if successful. On failure, login must be called to authenticate to the server</returns>
    Task<IAuthenticationInformation?> Authenticate(string? connectionName = null, bool forceRefesh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if client has unexpired authentication with the server. This will not attempt to re-authenticate to the server.
    /// </summary>
    bool IsAuthenticated(string? connectionName = null);

    /// <summary>
    /// Provides information from the authentication token if available.
    /// Returns <c>null</c> if the user is not logged in.
    /// </summary>
    IAuthenticationInformation? GetUser(string? connectionName = null);

    /// <summary>
    /// Log out from the server and clear authentication information from the local system
    /// </summary>
    Task Logout(string? connectionName = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// This event is signaled whenever the authentication status for any connection has changed
    /// </summary>
    event AuthenticationChangedEventHandler? AuthenticationChanged;
}

/// <summary>
/// Handles an authentication-state change for a MetalGuardian connection.
/// </summary>
/// <param name="connectionName">The named connection whose authentication state changed.</param>
/// <param name="accessToken">The current authentication information, or <c>null</c> when logged out.</param>
/// <param name="cancellationToken">A cancellation token for asynchronous handlers.</param>
public delegate Task AuthenticationChangedEventHandler(string connectionName, IAuthenticationInformation? accessToken, CancellationToken cancellationToken = default);

/// <summary>Convenience extension methods for <see cref="IMetalGuardianAuthenticationClient"/> that omit the <c>connectionName</c> parameter.</summary>
public static class IMetalGuardianAuthenticationClientExtensions
{
    /// <inheritdoc cref="IMetalGuardianAuthenticationClient.Login(string, string, string?, CancellationToken)" />
    public static Task<IAuthenticationInformation?> Login(this IMetalGuardianAuthenticationClient client, string userIdentity, string password, CancellationToken cancellationToken = default)
        => client.Login(userIdentity, password, null, cancellationToken);
    /// <inheritdoc cref="IMetalGuardianAuthenticationClient.Login(AuthenticationTokens, string?, CancellationToken)" />
    public static Task<IAuthenticationInformation?> Login(this IMetalGuardianAuthenticationClient client, AuthenticationTokens tokens, CancellationToken cancellationToken = default)
        => client.Login(tokens, null, cancellationToken);
    /// <inheritdoc cref="IMetalGuardianAuthenticationClient.Authenticate(string?, bool, CancellationToken)" />
    public static Task<IAuthenticationInformation?> Authenticate(this IMetalGuardianAuthenticationClient client, bool forceRefesh = false, CancellationToken cancellationToken = default)
        => client.Authenticate(null, forceRefesh, cancellationToken);
    /// <inheritdoc cref="IMetalGuardianAuthenticationClient.Logout(string?, CancellationToken)" />
    public static Task Logout(this IMetalGuardianAuthenticationClient client, CancellationToken cancellationToken = default)
        => client.Logout(null, cancellationToken);
}
