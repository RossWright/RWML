using RossWright.MetalNexus.Schema;

namespace RossWright.MetalNexus;

/// <summary>
/// Controls how MetalNexus derives HTTP paths, tags, methods, and authentication requirements
/// from request type names, namespaces, and attributes.
/// </summary>
/// <remarks>
/// Configure these options via
/// <c>ConfigureEndpointSchema</c> inside
/// <c>AddMetalNexusClient</c> or <c>AddMetalNexusServer</c>.
/// </remarks>
public interface IEndpointSchemaOptions
{
    /// <summary>
    /// When <c>true</c> (the default), every endpoint requires authentication unless it is
    /// explicitly decorated with <see cref="AnonymousAttribute"/>.
    /// Set to <c>false</c> to make endpoints anonymous by default and opt individual endpoints
    /// into authentication using <see cref="AuthenticatedAttribute"/>.
    /// </summary>
    bool RequiresAuthenticationByDefault { get; set; }

    /// <summary>
    /// A prefix prepended to every derived API path, e.g. <c>"/api"</c>.
    /// Defaults to <c>"/api"</c>.
    /// </summary>
    string ApiPathPrefix { get; set; }

    /// <summary>
    /// When <c>true</c>, the derived path segments are converted to lower-case.
    /// </summary>
    bool ApiPathToLower { get; set; }

    /// <summary>
    /// One or more suffixes (e.g. <c>"Request"</c>, <c>"Query"</c>, <c>"Command"</c>) that
    /// MetalNexus strips from request type names when building the URL path.
    /// </summary>
    string[] RequestSuffixesToTrim { get; set; }

    /// <summary>
    /// The <see cref="IPathStrategy"/> that controls how namespace segments are mapped to
    /// URL path segments.  When <c>null</c>, MetalNexus uses its built-in default strategy.
    /// </summary>
    IPathStrategy? PathStrategy { get; set; }

    /// <summary>
    /// When <see cref="HttpProtocol.Auto"/> is used, controls the threshold at which the
    /// endpoint switches from GET (query parameters) to POST (request body).  A request type
    /// with more properties than this value — or with any complex/array property — will use
    /// POST body regardless of this setting.  Default is 5.
    /// </summary>
    int MaximumRequestParameters { get; set; }

    /// <summary>
    /// The fallback HTTP protocol to use when <see cref="HttpProtocol.Auto"/> does not
    /// promote the endpoint to POST.  Defaults to <see cref="HttpProtocol.Get"/>.
    /// </summary>
    HttpProtocol DefaultHttpProtocol { get; set; }
}
