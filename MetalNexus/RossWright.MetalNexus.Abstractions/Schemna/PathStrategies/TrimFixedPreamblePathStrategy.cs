namespace RossWright.MetalNexus.Schemna.PathStrategies;


/// <summary>
/// Determine a request's path by stripping a fixed prefix
/// For example, if all your requests are under the MyCorp.MyApp.Endpoints namespace,
/// set the preamble to "MyApp.Endpoints" and the path for a request type GetUserRequest in
/// the MyCorp.MyApp.Endpoints.Users namespace would be /Users/GetUser.
/// The advantage of this strategy over the TrimDefaultNamespace or TrimRequestNamespace
/// strategies is the expensive process of detecting the namespace using relfection is skipped
/// </summary>
public class TrimFixedPreamblePathStrategy : IPathStrategy
{
    public TrimFixedPreamblePathStrategy(string preamble) => _preamble = preamble.Split('.');
    private string[] _preamble = null!;
    public string? Trim(Type type)
    {
        var pieces = type.FullName!.Split('.', '+').ToList();
        pieces.RemoveAt(pieces.Count - 1);
        if (pieces.Count == 0) return null;

        int snipIndex = 0;
        while (snipIndex < Math.Min(pieces.Count, _preamble.Length) &&
            pieces[snipIndex] == _preamble[snipIndex])
        {
            snipIndex++;
        }
        if (snipIndex > 0) pieces.RemoveRange(0, snipIndex);
        if (pieces.Count == 0) return null;
        return string.Join('/', pieces);
    }
}