namespace RossWright.MetalNexus;

/// <summary>
/// The base exception type for MetalNexus client-side errors.
/// </summary>
/// <remarks>
/// <para>
/// When a MetalNexus server handler throws and the exception type cannot be reconstructed
/// on the client (either because the type is not present in the client assembly or because
/// the server returned a non-MetalNexus error shape), the client wraps the raw response body
/// in a <see cref="MetalNexusException"/> and rethrows it.
/// </para>
/// <para>
/// Catch <see cref="MetalNexusException"/> at call sites where you want to handle any
/// unrecognised server error generically; use the specific derived exception type for
/// precise error handling when the shared exception type is available.
/// </para>
/// </remarks>
public class MetalNexusException : Exception
{
    /// <summary>Initializes a new <see cref="MetalNexusException"/> with no message.</summary>
    public MetalNexusException() { }
    /// <summary>Initializes a new <see cref="MetalNexusException"/> with the specified message.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public MetalNexusException(string message) : base(message) { }
    /// <summary>Initializes a new <see cref="MetalNexusException"/> with the specified message and optional inner exception.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">The exception that caused this exception, or <c>null</c>.</param>
    public MetalNexusException(string message, Exception? inner = null) : base(message, inner) { }
}
