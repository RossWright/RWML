namespace RossWright.MetalGuardian;

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
    /// Attempt to authenticate with the server. This will only connect to the server if the current authentication has expired
    /// </summary>
    /// <returns>null if a token was unable to be aquired or authentication information if sucessful. On failure, login must be called to authenticate to the server</returns>
    Task<IAuthenticationInformation?> Authenticate(string? connectionName = null, bool forceRefesh = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if client has unexpired authentication with the server. This will not attempt to re-authenticate to the server.
    /// </summary>
    bool IsAuthenticated(string? connectionName = null);

    /// <summary>
    /// Provides information from the authentication token if available.
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

public delegate Task AuthenticationChangedEventHandler(string connectionName, IAuthenticationInformation? accessToken, CancellationToken cancellationToken = default);

public static class IMetalGuardianAuthenticationClientExtensions
{
    public static Task<IAuthenticationInformation?> Login(this IMetalGuardianAuthenticationClient client, string userIdentity, string password, CancellationToken cancellationToken = default)
        => client.Login(userIdentity, password, null, cancellationToken);
    public static Task<IAuthenticationInformation?> Login(this IMetalGuardianAuthenticationClient client, AuthenticationTokens tokens, CancellationToken cancellationToken = default)
        => client.Login(tokens, null, cancellationToken);
    public static Task<IAuthenticationInformation?> Authenticate(this IMetalGuardianAuthenticationClient client, bool forceRefesh = false, CancellationToken cancellationToken = default)
        => client.Authenticate(null, forceRefesh, cancellationToken);
    public static Task Logout(this IMetalGuardianAuthenticationClient client, CancellationToken cancellationToken = default)
        => client.Logout(null, cancellationToken);
}