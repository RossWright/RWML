namespace RossWright.MetalGuardian;

public interface IMetalGuardianAuthenticationService
{
    Task<AuthenticationTokens> Login(string userIdentity, string password, string? deviceFingerprint, CancellationToken cancellationToken);
    Task<AuthenticationTokens> Login(IAuthenticationUser user, CancellationToken cancellationToken);
    Task Logout(AuthenticationTokens tokens, CancellationToken cancellationToken);
    Task<AuthenticationTokens> Refresh(AuthenticationTokens tokens, CancellationToken cancellationToken);
}
