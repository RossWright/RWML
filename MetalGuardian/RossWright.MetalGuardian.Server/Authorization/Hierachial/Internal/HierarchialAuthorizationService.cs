using Microsoft.AspNetCore.Http;

namespace RossWright.MetalGuardian.Authorization;

internal class HierarchialAuthorizationService<TPrivilege, TRole> :
    EntityAuthorizationService<TPrivilege, TRole>
{
    public HierarchialAuthorizationService(
        IHierarchialAuthorizationRepository<TPrivilege, TRole> authorizationRepository,
        IHttpContextAccessor httpContextAccessor)
        : base(authorizationRepository, httpContextAccessor) =>
        _authorizationRepository = authorizationRepository;
    private readonly IHierarchialAuthorizationRepository<TPrivilege, TRole> _authorizationRepository;

    public override async Task<HashSet<TPrivilege>> GetUserPrivileges(Guid entityId)
    {
        var userId = HttpContextAccessor.GetUserId();
        if (userId == null) return [];
        var rolePrivileges = await _authorizationRepository.GetRolePrivileges();
        var privileges = await GetGlobalRolePrivileges(userId.Value, rolePrivileges);
        var ancestorIds = await _authorizationRepository.GetAncestry(entityId);
        foreach (var ancestorId in ancestorIds)
        {
            await ApplyEntityPrivileges(privileges, ancestorId, userId.Value, rolePrivileges);
        }
        await ApplyEntityPrivileges(privileges, entityId, userId.Value, rolePrivileges);
        return privileges;
    }
}