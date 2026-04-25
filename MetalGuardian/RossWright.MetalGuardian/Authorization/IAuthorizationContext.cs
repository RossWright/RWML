namespace RossWright.MetalGuardian.Authorization;

public interface IAuthorizationContext<TPrivilege>
{
    bool MayUserDo(TPrivilege privilege);
}

public static class IAuthorizationContextExtensions
{
    public static bool MayUserDoAny<TPrivilege>(this IAuthorizationContext<TPrivilege> service, params TPrivilege[] privileges) =>
        privileges.Any(_ => service.MayUserDo(_));

    public static bool MayUserDoAll<TPrivilege>(this IAuthorizationContext<TPrivilege> service, params TPrivilege[] privileges) =>
        privileges.All(_ => service.MayUserDo(_));
}

public class AuthorizationContext<TPrivilege> : IAuthorizationContext<TPrivilege>
{
    public AuthorizationContext(HashSet<TPrivilege> privileges) => _privileges = privileges;
    internal HashSet<TPrivilege> _privileges;
    public bool MayUserDo(TPrivilege privilege) => _privileges.Contains(privilege);
}
