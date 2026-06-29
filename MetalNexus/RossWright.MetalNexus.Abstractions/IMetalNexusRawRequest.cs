using RossWright.MetalChain;

namespace RossWright.MetalNexus;

/// <summary>
/// Marks a fire-and-forget request as needing access to the raw, unparsed HTTP request body.
/// </summary>
/// <remarks>
/// The MetalNexus server middleware populates <see cref="RawRequestBody"/> with the raw
/// request body string before invoking the handler, allowing the handler to perform its own
/// deserialization or validation.  Use the generic overload <see cref="IMetalNexusRawRequest{TResponse}"/>
/// for requests that return a value.
/// </remarks>
public interface IMetalNexusRawRequest : IRequest
{
    /// <summary>
    /// The raw, unparsed HTTP request body string.  Populated by the MetalNexus server
    /// middleware before the handler is invoked.
    /// </summary>
    string? RawRequestBody { get; set; }
}

/// <summary>
/// Marks a request with a response as needing access to the raw, unparsed HTTP request body.
/// </summary>
/// <typeparam name="TResponse">The response type returned by the handler.</typeparam>
/// <remarks>
/// The MetalNexus server middleware populates <see cref="RawRequestBody"/> with the raw
/// request body string before invoking the handler.
/// </remarks>
public interface IMetalNexusRawRequest<out TResponse>
    : IRequest<TResponse>
{
    /// <summary>
    /// The raw, unparsed HTTP request body string.  Populated by the MetalNexus server
    /// middleware before the handler is invoked.
    /// </summary>
    string? RawRequestBody { get; set; }
}