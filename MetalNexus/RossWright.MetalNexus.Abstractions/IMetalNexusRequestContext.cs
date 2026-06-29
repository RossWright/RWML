namespace RossWright.MetalNexus;

/// <summary>
/// An optional, ambient context that server-side request handlers can inject to inspect
/// properties of the inbound HTTP request.
/// </summary>
/// <remarks>
/// <para>
/// This interface is registered by <c>AddMetalNexusServer</c> and is available for
/// injection in any handler running inside a MetalNexus request context. Inject it only
/// when the handler needs to inspect request metadata — for example, reading the
/// <c>Accept</c> header for content negotiation. Handlers that do not inject it are
/// unaffected.
/// </para>
/// <para>
/// For content negotiation, compose this interface with <see cref="IMetalNexusRawResponse"/>:
/// read <see cref="AcceptHeader"/> to choose a format, then return an
/// <c>IMetalNexusRawResponse</c> with the appropriate <see cref="IMetalNexusRawResponse.ContentType"/>
/// and serialized body.
/// </para>
/// <para>
/// This follows the same ambient pattern as <see cref="IMetalNexusResponseContext"/> and
/// MetalGuardian's <c>ICurrentUser</c>: a thin, testable interface that keeps handler code
/// decoupled from ASP.NET Core's <c>HttpContext</c>.
/// </para>
/// </remarks>
public interface IMetalNexusRequestContext
{
    /// <summary>
    /// The value of the inbound <c>Accept</c> HTTP header, or <c>null</c> if not present.
    /// </summary>
    string? AcceptHeader { get; }

    /// <summary>
    /// The value of the inbound <c>Content-Type</c> HTTP header, or <c>null</c> if not present.
    /// </summary>
    string? ContentType { get; }

    /// <summary>
    /// The full collection of inbound HTTP request headers.
    /// </summary>
    IReadOnlyDictionary<string, string?> RequestHeaders { get; }
}
