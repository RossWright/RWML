namespace RossWright.MetalGuardian.Authorization.Support;

public interface IGlobalAuthorizationApiService<TPrivilege>
{
    Task<TPrivilege[]> GetUserPrivileges();
}

