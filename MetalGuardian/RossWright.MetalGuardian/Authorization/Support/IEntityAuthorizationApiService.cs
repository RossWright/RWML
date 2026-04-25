namespace RossWright.MetalGuardian.Authorization.Support;

public interface IEntityAuthorizationApiService<TPrivilege> : IGlobalAuthorizationApiService<TPrivilege>
{
    Task<TPrivilege[]> GetUserPrivileges(Guid entityId);
}

