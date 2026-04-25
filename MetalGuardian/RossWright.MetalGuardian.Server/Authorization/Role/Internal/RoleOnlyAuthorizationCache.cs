namespace RossWright.MetalGuardian.Authorization;

internal interface IRoleOnlyAuthorizationCache<TPrivilege, TRole> : IAuthorizationCache
{
    Task<IDictionary<TRole, TPrivilege[]>> GetRolePrivileges(IRoleOnlyAuthorizationRepository<TPrivilege, TRole> repository);
    Task<TRole[]> GetUserRoles(IRoleOnlyAuthorizationRepository<TPrivilege, TRole> repository, Guid userId);
}

internal class RoleOnlyAuthorizationCache<TPrivilege, TRole> : 
    IRoleOnlyAuthorizationCache<TPrivilege, TRole>
{
    public virtual void BustCache(Guid? userId = null, Guid? entityId = null)
    {
        if (userId == null)
        {
            _rolePrivileges = null;
            _userRoles.Clear();
        }
        else
        {
            _userRoles.Remove(userId.Value);
        }
    }

    public async Task<IDictionary<TRole, TPrivilege[]>> GetRolePrivileges(
        IRoleOnlyAuthorizationRepository<TPrivilege, TRole> repository) =>
        _rolePrivileges ?? (_rolePrivileges = await repository.GetRolePrivileges());
    private IDictionary<TRole, TPrivilege[]>? _rolePrivileges = null;

    public async Task<TRole[]> GetUserRoles(
        IRoleOnlyAuthorizationRepository<TPrivilege, TRole> repository, Guid userId)
    {
        if (_userRoles.TryGetValue(userId, out var userRoles)) return userRoles;
        userRoles = await repository.GetUserRoles(userId);
        _userRoles.Add(userId, userRoles);
        return userRoles;
    }
    private Dictionary<Guid, TRole[]> _userRoles = new();
}
