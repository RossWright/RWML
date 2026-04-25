using System.Reflection;

namespace RossWright.MetalNexus.Schemna.PathStrategies;

/// <summary>
/// Determine a request's path by stripping the auto-detected request root namespace
/// For example, if all your Api Requests are under the MyCorp.MyApp.Endpoints namespace,
/// the path for a request type GetUserRequest in the MyCorp.MyApp.Endpoints.Users 
/// namespace would be /Users/GetUser
/// Consider using the TrimFixedPreamble strategy to avoid the expensive process of 
/// detecting the namespace using relfection is skipped
/// </summary>
public class TrimRequestNamespacePathStrategy : TrimDefaultNamespacePathStrategy
{
    public TrimRequestNamespacePathStrategy() : base(1) { }
    protected override Type[] GetConsideredTypes(Assembly assembly) => assembly.GetTypes()
        .Where(_ => _.GetCustomAttribute<ApiRequestAttribute>() != null)
        .ToArray();
}