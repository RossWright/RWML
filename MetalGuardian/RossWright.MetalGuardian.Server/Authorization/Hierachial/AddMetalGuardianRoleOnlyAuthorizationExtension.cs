using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

public static class AddMetalGuardianHierarchialAuthorizationExtension
{
    public static IServiceCollection AddMetalGuardianHierarchialAuthorization<TPrivilege, TRole, TRepository>(this IServiceCollection services, bool useCaching = true)
        where TRepository : class, IHierarchialAuthorizationRepository<TPrivilege, TRole>
    {
        if (!typeof(TPrivilege).IsSimpleType())
        {
            throw new MetalGuardianException($"{typeof(TPrivilege).Name} is too complex to use as a privilege type. Use something like an int, string or enum.");
        }

        if (useCaching)
        {
            services.TryAddSingleton<IHierarchialAuthorizationCache<TPrivilege, TRole>,
                HierarchialAuthorizationCache<TPrivilege, TRole>>();

            services.TryAddSingleton(_ => (IAuthorizationCache)
                _.GetRequiredService<IHierarchialAuthorizationCache<TPrivilege, TRole>>());

            services.TryAddScoped<IHierarchialAuthorizationRepository<TPrivilege, TRole>>(_ =>
                new CachedHierarchialAuthorizationService<TPrivilege, TRole>(
                    (IHierarchialAuthorizationRepository<TPrivilege, TRole>)RossWright.MetalInjection.ActivatorUtilities.CreateInstance(_, typeof(TRepository)),
                    _.GetRequiredService<IHierarchialAuthorizationCache<TPrivilege, TRole>>()));
        }
        else
        {
            services.TryAddScoped<IHierarchialAuthorizationRepository<TPrivilege, TRole>, TRepository>();
        }
        
        services.AddScoped<IEntityAuthorizationApiService<TPrivilege>, HierarchialAuthorizationService<TPrivilege, TRole>>();
        services.AddScoped(_ => (IGlobalAuthorizationApiService<TPrivilege>)_.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());
        services.AddScoped(_ => (IEntityAuthorizationService<TPrivilege>)_.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());
        services.AddScoped(_ => (IGlobalAuthorizationService<TPrivilege>)_.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());

        return services;
    }
}
