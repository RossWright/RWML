namespace RossWright.MetalNexus;

/// <summary>
/// Declares that this endpoint may return the given exception type as an error response.
/// MetalNexus uses this to enrich the Swagger document with accurate HTTP error status codes.
/// Multiple attributes may be applied to the same request type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ProducesErrorAttribute<TException> : ProducesErrorAttribute
    where TException : Exception
{
    /// <summary>
    /// Creates an error-response declaration for <typeparamref name="TException"/>.
    /// </summary>
    public ProducesErrorAttribute() : base(typeof(TException)) { }
}

/// <summary>Non-generic base — for internal reflection use only.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class ProducesErrorAttribute(Type exceptionType) : Attribute
{
    /// <summary>
    /// Gets the exception type declared by this attribute.
    /// </summary>
    public Type ExceptionType { get; } = exceptionType;
}
