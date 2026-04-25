using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

internal class EntityAuthorizationClientService<TPrivilege> : GlobalAuthorizationClientService<TPrivilege>, IEntityAuthorizationService<TPrivilege>
{
    public EntityAuthorizationClientService(IEntityAuthorizationApiService<TPrivilege> apiService, string? connectionName)
        : base(apiService, connectionName) => _apiService = apiService;
    private readonly IEntityAuthorizationApiService<TPrivilege> _apiService;

    private Dictionary<Guid, AuthorizationContext<TPrivilege>> _contexts = new();

    public override async Task LoadAuthorizations()
    {
        await base.LoadAuthorizations();
        foreach (var context in _contexts)
        {
            context.Value._privileges = new HashSet<TPrivilege>(
                await _apiService.GetUserPrivileges(context.Key));
        }
    }

    public override void ClearAuthorizations()
    {
        _contexts.Clear();
        base.ClearAuthorizations();
    }

    public async Task<IAuthorizationContext<TPrivilege>> GetContext(Guid securedEntityId)
    {
        if (!_contexts.TryGetValue(securedEntityId, out var context))
        {
            context = new AuthorizationContext<TPrivilege>(
                new HashSet<TPrivilege>(await _apiService.GetUserPrivileges(securedEntityId)));
            _contexts.Add(securedEntityId, context);
        }
        return context!;
    }
}