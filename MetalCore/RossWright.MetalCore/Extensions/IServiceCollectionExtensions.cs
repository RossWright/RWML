namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TService"/> as a scoped service that resolves
    /// by casting an already-registered <typeparamref name="TAliasOf"/> instance.
    /// Useful for exposing a single implementation under multiple service types.
    /// </summary>
    /// <typeparam name="TService">The alias service type to register.</typeparam>
    /// <typeparam name="TAliasOf">The existing service type whose instance will be cast.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddScopedAlias<TService, TAliasOf>(this IServiceCollection services)
        where TService : class
        where TAliasOf : class
    {
        services.AddScoped(_ => (_.GetRequiredService<TAliasOf>() as TService)!);
        return services;
    }

    /// <summary>
    /// Returns <see langword="true"/> if <typeparamref name="TService"/> is already
    /// registered in the service collection.
    /// </summary>
    /// <typeparam name="TService">The service type to check.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns><see langword="true"/> if the service is registered; otherwise <see langword="false"/>.</returns>
    public static bool HasService<TService>(this IServiceCollection services) =>
        services.Any(_ => _.ServiceType == typeof(TService));

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="serviceType"/> is already
    /// registered in the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceType">The service type to check.</param>
    /// <returns><see langword="true"/> if the service is registered; otherwise <see langword="false"/>.</returns>
    public static bool HasService(this IServiceCollection services, Type serviceType) =>
        services.Any(_ => _.ServiceType == serviceType);

    /// <summary>
    /// Applies a batch of service registration delegates to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="registrations">The registration delegates to apply.</param>
    public static void AddServices(this IServiceCollection services,
        IEnumerable<Action<IServiceCollection>> registrations)
        {
            foreach (var registration in registrations)
            {
                registration(services);
            }
        }
}