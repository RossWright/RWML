using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalInjection;

/// <summary>
/// Marker interface that registers the implementing class as a singleton for <typeparamref name="T"/>.
/// An alternative to decorating the class with <see cref="SingletonAttribute{T}"/>.
/// </summary>
/// <typeparam name="T">The service type (interface or base class) to register the implementation under.</typeparam>
public interface ISingleton<T> { }

/// <summary>
/// Registers the decorated class as a singleton in the MetalInjection container for the specified service type.
/// The class is discovered automatically during assembly scanning.
/// Multiple <see cref="SingletonAttribute"/> attributes may be applied to the same class to register it
/// under more than one service type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonAttribute : AutoServiceAttributeBase
{
    /// <summary>Initializes a new singleton registration for the specified service type with an optional keyed-service key.</summary>
    /// <param name="type">The service type (interface or base class) to register the decorated class under.</param>
    /// <param name="key">An optional keyed-service key, or <see langword="null"/> for an unkeyed registration.</param>
    public SingletonAttribute(Type type, object? key = null) 
        : base(ServiceLifetime.Singleton, type, key) { }
}

/// <summary>
/// Registers the decorated class as a singleton in the MetalInjection container for <typeparamref name="T"/>.
/// Shorthand for <c>[Singleton(typeof(T))]</c>.
/// Multiple attributes may be applied to register the class under more than one service type.
/// </summary>
/// <typeparam name="T">The service type (interface or base class) to register the decorated class under.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SingletonAttribute<T> : SingletonAttribute
{
    /// <summary>Initializes a new singleton registration for <typeparamref name="T"/> with an optional keyed-service key.</summary>
    /// <param name="key">An optional keyed-service key, or <see langword="null"/> for an unkeyed registration.</param>
    public SingletonAttribute(object? key = null) 
        : base(typeof(T), key) { }
}