namespace RossWright.MetalGuardian.Authorization;

internal interface IEntityAuthorizationCache<TPrivilege, TRole> :
    IRoleAndPermissionAuthorizationCache<TPrivilege, TRole>
{
    Task<TRole[]> GetUserRoles(IEntityAuthorizationRepository<TPrivilege, TRole> repository, Guid securedEntityId, Guid userId);
    Task<Permission<TPrivilege>[]> GetUserPermissions(IEntityAuthorizationRepository<TPrivilege, TRole> repository, Guid securedEntityId, Guid userId);
}

internal class EntityAuthorizationCache<TPrivilege, TRole> :
    RoleAndPermissionAuthorizationCache<TPrivilege, TRole>,
    IEntityAuthorizationCache<TPrivilege, TRole>
{
    public override void BustCache(Guid? userId = null, Guid? entityId = null)
    {
        base.BustCache(userId, entityId);
        if (entityId == null)
        {
            if (userId == null)
            {
                _entityUserRoles.Clear();
                _entityUserPermissions.Clear();
            }
            else
            {
                foreach (var entityUserRoles in _entityUserRoles.Values)
                    entityUserRoles.Remove(userId.Value);
                foreach (var entityUserPermissions in _entityUserPermissions.Values)
                    entityUserPermissions.Remove(userId.Value);
            }
        }
        else
        {
            if (userId == null)
            {
                _entityUserRoles.Remove(entityId.Value);
                _entityUserPermissions.Remove(entityId.Value);
            }
            else
            {
                if (_entityUserRoles.TryGetValue(entityId.Value, out var entityUserRoles))
                    entityUserRoles.Remove(userId.Value);
                if (_entityUserPermissions.TryGetValue(entityId.Value, out var entityUserPermissions))
                    entityUserPermissions.Remove(userId.Value);
            }
        }
    }

    public async Task<TRole[]> GetUserRoles(
        IEntityAuthorizationRepository<TPrivilege, TRole> repository,
        Guid securedEntityId, Guid userId)
    {
        if (!_entityUserRoles.TryGetValue(securedEntityId, out var userRoles))
        {
            _entityUserRoles[securedEntityId] = userRoles = new();
        }
        if (userRoles.TryGetValue(userId, out var roles))
        {
            return roles;
        }
        roles = await repository.GetUserRoles(securedEntityId, userId);
        userRoles.Add(userId, roles);
        return roles;
    }
    private Dictionary<Guid, Dictionary<Guid, TRole[]>> _entityUserRoles = new();

    public async Task<Permission<TPrivilege>[]> GetUserPermissions(
        IEntityAuthorizationRepository<TPrivilege, TRole> repository,
        Guid securedEntityId, Guid userId)
    {
        if (!_entityUserPermissions.TryGetValue(securedEntityId, out var userPermissions))
        {
            _entityUserPermissions[securedEntityId] = userPermissions = new();
        }
        if (userPermissions.TryGetValue(userId, out var permissions))
        {
            return permissions;
        }
        permissions = await repository.GetUserPermissions(securedEntityId, userId);
        userPermissions.Add(userId, permissions);
        return permissions;
    }
    private Dictionary<Guid, Dictionary<Guid, Permission<TPrivilege>[]>> _entityUserPermissions = new();
}
