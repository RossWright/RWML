using System.Reflection;

namespace RossWright.MetalNexus.Schema.PathStrategies;

/// <summary>
/// Derives a request's URL path by stripping the namespace common to all types decorated with
/// <see cref="ApiRequestAttribute"/> in the same assembly.
/// </summary>
/// <remarks>
/// <para>
/// For example, if all your API requests are under <c>MyCorp.MyApp.Endpoints</c>, a request
/// type <c>GetUserRequest</c> in <c>MyCorp.MyApp.Endpoints.Users</c> would yield
/// <c>/Users/GetUser</c>.
/// </para>
/// <para>
/// This strategy restricts the namespace-detection scan to request types only, making it
/// more accurate than <see cref="TrimDefaultNamespacePathStrategy"/> when non-request types
/// have different namespace roots.  However, the scan still uses reflection at startup.
/// Consider using <see cref="TrimFixedPreamblePathStrategy"/> when the prefix is known
/// in advance.
/// </para>
/// </remarks>
public class TrimRequestNamespacePathStrategy : TrimDefaultNamespacePathStrategy
{
    /// <summary>Initializes a new <see cref="TrimRequestNamespacePathStrategy"/>.</summary>
    public TrimRequestNamespacePathStrategy() : base(1) { }
    /// <inheritdoc/>
    protected override Type[] GetConsideredTypes(Assembly assembly) => assembly.GetTypes()
        .Where(_ => _.GetCustomAttribute<ApiRequestAttribute>() != null)
        .ToArray();
}