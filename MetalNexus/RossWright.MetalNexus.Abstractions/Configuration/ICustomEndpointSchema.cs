namespace RossWright.MetalNexus.Schema;

/// <summary>
/// Provides a hook for fully customizing how individual endpoint schema properties are derived
/// from request types, supplementing or replacing attribute-based and convention-based inference.
/// </summary>
/// <remarks>
/// Register an implementation via
/// <c>UseCustomEndpointSchema</c>. Each method receives the value that MetalNexus would have
/// used based on attributes and conventions and may return either the same value or a replacement.
/// </remarks>
public interface ICustomEndpointSchema
{
    /// <summary>
    /// Returns the URL path to use for <paramref name="requestType"/>, or
    /// <paramref name="proposal"/> to accept the derived value.
    /// </summary>
    /// <param name="requestType">The request type being registered.</param>
    /// <param name="proposal">The path MetalNexus derived from the type name, namespace, and <see cref="ApiRequestAttribute"/>.</param>
    /// <returns>The final path string for this endpoint.</returns>
    string DeterminePath(Type requestType, string proposal);

    /// <summary>
    /// Returns the Swagger/OpenAPI tag to use for <paramref name="requestType"/>, or
    /// <paramref name="proposal"/> to accept the derived value.
    /// </summary>
    /// <param name="requestType">The request type being registered.</param>
    /// <param name="proposal">The tag MetalNexus derived from the path.</param>
    /// <returns>The final tag string for this endpoint.</returns>
    string DetermineTag(Type requestType, string proposal);

    /// <summary>
    /// Returns the <see cref="HttpProtocol"/> to use for <paramref name="requestType"/>, or
    /// <paramref name="proposal"/> to accept the inferred value.
    /// </summary>
    /// <param name="requestType">The request type being registered.</param>
    /// <param name="proposal">The protocol MetalNexus inferred from attributes and schema options.</param>
    /// <returns>The final <see cref="HttpProtocol"/> for this endpoint.</returns>
    HttpProtocol DetermineHttpProtocol(Type requestType, HttpProtocol proposal);

    /// <summary>
    /// Returns whether <paramref name="requestType"/> requires authentication, or
    /// <paramref name="proposal"/> to accept the inferred value.
    /// </summary>
    /// <param name="requestType">The request type being registered.</param>
    /// <param name="proposal">The authentication requirement MetalNexus inferred from attributes and defaults.</param>
    /// <returns><c>true</c> if the endpoint requires authentication; otherwise <c>false</c>.</returns>
    bool DetermineRequiresAuthentication(Type requestType, bool proposal);

    /// <summary>
    /// Returns the authorized roles for <paramref name="requestType"/>, or
    /// <paramref name="proposal"/> to accept the inferred value.
    /// </summary>
    /// <param name="requestType">The request type being registered.</param>
    /// <param name="proposal">The role array from <see cref="AuthenticatedAttribute"/>, or <c>null</c> if none.</param>
    /// <returns>The final role restriction array, or <c>null</c> to allow any authenticated user.</returns>
    string[]? DetermineAuthorizedRoles(Type requestType, string[]? proposal);
}
