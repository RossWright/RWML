namespace RossWright.MetalGuardian.Authorization;

internal class CachedRoleOnlyAuthorizationRepositoryAdapter<TPrivilege, TRole> : IRoleOnlyAuthorizationRepository<TPrivilege, TRole>
{
    public CachedRoleOnlyAuthorizationRepositoryAdapter(
        IRoleOnlyAuthorizationRepository<TPrivilege, TRole> repository,
        IRoleOnlyAuthorizationCache<TPrivilege, TRole> cache) =>
        (_repository, _cache) = (repository, cache);
    private readonly IRoleOnlyAuthorizationRepository<TPrivilege, TRole> _repository;
    private readonly IRoleOnlyAuthorizationCache<TPrivilege, TRole> _cache;

    public Task<IDictionary<TRole, TPrivilege[]>> GetRolePrivileges() =>
        _cache.GetRolePrivileges(_repository);

    public Task<TRole[]> GetUserRoles(Guid userId) =>
        _cache.GetUserRoles(_repository, userId);
}