using System.Net;

namespace RossWright.MetalNexus;

/// <summary>
/// An optional, ambient context that server-side request handlers can inject to influence
/// the HTTP success response for the current request.
/// </summary>
/// <remarks>
/// <para>
/// This interface is registered by <c>AddMetalNexusServer</c> and is available for
/// injection in any handler running inside a MetalNexus request context.  Inject it only
/// when you need to return a non-200 success code or set a <c>Location</c> header — handlers
/// that do not inject it always receive <c>200 OK</c>.
/// </para>
/// <para>
/// Values set on this context are applied <b>only</b> when the handler completes without
/// throwing.  If the handler throws any exception the context values are ignored and the
/// normal MetalNexus exception-to-status-code mapping applies.
/// </para>
/// <para>
/// For a fixed success code that never changes, prefer <see cref="ApiRequestAttribute.SuccessStatusCode"/>
/// on the request class instead.  When both are set the runtime value (this interface) wins.
/// </para>
/// </remarks>
public interface IMetalNexusResponseContext
{
    /// <summary>
    /// The HTTP status code to return on a successful response.  Defaults to
    /// <see cref="HttpStatusCode.OK"/> (200).  Ignored if the handler throws.
    /// </summary>
    HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Optional value for the <c>Location</c> response header.  Typically set alongside
    /// <see cref="HttpStatusCode.Created"/> (201).  Ignored if the handler throws.
    /// </summary>
    string? Location { get; set; }
}
