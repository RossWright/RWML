namespace RossWright.MetalGuardian.Authorization;

public interface IRoleOnlyAuthorizationRepository<TPrivilege, TRole>
{
    Task<IDictionary<TRole, TPrivilege[]>> GetRolePrivileges();
    Task<TRole[]> GetUserRoles(Guid userId);
}
