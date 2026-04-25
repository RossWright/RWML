namespace RossWright.MetalGuardian.Authorization;

internal interface IRoleAndPermissionAuthorizationCache<TPrivilege, TRole> :
    IRoleOnlyAuthorizationCache<TPrivilege, TRole>
{
    Task<Permission<TPrivilege>[]> GetUserPermissions(IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole> repository, Guid userId);
}

internal class RoleAndPermissionAuthorizationCache<TPrivilege, TRole> :
    RoleOnlyAuthorizationCache<TPrivilege, TRole>,
    IRoleAndPermissionAuthorizationCache<TPrivilege, TRole>
{
    public override void BustCache(Guid? userId = null, Guid? entityId = null)
    {
        base.BustCache(userId, entityId);
        if (userId == null)
        {
            _userPermissions.Clear();
        }
        else
        {
            _userPermissions.Remove(userId.Value);
        }
    }

    public async Task<Permission<TPrivilege>[]> GetUserPermissions(IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole> repository, Guid userId)
    {
        if (_userPermissions.TryGetValue(userId, out var permissions)) return permissions;
        permissions = await repository.GetUserPermissions(userId);
        _userPermissions.Add(userId, permissions);
        return permissions;
    }
    private Dictionary<Guid, Permission<TPrivilege>[]> _userPermissions = new();
}