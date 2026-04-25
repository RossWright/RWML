namespace RossWright.MetalGuardian.Authorization;

internal interface IHierarchialAuthorizationCache<TPrivilege, TRole> :
    IEntityAuthorizationCache<TPrivilege, TRole>
{
    Task<Guid[]> GetAncestry(IHierarchialAuthorizationRepository<TPrivilege, TRole> repositoryy, Guid securedEntityId);
}

internal class HierarchialAuthorizationCache<TPrivilege, TRole> :
    EntityAuthorizationCache<TPrivilege, TRole>,
    IHierarchialAuthorizationCache<TPrivilege, TRole>
{
    private Dictionary<Guid, Guid[]> _ancestorCache = new();

    public override void BustCache(Guid? userId = null, Guid? entityId = null)
    {
        base.BustCache(userId, entityId);
        if (entityId == null)
        {
            _ancestorCache.Clear();
        }
        else
        {
            _ancestorCache.Remove(entityId.Value);
        }
    }

    public async Task<Guid[]> GetAncestry(
        IHierarchialAuthorizationRepository<TPrivilege, TRole> repository,
        Guid securedEntityId)
    {
        if (!_ancestorCache.TryGetValue(securedEntityId, out var ancestors))
        {
            ancestors = await repository.GetAncestry(securedEntityId);
            _ancestorCache.Add(securedEntityId, ancestors);
        }
        return ancestors;
    }
}
