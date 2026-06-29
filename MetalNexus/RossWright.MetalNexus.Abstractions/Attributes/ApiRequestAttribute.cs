using System.Net;

namespace RossWright.MetalNexus;

/// <summary>
/// Marks a MetalChain request type as a MetalNexus API endpoint, controlling its HTTP method,
/// URL path, Swagger tag, and named <see cref="HttpClient"/> connection.
/// </summary>
/// <remarks>
/// Apply this attribute to any <c>IRequest</c> or <c>IRequest&lt;TResponse&gt;</c> class to
/// expose it as a REST endpoint (server) and/or dispatch it via <c>HttpClient</c> (client).
/// When <see cref="HttpProtocol"/> is <see cref="RossWright.MetalNexus.HttpProtocol.Auto"/>, MetalNexus
/// inspects the request type's properties to choose between GET with query parameters and
/// POST with a JSON body.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ApiRequestAttribute : Attribute
{
    /// <summary>
    /// Initializes a new <see cref="ApiRequestAttribute"/>.
    /// </summary>
    /// <param name="protocol">
    /// The HTTP method and request binding to use.  Defaults to
    /// <see cref="RossWright.MetalNexus.HttpProtocol.Auto"/>, which lets MetalNexus infer the
    /// best method based on the request type's properties.
    /// </param>
    /// <param name="path">
    /// An explicit URL path for the endpoint, e.g. <c>"/api/widgets/{Id}"</c>.  When
    /// <c>null</c>, MetalNexus derives the path from the request type's name and namespace
    /// using the configured <see cref="RossWright.MetalNexus.Schema.IPathStrategy"/>.
    /// </param>
    /// <param name="tag">
    /// The Swagger / OpenAPI tag used to group this endpoint in generated documentation.
    /// When <c>null</c>, MetalNexus derives a tag from the path.
    /// </param>
    /// <param name="connectionName">
    /// The named <see cref="HttpClient"/> connection to use when dispatching this request
    /// from a MetalNexus client.  When <c>null</c>, the default connection is used.
    /// </param>
    public ApiRequestAttribute(
        HttpProtocol protocol = HttpProtocol.Auto,
        string? path = null,
        string? tag = null,
        string? connectionName = null)
    {
        Tag = tag;
        Path = path;
        HttpProtocol = protocol;
        ConnectionName = connectionName;
    }
    /// <summary>The Swagger / OpenAPI tag for this endpoint, or <c>null</c> to use the derived tag.</summary>
    public string? Tag { get; }
    /// <summary>The explicit URL path for this endpoint, or <c>null</c> to derive from the type name.</summary>
    public string? Path { get; }
    /// <summary>The HTTP method and parameter binding strategy for this endpoint.</summary>
    public HttpProtocol HttpProtocol { get; }
    /// <summary>The named <see cref="HttpClient"/> connection, or <c>null</c> to use the default connection.</summary>
    public string? ConnectionName { get; }

    /// <summary>
    /// Optional fixed HTTP success status code to return when this endpoint completes
    /// successfully.  If the handler also injects <see cref="IMetalNexusResponseContext"/>
    /// and sets a status code at runtime, the runtime value takes precedence.  When
    /// neither is set, <c>200 OK</c> is returned.  Ignored if the handler throws.
    /// </summary>
    /// <example>
    /// Return 201 Created for every successful call to this endpoint:
    /// <code>
    /// [ApiRequest(SuccessStatusCode = HttpStatusCode.Created)]
    /// public class CreateWidgetRequest : IRequest&lt;Widget&gt; { ... }
    /// </code>
    /// </example>
    public HttpStatusCode? SuccessStatusCode { get; set; }
}

/// <summary>
/// Specifies the HTTP method and parameter-binding strategy for a MetalNexus endpoint.
/// </summary>
public enum HttpProtocol
{
    /// <summary>
    /// MetalNexus automatically selects the HTTP method and binding based on the request type's
    /// properties.  Types with few simple properties use GET with query parameters; types with
    /// more than <see cref="IEndpointSchemaOptions.MaximumRequestParameters"/> properties, or
    /// any complex or array property, use POST with a JSON body.
    /// </summary>
    Auto,
    /// <summary>HTTP GET. All request properties are sent as query-string parameters.</summary>
    Get,
    /// <summary>HTTP POST. The request is serialized as a JSON body.</summary>
    PostViaBody,
    /// <summary>HTTP POST. All request properties are sent as query-string parameters.</summary>
    PostViaQuery,
    /// <summary>HTTP PUT. The request is serialized as a JSON body.</summary>
    PutViaBody,
    /// <summary>HTTP PUT. All request properties are sent as query-string parameters.</summary>
    PutViaQuery,
    /// <summary>HTTP PATCH. The request is serialized as a JSON body.</summary>
    PatchViaBody,
    /// <summary>HTTP PATCH. All request properties are sent as query-string parameters.</summary>
    PatchViaQuery,
    /// <summary>HTTP DELETE. All request properties are sent as query-string parameters.</summary>
    Delete,
    /// <summary>HTTP DELETE. The request is serialized as a JSON body.</summary>
    DeleteViaBody
}

/// <summary>Extension methods for <see cref="HttpProtocol"/>.</summary>
public static class HttpProtocolExtensions
{
    /// <summary>
    /// Returns the <see cref="HttpMethod"/> that corresponds to this <see cref="HttpProtocol"/> value.
    /// </summary>
    /// <param name="protocol">The protocol value to convert.</param>
    /// <returns>The matching <see cref="HttpMethod"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="protocol"/> is <see cref="HttpProtocol.Auto"/>, which has no single HTTP method.
    /// </exception>
    public static HttpMethod ToHttpMethod(this HttpProtocol protocol) => protocol switch
    {
        HttpProtocol.Get => HttpMethod.Get,
        HttpProtocol.PostViaBody or HttpProtocol.PostViaQuery => HttpMethod.Post,
        HttpProtocol.PutViaBody or HttpProtocol.PutViaQuery => HttpMethod.Put,
        HttpProtocol.PatchViaBody or HttpProtocol.PatchViaQuery => HttpMethod.Patch,
        HttpProtocol.Delete or HttpProtocol.DeleteViaBody => HttpMethod.Delete,
        _ => throw new InvalidOperationException($"Cannot determine HTTP method for Auto")
    };

    /// <summary>
    /// Returns <c>true</c> when this protocol sends request properties as query-string
    /// parameters rather than as a JSON body.
    /// </summary>
    /// <param name="protocol">The protocol value to test.</param>
    /// <returns>
    /// <c>true</c> for <see cref="HttpProtocol.Get"/>, <see cref="HttpProtocol.PostViaQuery"/>,
    /// <see cref="HttpProtocol.PutViaQuery"/>, and <see cref="HttpProtocol.PatchViaQuery"/>;
    /// otherwise <c>false</c>.
    /// </returns>
    public static bool UsesQueryParams(this HttpProtocol protocol) => protocol.In(
        HttpProtocol.Get, 
        HttpProtocol.PostViaQuery, 
        HttpProtocol.PutViaQuery,
        HttpProtocol.PatchViaQuery);
}