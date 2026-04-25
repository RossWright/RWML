namespace RossWright.MetalGuardian.Authorization;

internal interface IAuthenticationAuthorizationConnection
{
    string? ConnectionName { get; }
    Task LoadAuthorizations();
    void ClearAuthorizations();
}
