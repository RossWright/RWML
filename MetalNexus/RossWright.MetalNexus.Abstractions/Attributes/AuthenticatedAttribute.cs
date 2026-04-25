namespace RossWright.MetalNexus;

[AttributeUsage(AttributeTargets.Class)]
public class AuthenticatedAttribute : Attribute
{
    public AuthenticatedAttribute(params object[] authorizedRoles)
    {
        AuthorizedRoles = !authorizedRoles.Any() ? null :
            authorizedRoles.Select(_ => _.ToString()!).ToArray();
    }
    public string[]? AuthorizedRoles { get; protected set; }
    public bool AllowProvisional { get; set; }
}