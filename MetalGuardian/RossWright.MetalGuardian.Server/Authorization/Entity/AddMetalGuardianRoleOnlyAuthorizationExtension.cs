using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

public static class AddMetalGuardianEntityAuthorizationExtension
{
    public static IServiceCollection AddMetalGuardianEntityAuthorization<TPrivilege, TRole, TRepository>(this IServiceCollection services, bool useCaching = true)
        where TRepository : class, IEntityAuthorizationRepository<TPrivilege, TRole>
    {
        if (!typeof(TPrivilege).IsSimpleType())
        {
            throw new MetalGuardianException($"{typeof(TPrivilege).Name} is too complex to use as a privilege type. Use something like an int, string or enum.");
        }

        if (useCaching)
        {
            services.TryAddSingleton<IEntityAuthorizationCache<TPrivilege, TRole>,
                EntityAuthorizationCache<TPrivilege, TRole>>();

            services.TryAddSingleton(_ => (IAuthorizationCache)
                _.GetRequiredService<IEntityAuthorizationCache<TPrivilege, TRole>>());

            services.TryAddScoped<IEntityAuthorizationRepository<TPrivilege, TRole>>(_ =>
                new CachedEntityAuthorizationService<TPrivilege, TRole>(
                    (IEntityAuthorizationRepository<TPrivilege, TRole>)RossWright.MetalInjection.ActivatorUtilities.CreateInstance(_, typeof(TRepository)),
                    _.GetRequiredService<IEntityAuthorizationCache<TPrivilege, TRole>>()));
        }
        else
        {
            services.TryAddScoped<IEntityAuthorizationRepository<TPrivilege, TRole>, TRepository>();
        }

        services.AddScoped<IEntityAuthorizationApiService<TPrivilege>, EntityAuthorizationService<TPrivilege, TRole>>();
        services.AddScoped(_ => (IGlobalAuthorizationApiService<TPrivilege>)_.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());
        services.AddScoped(_ => (IEntityAuthorizationService<TPrivilege>)_.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());
        services.AddScoped(_ => (IGlobalAuthorizationService<TPrivilege>)_.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());

        return services;
    }
}
