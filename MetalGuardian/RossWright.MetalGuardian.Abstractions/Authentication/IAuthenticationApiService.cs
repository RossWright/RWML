namespace RossWright.MetalGuardian;

public interface IAuthenticationApiService
{
    Task<AuthenticationTokens?> Login(string userIdentity, string password, 
        string connectionName, CancellationToken cancellationToken = default);
    
    Task Logout(AuthenticationTokens tokens,
        string connectionName, CancellationToken cancellationToken = default);

    Task<AuthenticationTokens?> Refresh(AuthenticationTokens tokens,
        string connectionName, CancellationToken cancellationToken = default);
}
