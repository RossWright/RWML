using Microsoft.AspNetCore.Http;
using RossWright.MetalGuardian.Authorization.Support;
using System.Data;

namespace RossWright.MetalGuardian.Authorization;

internal class EntityAuthorizationService<TPrivilege, TRole> :
    RoleAndPermissionAuthorizationService<TPrivilege, TRole>,
    IEntityAuthorizationApiService<TPrivilege>,
    IEntityAuthorizationService<TPrivilege>
{
    public EntityAuthorizationService(
        IEntityAuthorizationRepository<TPrivilege, TRole> authorizationRepository,
        IHttpContextAccessor httpContextAccessor)
        : base(authorizationRepository, httpContextAccessor) =>
        _authorizationRepository = authorizationRepository;
    private readonly IEntityAuthorizationRepository<TPrivilege, TRole> _authorizationRepository;

    async Task<TPrivilege[]> IEntityAuthorizationApiService<TPrivilege>.GetUserPrivileges(Guid entityId) => 
        (await GetUserPrivileges(entityId)).ToArray();

    public virtual async Task<HashSet<TPrivilege>> GetUserPrivileges(Guid entityId)
    {
        var userId = HttpContextAccessor.GetUserId();
        if (userId == null) return [];
        var rolePrivileges = await _authorizationRepository.GetRolePrivileges();
        var privileges = await GetGlobalRolePrivileges(userId.Value, rolePrivileges);
        await ApplyEntityPrivileges(privileges, entityId, userId.Value, rolePrivileges);
        return privileges;
    }

    protected async Task ApplyEntityPrivileges(HashSet<TPrivilege> privileges, Guid entityId, Guid userId, IDictionary<TRole, TPrivilege[]> rolePrivileges)
    {
        var roles = await _authorizationRepository.GetUserRoles(entityId, userId);
        privileges.AddRange(roles.SelectMany(_ => rolePrivileges[_]));
        var permissions = await _authorizationRepository.GetUserPermissions(entityId, userId);
        privileges.AddRange(permissions.Where(_ => _.IsAllowed).Select(_ => _.Privilege));
        privileges.RemoveRange(permissions.Where(_ => !_.IsAllowed).Select(_ => _.Privilege));
    }

    public async Task<IAuthorizationContext<TPrivilege>> GetContext(Guid securedEntityId) =>
        new AuthorizationContext<TPrivilege>(await GetUserPrivileges(securedEntityId));
}