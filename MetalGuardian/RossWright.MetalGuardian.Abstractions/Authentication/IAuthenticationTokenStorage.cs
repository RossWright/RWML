namespace RossWright.MetalGuardian;

/// <summary>
/// Optional service for persisting authentication tokens across app restarts.
/// Without an implementation registered, tokens are held only in memory and are
/// lost when the application restarts.
/// </summary>
public interface IAuthenticationTokenStorage
{
    /// <summary>Loads the persisted tokens for the specified connection, or <c>null</c> if none are stored.</summary>
    Task<AuthenticationTokens?> LoadTokens(string connectionName,
        CancellationToken cancellationToken = default);

    /// <summary>Persists the tokens for the specified connection.</summary>
    Task SaveTokens(string connectionName, AuthenticationTokens tokens, 
       CancellationToken cancellationToken = default);

    /// <summary>Removes any persisted tokens for the specified connection.</summary>
    Task ClearTokens(string connectionName,
       CancellationToken cancellationToken = default);
}
