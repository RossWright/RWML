using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RossWright.MetalChain;

internal class MetalChainOptionsBuilder()
    : AssemblyScanningOptionsBuilder("MetalChain"),
    IMetalChainOptionsBuilder
{
    private bool _allowUnhandledQueries;
    private bool _allowUnhandledCommands;
    private bool _allowMultipleCommandHandlers;
    private MultipleHandlerExecutionMode _defaultCommandExecutionMode = MultipleHandlerExecutionMode.SequentialFailFast;
    private readonly HashSet<Type> _ignoredHandlers = [];

    public IMetalChainOptionsBuilder AllowUnhandledQueries()
    {
        _allowUnhandledQueries = true;
        return this;
    }

    public IMetalChainOptionsBuilder AllowUnhandledCommands()
    {
        _allowUnhandledCommands = true;
        return this;
    }

    public IMetalChainOptionsBuilder AllowMultipleCommandHandlers(
        MultipleHandlerExecutionMode mode = MultipleHandlerExecutionMode.SequentialFailFast)
    {
        _allowMultipleCommandHandlers = true;
        _defaultCommandExecutionMode = mode;
        return this;
    }

    public IMetalChainOptionsBuilder IgnoreHandler(Type handlerType)
    {
        _ignoredHandlers.Add(handlerType);
        return this;
    }

    public IMetalChainOptionsBuilder IgnoreHandler<THandler>() =>
        IgnoreHandler(typeof(THandler));

    public void Initialize(IServiceCollection services) =>
        InitializeOrUpdate(services, DiscoveredConcreteTypes, this);

    public static void InitializeOrUpdate(IServiceCollection services, Type[] types,
        MetalChainOptionsBuilder? options = null)
    {
        var registryServiceDesc = services.FirstOrDefault(_ => _.ServiceType == typeof(IMetalChainRegistry));
        var registry = registryServiceDesc?.ImplementationInstance as MetalChainRegistry;
        if (registry == null)
        {
            registry = new MetalChainRegistry(options?.GetBootstrapLogger());
            ApplyOptions(registry, options);
            registry.AddHandlers(types);
            services.AddSingleton<IMetalChainRegistry>(registry);
        }
        else
        {
            ApplyOptions(registry, options);
            registry.AddHandlers(types);
        }
        if (!services.Any(_ => _.ServiceType == typeof(IMediator)))
            services.AddSingleton<IMediator, Mediator>();
    }

    private static void ApplyOptions(MetalChainRegistry registry, MetalChainOptionsBuilder? options)
    {
        if (options == null) return;
        if (options._allowUnhandledQueries) registry.AllowUnhandledQueries = true;
        if (options._allowUnhandledCommands) registry.AllowUnhandledCommands = true;
        if (options._allowMultipleCommandHandlers)
        {
            registry.AllowMultipleCommandHandlers = true;
            registry.DefaultCommandExecutionMode = options._defaultCommandExecutionMode;
        }
        foreach (var ignoredType in options._ignoredHandlers)
            registry.IgnoredHandlers.Add(ignoredType);
    }
}
