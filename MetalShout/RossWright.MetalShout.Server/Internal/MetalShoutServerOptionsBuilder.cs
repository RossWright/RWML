using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RossWright.MetalShout;

internal class MetalShoutServerOptionsBuilder : OptionsBuilder, IMetalShoutServerOptionsBuilder
{
    public void SetJsonSerializerOptions(JsonSerializerOptions jsonOptions) => _jsonOptions = jsonOptions;
    private JsonSerializerOptions _jsonOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public void InitializeServer(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions = _jsonOptions);
        services.TryAddSingleton<IPushConnectionRepository, PushConnectionRepository>();
        services.TryAddSingleton<IPushSubscriptionRepository, PushSubscriptionRepository>();
        services.AddScoped<IPushServerService, PushServerService>();
        AddServices(services);
    }
}