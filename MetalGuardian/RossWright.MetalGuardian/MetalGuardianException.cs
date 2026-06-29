namespace RossWright.MetalGuardian;

/// <summary>
/// Represents errors raised by the MetalGuardian client library.
/// </summary>
public class MetalGuardianException : Exception
{
    /// <summary>
    /// Initializes a new <see cref="MetalGuardianException"/>.
    /// </summary>
    public MetalGuardianException() { }

    /// <summary>
    /// Initializes a new <see cref="MetalGuardianException"/> with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public MetalGuardianException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new <see cref="MetalGuardianException"/> with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public MetalGuardianException(string message, Exception innerException) : base(message, innerException) { }
}
