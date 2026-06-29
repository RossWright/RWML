namespace RossWright.MetalNexus;

/// <summary>
/// Requires that the caller be authenticated before the endpoint handler is invoked.
/// Optionally restricts access to specific roles or an authorization policy.
/// </summary>
/// <remarks>
/// When applied without arguments, any authenticated user is allowed.  Pass one or more
/// role values to restrict access to users who hold at least one of those roles.
/// Use <see cref="Policy"/> to delegate to a named ASP.NET Core authorization policy instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class AuthenticatedAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="AuthenticatedAttribute"/>.
    /// </summary>
    /// <param name="authorizedRoles">
    /// Zero or more role values (string or enum) that are permitted to call this endpoint.
    /// When empty, all authenticated users are allowed regardless of role.
    /// </param>
    public AuthenticatedAttribute(params object[] authorizedRoles)
    {
        AuthorizedRoles = !authorizedRoles.Any() ? null :
            authorizedRoles.Select(_ => _.ToString()!).ToArray();
    }
    /// <summary>
    /// The roles permitted to call this endpoint, or <c>null</c> when no role restriction is applied.
    /// </summary>
    public string[]? AuthorizedRoles { get; protected set; }
    /// <summary>
    /// When <c>true</c>, users whose authentication is marked as provisional (e.g. pending MFA)
    /// are also allowed to call this endpoint.
    /// </summary>
    public bool AllowProvisional { get; set; }
    /// <summary>
    /// An optional named ASP.NET Core authorization policy to evaluate instead of (or in addition to)
    /// the <see cref="AuthorizedRoles"/> list.
    /// </summary>
    public string? Policy { get; set; }
}