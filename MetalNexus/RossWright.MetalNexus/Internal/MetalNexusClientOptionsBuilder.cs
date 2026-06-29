using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;

namespace RossWright.MetalNexus.Internal;

internal interface IMetalNexusClientOptions : IMetalNexusOptions
{
    string? DefaultConnectionName { get; }
    JsonSerializerOptions RequestBodyJsonSerializerOptions { get; }
}

internal class MetalNexusClientOptionsBuilder :
    MetalNexusOptionsBuilderBase,
    IMetalNexusClientOptionsBuilder,
    IMetalNexusClientOptions
{
    public void SetDefaultConnection(string connectionName) => 
        DefaultConnectionName = connectionName;
    public string? DefaultConnectionName { get; private set; } = Microsoft.Extensions.Options.Options.DefaultName;

    public void SetRequestBodyJsonSerializerOptions(JsonSerializerOptions options) => 
        RequestBodyJsonSerializerOptions = options;
    public JsonSerializerOptions RequestBodyJsonSerializerOptions { get; private set; } = new();

    public void InitializeClient(IServiceCollection services)
    {
        Initialize(services, isServer: false);
        services.AddSingleton<IMetalNexusClientOptions>(this);
        services.TryAddScoped<IMetalNexusUrlHelper, MetalNexusUrlHelper>();

        services.AddMetalChainHandlers(
            typeof(SendViaRequestHandler<>), 
            typeof(SendViaRequestHandler<,>));
    }
}
