namespace RossWright.MetalChain;

/// <summary>Base exception for errors raised by the MetalChain library.</summary>
public class MetalChainException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public MetalChainException() { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/>.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public MetalChainException(string message) : base(message) { }
    /// <summary>Initializes a new instance with the specified <paramref name="message"/> and optional <paramref name="inner"/> exception.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="inner">The exception that caused this one, or <see langword="null"/>.</param>
    public MetalChainException(string message, Exception? inner = null) : base(message, inner) { }
}
