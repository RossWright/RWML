using System.Diagnostics.CodeAnalysis;

namespace RossWright.MetalInjection;

/// <summary>
/// Extension methods on <see cref="IServiceProvider"/> that add MetalInjection-aware
/// object instantiation and property injection.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="provider">The service provider used to resolve constructor dependencies and inject properties.</param>
    /// <param name="instanceType">The type to activate</param>
    /// <param name="parameters">Constructor arguments not provided by the service provider.</param>
    /// <returns>An activated object of type instanceType</returns>
    public static object CreateInstance(this IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, params object[] parameters) =>
        provider.InjectProperties(Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(provider, instanceType, parameters));

    /// <summary>
    /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type to activate</typeparam>
    /// <param name="provider">The service provider used to resolve constructor dependencies and inject properties.</param>
    /// <param name="parameters">Constructor arguments not provided by the service provider.</param>
    /// <returns>An activated object of type T</returns>
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(this IServiceProvider provider, params object[] parameters) =>
        provider.InjectProperties(Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<T>(provider, parameters));

    /// <summary>
    /// Runs MetalInjection property injection on an already-constructed object, resolving and assigning
    /// all <see cref="InjectAttribute"/>-decorated properties from the service provider.
    /// If <paramref name="provider"/> is not a <see cref="IMetalInjectionServiceProvider"/>, the object
    /// is returned unchanged.
    /// </summary>
    /// <typeparam name="T">The type of the object to inject into.</typeparam>
    /// <param name="provider">The service provider used to resolve injected properties.</param>
    /// <param name="obj">The object whose properties should be injected.</param>
    /// <returns><paramref name="obj"/> with all injectable properties populated.</returns>
    public static T InjectProperties<T>(this IServiceProvider provider, T obj)
    {
        if (provider is IMetalInjectionServiceProvider metalServiceProvider)
        {
            metalServiceProvider.InjectProperties(obj);
        }
        return obj;
    }
}