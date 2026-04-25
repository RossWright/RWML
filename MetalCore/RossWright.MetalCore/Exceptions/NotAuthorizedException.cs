namespace RossWright;

/// <summary>Thrown when an authenticated identity lacks permission to perform an operation.</summary>
public class NotAuthorizedException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public NotAuthorizedException() { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public NotAuthorizedException(string message) : base(message) { }
    /// <summary>Initializes a new instance with the specified <paramref name="innerException"/>.</summary>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotAuthorizedException(Exception innerException) : base(null, innerException) { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotAuthorizedException(string message, Exception innerException) : base(message, innerException) { }
}
