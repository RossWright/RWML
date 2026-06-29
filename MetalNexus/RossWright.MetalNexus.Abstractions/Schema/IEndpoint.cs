namespace RossWright.MetalNexus.Schema;

/// <summary>
/// Describes the fully-resolved HTTP endpoint schema for a single MetalNexus request type.
/// </summary>
/// <remarks>
/// Instances are created by the MetalNexus registry at startup by combining
/// <see cref="ApiRequestAttribute"/> metadata, <see cref="IEndpointSchemaOptions"/> defaults,
/// and any <see cref="ICustomEndpointSchema"/> overrides.  Read this interface at runtime
/// to inspect how an endpoint will behave without dispatching a request.
/// </remarks>
public interface IEndpoint
{
    /// <summary>The request type associated with this endpoint.</summary>
    Type RequestType { get; }
    /// <summary>The response type, or <c>null</c> for fire-and-forget requests.</summary>
    Type? ResponseType { get; }

    /// <summary>The named <see cref="HttpClient"/> connection, or <c>null</c> for the default connection.</summary>
    string? HttpClientName { get; }
    /// <summary>The HTTP method used to call this endpoint.</summary>
    HttpMethod HttpMethod { get; }
    /// <summary>The URL path for this endpoint, relative to the base address (e.g. <c>/api/widgets/{Id}</c>).</summary>
    string Path { get; }
    /// <summary>When <c>true</c>, request properties are sent as query-string parameters instead of a JSON body.</summary>
    bool RequestAsQueryParams { get; }
    /// <summary>When <c>true</c>, the path contains one or more <c>{Property}</c> segments resolved from request properties.</summary>
    bool HasPathParams { get; }

    /// <summary>The Swagger / OpenAPI tag used to group this endpoint in generated documentation.</summary>
    string? Tag { get; }

    /// <summary>When <c>true</c>, the endpoint requires an authenticated caller.</summary>
    bool RequiresAuthentication { get; }
    /// <summary>The roles permitted to call this endpoint, or <c>null</c> when no role restriction applies.</summary>
    string[]? AuthorizedRoles { get; }
    /// <summary>The named ASP.NET Core authorization policy, or <c>null</c> when none is set.</summary>
    string? AuthorizationPolicy { get; }
    /// <summary>When <c>true</c>, provisionally-authenticated users are also permitted to call this endpoint.</summary>
    bool AllowProvisional { get; }

    /// <summary>Exception types declared via <see cref="ProducesErrorAttribute{TException}"/>.</summary>
    Type[] ProducedErrorTypes { get; }

    /// <summary>The per-request <see cref="HttpClient"/> send timeout, or <c>null</c> to use the client default.</summary>
    TimeSpan? HttpClientTimeout { get; }
    /// <summary>The names of request properties that are sent as HTTP headers via <see cref="FromHeaderAttribute"/>.</summary>
    string[] HeaderProperties { get; }

    /// <summary>
    /// Optional fixed HTTP success status code declared via
    /// <see cref="ApiRequestAttribute.SuccessStatusCode"/>.  <c>null</c> means
    /// no static override; the handler may still set one at runtime via
    /// <see cref="IMetalNexusResponseContext"/>.
    /// </summary>
    System.Net.HttpStatusCode? SuccessStatusCode { get; }
}

