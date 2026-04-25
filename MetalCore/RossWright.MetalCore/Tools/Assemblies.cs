using System.Reflection;

namespace RossWright;

/// <summary>
/// Convenience factory for building an assembly list using
/// <see cref="IAssemblyScanningOptionsBuilder"/>.
/// </summary>
public static class Assemblies
{
    /// <summary>
    /// Creates an <see cref="AssemblyScanningOptionsBuilder"/>, applies the
    /// optional <paramref name="builder"/> callback, and returns the collected
    /// <see cref="Assembly"/> array.
    /// </summary>
    /// <param name="builder">
    /// Optional callback to configure which assemblies are included. When
    /// <see langword="null"/>, an empty list is returned.
    /// </param>
    /// <returns>The assemblies added via the builder callback.</returns>
    public static Assembly[] BuildList(Action<IAssemblyScanningOptionsBuilder>? builder = null)
    {
        var options = new AssemblyScanningOptionsBuilder("MetalCore");
        if (builder != null) builder(options);
        return options.Assemblies.ToArray();
    }
}

