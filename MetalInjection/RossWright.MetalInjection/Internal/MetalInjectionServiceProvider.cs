using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Reflection;

namespace RossWright.MetalInjection;

internal sealed class MetalInjectionServiceProvider :
    IKeyedServiceProvider,
    ISupportRequiredService,
    IServiceScopeFactory,
    IServiceScope,
    IServiceProviderIsKeyedService,
    IMetalInjectionServiceProvider,
    IServiceProviderIsService,
    IAsyncDisposable
{
    public MetalInjectionServiceProvider(
        IServiceCollection services, 
        MetalInjectionOptionsBuilder options)
    {
        _isRootProvider = true;
        _serviceDescriptors = new(services
            .GroupBy(_ => _.ServiceType)
            .ToDictionary(_ => _.Key, _ => _.ToList()));
        _singletonInstances = new();
        _options = options;
    }

    private readonly bool _isRootProvider;
    private readonly MetalInjectionOptionsBuilder _options;
    private readonly ConcurrentDictionary<Type, List<ServiceDescriptor>> _serviceDescriptors;
    private readonly ConcurrentDictionary<int, object?> _singletonInstances;
    private readonly ConcurrentDictionary<int, object?>? _scopeInstances;
    private readonly ConcurrentBag<IDisposable> _disposableTransientInstances = new();
    private readonly ConcurrentBag<IAsyncDisposable> _asyncOnlyDisposableTransientInstances = new();

    public IServiceScope CreateScope() => new MetalInjectionServiceProvider(this);
    private MetalInjectionServiceProvider(MetalInjectionServiceProvider parent)
    {
        _isRootProvider = false;
        _serviceDescriptors = parent._serviceDescriptors;
        _singletonInstances = parent._singletonInstances;
        _options = parent._options;
        _scopeInstances = new();
    }

    public IServiceProvider ServiceProvider => this;
    public void Dispose()
    {
        foreach (var disposable in _disposableTransientInstances)
            disposable.Dispose();
        // Sync-over-async for IAsyncDisposable-only transients — matches the BCL's own pattern for the sync path.
        // Prefer IDisposable.Dispose() for dual-disposable instances; only block on DisposeAsync for async-only.
        foreach (var asyncDisposable in _asyncOnlyDisposableTransientInstances)
        {
            if (asyncDisposable is IDisposable syncFallback)
                syncFallback.Dispose();
            else
                asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        if (_scopeInstances != null)
        {
            foreach (var instance in _scopeInstances.Values)
            {
                if (instance is IDisposable disposable)
                    disposable.Dispose();
                else if (instance is IAsyncDisposable asyncDisposable)
                    asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            _scopeInstances.Clear();
        }

        if (_isRootProvider && !isDisposingRoot)
        {
            isDisposingRoot = true;
            foreach (var kvp in _singletonInstances.ToList())
            {
                if (kvp.Value is IDisposable disposable)
                    disposable.Dispose();
                else if (kvp.Value is IAsyncDisposable asyncDisposable)
                    asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _singletonInstances.TryRemove(kvp.Key, out _);
            }
        }
    }
    private bool isDisposingRoot = false;

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in _disposableTransientInstances)
            disposable.Dispose();
        foreach (var asyncDisposable in _asyncOnlyDisposableTransientInstances)
            await asyncDisposable.DisposeAsync();

        if (_scopeInstances != null)
        {
            foreach (var instance in _scopeInstances.Values)
            {
                if (instance is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else if (instance is IDisposable disposable)
                    disposable.Dispose();
            }
            _scopeInstances.Clear();
        }

        if (_isRootProvider && !isDisposingRoot)
        {
            isDisposingRoot = true;
            foreach (var kvp in _singletonInstances.ToList())
            {
                if (kvp.Value is IAsyncDisposable asyncDisposable)
                    await asyncDisposable.DisposeAsync();
                else if (kvp.Value is IDisposable disposable)
                    disposable.Dispose();
                _singletonInstances.TryRemove(kvp.Key, out _);
            }
        }
    }

    public object GetRequiredService(Type serviceType)
    {
        var service = GetKeyedService(serviceType, null);
        if (service == null)
        {
            throw new MetalInjectionException($"No service found for {serviceType.FullName}");
        }
        return service;
    }

    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        var service = GetKeyedService(serviceType, serviceKey);
        if (service == null)
        {
            throw new MetalInjectionException($"No service found for {serviceType.FullName} and key {serviceKey}");
        }
        return service;
    }

    public bool IsKeyedService(Type serviceType, object? serviceKey) =>
        TryResolve(serviceType, serviceKey, isCheck: true, out var _);

    public bool IsService(Type serviceType) =>
        TryResolve(serviceType, null, isCheck: true, out var _);

    public object? GetService(Type serviceType)
    {
        TryResolve(serviceType, null, isCheck: false, out var serviceImpl);
        return serviceImpl; 
    }

    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        TryResolve(serviceType, serviceKey, isCheck: false, out var serviceImpl);
        return serviceImpl;
    }

    private bool TryResolve(Type serviceType, object? serviceKey, bool isCheck, out object? result)
    {
        var displayName = $"{serviceType}{serviceKey.ToStringIfPresent(_ => $" [{_}]")}";

        bool isServiceList = serviceType.IsGenericType &&
            serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        if (isServiceList)
        {
            serviceType = serviceType.GetGenericArguments()[0];
        }

        var log = isCheck ? null : _options.LoadLog;


        log?.LogTrace($"{(isCheck ? "IsService" : "GetService")}({serviceType})");
        using var indent = log?.BeginScope(); 
        if (serviceType == typeof(IKeyedServiceProvider) ||
            serviceType == typeof(IServiceProvider) ||
            serviceType == typeof(ISupportRequiredService) ||
            serviceType == typeof(IServiceScopeFactory) ||
            serviceType == typeof(IServiceProviderIsKeyedService) ||
            serviceType == typeof(IServiceProviderIsService) ||
            serviceType == typeof(IMetalInjectionServiceProvider))
        {
            if (isServiceList)
            {
                var array = Array.CreateInstance(serviceType, 1);
                array.SetValue(this, 0);
                result = array;
            }
            else
            {
                result = this;
            }
            log?.LogTrace("Resolved as ServiceProvider itself");            
            return true;
        }

        Type[] genericParamTypes = [];
        if (!_serviceDescriptors.TryGetValue(serviceType, out var serviceDescriptors))
        {
            if (serviceType.IsGenericType &&
                _serviceDescriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out serviceDescriptors))
            {
                genericParamTypes = serviceType.GetGenericArguments();
            }
            else
            {
                if (!isServiceList && serviceType.IsGenericType)
                {
                    var def = serviceType.GetGenericTypeDefinition();
                    var requestedArgs = serviceType.GetGenericArguments();
                    var defParams = def.GetGenericArguments(); // carries GenericParameterAttributes (out/in)
                    List<List<ServiceDescriptor>> candidateGroups = new();
                    foreach (var kvp in _serviceDescriptors)
                    {
                        if (!kvp.Key.IsGenericType || kvp.Key.GetGenericTypeDefinition() != def) continue;
                        var regArgs = kvp.Key.GetGenericArguments();
                        if (regArgs.Length != requestedArgs.Length) continue;

                        List<ServiceDescriptor> matchingDescs = new();
                        foreach (var desc in kvp.Value)
                        {
                            if (serviceKey != null && !object.Equals(serviceKey, desc.ServiceKey)) continue;
                            var implType = GetImplementationType(desc);
                            if (implType == null) continue;
                            var attrs = implType.GetCustomAttributes<AutoServiceAttributeBase>(true);
                            var matchingAttr = attrs.FirstOrDefault(a => a.ServiceInterfaceType == kvp.Key && object.Equals(a.Key, desc.ServiceKey));
                            if (matchingAttr == null || matchingAttr.CovariantResolution == Covariance.Disabled) continue;

                            var isMatch = matchingAttr.CovariantResolution switch
                            {
                                // All positions: registered base must be assignable from requested derived.
                                // Exact match is always accepted for any type (incl. value types like int);
                                // covariant widening is only valid for reference types.
                                Covariance.Covariant =>
                                    regArgs.Zip(requestedArgs).All(p =>
                                        p.First == p.Second ||
                                        ((p.First.IsInterface || p.First.IsClass) &&
                                         p.First.IsAssignableFrom(p.Second))),

                                // Per-position: honour the CLR out/in/invariant annotation on each type parameter.
                                Covariance.HonorInOut =>
                                    regArgs.Zip(requestedArgs).Select((p, i) => (Reg: p.First, Req: p.Second, Param: defParams[i]))
                                          .All(p =>
                                          {
                                              var variance = p.Param.GenericParameterAttributes & System.Reflection.GenericParameterAttributes.VarianceMask;
                                              return variance switch
                                              {
                                                  // out T — covariant: base registered, derived requested
                                                  System.Reflection.GenericParameterAttributes.Covariant =>
                                                      (p.Reg.IsInterface || p.Reg.IsClass) && p.Reg.IsAssignableFrom(p.Req),
                                                  // in T — contravariant: derived registered, base requested
                                                  System.Reflection.GenericParameterAttributes.Contravariant =>
                                                      (p.Req.IsInterface || p.Req.IsClass) && p.Req.IsAssignableFrom(p.Reg),
                                                  // no annotation — invariant: exact match required
                                                  _ => p.Reg == p.Req,
                                              };
                                          }),

                                _ => false,
                            };

                            if (isMatch) matchingDescs.Add(desc);
                        }
                        if (matchingDescs.Count > 0) candidateGroups.Add(matchingDescs);
                    }
                    if (candidateGroups.Count == 1)
                    {
                        serviceDescriptors = candidateGroups[0];
                    }
                    else if (candidateGroups.Count > 1)
                    {
                        log?.LogError($"Multiple covariant matches found for {displayName}");
                        result = null;
                        return false;
                    }
                }
                if (serviceDescriptors == null)
                {
                    // Check open-generic factory registrations before giving up.
                    // genericParamTypes may be empty here (no open-generic entry in _serviceDescriptors),
                    // so we derive type arguments directly from serviceType.
                    if (!isServiceList && serviceType.IsGenericType)
                    {
                        var factoryTypeArgs = genericParamTypes.Length > 0
                            ? genericParamTypes
                            : serviceType.GetGenericArguments();
                        var def = serviceType.GetGenericTypeDefinition();
                        var factoryEntry = _options.OpenGenericFactories
                            .FirstOrDefault(e => e.OpenServiceType == def);
                        if (factoryEntry.Factory != null)
                        {
                            var capturedTypeArgs = factoryTypeArgs;
                            var capturedFactory = factoryEntry.Factory;
                            var syntheticDescriptor = new ServiceDescriptor(
                                serviceType,
                                sp => capturedFactory(sp, capturedTypeArgs),
                                factoryEntry.Lifetime);
                            // Register the closed descriptor so repeat lookups find a stable entry
                            // (same GetHashCode means singleton/scoped caching works correctly).
                            var newList = new List<ServiceDescriptor> { syntheticDescriptor };
                            _serviceDescriptors.TryAdd(serviceType, newList);
                            serviceDescriptors = newList;
                        }
                    }
                }

                if (serviceDescriptors == null)
                {
                    result = isServiceList ? Array.CreateInstance(serviceType, 0) : null;
                    if (isCheck)
                        log?.LogTrace($"No applicable service descriptor found for {displayName}");
                    else
                        log?.Log(LogLevel.Warning, $"No applicable service descriptor found for {displayName}");
                    return false;
                }
            }
        }

        if (serviceKey != null && !object.ReferenceEquals(serviceKey, KeyedService.AnyKey))
            serviceDescriptors = serviceDescriptors.Where(_ => serviceKey.Equals(_.ServiceKey)).ToList();

        if (_isRootProvider && serviceDescriptors.Count > 0)
        {
            if (!_options.AllowRootScopedResolutionEnabled)
            {
                var unguardedScoped = serviceDescriptors
                    .Where(d => d.Lifetime == ServiceLifetime.Scoped &&
                                GetImplementationType(d)?.GetCustomAttribute<AllowRootResolutionAttribute>() == null)
                    .ToList();
                if (unguardedScoped.Count > 0)
                {
                    if (unguardedScoped.Count == serviceDescriptors.Count)
                    {
                        var msg = $"Found a service for {displayName}, but its lifetime is scoped and the service provider is " +
                            $"not scoped. Use IServiceProvider.CreateScope to get a service provider scope to inject this service.";
                        log?.LogError(msg);
                        if (!isCheck) throw new MetalInjectionException(msg);
                        result = null;
                        return false;
                    }
                    serviceDescriptors = serviceDescriptors.Except(unguardedScoped).ToList();
                }
            }
        }

        if (!isServiceList && serviceDescriptors.Count > 1)
        {
            // If the descriptors are all the same type, instance, and factory, just use that.
            if (serviceDescriptors.AllSame(_ => _.ImplementationType) &&
                serviceDescriptors.AllSame(_ => _.ImplementationInstance) &&
                serviceDescriptors.AllSame(_ => _.ImplementationFactory))
            {
                serviceDescriptors = serviceDescriptors.Take(1).ToList();
            }
            else
            {
                var msg = $"Found multiple services for {displayName}. Use GetServices to inject multiple service implementations.";
                log?.LogError(msg);
                if (!isCheck) throw new MetalInjectionException(msg);
                result = null;
                return false;
            }
        }

        if (isCheck)
        {
            result = null;
            return serviceDescriptors.Count > 0;
        }

        List<object> serviceImpls = new();
        foreach (var serviceDescriptor in serviceDescriptors)
        {
            var key = genericParamTypes.Select(_ => _.GetHashCode())
                .Append(serviceDescriptor.GetHashCode())
                .GetAggregateHashCode();

            if (_singletonInstances.TryGetValue(key, out var serviceImpl) 
                || (!_isRootProvider && 
                    serviceDescriptor.Lifetime == ServiceLifetime.Scoped &&
                    _scopeInstances?.TryGetValue(key, out serviceImpl) == true))
            {
                log?.LogTrace($"{displayName} - Found {serviceDescriptor.Lifetime} previous instance");
                serviceImpls.Add(serviceImpl!);
            }
            else
            {
                Type? implementationType;
                if (serviceDescriptor.ImplementationInstance != null ||
                    (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationInstance != null))
                {
                    implementationType = serviceDescriptor.IsKeyedService
                        ? serviceDescriptor.KeyedImplementationInstance!.GetType()
                        : serviceDescriptor.ImplementationInstance!.GetType();

                    serviceImpl = serviceDescriptor.IsKeyedService
                        ? serviceDescriptor.KeyedImplementationInstance
                        : serviceDescriptor.ImplementationInstance;
                    log?.LogTrace($"{displayName} - Found {serviceDescriptor.Lifetime} implementation instance: {implementationType.Name} {serviceImpl!.GetHashCode()}");
                }
                else if (serviceDescriptor.ImplementationFactory != null ||
                         (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationFactory != null))
                {
                    implementationType = serviceDescriptor.IsKeyedService
                        ? serviceDescriptor.KeyedImplementationFactory!.Method.ReturnType
                        : serviceDescriptor.ImplementationFactory!.Method.ReturnType;
                    log?.LogTrace($"{displayName} - Found {serviceDescriptor.Lifetime} implementation factory");
                    try
                    {
                        serviceImpl = serviceDescriptor.IsKeyedService
                            ? serviceDescriptor.KeyedImplementationFactory!(this, serviceKey)
                            : serviceDescriptor.ImplementationFactory!(this);
                        log?.LogTrace($"{displayName} - Implementation Factory produced an object of type {serviceImpl?.GetType().Name}");
                    }
                    catch (Exception ex)
                    {
                        log?.LogError($"{displayName} - Implementation factory threw exception: {ex.ToBetterString()}");
                        throw;
                    }
                }
                else if (serviceDescriptor.ImplementationType != null ||
                         (serviceDescriptor.IsKeyedService && serviceDescriptor.KeyedImplementationType != null))
                {
                    implementationType = serviceDescriptor.IsKeyedService
                        ? serviceDescriptor.KeyedImplementationType!
                        : serviceDescriptor.ImplementationType!;
                    if (genericParamTypes.Length > 0)
                    {
                        implementationType = implementationType.MakeGenericType(genericParamTypes);
                    }
                     
                    log?.LogTrace($"{displayName} - Found {serviceDescriptor.Lifetime} Implementation Type {implementationType.Name}");

                    var constructors = implementationType.GetConstructors().OrderByDescending(_ => _.GetParameters().Length).ToArray();
                    var setConstructor = constructors.FirstOrDefault(_ => _.GetCustomAttribute<ActivatorUtilitiesConstructorAttribute>() != null);
                    if (setConstructor != null) constructors = [setConstructor];

                    foreach (var constructor in constructors)
                    {
                        var parameters = constructor.GetParameters();
                        log?.LogTrace($"{displayName} - Found {implementationType.Name} Constructor with {parameters.Length} parameters");

                        if (parameters.Any(_ => _.ParameterType.IsSimpleType() && !_.HasDefaultValue))
                        {
                            log?.LogTrace($"{displayName} - Found {implementationType.Name} Constructor with {parameters.Length} " +
                                $"parameters has required simple parameters - trying a different constructor if available...");
                            continue;
                        }

                        List<object?> arguments = new();
                        foreach (var parameter in parameters)
                        {
                            var parameterInjectAttr = parameter.GetCustomAttribute<InjectAttribute>();
                            var parameterServiceKey = parameter.GetCustomAttribute<FromKeyedServicesAttribute>()?.Key
                                ?? parameterInjectAttr?.Key;

                            log?.LogTrace($"{displayName} - getting {implementationType.Name} ctor parameter {parameter.Name} of type " +
                                $"{parameter.ParameterType.Name}{parameterServiceKey.ToStringIfPresent(_ => $" with key {_}")}");
                            if (serviceDescriptor.Lifetime == ServiceLifetime.Singleton &&
                                GetRegisteredLifetime(parameter.ParameterType, parameterServiceKey) == ServiceLifetime.Scoped)
                            {
                                var captiveMsg = $"Cannot inject scoped service {parameter.ParameterType.Name} into singleton " +
                                    $"{implementationType.Name}. Scoped services cannot be captured by singletons.";
                                log?.LogError(captiveMsg);
                                if (!isCheck) throw new MetalInjectionException(captiveMsg);
                                break;
                            }
                            var argument = GetKeyedService(parameter.ParameterType, parameterServiceKey);
                            if (argument == null)
                            {
                                bool isOptional = parameterInjectAttr?.EffectiveOptional
                                    ?? (parameter.HasDefaultValue || new NullabilityInfoContext().Create(parameter).WriteState is NullabilityState.Nullable);
                                if (!isOptional)
                                {
                                    log?.LogTrace($"{displayName} - error while contructing {implementationType.Name} contructor parameter {parameter.Name} - " +
                                        $"ctor parameter {parameter.Name} Service Not Found and has no default value");
                                    break;
                                }
                            }
                            if (false == argument?.GetType().IsAssignableTo(parameter.ParameterType))
                            {
                                log?.LogTrace($"{displayName} - error while contructing {implementationType.Name} - " +
                                    $"found service of type {argument.GetType().Name} that cannot be assigned to parameter {parameter.Name} of type {parameter.ParameterType.Name}");
                                break;
                            }
                            arguments.Add(argument);

                        }
                        if (arguments.Count == parameters.Length)
                        {
                            log?.LogTrace($"{displayName} - Creating instance of {implementationType}");
                            try
                            {
                                serviceImpl = constructor.Invoke(arguments.ToArray());
                                log?.LogTrace($"{displayName} - Created instance of {implementationType}");
                            }
                            catch (Exception ex)
                            {
                                log?.LogTrace($"{displayName} - Creating instance of {implementationType} threw exception {ex.ToBetterString()}");
                            }
                            break;
                        }
                        else
                        {
                            log?.LogTrace($"{displayName} - {implementationType.Name} - failed to find ctor parameters");
                        }
                    }

                    if (serviceImpl == null)
                    {
                        log?.LogError($"{displayName} - Failed to instantiate {implementationType.FullName}");
                    }
                }
                else
                {
                    throw new MetalInjectionException($"ServiceDescriptor for {displayName} has no implementation type, instance or factory");
                }

                if (serviceImpl != null)
                {
                    InjectProperties(log, serviceImpl);

                    serviceImpls.Add(serviceImpl);

                    if (serviceDescriptor.Lifetime == ServiceLifetime.Singleton)
                    {
                        _singletonInstances.TryAdd(key, serviceImpl);
                    }
                    else if (serviceDescriptor.Lifetime == ServiceLifetime.Scoped)
                    {
                        // When _scopeInstances is null (root provider with AllowRootResolution),
                        // fall back to _singletonInstances so the instance is cached correctly.
                        (_scopeInstances ?? _singletonInstances).TryAdd(key, serviceImpl);
                    }
                    else if (serviceImpl is IDisposable disposable && serviceImpl is not IAsyncDisposable)
                    {
                        _disposableTransientInstances.Add(disposable);
                    }
                    else if (serviceImpl is IAsyncDisposable asyncDisposable)
                    {
                        _asyncOnlyDisposableTransientInstances.Add(asyncDisposable);
                    }

                    if (!isServiceList) break;
                }
                else
                {
                    var msg = $"Failed to get instance of registered service {implementationType} for {displayName}";
                    log?.LogError($"{displayName} - {msg}");
                    if (!isCheck) throw new MetalInjectionException(msg);
                }
            }
        }

        if (isServiceList)
        {
            var array = Array.CreateInstance(serviceType, serviceImpls.Count);
            Array.Copy(serviceImpls.ToArray(), array, serviceImpls.Count);
            result = array;
            log?.LogTrace($"{displayName} - Resolved to service list with " +
                string.Join(", ", serviceImpls.ToArray().Select(_ => _.GetType().FullName)));
        }
        else
        {
            result = serviceImpls.FirstOrDefault();            
            log?.LogTrace($"{displayName} - {(result != null
                    ? $"Resolved to {result?.GetType().FullName}"
                    : "Could not resolve")}");
        }

        return serviceImpls.Count > 0;
    }

    private void InjectProperties(ILoadLog? log, object? serviceImpl)
    {
        if (serviceImpl == null) return;
        Type serviceImplType = serviceImpl.GetType();
        var injectProperties = _options.InjectPropertyCache.GetOrAdd(serviceImplType, t =>
            t.GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.FlattenHierarchy)
             .Where(_ => _.IsDefined(typeof(InjectAttribute))
                 || (_options.AlternateInjectAttributeType != null &&
                     _.IsDefined(_options.AlternateInjectAttributeType)))
             .ToArray());
        foreach (var injectProperty in injectProperties)
        {
            log?.LogTrace($"{serviceImpl} - Inject Property {injectProperty.PropertyType.Name} found");
            if (!injectProperty.CanWrite)
            {
                throw new InvalidOperationException($"Cannot write to inject property {injectProperty.Name} " +
                    $"when instantiating {serviceImplType.Name}. Be sure to provide a setter on Inject properties.");
            }

            object? serviceKey = null;
            var injectAttr = injectProperty.GetCustomAttribute<InjectAttribute>();
            if (injectAttr != null)
            {
                serviceKey = injectAttr.Key;
            }
            else if (_options.AlternateInjectAttributeType != null)
            {
                serviceKey = _options
                    .GetServiceKeyFromAlternateInjectAttribute(injectProperty
                        .GetCustomAttribute(_options.AlternateInjectAttributeType)!);
            }

            if (TryResolve(injectProperty.PropertyType, serviceKey, isCheck: false, out var dependency) || dependency != null)
            {
                injectProperty.SetValue(serviceImpl, dependency);
            }
            else
            {
                bool isOptional = injectAttr?.EffectiveOptional
                    ?? _options.NullabilityCache.GetOrAdd(injectProperty,
                        p => new NullabilityInfoContext().Create(p).WriteState == NullabilityState.Nullable);
                if (!isOptional)
                {
                    throw new InvalidOperationException($"Unable to find service {injectProperty.PropertyType} " +
                        $"when instantiating {serviceImplType.Name} for inject property {injectProperty.Name}");
                }
            }
        }
    }

    public void InjectProperties(object? serviceImpl)=> InjectProperties(_options.LoadLog, serviceImpl);

    private ServiceLifetime? GetRegisteredLifetime(Type serviceType, object? serviceKey)
    {
        if (!_serviceDescriptors.TryGetValue(serviceType, out var descriptors))
        {
            if (serviceType.IsGenericType)
                _serviceDescriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out descriptors);
        }
        if (descriptors == null) return null;
        var matches = serviceKey == null
            ? descriptors
            : descriptors.Where(_ => serviceKey.Equals(_.ServiceKey)).ToList();
        return matches.Count == 0 ? null : matches[0].Lifetime;
    }

    private Type? GetImplementationType(ServiceDescriptor desc)
    {
        if (desc.IsKeyedService)
        {
            if (desc.KeyedImplementationType != null) return desc.KeyedImplementationType;
            if (desc.KeyedImplementationFactory != null) return desc.KeyedImplementationFactory.Method.ReturnType;
            if (desc.KeyedImplementationInstance != null) return desc.KeyedImplementationInstance.GetType();
        }
        else
        {
            if (desc.ImplementationType != null) return desc.ImplementationType;
            if (desc.ImplementationFactory != null) return desc.ImplementationFactory.Method.ReturnType;
            if (desc.ImplementationInstance != null) return desc.ImplementationInstance.GetType();
        }
        return null;
    }
}
