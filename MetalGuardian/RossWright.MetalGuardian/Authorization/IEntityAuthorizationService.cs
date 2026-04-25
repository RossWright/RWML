namespace RossWright.MetalGuardian.Authorization;

public interface IEntityAuthorizationService<TPrivilege> :
    IGlobalAuthorizationService<TPrivilege>
{
    Task<IAuthorizationContext<TPrivilege>> GetContext(Guid securedEntityId);
}

public static class IAdvancedAuthorizationServiceExtensions
{
    public static async Task<bool> MayUserDo<TPrivilege>(this IEntityAuthorizationService<TPrivilege> service, Guid securedEntityId, TPrivilege privilege) =>
        (await service.GetContext(securedEntityId)).MayUserDo(privilege);

    public static async Task<bool> MayUserDoAny<TPrivilege>(this IEntityAuthorizationService<TPrivilege> service, Guid securedEntityId, params TPrivilege[] privileges) =>
        (await service.GetContext(securedEntityId)).MayUserDoAny(privileges);

    public static async Task<bool> MayUserDoAll<TPrivilege>(this IEntityAuthorizationService<TPrivilege> service, Guid securedEntityId, params TPrivilege[] privileges) =>
        (await service.GetContext(securedEntityId)).MayUserDoAll(privileges);
}
