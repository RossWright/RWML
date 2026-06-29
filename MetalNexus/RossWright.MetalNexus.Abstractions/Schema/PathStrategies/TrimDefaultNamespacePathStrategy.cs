using System.Reflection;

namespace RossWright.MetalNexus.Schema.PathStrategies;

/// <summary>
/// Derives a request's URL path by stripping the auto-detected root namespace common to most
/// types in the assembly.
/// </summary>
/// <remarks>
/// <para>
/// For example, if most of your code lives under <c>MyCorp.MyApp</c>, a request type
/// <c>GetUserRequest</c> in the <c>MyCorp.MyApp.UserEndpoints</c> namespace would yield the
/// path <c>/UserEndpoints/GetUser</c>.
/// </para>
/// <para>
/// "Most" is controlled by <see cref="Threshold"/> (default 0.8, i.e. 80% of types must share
/// a namespace segment for it to be considered part of the common root).
/// </para>
/// <para>
/// The set of types used to detect the common namespace can be restricted by overriding
/// <see cref="GetConsideredTypes"/>.
/// </para>
/// <para>
/// Consider using <see cref="TrimFixedPreamblePathStrategy"/> when the common namespace prefix
/// is known in advance, to avoid the reflection overhead of auto-detection.
/// </para>
/// </remarks>
public class TrimDefaultNamespacePathStrategy : IPathStrategy
{
    /// <summary>
    /// Initializes a new <see cref="TrimDefaultNamespacePathStrategy"/>.\n    /// </summary>
    /// <param name="threshold">
    /// The minimum fraction (0.0–1.0) of types that must share a namespace segment for that
    /// segment to be treated as part of the common root prefix.  Default is <c>0.8</c> (80%).
    /// </param>
    public TrimDefaultNamespacePathStrategy(double threshold = 0.8) => Threshold = threshold;

    /// <inheritdoc />
    public string? Trim(Type type)
    {
        var pieces = type.FullName!.Split('.', '+').ToList();
        pieces.RemoveAt(pieces.Count - 1);
        if (pieces.Count == 0) return null;
        if (!_assemblyNamespaces.TryGetValue(type.Assembly, out var exclude))
        {
            var types = GetConsideredTypes(type.Assembly);
            int partIndex = 0;
            var parts = new List<string>();
            var ranked = types
                .Where(_ => _.Namespace != null)
                .GroupBy(_ => _.Namespace)
                .Select(_ => new { Parts = _.Key!.Split('.'), Count = _.Count() })
                .Where(_ => _.Parts.Length > partIndex)
                .GroupBy(_ => _.Parts[partIndex])
                .OrderByDescending(_ => _.Sum(x => x.Count))
                .ToArray();
            while (ranked.Any() && ranked[0].Sum(_ => _.Count) >= types.Length * Threshold)
            {
                parts.Add(ranked[0].Key);
                partIndex++;
                ranked = ranked[0]
                    .Where(_ => _.Parts.Length > partIndex)
                    .GroupBy(_ => _.Parts[partIndex])
                    .OrderByDescending(_ => _.Sum(x => x.Count))
                    .ToArray();
            }
            exclude = parts.ToArray();
            _assemblyNamespaces.Add(type.Assembly, exclude);
        }

        int snipIndex = 0;
        while (snipIndex < Math.Min(pieces.Count, exclude.Length) &&
            pieces[snipIndex] == exclude[snipIndex])
        {
            snipIndex++;
        }
        if (snipIndex > 0) pieces.RemoveRange(0, snipIndex);
        if (pieces.Count == 0) return null;
        return string.Join('/', pieces);
    }

    private Dictionary<Assembly, string[]> _assemblyNamespaces = new Dictionary<Assembly, string[]>();
    /// <summary>
    /// Returns the types in <paramref name="assembly"/> used to detect the common namespace root.
    /// Override to restrict detection to a subset of types (e.g. only request types).
    /// </summary>
    /// <param name="assembly">The assembly to inspect.</param>
    /// <returns>The types to consider when computing the common namespace prefix.</returns>
    protected virtual Type[] GetConsideredTypes(Assembly assembly) => assembly.GetTypes();
    /// <summary>
    /// The minimum fraction of types that must share a namespace segment for it to be included
    /// in the common root prefix.  Default is <c>0.8</c>.
    /// </summary>
    protected double Threshold { get; set; }
}
