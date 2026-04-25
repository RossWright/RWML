using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

public static class AddMetalGuardianRoleOnlyAuthorizationExtension
{
    public static IServiceCollection AddMetalGuardianRoleOnlyAuthorization<TPrivilege, TRole, TRepository>(this IServiceCollection services, bool useCaching = true)
        where TRepository : class, IRoleOnlyAuthorizationRepository<TPrivilege, TRole>
    {
        if (!typeof(TPrivilege).IsSimpleType())
        {
            throw new MetalGuardianException($"{typeof(TPrivilege).Name} is too complex to use as a privilege type. Use something like an int, string or enum.");
        }

        if (useCaching)
        {
            services.TryAddSingleton<IRoleOnlyAuthorizationCache<TPrivilege, TRole>,
                RoleOnlyAuthorizationCache<TPrivilege, TRole>>();

            services.TryAddSingleton(_ => (IAuthorizationCache)
                _.GetRequiredService<IRoleOnlyAuthorizationCache<TPrivilege, TRole>>());

            services.TryAddScoped<IRoleOnlyAuthorizationRepository<TPrivilege, TRole>>(_ =>
                new CachedRoleOnlyAuthorizationRepositoryAdapter<TPrivilege, TRole>(
                    (IRoleOnlyAuthorizationRepository<TPrivilege, TRole>)RossWright.MetalInjection.ActivatorUtilities.CreateInstance(_, typeof(TRepository)),
                    _.GetRequiredService<IRoleOnlyAuthorizationCache<TPrivilege, TRole>>()));
        }
        else
        {
            services.TryAddScoped<IRoleOnlyAuthorizationRepository<TPrivilege, TRole>, TRepository>();
        }

        services.AddScoped<IGlobalAuthorizationApiService<TPrivilege>, RoleOnlyAuthorizationApiService<TPrivilege, TRole>>();
        services.AddScoped(_ => (IGlobalAuthorizationService<TPrivilege>) _.GetRequiredService<IGlobalAuthorizationApiService<TPrivilege>>());

        return services;
    }
}
