using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalInjection;

/// <summary>
/// Marker interface that registers the implementing class as a transient service for <typeparamref name="T"/>.
/// An alternative to decorating the class with <see cref="TransientServiceAttribute{T}"/>.
/// </summary>
/// <typeparam name="T">The service type (interface or base class) to register the implementation under.</typeparam>
public interface ITransientService<T> { }

/// <summary>
/// Registers the decorated class as a transient service in the MetalInjection container for the specified service type.
/// A new instance is created on every resolution. The class is discovered automatically during assembly scanning.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientServiceAttribute : AutoServiceAttributeBase
{
    /// <summary>Initializes a new transient-service registration for the specified service type with an optional keyed-service key.</summary>
    /// <param name="type">The service type (interface or base class) to register the decorated class under.</param>
    /// <param name="key">An optional keyed-service key, or <see langword="null"/> for an unkeyed registration.</param>
    public TransientServiceAttribute(Type type, object? key = null) 
        : base(ServiceLifetime.Transient, type, key) { }
}

/// <summary>
/// Registers the decorated class as a transient service in the MetalInjection container for <typeparamref name="T"/>.
/// Shorthand for <c>[TransientService(typeof(T))]</c>.
/// A new instance is created on every resolution.
/// </summary>
/// <typeparam name="T">The service type (interface or base class) to register the decorated class under.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TransientServiceAttribute<T> : TransientServiceAttribute
{
    /// <summary>Initializes a new transient-service registration for <typeparamref name="T"/> with an optional keyed-service key.</summary>
    /// <param name="key">An optional keyed-service key, or <see langword="null"/> for an unkeyed registration.</param>
    public TransientServiceAttribute(object? key = null) 
        : base(typeof(T), key) { }
}