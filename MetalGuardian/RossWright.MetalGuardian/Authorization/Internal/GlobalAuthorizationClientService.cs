using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

internal class GlobalAuthorizationClientService<TPrivilege> :
    IGlobalAuthorizationService<TPrivilege>, IAuthenticationAuthorizationConnection
{
    public GlobalAuthorizationClientService(IGlobalAuthorizationApiService<TPrivilege> apiService, string? connectionName) => 
        (_apiService, ConnectionName) = (apiService, connectionName);
    private readonly IGlobalAuthorizationApiService<TPrivilege> _apiService;
    protected HashSet<TPrivilege> _privileges = new();

    public string? ConnectionName { get; }

    public virtual async Task LoadAuthorizations() =>
        _privileges = new(await _apiService.GetUserPrivileges());

    public virtual void ClearAuthorizations() => _privileges.Clear();

    public Task<IAuthorizationContext<TPrivilege>> GetContext() =>
        Task.FromResult<IAuthorizationContext<TPrivilege>>(
            new AuthorizationContext<TPrivilege>(_privileges));
}
