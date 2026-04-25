namespace RossWright.MetalGuardian.Authorization;

internal class CachedRoleAndPermissionAuthorizationRepositoryAdapter<TPrivilege, TRole> :
    CachedRoleOnlyAuthorizationRepositoryAdapter<TPrivilege, TRole>,
    IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>
{
    public CachedRoleAndPermissionAuthorizationRepositoryAdapter(
        IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole> repository,
        IRoleAndPermissionAuthorizationCache<TPrivilege, TRole> cache)
        : base(repository, cache) => 
        (_repository, _cache) = (repository, cache);
    private readonly IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole> _repository;
    private readonly IRoleAndPermissionAuthorizationCache<TPrivilege, TRole> _cache;

    public Task<Permission<TPrivilege>[]> GetUserPermissions(Guid userId) =>
        _cache.GetUserPermissions(_repository, userId);
}
