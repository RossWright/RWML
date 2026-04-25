using Microsoft.Extensions.DependencyInjection;

namespace RossWright;

/// <summary>
/// Extension methods for registering <see cref="IGeoCoderService"/> in the DI container.
/// </summary>
public static class GeoCoderServiceExtensions
{
    /// <summary>
    /// Registers <see cref="IGeoCoderService"/> as a singleton service.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <returns>The same <paramref name="services"/> for fluent chaining.</returns>
    public static IServiceCollection AddGeoCoderService(this IServiceCollection services)
    {
        services.AddSingleton<IGeoCoderService, GeoCoderService>();
        return services;
    }
}