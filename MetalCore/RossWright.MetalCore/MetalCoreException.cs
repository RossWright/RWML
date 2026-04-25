namespace RossWright;

/// <summary>Base exception for errors raised by the MetalCore library.</summary>
public class MetalCoreException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public MetalCoreException() { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public MetalCoreException(string message) : base(message) { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public MetalCoreException(string message, Exception innerException) : base(message, innerException) { }
}
