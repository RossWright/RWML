namespace RossWright.MetalNexus;

/// <summary>
/// Implement this interface on a handler's return type to take direct control of the HTTP
/// response, bypassing MetalNexus's default JSON serialization.
/// </summary>
/// <remarks>
/// <para>
/// When the handler returns a value that implements <c>IMetalNexusRawResponse</c>, the
/// MetalNexus middleware writes the content directly to the response body using the provided
/// <see cref="ContentType"/>, <see cref="Data"/>, or <see cref="DataStream"/> — exactly as
/// it does for <see cref="MetalNexusFile"/> responses.
/// </para>
/// <para>
/// Either <see cref="Data"/> or <see cref="DataStream"/> must be non-null. If both are
/// set, <see cref="DataStream"/> takes precedence.
/// </para>
/// <para>
/// Use this when a handler must return XML, CSV, a custom binary format, or a
/// pre-serialized JSON string with non-default options. For the common case of returning
/// a POCO as JSON, the normal handler return type is preferred.
/// </para>
/// </remarks>
public interface IMetalNexusRawResponse
{
    /// <summary>The MIME type to set as the HTTP <c>Content-Type</c> header.</summary>
    string ContentType { get; }

    /// <summary>The response body as a byte array, or <c>null</c> if <see cref="DataStream"/> is used.</summary>
    byte[]? Data { get; }

    /// <summary>
    /// The response body as a stream, or <c>null</c> if <see cref="Data"/> is used.
    /// When non-null this takes precedence over <see cref="Data"/> and the stream is
    /// disposed after writing.
    /// </summary>
    Stream? DataStream { get; }
}
