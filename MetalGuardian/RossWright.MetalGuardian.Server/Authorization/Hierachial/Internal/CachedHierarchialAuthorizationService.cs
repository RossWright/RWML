namespace RossWright.MetalGuardian.Authorization;

internal class CachedHierarchialAuthorizationService<TPrivilege, TRole> :
    CachedEntityAuthorizationService<TPrivilege, TRole>,
    IHierarchialAuthorizationRepository<TPrivilege, TRole>
{
    public CachedHierarchialAuthorizationService(
        IHierarchialAuthorizationRepository<TPrivilege, TRole> repository,
        IHierarchialAuthorizationCache<TPrivilege, TRole> cache) 
        : base(repository, cache) =>
        (_repository, _cache) = (repository, cache);
    private readonly IHierarchialAuthorizationRepository<TPrivilege, TRole> _repository;
    private readonly IHierarchialAuthorizationCache<TPrivilege, TRole> _cache;

    public Task<Guid[]> GetAncestry(Guid securedEntityId) =>
        _cache.GetAncestry(_repository, securedEntityId);
}
