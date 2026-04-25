using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace RossWright.MetalInjection;

/// <summary>
/// An <see cref="IServiceProvider"/> that supports MetalInjection property injection.
/// Resolved services and objects created via <see cref="ActivatorUtilities"/> have their
/// <see cref="InjectAttribute"/>-marked properties automatically populated.
/// </summary>
public interface IMetalInjectionServiceProvider : IServiceProvider
{
    /// <summary>
    /// Resolves and sets all <see cref="InjectAttribute"/>-marked (and alternate-inject-attribute-marked)
    /// properties on the given object using this service provider.
    /// </summary>
    /// <param name="serviceImpl">The object whose injectable properties will be populated. Null is silently ignored.</param>
    void InjectProperties(object? serviceImpl);
}

/// <summary>
/// Helper code for the various activator services.
/// </summary>
public static class ActivatorUtilities
{
    /// <summary>
    /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="provider">The service provider used to resolve dependencies</param>
    /// <param name="instanceType">The type to activate</param>
    /// <param name="parameters">Constructor arguments not provided by the <paramref name="provider"/>.</param>
    /// <returns>An activated object of type instanceType</returns>
    public static object CreateInstance(IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, params object[] parameters) =>
        InnerCreateInstance(provider, instanceType, parameters);

    /// <summary>
    /// Create a delegate that will instantiate a type with constructor arguments provided directly
    /// and/or from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <param name="instanceType">The type to activate</param>
    /// <param name="argumentTypes">
    /// The types of objects, in order, that will be passed to the returned function as its second parameter
    /// </param>
    /// <returns>
    /// A factory that will instantiate instanceType using an <see cref="IServiceProvider"/>
    /// and an argument array containing objects matching the types defined in argumentTypes.
    /// MetalInjection property injection is applied to each activated instance.
    /// </returns>
    public static ObjectFactory CreateFactory([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instanceType, Type[] argumentTypes) =>
        new ObjectFactory(InnerCreateFactoryFunc <object>(instanceType, argumentTypes));

    /// <summary>
    /// Create a delegate that will instantiate a type with constructor arguments provided directly
    /// and/or from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type to activate</typeparam>
    /// <param name="argumentTypes">
    /// The types of objects, in order, that will be passed to the returned function as its second parameter
    /// </param>
    /// <returns>
    /// A factory that will instantiate type T using an <see cref="IServiceProvider"/>
    /// and an argument array containing objects matching the types defined in argumentTypes.
    /// MetalInjection property injection is applied to each activated instance.
    /// </returns>
    public static ObjectFactory<T> CreateFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(Type[] argumentTypes) =>
        new ObjectFactory<T>(InnerCreateFactoryFunc<T>(typeof(T), argumentTypes));

    /// <summary>
    /// Instantiate a type with constructor arguments provided directly and/or from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type to activate</typeparam>
    /// <param name="provider">The service provider used to resolve dependencies</param>
    /// <param name="parameters">Constructor arguments not provided by the <paramref name="provider"/>.</param>
    /// <returns>An activated object of type T</returns>
    public static T CreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider, params object[] parameters) =>
        (T)InnerCreateInstance(provider, typeof(T), parameters);

    /// <summary>
    /// Retrieve an instance of the given type from the service provider. If one is not found then instantiate it directly.
    /// </summary>
    /// <typeparam name="T">The type of the service</typeparam>
    /// <param name="provider">The service provider used to resolve dependencies</param>
    /// <returns>The resolved service or created instance</returns>
    public static T GetServiceOrCreateInstance<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(IServiceProvider provider) =>
        (T)InnerGetServiceOrCreateInstance(provider, typeof(T));

    /// <summary>
    /// Retrieve an instance of the given type from the service provider. If one is not found then instantiate it directly.
    /// </summary>
    /// <param name="provider">The service provider</param>
    /// <param name="type">The type of the service</param>
    /// <returns>The resolved service or created instance</returns>
    public static object GetServiceOrCreateInstance(IServiceProvider provider, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type) =>
        InnerGetServiceOrCreateInstance(provider, type);


    private static object InnerCreateInstance(IServiceProvider provider, Type instanceType, object[] parameters) =>
        provider.InjectProperties(Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance(provider, instanceType, parameters));

    private static Func<IServiceProvider, object?[]?, T> InnerCreateFactoryFunc<T>(Type instanceType, Type[] argumentTypes)
    {
        var fac = Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateFactory(instanceType, argumentTypes);
        return (provider, arguments) => provider.InjectProperties((T)fac(provider, arguments));
    }

    private static object InnerGetServiceOrCreateInstance(IServiceProvider provider, Type type) =>
        provider.InjectProperties(Microsoft.Extensions.DependencyInjection.ActivatorUtilities.GetServiceOrCreateInstance(provider, type));
}
