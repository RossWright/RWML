namespace RossWright.MetalNexus.Schema;

/// <summary>
/// Determines which namespace prefix to strip from a request type's fully-qualified name
/// when deriving its URL path segment.
/// </summary>
/// <remarks>
/// Assign an implementation to <see cref="IEndpointSchemaOptions.PathStrategy"/> to control
/// how namespaces are converted to URL paths.  Built-in implementations include
/// <see cref="PathStrategies.TrimFixedPreamblePathStrategy"/>,
/// <see cref="PathStrategies.TrimDefaultNamespacePathStrategy"/>,
/// <see cref="PathStrategies.TrimRequestNamespacePathStrategy"/>,
/// <see cref="PathStrategies.NoNamespacePathStrategy"/>, and
/// <see cref="PathStrategies.UseFullNameSpacePathStrategy"/>.
/// </remarks>
public interface IPathStrategy
{
    /// <summary>
    /// Returns the namespace prefix to remove from <paramref name="type"/>'s fully-qualified
    /// name, expressed as a slash-delimited path string, or <c>null</c> to strip nothing.
    /// </summary>
    /// <param name="type">The request type whose namespace prefix should be determined.</param>
    /// <returns>A slash-delimited prefix string to remove, or <c>null</c>.</returns>
    string? Trim(Type type);
}
