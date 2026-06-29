namespace RossWright.MetalNexus;

/// <summary>
/// Explicitly marks an endpoint as accessible by unauthenticated callers, overriding any
/// default authentication requirement configured via
/// <see cref="IEndpointSchemaOptions.RequiresAuthenticationByDefault"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AnonymousAttribute : Attribute
{
    /// <summary>Initializes a new <see cref="AnonymousAttribute"/>.</summary>
    public AnonymousAttribute() { }
}
