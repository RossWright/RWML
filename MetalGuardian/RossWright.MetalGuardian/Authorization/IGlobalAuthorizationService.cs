namespace RossWright.MetalGuardian.Authorization;

public interface IGlobalAuthorizationService<TPrivilege>
{
    Task<IAuthorizationContext<TPrivilege>> GetContext();
}

public static class IGlobalAuthorizationServiceExtensions
{
    public static async Task<bool> MayUserDo<TPrivilege>(this IGlobalAuthorizationService<TPrivilege> service, TPrivilege privilege) =>
        (await service.GetContext()).MayUserDo(privilege);

    public static async Task<bool> MayUserDoAny<TPrivilege>(this IGlobalAuthorizationService<TPrivilege> service, params TPrivilege[] privileges) =>
        (await service.GetContext()).MayUserDoAny(privileges);

    public static async Task<bool> MayUserDoAll<TPrivilege>(this IGlobalAuthorizationService<TPrivilege> service, params TPrivilege[] privileges) =>
        (await service.GetContext()).MayUserDoAll(privileges);
}