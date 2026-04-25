namespace RossWright.MetalGuardian.Authorization;

public class Permission<TPrivilege>
{
    public Permission(TPrivilege privilege, bool isAllowed) =>
        (Privilege, IsAllowed) = (privilege, isAllowed);
    public TPrivilege Privilege { get; }
    public bool IsAllowed { get; }
}
