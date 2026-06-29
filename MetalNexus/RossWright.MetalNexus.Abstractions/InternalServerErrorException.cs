namespace RossWright.MetalNexus;

/// <summary>
/// Thrown by a MetalNexus server handler when an unexpected error occurs that should be
/// reported as HTTP 500 Internal Server Error.
/// </summary>
/// <remarks>
/// <para>
/// MetalNexus maps this exception type to a 500 response.  Use it in handlers to signal
/// unexpected server-side failures while keeping the handler's contract explicit.
/// </para>
/// <para>
/// When <c>TreatUnknownExceptionsAsInternalServiceError</c> is enabled in the server options,
/// any unhandled exception that is not already an <see cref="InternalServerErrorException"/> is
/// automatically wrapped in one before being serialized into the error response.
/// </para>
/// </remarks>
public class InternalServerErrorException : Exception
{
    /// <summary>Initializes a new <see cref="InternalServerErrorException"/> with no message.</summary>
    public InternalServerErrorException() { }
    /// <summary>Initializes a new <see cref="InternalServerErrorException"/> with the specified message.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public InternalServerErrorException(string? message) : base(message) { }
    /// <summary>Initializes a new <see cref="InternalServerErrorException"/> with the specified message and inner exception.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this exception, or <c>null</c>.</param>
    public InternalServerErrorException(string? message, Exception? innerException) : base(message, innerException) { }
}
