namespace RossWright.MetalGuardian.Authorization;

public interface IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>
    : IRoleOnlyAuthorizationRepository<TPrivilege, TRole>
{
    Task<Permission<TPrivilege>[]> GetUserPermissions(Guid userId);
}
