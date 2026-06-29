namespace RossWright.MetalGuardian;

/// <summary>
/// Builds fully-qualified server URLs for MetalGuardian API request types.
/// </summary>
public interface IMetalGuardianUrlHelper
{
    /// <summary>
    /// Returns the fully-qualified URL for the given request against the specified connection,
    /// or the default connection if <paramref name="connectionName"/> is <c>null</c>.
    /// </summary>
    string GetUrlFor<TRequest>(TRequest request, string? connectionName = null)
        where TRequest : new();
}
