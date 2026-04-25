using System.Reflection;

namespace RossWright.MetalNexus.Schemna.PathStrategies;

/// <summary>
/// Determine a request's path by stripping the auto-detected root namespace
/// For example, if most of your code has a base namespace of MyCorp.MyApp,
/// the path for a request type GetUserRequest in the MyCorp.MyApp.UserEndpoints 
/// namespace would be /UserEndpoints/GetUser.
/// The definition of "most" can be adjusted using the Threshold property (default is 0.8 or 80%)
/// The types that are considered can by filtered by overriding GetConsideredTypes
/// Consider using the TrimFixedPreamble strategy to avoid the expensive process of 
/// detecting the namespace using relfection is skipped
/// </summary>
public class TrimDefaultNamespacePathStrategy : IPathStrategy
{
    public TrimDefaultNamespacePathStrategy(double threshold = 0.8) => Threshold = threshold;

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
    protected virtual Type[] GetConsideredTypes(Assembly assembly) => assembly.GetTypes();
    protected double Threshold { get; set; }
}