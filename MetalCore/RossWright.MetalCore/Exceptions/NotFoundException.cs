namespace RossWright;

/// <summary>Thrown when a requested resource cannot be found.</summary>
public class NotFoundException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public NotFoundException() { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public NotFoundException(string message) : base(message) { }
    /// <summary>Initializes a new instance with the specified <paramref name="innerException"/>.</summary>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotFoundException(Exception innerException) : base(null, innerException) { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/> and <paramref name="innerException"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The exception that caused this one.</param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
