using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalGuardian;

namespace RossWright.MetalShout;

public static class MetalShoutExtensions
{
    public static IServiceCollection AddMetalShoutClient(
        this IServiceCollection services, 
        Action<IMetalShoutClientOptionsBuilder>? setOptions = null)
    {
        var optionBuilder = new MetalShoutClientOptionsBuilder();
        if (setOptions != null) setOptions(optionBuilder);
        optionBuilder.InitializeClient(services);
        return services;
    }

    public static async Task UseMetalShoutClient(
        this IServiceProvider services,
        string? connectionName = null, 
        CancellationToken cancellationToken = default)
    {
        await services.GetRequiredService<IMetalGuardianAuthenticationClient>()
            .Authenticate(connectionName, true, cancellationToken);

        await services.GetRequiredService<IPushServiceConnector>()
            .Connect(connectionName, cancellationToken);
    }
}