using Microsoft.AspNetCore.Http;

namespace RossWright.MetalGuardian.Authorization;

internal class RoleAndPermissionAuthorizationService<TPrivilege, TRole> :
    RoleOnlyAuthorizationApiService<TPrivilege, TRole>
{
    public RoleAndPermissionAuthorizationService(
        IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole> authorizationRepository,
        IHttpContextAccessor httpContextAccessor) 
        : base(authorizationRepository, httpContextAccessor) =>
        _authorizationRepository = authorizationRepository;
    private readonly IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole> _authorizationRepository;

    protected override async Task<HashSet<TPrivilege>> GetGlobalRolePrivileges(Guid userId, IDictionary<TRole, TPrivilege[]> rolePrivileges)
    {
        var privileges = await base.GetGlobalRolePrivileges(userId, rolePrivileges);
        var permissions = await _authorizationRepository.GetUserPermissions(userId);
        if (permissions.Any())
        {
            privileges.AddRange(permissions.Where(_ => _.IsAllowed).Select(_ => _.Privilege));
            privileges.RemoveRange(permissions.Where(_ => !_.IsAllowed).Select(_ => _.Privilege));
        }
        return privileges;
    }
}
