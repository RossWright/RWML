using Microsoft.AspNetCore.Http;
using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

internal class RoleOnlyAuthorizationApiService<TPrivilege, TRole> :
    IGlobalAuthorizationApiService<TPrivilege>,
    IGlobalAuthorizationService<TPrivilege>
{
    public RoleOnlyAuthorizationApiService(
        IRoleOnlyAuthorizationRepository<TPrivilege, TRole> authorizationRepository,
        IHttpContextAccessor httpContextAccessor) =>
        (_authorizationRepository, HttpContextAccessor) =
        (authorizationRepository, httpContextAccessor);
    private readonly IRoleOnlyAuthorizationRepository<TPrivilege, TRole> _authorizationRepository;
    protected IHttpContextAccessor HttpContextAccessor { get; }

    async Task<TPrivilege[]> IGlobalAuthorizationApiService<TPrivilege>.GetUserPrivileges() => 
        (await GetUserPrivileges()).ToArray();

    public virtual async Task<HashSet<TPrivilege>> GetUserPrivileges()
    {
        var userId = HttpContextAccessor.GetUserId();
        if (userId == null) return [];
        var rolePrivileges = await _authorizationRepository.GetRolePrivileges();
        return await GetGlobalRolePrivileges(userId.Value, rolePrivileges);
    }

    protected virtual async Task<HashSet<TPrivilege>> GetGlobalRolePrivileges(Guid userId,
        IDictionary<TRole, TPrivilege[]> rolePrivileges)
    {
        var roles = await _authorizationRepository.GetUserRoles(userId);
        return roles.SelectMany(role => rolePrivileges[role]).Distinct().ToHashSet();
    }

    public async Task<IAuthorizationContext<TPrivilege>> GetContext() =>
        new AuthorizationContext<TPrivilege>(await GetUserPrivileges());
}
