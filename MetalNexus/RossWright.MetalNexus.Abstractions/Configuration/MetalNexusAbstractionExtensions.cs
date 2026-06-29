using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;

namespace RossWright.MetalNexus;

/// <summary>Extension methods for registering MetalNexus endpoints from class libraries.</summary>
public static class MetalNexusAbstractionExtensions
{
    /// <summary>
    /// Registers one or more request types as MetalNexus endpoints with the DI container.
    /// </summary>
    /// <param name="services">The service collection to register endpoints with.</param>
    /// <param name="types">
    /// One or more request types (decorated with <see cref="ApiRequestAttribute"/>) to register.
    /// When called before <c>AddMetalNexusClient</c> or <c>AddMetalNexusServer</c>, the types are
    /// queued and applied when the registry is created at startup.  When called after, they are
    /// added directly to the existing registry.
    /// </param>
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
