using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalChain;

/// <summary>Extension methods for registering MetalChain with an <see cref="IServiceCollection"/>.</summary>
public static class MetalChainExtensions
{
    /// <summary>
    /// Registers MetalChain and discovers handlers by scanning assemblies configured via <paramref name="buildOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="buildOptions">Optional delegate to configure scanning and handler behaviour.</param>
    public static IServiceCollection AddMetalChain(this IServiceCollection services,
        Action<IMetalChainOptionsBuilder>? buildOptions = null)
    {
        var options = new MetalChainOptionsBuilder();
        if (buildOptions != null) buildOptions(options);
        options.Initialize(services);
        return services;
    }

    /// <summary>
    /// Registers MetalChain handlers found in the assemblies that contain the specified <paramref name="types"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="types">Anchor types whose assemblies are scanned for handlers.</param>
    public static void AddMetalChainHandlers(this IServiceCollection services, params Type[] types) =>
        MetalChainOptionsBuilder.InitializeOrUpdate(services, types);
}
