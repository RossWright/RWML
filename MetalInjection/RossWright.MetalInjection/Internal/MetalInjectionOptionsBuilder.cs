using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace RossWright.MetalInjection;

/// <summary>
/// Internal implementation of <see cref="IMetalInjectionOptionsBuilder"/>.
/// Create instances via <see cref="Create(Guid)"/>; not intended for direct public use.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public class MetalInjectionOptionsBuilder
    : AssemblyScanningOptionsBuilder,
    IMetalInjectionOptionsBuilder
{
    /// <summary>Creates a new builder instance, guarded by an internal key to prevent accidental public construction.</summary>
    /// <param name="internalKey">Must equal the internal guard key; returns <see langword="null"/> (non-null enforced by caller) otherwise.</param>
    public static MetalInjectionOptionsBuilder Create(Guid internalKey) => 
        internalKey == InternalKey.Value ? new MetalInjectionOptionsBuilder() : null!;
    internal MetalInjectionOptionsBuilder() : base("MetalInjection") { }

    /// <summary>Returns an <see cref="IServiceProviderFactory{TContainerBuilder}"/> that uses this builder's configuration.</summary>
    public IServiceProviderFactory<IServiceCollection> CreateServiceProviderFactory() 
        => new MetalInjectionServiceProviderFactory(this);

    /// <summary>Replaces <see cref="InjectAttribute"/> with a custom attribute type for injection discovery.</summary>
    /// <param name="injectAttributeType">The custom attribute type; must derive from <see cref="Attribute"/>.</param>
    /// <param name="getServiceKey">Optional delegate to extract a keyed-service key from an instance of the custom attribute.</param>
    public void SetAlternateInjectAttribute(Type injectAttributeType, Func<Attribute, object?>? getServiceKey = null)
    {
        if (injectAttributeType.IsAssignableTo(typeof(Attribute)))
        {
            AlternateInjectAttributeType = injectAttributeType;
            _getServiceKey = getServiceKey;
        }
        else
        {
            throw new ArgumentException("inject attribute type must derive from Attribute");
        }
    }
    /// <summary>
    /// Gets or sets an additional attribute type checked during property-injection discovery.
    /// <see cref="InjectAttribute"/> is always checked independently; setting this property registers
    /// an <em>additional</em> marker rather than replacing the default.
    /// </summary>
    public Type AlternateInjectAttributeType { get; set; } = typeof(InjectAttribute);
    /// <summary>Extracts the service key from an instance of the alternate inject attribute using the delegate supplied to <see cref="SetAlternateInjectAttribute"/>.</summary>
    /// <param name="altInjectAttr">An instance of the alternate inject attribute.</param>
    /// <returns>The service key, or <see langword="null"/> if no key extractor was configured.</returns>
    public object? GetServiceKeyFromAlternateInjectAttribute(Attribute altInjectAttr) =>
        _getServiceKey == null ? null : _getServiceKey(altInjectAttr);
    private Func<Attribute, object?>? _getServiceKey;

    /// <summary>Cache of injectable properties per type, keyed by implementation type.</summary>
    internal readonly ConcurrentDictionary<Type, PropertyInfo[]> InjectPropertyCache = new();
    /// <summary>Cache of nullability (true = nullable) per property to avoid re-allocating NullabilityInfoContext.</summary>
    internal readonly ConcurrentDictionary<PropertyInfo, bool> NullabilityCache = new();

    private readonly List<(Type OpenServiceType, ServiceLifetime Lifetime, Func<IServiceProvider, Type[], object> Factory)>
        _openGenericFactories = new();

    internal void AddOpenGenericFactory(Type openGenericServiceType, ServiceLifetime lifetime,
        Func<IServiceProvider, Type[], object> factory)
    {
        ArgumentNullException.ThrowIfNull(openGenericServiceType);
        ArgumentNullException.ThrowIfNull(factory);
        if (!openGenericServiceType.IsGenericTypeDefinition)
            throw new ArgumentException("Must be an open generic type definition (e.g. typeof(IRepo<>)).", nameof(openGenericServiceType));
        _openGenericFactories.Add((openGenericServiceType, lifetime, factory));
    }

    internal IReadOnlyList<(Type OpenServiceType, ServiceLifetime Lifetime, Func<IServiceProvider, Type[], object> Factory)>
        OpenGenericFactories => _openGenericFactories;

    /// <summary>Allows scoped services to be resolved from the root scope when <paramref name="value"/> is <see langword="true"/>.</summary>
    /// <param name="value"><see langword="true"/> to enable root-scoped resolution; <see langword="false"/> to disable (default).</param>
    public void AllowRootScopedResolution(bool value = true) => AllowRootScopedResolutionEnabled = value;
    internal bool AllowRootScopedResolutionEnabled { get; private set; } = false;

    /// <summary>Allows multiple registrations for any service type when <paramref name="value"/> is <see langword="true"/>.</summary>
    /// <param name="value"><see langword="true"/> to permit multiple registrations for all types; <see langword="false"/> to revert to per-type control (default).</param>
    public void AllowMultipleServicesOfAnyType(bool value = true) => _allowMultipleAnyType = value;
    private bool _allowMultipleAnyType = false;

    /// <summary>Allows multiple registrations for the specified <paramref name="type"/>.</summary>
    /// <param name="type">The service type for which multiple registrations are permitted.</param>
    public void AllowMultipleServicesOf(Type type)=> _allowMultipleTypes.Add(type);
    private readonly List<Type> _allowMultipleTypes = new();

    /// <summary>Excludes <paramref name="type"/> from auto-registration scanning.</summary>
    /// <param name="type">The type to suppress.</param>
    public void Ignore(Type type) => _ignoreTypes.Add(type);
    private readonly List<Type> _ignoreTypes = new();
    /// <summary>Gets the concrete types that are eligible for auto-registration after applying <see cref="Ignore"/> exclusions.</summary>
    public IEnumerable<Type> ConsideredTypes => DiscoveredConcreteTypes
        .Where(_ => _ignoreTypes?.Contains(_) != true);

    /// <summary>Overrides the entry assembly used as the starting point for scanning.</summary>
    /// <param name="entryAssembly">The assembly to treat as the application entry point.</param>
    public void SetEntryAssembly(Assembly entryAssembly) => _alternateEntryAssembly = entryAssembly;
    private Assembly? _alternateEntryAssembly;
    
    /// <summary>Scans discovered types and registers all eligible services and configuration objects into <paramref name="services"/>.</summary>
    /// <param name="services">The service collection to populate.</param>
    /// <param name="configuration">Optional application configuration used to bind config objects.</param>
    public void InitializeServices(IServiceCollection services, IConfiguration? configuration)
    {
        LoadLog?.LogTrace($"Autoloading Services and Config Objects");
        using var logScope = LoadLog?.BeginScope();

        Func<Type, bool> IsServiceInterface = _ => _.IsGenericType &&
            (typeof(ISingleton<>).IsAssignableFrom(_.GetGenericTypeDefinition())
            || typeof(IScopedService<>).IsAssignableFrom(_.GetGenericTypeDefinition())
            || typeof(ITransientService<>).IsAssignableFrom(_.GetGenericTypeDefinition()));

        // Find all concrete types that have a MetalInjection attribute or interface on it
        var discoveries = ConsideredTypes
            .Where(_ => _.GetCustomAttributes<AutoServiceAttributeBase>().Any() ||
                        _.GetInterfaces().Any(IsServiceInterface))
            .SelectMany(implType => implType
                .GetCustomAttributes<AutoServiceAttributeBase>()
                .Select(attr => new DiscoveredService
                {
                    ServiceImplementationType = implType,
                    ServiceInterfaceType = attr.ServiceInterfaceType,
                    Lifetime = attr.Lifetime,
                    Key = attr.Key
                })
                .Concat(implType.GetInterfaces()
                    .Where(IsServiceInterface)
                    .Select(_ => new DiscoveredService
                    {
                        ServiceImplementationType = implType,
                        ServiceInterfaceType = _.GetGenericArguments()[0],
                        Lifetime = typeof(ISingleton<>).IsAssignableFrom(_.GetGenericTypeDefinition()) ? ServiceLifetime.Singleton
                            : (typeof(IScopedService<>).IsAssignableFrom(_.GetGenericTypeDefinition()) ? ServiceLifetime.Scoped 
                            : ServiceLifetime.Transient),
                        Key = null
                    }))
                .DistinctBy(_ => (_.ServiceInterfaceType, _.Key)))
            .ToList();

        LoadLog?.LogTrace($"Found {discoveries.Count} services:");
        foreach (var service in discoveries)
        {
            if ((!service.ServiceInterfaceType.ContainsGenericParameters &&
                !service.ServiceInterfaceType.IsAssignableFrom(service.ServiceImplementationType)) ||
                (service.ServiceInterfaceType.ContainsGenericParameters &&
                !service.ServiceImplementationType.GetInterfaces()
                    .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == service.ServiceInterfaceType)))
            {
                var msg = $"Cannot register service {service.ServiceImplementationType} as it does not implement {service.ServiceInterfaceType}";
                LoadLog?.LogError(msg);
                throw new MetalInjectionException(msg);
            }
            else
            {
                LoadLog?.LogTrace($"{service.ServiceImplementationType} implements {service.Lifetime} {service.ServiceInterfaceType}{(service.Key != null ? $" (key: {service.Key})" : "" )}");
            }
        }
        discoveries.RemoveAll(_ => _.IsFaulted);

        if (!_allowMultipleAnyType)
        {
            // Get the entry assembly name for any dominance collisions
            var entryAsmName = _alternateEntryAssembly?.FullName
                ?? Assembly.GetEntryAssembly()?.FullName;
            LoadLog?.LogTrace($"Entry Asm Name: {entryAsmName}");

            // Inspect the services that have multiple implementations
            foreach (var group in discoveries
                .GroupBy(_ => _.ServiceInterfaceType)
                .Where(_ => _.Count() > 1))
            {
                // if type was specified as allowing multiple registrations (via option or attribute), skip the check
                if (_allowMultipleTypes?.Contains(group.Key) == true) continue;
                if (group.Key.GetCustomAttribute<AllowMultipleRegistrationsAttribute>() != null) continue;

                // if additional services added using a key, we can ignore them
                if (group.Count(_ => _.Key == null) <= 1) continue;

                // if only one of the non-keyed services is the entry assembly, use it and drop the rest
                var winners = group
                    .Where(_ => _.Key == null &&
                                _.ServiceImplementationType.Assembly.FullName == entryAsmName)
                    .ToList();
                if (winners.Count == 1)
                {
                    var losers = group
                        .Where(_ => _.Key == null &&
                                    _.ServiceImplementationType.Assembly.FullName != entryAsmName)
                        .ToList();
                    LoadLog?.LogWarning($"Multiple services were registered for {group.Key} without keys. Since " +
                        $"{winners[0].ServiceImplementationType.FullName} is the lone service registered in the entry assembly, " +
                        $"only it will be registered without key. The following service implementations will not be registered: " +
                        losers.Select(_ => $"{_.ServiceImplementationType.FullName}{_.Key.ToStringIfPresent(_ => $" [{_}]")}").CommaListJoin());
                    foreach (var service in losers) service.IsFaulted = true;
                    continue;
                }

                var msg = $"Cannot register multiple services for {group.Key} without using keys (for all but up to one registration), " +
                        $"or configuring MetalInjection to allow multiple registrations for this (or all) type(s). " +
                        $"The following implementations were registered: " +
                        $"{(group.Select(_ => $"{_.ServiceImplementationType.FullName}{_.Key.ToStringIfPresent(_ => $" [{_}]")}").CommaListJoin())}";
                LoadLog?.LogError(msg);
                throw new MetalInjectionException(msg);
            }
            discoveries.RemoveAll(_ => _.IsFaulted);
        }

        foreach(var group in discoveries
            .Where(_ => _.Lifetime == ServiceLifetime.Singleton)
            .GroupBy(_ => _.ServiceImplementationType))
        {
            var primary = group.First();
            if (primary.Key != null)
                services.AddKeyedSingleton(primary.ServiceInterfaceType, primary.Key, primary.ServiceImplementationType);
            else
                services.AddSingleton(primary.ServiceInterfaceType, primary.ServiceImplementationType);

            // If the implementation exposes multiple singleton services, ensure they all get the same instance
            foreach (var bonusInterface in group.Skip(1))
            {
                if (bonusInterface.Key != null)
                    services.AddKeyedSingleton(bonusInterface.ServiceInterfaceType, primary.Key,
                        (svcs, key) => svcs.GetRequiredService(primary.ServiceInterfaceType));
                else
                    services.AddSingleton(bonusInterface.ServiceInterfaceType,
                        svcs => svcs.GetRequiredService(primary.ServiceInterfaceType));
            }
        }

        foreach (var group in discoveries
            .Where(_ => _.Lifetime == ServiceLifetime.Scoped)
            .GroupBy(_ => _.ServiceImplementationType))
        {
            var primary = group.First();
            if (primary.Key != null)
                services.AddKeyedScoped(primary.ServiceInterfaceType, primary.Key, primary.ServiceImplementationType);
            else
                services.AddScoped(primary.ServiceInterfaceType, primary.ServiceImplementationType);

            // If the implementation exposes multiple scoped services, ensure they all get the same instance per scope
            foreach (var bonusInterface in group.Skip(1))
            {
                if (bonusInterface.Key != null)
                    services.AddKeyedScoped(bonusInterface.ServiceInterfaceType, primary.Key,
                        (svcs, key) => svcs.GetRequiredService(primary.ServiceInterfaceType));
                else
                    services.AddScoped(bonusInterface.ServiceInterfaceType,
                        svcs => svcs.GetRequiredService(primary.ServiceInterfaceType));
            }
        }

        foreach (var service in discoveries.Where(_ => _.Lifetime == ServiceLifetime.Transient))
        {
            if (service.Key != null)
                services.AddKeyedTransient(service.ServiceInterfaceType, service.Key, service.ServiceImplementationType);
            else
                services.AddTransient(service.ServiceInterfaceType, service.ServiceImplementationType);
        }

        if (configuration != null)
        {
            var foundConfigTypes = ConsideredTypes
                .Where(type => type.GetCustomAttributes(typeof(ConfigSectionAttribute), true).Length > 0)
                .ToArray();
            LoadLog?.LogTrace($"Found {foundConfigTypes.Length} Config Objects:");
            foreach (var configType in foundConfigTypes)
            {
                var sectionAttributes = configType.GetCustomAttributes(typeof(ConfigSectionAttribute), false);
                var instance = MetalActivator.CreateInstance(configType)!;
                bool hasRegisteredConcrete = false;
                foreach (ConfigSectionAttribute sectionAttribute in sectionAttributes)
                {
                    configuration.Bind(sectionAttribute.SectionTitle, instance);
                    if (instance is IValidatingConfigSection valCfg)
                        valCfg.ValidateOrDie();

                    if (sectionAttribute.RegisterAs is not null)
                    {
                        if (!configType.IsAssignableTo(sectionAttribute.RegisterAs))
                        {
                            var msg = $"Cannot register configuration class {configType} as it does not implement {sectionAttribute.RegisterAs} as specified";
                            LoadLog?.LogError(msg);
                            throw new MetalInjectionException(msg);
                        }
                        services.AddSingleton(sectionAttribute.RegisterAs, instance);
                    }
                    else if (!hasRegisteredConcrete)
                    {
                        services.AddSingleton(instance.GetType(), instance);
                        hasRegisteredConcrete = true;
                    }
                    var sectionTitles = sectionAttributes
                        .Select(_ => ((ConfigSectionAttribute)_).SectionTitle)
                        .ToArray();
                    var sectionRegisterTypes = sectionAttributes
                        .Select(_ => ((ConfigSectionAttribute)_).RegisterAs)
                        .Where(_ => _ is not null)
                        .ToList();
                    if (hasRegisteredConcrete) sectionRegisterTypes.Add(instance.GetType());
                    LoadLog?.LogTrace($"Bound type {configType} to section(s) {string.Join(',', sectionTitles)} " +
                        $"and registered as {string.Join(',', sectionRegisterTypes.Select(_ => _!.Name))}");
                }
            }
        }
        AddServices(services);
    }

    private sealed class DiscoveredService
    {
        public Type ServiceInterfaceType { get; set; } = null!;
        public Type ServiceImplementationType { get; set; } = null!;
        public ServiceLifetime Lifetime { get; set; }
        public object? Key { get; set; }
        public bool IsFaulted { get; internal set; }
    }
}
