using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalChain;
using RossWright.MetalGuardian;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RossWright.MetalShout;

internal class MetalShoutClientOptionsBuilder()
    : UsesLoggerOptionsBuilder("MetalShout"), 
    IMetalShoutClientOptionsBuilder
{
    public void SetHubName(string hubName) => HubName = hubName;
    public string HubName { get; private set; } = "PushHub";

    public void SetJsonSerializerOptions(JsonSerializerOptions jsonOptions) => JsonOptions = jsonOptions;
    public JsonSerializerOptions JsonOptions { get; private set; } = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public void InitializeClient(IServiceCollection services)
    {
        services
            .AddScoped<IPushClientService>(_ => new PushClientService(
                _.GetRequiredService<IMetalGuardianAuthenticationClient>(),
                _.GetRequiredService<IBaseAddressRepository>(),
                _.GetService<IMediator>(),
                HubName,
                JsonOptions))
            .AddScopedAlias<IPushServiceConnector, IPushClientService>();
        AddServices(services);
    }
}
