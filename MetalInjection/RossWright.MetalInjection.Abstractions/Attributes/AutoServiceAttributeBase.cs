using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalInjection;

/// <summary>
/// Base class for all MetalInjection auto-registration attributes
/// (<see cref="SingletonAttribute"/>, <see cref="ScopedServiceAttribute"/>, <see cref="TransientServiceAttribute"/>).
/// When MetalInjection scans an assembly it discovers every concrete type decorated with a
/// derived attribute and registers it with the DI container using the lifetime, service type,
/// and optional key carried by this attribute.
/// </summary>
public abstract class AutoServiceAttributeBase : Attribute
{
    /// <summary>Initializes a new auto-registration attribute with the specified lifetime, service type, and optional key.
    /// Called by derived attribute constructors (<see cref="SingletonAttribute"/>, <see cref="ScopedServiceAttribute"/>,
    /// <see cref="TransientServiceAttribute"/>) and any custom extenders.
    /// </summary>
    /// <param name="lifetime">The DI lifetime to use when registering the decorated type.</param>
    /// <param name="serviceInterfaceType">The service type (interface or base class) to register the implementation under.</param>
    /// <param name="key">An optional keyed-service key, or <see langword="null"/> for an unkeyed registration.</param>
    protected AutoServiceAttributeBase(ServiceLifetime lifetime, Type serviceInterfaceType, object? key)
    {
        Lifetime = lifetime;
        ServiceInterfaceType = serviceInterfaceType;
        Key = key;
    }

    /// <summary>The service type (interface or base class) that the decorated class is registered under.</summary>
    public Type ServiceInterfaceType { get; init; }

    /// <summary>The DI lifetime (<see cref="ServiceLifetime.Singleton"/>, <see cref="ServiceLifetime.Scoped"/>, or <see cref="ServiceLifetime.Transient"/>) used when registering the decorated class.</summary>
    public ServiceLifetime Lifetime { get; init; }

    /// <summary>The optional keyed-service key for this registration. <see langword="null"/> indicates an unkeyed registration.</summary>
    public object? Key { get; init; }

    /// <summary>
    /// Controls whether and how MetalInjection resolves this registration when the caller
    /// requests a closed generic type that is not registered exactly.
    /// </summary>
    /// <value>
    /// <see cref="Covariance.Disabled"/> by default (exact match only).
    /// Set to <see cref="Covariance.Covariant"/> or
    /// <see cref="Covariance.HonorInOut"/> to enable
    /// covariant resolution. See <see cref="Covariance"/> for full
    /// documentation, semantics, and code examples.
    /// </value>
    public Covariance CovariantResolution { get; set; }
}
