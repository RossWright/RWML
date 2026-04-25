namespace RossWright.MetalNexus.Schemna.PathStrategies;

/// <summary>
/// Determine a request's path by converting the entire namespace into a path
/// For example, if all your requests are under the MyCorp.MyApp.Endpoints namespace,
/// the path for a request type GetUserRequest in the MyCorp.MyApp.Endpoints.Users 
/// namespace would be MyCorp/MyApp/Endpoints/Users/GetUser
/// </summary>
public class UseFullNameSpacePathStrategy : IPathStrategy
{
    public string? Trim(Type type)
    {
        var pieces = type.FullName!.Split('.', '+').ToList();
        pieces.RemoveAt(pieces.Count - 1);
        if (pieces.Count == 0) return null;
        return string.Join('/', pieces);
    }
}