using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand;

namespace RossWright.MetalInjection;

/// <summary>
/// Extension methods that wire MetalInjection into a console host builder or a bare service collection.
/// </summary>
public static class MetalInjectionExtensions
{
    /// <summary>
    /// Adds MetalInjection to a <see cref="IConsoleApplicationBuilder"/>, performing assembly scanning
    /// and configuring the service provider factory for property injection.
    /// </summary>
    /// <param name="hostBuilder">The console application builder to configure.</param>
    /// <param name="setOptions">An optional delegate to configure MetalInjection options.</param>
    /// <returns>The <paramref name="hostBuilder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddMetalInjection(
        this IConsoleApplicationBuilder hostBuilder,
        Action<IMetalInjectionOptionsBuilder>? setOptions = null)
    {
        var optionsBuilder = MetalInjectionOptionsBuilder.Create(InternalKey.Value);
        if (setOptions != null) setOptions(optionsBuilder);
        optionsBuilder.InitializeServices(hostBuilder.Services, hostBuilder.Configuration);
        LoadOpenGenericFactories(hostBuilder.Services, optionsBuilder);
        hostBuilder.SetServiceProviderFactory(optionsBuilder.CreateServiceProviderFactory());
        return hostBuilder;
    }

    /// <summary>
    /// Performs assembly scanning, registers discovered services into <paramref name="serviceCollection"/>,
    /// and builds an <see cref="IServiceProvider"/> that supports MetalInjection property injection.
    /// </summary>
    /// <param name="serviceCollection">The service collection to scan into and build from.</param>
    /// <param name="buildOptions">An optional delegate to configure MetalInjection options.</param>
    /// <param name="configuration">An optional configuration instance used for config-section binding.</param>
    /// <returns>An <see cref="IServiceProvider"/> with MetalInjection property injection enabled.</returns>
    public static IServiceProvider BuildMetalInjectionServiceProvider(
        this IServiceCollection serviceCollection,
        Action<IMetalInjectionOptionsBuilder>? buildOptions = null,
        IConfiguration? configuration = null)
    {
        var options = new MetalInjectionOptionsBuilder();
        if (buildOptions != null) buildOptions(options);
        options.InitializeServices(serviceCollection, configuration);
        LoadOpenGenericFactories(serviceCollection, options);
        return new MetalInjectionServiceProvider(serviceCollection, options);
    }

    /// <summary>
    /// Registers an open-generic service factory with the specified lifetime.
    /// The <paramref name="factory"/> delegate receives the <see cref="IServiceProvider"/> and the
    /// resolved closed type arguments so it can construct the appropriate implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="openGenericServiceType">An open-generic type definition such as <c>typeof(IRepo&lt;&gt;)</c>.</param>
    /// <param name="lifetime">The service lifetime.</param>
    /// <param name="factory">Factory invoked with (<see cref="IServiceProvider"/>, <see cref="Type"/>[]) returning the service instance.</param>
    /// <returns>The <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddOpenGenericFactory(
        this IServiceCollection services,
        Type openGenericServiceType,
        ServiceLifetime lifetime,
        Func<IServiceProvider, Type[], object> factory)
    {
        ArgumentNullException.ThrowIfNull(openGenericServiceType);
        ArgumentNullException.ThrowIfNull(factory);
        if (!openGenericServiceType.IsGenericTypeDefinition)
            throw new ArgumentException("Must be an open generic type definition (e.g. typeof(IRepo<>)).", nameof(openGenericServiceType));

        OpenGenericFactoryRegistry.GetOrAdd(services).Add(openGenericServiceType, lifetime, factory);
        return services;
    }

    /// <summary>Registers an open-generic singleton factory. See <see cref="AddOpenGenericFactory"/> for details.</summary>
    public static IServiceCollection AddOpenGenericSingleton(
        this IServiceCollection services,
        Type openGenericServiceType,
        Func<IServiceProvider, Type[], object> factory) =>
        services.AddOpenGenericFactory(openGenericServiceType, ServiceLifetime.Singleton, factory);

    /// <summary>Registers an open-generic scoped factory. See <see cref="AddOpenGenericFactory"/> for details.</summary>
    public static IServiceCollection AddOpenGenericScoped(
        this IServiceCollection services,
        Type openGenericServiceType,
        Func<IServiceProvider, Type[], object> factory) =>
        services.AddOpenGenericFactory(openGenericServiceType, ServiceLifetime.Scoped, factory);

    /// <summary>Registers an open-generic transient factory. See <see cref="AddOpenGenericFactory"/> for details.</summary>
    public static IServiceCollection AddOpenGenericTransient(
        this IServiceCollection services,
        Type openGenericServiceType,
        Func<IServiceProvider, Type[], object> factory) =>
        services.AddOpenGenericFactory(openGenericServiceType, ServiceLifetime.Transient, factory);

    private static void LoadOpenGenericFactories(IServiceCollection services, MetalInjectionOptionsBuilder options)
    {
        var registry = OpenGenericFactoryRegistry.TryGet(services);
        if (registry == null) return;
        foreach (var (openType, lifetime, factory) in registry.Entries)
            options.AddOpenGenericFactory(openType, lifetime, factory);
    }
}

/// <summary>
/// Internal marker registered as a singleton in the <see cref="IServiceCollection"/> to carry
/// open-generic factory registrations until the provider is built.
/// </summary>
internal sealed class OpenGenericFactoryRegistry
{
    internal readonly List<(Type OpenServiceType, ServiceLifetime Lifetime, Func<IServiceProvider, Type[], object> Factory)>
        Entries = new();

    internal void Add(Type openType, ServiceLifetime lifetime, Func<IServiceProvider, Type[], object> factory) =>
        Entries.Add((openType, lifetime, factory));

    internal static OpenGenericFactoryRegistry GetOrAdd(IServiceCollection services)
    {
        var existing = TryGet(services);
        if (existing != null) return existing;
        var registry = new OpenGenericFactoryRegistry();
        services.AddSingleton(registry);
        return registry;
    }

    internal static OpenGenericFactoryRegistry? TryGet(IServiceCollection services)
    {
        foreach (var descriptor in services)
        {
            if (descriptor.ServiceType == typeof(OpenGenericFactoryRegistry) &&
                descriptor.ImplementationInstance is OpenGenericFactoryRegistry r)
                return r;
        }
        return null;
    }
}
