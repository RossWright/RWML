using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RossWright.MetalNexus.Schema;
using System.ComponentModel;

namespace RossWright.MetalNexus.Internal;


/// <summary>
/// Internal options contract shared by MetalNexus client and server setup.
/// This type is public for package-to-package infrastructure use only.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IMetalNexusOptions
{
    /// <summary>
    /// Used by MetalNexusOptionsBuilderBase only.
    /// </summary>
    internal ILogger? GetBootstrapLogger();
    /// <summary>Gets whether server stack traces are included in serialized exception responses.</summary>
    bool ServerStackTraceOnExceptionsIncluded { get; }

    /// <summary>Gets whether unknown exception types map to HTTP 400 rather than HTTP 500.</summary>
    bool DefaultToBadRequest { get; }
}

/// <summary>
/// Base implementation for MetalNexus client and server options builders.
/// This type is public so integration packages can derive from it; application code should use the public builder interfaces.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class MetalNexusOptionsBuilderBase() 
    : AssemblyScanningOptionsBuilder("MetalNexus"),
    IMetalNexusOptions,
    IMetalNexusOptionsBuilder
{
    /// <inheritdoc />
    public void IncludeServerStackTraceOnExceptions(bool include = true) =>
        _serverStackTraceOnExceptionsIncluded = include;

    /// <summary>Stores whether server stack traces are included in serialized exception responses.</summary>
    protected bool _serverStackTraceOnExceptionsIncluded;

    /// <inheritdoc />
    public bool ServerStackTraceOnExceptionsIncluded => _serverStackTraceOnExceptionsIncluded;

    /// <inheritdoc />
    public void TreatUnknownExceptionsAsInternalServiceError(bool throwIseByDefault = true) =>
        _throwIseByDefault = throwIseByDefault;

    /// <summary>Stores whether unknown exception types should map to HTTP 500.</summary>
    protected bool _throwIseByDefault;

    /// <inheritdoc />
    public bool DefaultToBadRequest => !_throwIseByDefault;

    /// <inheritdoc />
    public void ConfigureEndpointSchema(Action<IEndpointSchemaOptions> config) => config(_endpointSchema);

    /// <summary>Holds the endpoint schema options configured during setup.</summary>
    public IEndpointSchemaOptions _endpointSchema = new EndpointSchemaOptions();

    /// <inheritdoc />
    public void UseCustomEndpointSchema(ICustomEndpointSchema schema) => _customEndpointSchema = schema;
    private ICustomEndpointSchema? _customEndpointSchema;

    /// <summary>
    /// Registers the MetalNexus services built from the configured options.
    /// </summary>
    /// <param name="services">The service collection to update.</param>
    /// <param name="isServer">Whether the builder is initializing server-side services.</param>
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
            isServer, GetBootstrapLogger());

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
        new Schema.PathStrategies.TrimRequestNamespacePathStrategy();
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
