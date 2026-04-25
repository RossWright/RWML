namespace RossWright;

/// <summary>Thrown when an operation requires an authenticated identity but none is present.</summary>
public class NotAuthenticatedException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public NotAuthenticatedException() { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public NotAuthenticatedException(string message) : base(message) { }
    /// <summary>Initializes a new instance with the specified <paramref name="innerException"/>.</summary>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotAuthenticatedException(Exception innerException) : base(null, innerException) { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotAuthenticatedException(string message, Exception innerException) : base(message, innerException) { }
}