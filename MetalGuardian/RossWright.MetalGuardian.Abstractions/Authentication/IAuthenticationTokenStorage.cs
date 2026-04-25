namespace RossWright.MetalGuardian;

public interface IAuthenticationTokenStorage
{
    Task<AuthenticationTokens?> LoadTokens(string connectionName,
        CancellationToken cancellationToken = default);

    Task SaveTokens(string connectionName, AuthenticationTokens tokens, 
       CancellationToken cancellationToken = default);

    Task ClearTokens(string connectionName,
       CancellationToken cancellationToken = default);
}
