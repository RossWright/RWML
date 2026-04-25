namespace RossWright.MetalGuardian.Authorization;

internal class CachedEntityAuthorizationService<TPrivilege, TRole> :
    CachedRoleAndPermissionAuthorizationRepositoryAdapter<TPrivilege, TRole>,
    IEntityAuthorizationRepository<TPrivilege, TRole>
{
    public CachedEntityAuthorizationService(
        IEntityAuthorizationRepository<TPrivilege, TRole> repository,
        IEntityAuthorizationCache<TPrivilege, TRole> cache) 
        : base(repository, cache) =>
        (_repository, _cache) = (repository, cache);
    private readonly IEntityAuthorizationRepository<TPrivilege, TRole> _repository;
    private readonly IEntityAuthorizationCache<TPrivilege, TRole> _cache;
    
    public Task<TRole[]> GetUserRoles(Guid securedEntityId, Guid userId) =>
        _cache.GetUserRoles(_repository, securedEntityId, userId);

    public Task<Permission<TPrivilege>[]> GetUserPermissions(Guid securedEntityId, Guid userId) =>
        _cache.GetUserPermissions(_repository, securedEntityId, userId);
}
