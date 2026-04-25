using System.Reflection;

namespace RossWright.MetalInjection;

/// <summary>
/// Fluent options surface for configuring MetalInjection at startup.
/// Controls assembly scanning behaviour, property injection, service conflict resolution,
/// and error handling.
/// </summary>
public interface IMetalInjectionOptionsBuilder : IAssemblyScanningOptionsBuilder
{
    /// <summary>
    /// Registers an additional attribute type that MetalInjection should treat as an injection marker,
    /// alongside <see cref="InjectAttribute"/>. Use this to support third-party inject attributes
    /// (e.g. <c>Microsoft.AspNetCore.Components.InjectAttribute</c> for Blazor).
    /// </summary>
    /// <param name="type">The attribute type to recognise as an inject marker.</param>
    /// <param name="getServiceKey">
    /// An optional delegate that extracts the keyed-service key from an instance of the attribute,
    /// or <see langword="null"/> for unkeyed resolution.
    /// </param>
    void SetAlternateInjectAttribute(Type type, Func<Attribute, object?>? getServiceKey = null);

    /// <summary>
    /// Designates the assembly whose service registrations take precedence when a conflict is
    /// detected between multiple assemblies during scanning and strict mode is not enabled.
    /// </summary>
    /// <param name="entryAssembly">The assembly to treat as the conflict-resolution winner.</param>
    void SetEntryAssembly(Assembly entryAssembly);

    /// <summary>
    /// Excludes the specified type from all MetalInjection assembly scanning and auto-registration.
    /// </summary>
    /// <param name="type">The type to exclude.</param>
    void Ignore(Type type);

    /// <summary>
    /// Permits more than one implementation to be registered for the specified service type,
    /// suppressing the duplicate-registration error for that type only.
    /// </summary>
    /// <param name="type">The service type for which multiple registrations are allowed.</param>
    void AllowMultipleServicesOf(Type type);

    /// <summary>
    /// When <see langword="true"/>, permits duplicate registrations for all service types,
    /// suppressing all duplicate-registration errors globally.
    /// </summary>
    /// <param name="value"><see langword="true"/> to allow duplicates globally; <see langword="false"/> to restore per-type enforcement.</param>
    void AllowMultipleServicesOfAnyType(bool value = true);

    /// <summary>
    /// Suppresses the scoped-from-root guard globally, allowing scoped services to be resolved
    /// directly from the root <see cref="System.IServiceProvider"/> without creating a scope.
    /// By default MetalInjection throws when a scoped service is resolved from the root provider.
    /// Use <see cref="AllowRootResolutionAttribute"/> on an individual implementation class to
    /// suppress the guard for that type only.
    /// </summary>
    /// <param name="value"><see langword="true"/> to allow all scoped services to be resolved from root.</param>
    void AllowRootScopedResolution(bool value = true);
}

/// <summary>
/// Generic convenience overloads for <see cref="IMetalInjectionOptionsBuilder"/> methods,
/// allowing callers to specify types via type parameters instead of <see cref="Type"/> arguments.
/// </summary>
public static class IMetalInjectionOptionsBuilderExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TInjectAttribute"/> as an additional injection-marker attribute
    /// alongside <see cref="InjectAttribute"/>.
    /// </summary>
    /// <typeparam name="TInjectAttribute">The attribute type to recognise as an inject marker.</typeparam>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    /// <param name="getServiceKey">A delegate that extracts the keyed-service key from an instance of <typeparamref name="TInjectAttribute"/>.</param>
    public static void SetAlternateInjectAttribute<TInjectAttribute>(
        this IMetalInjectionOptionsBuilder optionsBuilder,
        Func<TInjectAttribute, object?> getServiceKey)
        where TInjectAttribute : Attribute =>
        optionsBuilder.SetAlternateInjectAttribute(typeof(TInjectAttribute), 
            _ => getServiceKey((TInjectAttribute)_));

    /// <summary>
    /// Excludes <typeparamref name="TIgnoreServiceType"/> from all MetalInjection assembly scanning
    /// and auto-registration.
    /// </summary>
    /// <typeparam name="TIgnoreServiceType">The type to exclude.</typeparam>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    public static void Ignore<TIgnoreServiceType>(
        this IMetalInjectionOptionsBuilder optionsBuilder) =>
        optionsBuilder.Ignore(typeof(TIgnoreServiceType));

    /// <summary>
    /// Permits more than one implementation to be registered for <typeparamref name="TMultiServiceType"/>,
    /// suppressing the duplicate-registration error for that type only.
    /// </summary>
    /// <typeparam name="TMultiServiceType">The service type for which multiple registrations are allowed.</typeparam>
    /// <param name="optionsBuilder">The options builder to configure.</param>
    public static void AllowMultipleServicesOf<TMultiServiceType>(
        this IMetalInjectionOptionsBuilder optionsBuilder) =>
        optionsBuilder.AllowMultipleServicesOf(typeof(TMultiServiceType));
}