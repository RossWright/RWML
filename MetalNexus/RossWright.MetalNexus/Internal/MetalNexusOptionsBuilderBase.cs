using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalNexus.Schemna;
using System.ComponentModel;

namespace RossWright.MetalNexus.Internal;


[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMetalNexusOptions
{
    ILoadLog? LoadLog { get; }
    bool ServerStackTraceOnExceptionsIncluded { get; }
    bool DefaultToBadRequest { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class MetalNexusOptionsBuilderBase() 
    : AssemblyScanningOptionsBuilder("MetalNexus"),
    IMetalNexusOptions,
    IMetalNexusOptionsBuilder
{
    public void IncludeServerStackTraceOnExceptions(bool include = true) =>
        _serverStackTraceOnExceptionsIncluded = include;
    protected bool _serverStackTraceOnExceptionsIncluded;
    public bool ServerStackTraceOnExceptionsIncluded => _serverStackTraceOnExceptionsIncluded;

    public void TreatUnknownExceptionsAsInternalServiceError(bool throwIseByDefault = true) =>
        _throwIseByDefault = throwIseByDefault;
    protected bool _throwIseByDefault;
    public bool DefaultToBadRequest => !_throwIseByDefault;

    public void ConfigureEndpointSchema(Action<IEndpointSchemaOptions> config) => config(_endpointSchema);
    public IEndpointSchemaOptions _endpointSchema = new EndpointSchemaOptions();

    public void UseCustomEndpointSchema(ICustomEndpointSchema schema) => _customEndpointSchema = schema;
    private ICustomEndpointSchema? _customEndpointSchema;

    protected void Initialize(IServiceCollection services, bool isServer)
    {
        services.AddSingleton<IMetalNexusOptions>(this);

        if (!services.HasService<IMediator>())
        {
            services.AddMetalChain(_ => _.ScanAssemblies(Assemblies.ToArray()));
        }

        var registry = new MetalNexusRegistry(
            _ => services.AddMetalChainHandlers(_),
            _endpointSchema,
            _customEndpointSchema,
            isServer, LoadLog);

        var preloads = services
            .Where(_ => _.ServiceType == typeof(MetalNexusPreLoads))
            .SelectMany(_ => ((MetalNexusPreLoads)_.ImplementationInstance!).Types);
        registry.AddEndpoints(preloads.Concat(DiscoveredConcreteTypes).Distinct().ToArray());
        services.RemoveAll<MetalNexusPreLoads>();

        services.AddSingleton<IMetalNexusRegistry>(registry);

        AddServices(services);
    }
}

internal class EndpointSchemaOptions : IEndpointSchemaOptions
{
    public IPathStrategy? PathStrategy { get; set; } =
        new Schemna.PathStrategies.TrimRequestNamespacePathStrategy();
    public bool RequiresAuthenticationByDefault { get; set; } = true;
    public string ApiPathPrefix { get; set; } = "/api";
    public bool ApiPathToLower { get; set; } = true;
    public string[] RequestSuffixesToTrim { get; set; } = new string[]
    {
        "Request",
        "Command",
        "Query"
    };
    public int MaximumRequestParameters { get; set; } = 5;
    public HttpProtocol DefaultHttpProtocol { get; set; } = HttpProtocol.Get;
}