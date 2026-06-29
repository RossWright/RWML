namespace RossWright.MetalNexus;

/// <summary>
/// Builds the absolute or relative URL for a MetalNexus request, incorporating path parameters,
/// query-string values, and the configured base address for the endpoint's connection.
/// </summary>
/// <remarks>
/// Inject this interface wherever you need to construct a URL without dispatching the request
/// — for example, to set a <c>Location</c> response header or pass a pre-built URL to a
/// Blazor component.
/// </remarks>
public interface IMetalNexusUrlHelper
{
    /// <summary>
    /// Returns the URL that would be used to send <paramref name="request"/> via the MetalNexus client.
    /// </summary>
    /// <typeparam name="TRequest">The request type decorated with <see cref="ApiRequestAttribute"/>.</typeparam>
    /// <param name="request">The request instance whose properties are used to populate path and query parameters.</param>
    /// <returns>The fully-formed URL string for the endpoint.</returns>
    string GetUrlFor<TRequest>(TRequest request);
}