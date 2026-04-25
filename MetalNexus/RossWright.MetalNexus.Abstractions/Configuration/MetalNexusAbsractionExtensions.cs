using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schemna;

namespace RossWright.MetalNexus;

public static class MetalNexusAbsractionExtensions
{
    public static void AddMetalNexusEndpoints(this IServiceCollection services, params Type[] types)
    {
        var registryServiceDesc = services.FirstOrDefault(_ => _.ServiceType == typeof(IMetalNexusRegistry));
        if (registryServiceDesc?.ImplementationInstance is IMetalNexusRegistry registry)
        {
            registry.AddEndpoints(types);
        }
        else
        {
            services.AddSingleton(new MetalNexusPreLoads(types));
        }
    }
}
