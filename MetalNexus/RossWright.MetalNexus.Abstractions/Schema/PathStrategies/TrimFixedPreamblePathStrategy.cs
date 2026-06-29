namespace RossWright.MetalNexus.Schema.PathStrategies;


/// <summary>
/// Derives a request's URL path by stripping a fixed, known namespace prefix.
/// </summary>
/// <remarks>
/// <para>
/// For example, if all your requests are under <c>MyCorp.MyApp.Endpoints</c>, set the
/// preamble to <c>"MyCorp.MyApp.Endpoints"</c> and a request type <c>GetUserRequest</c> in
/// <c>MyCorp.MyApp.Endpoints.Users</c> would yield <c>/Users/GetUser</c>.
/// </para>
/// <para>
/// This strategy is more efficient than <see cref="TrimDefaultNamespacePathStrategy"/> or
/// <see cref="TrimRequestNamespacePathStrategy"/> because it does not use reflection to
/// detect the common namespace prefix at startup.
/// </para>
/// </remarks>
public class TrimFixedPreamblePathStrategy : IPathStrategy
{
    /// <summary>
    /// Initializes a new <see cref="TrimFixedPreamblePathStrategy"/>.
    /// </summary>
    /// <param name="preamble">
    /// The dot-delimited namespace prefix to strip, e.g. <c>"MyCorp.MyApp.Endpoints"</c>.
    /// Segments not present at the start of a type's namespace are left unchanged.
    /// </param>
    public TrimFixedPreamblePathStrategy(string preamble) => _preamble = preamble.Split('.');
    private string[] _preamble = null!;
    /// <inheritdoc />
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
