namespace RossWright.MetalGuardian.Authorization;

public interface IEntityAuthorizationRepository<TPrivilege, TRole>  // Role + Permission on Entities
    : IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>
{
    Task<TRole[]> GetUserRoles(Guid securedEntityId, Guid userId);
    Task<Permission<TPrivilege>[]> GetUserPermissions(Guid securedEntityId, Guid userId);
}
