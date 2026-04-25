namespace RossWright.MetalInjection;

/// <summary>
/// The exception thrown by MetalInjection when a service configuration or resolution failure occurs.
/// Examples include type mismatches on registration attributes, duplicate registrations in strict mode,
/// and captive-dependency violations.
/// </summary>
public class MetalInjectionException : Exception
{
    /// <summary>Initializes a new instance with the specified error message.</summary>
    /// <param name="message">A message that describes the error.</param>
    public MetalInjectionException(string? message) : base(message) { }
}
