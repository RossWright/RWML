namespace RossWright.MetalNexus.Schemna.PathStrategies;

/// <summary>
/// Determine a request's path by stripping the namespace
/// For example, if all your requests are under the MyCorp.MyApp.Endpoints namespace,
/// the path for a request type GetUserRequest in the MyCorp.MyApp.Endpoints.Users 
/// namespace would be /GetUser
/// </summary>
public class NoNamespacePathStrategy : IPathStrategy
{
    public string? Trim(Type type) => null;
}