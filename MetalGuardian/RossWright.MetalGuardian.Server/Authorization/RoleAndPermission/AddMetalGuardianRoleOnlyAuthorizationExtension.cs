using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization; 

public static class AddMetalGuardianRoleAndPermissionAuthorizationExtension
{
    public static IServiceCollection AddMetalGuardianRoleAndPermissionAuthorization<TPrivilege, TRole, TRepository>(this IServiceCollection services, bool useCaching = true)
        where TRepository : class, IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>
    {
        if (!typeof(TPrivilege).IsSimpleType())
        {
            throw new MetalGuardianException($"{typeof(TPrivilege).Name} is too complex to use as a privilege type. Use something like an int, string or enum.");
        }

        if (useCaching)
        {
            services.TryAddSingleton<IRoleAndPermissionAuthorizationCache<TPrivilege, TRole>,
                RoleAndPermissionAuthorizationCache<TPrivilege, TRole>>();

            services.TryAddSingleton(_ => (IAuthorizationCache)
                _.GetRequiredService<IRoleAndPermissionAuthorizationCache<TPrivilege, TRole>>());

            services.TryAddScoped<IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>>(_ =>
                new CachedRoleAndPermissionAuthorizationRepositoryAdapter<TPrivilege, TRole>(
                    (IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>)RossWright.MetalInjection.ActivatorUtilities.CreateInstance(_, typeof(TRepository)),
                    _.GetRequiredService<IRoleAndPermissionAuthorizationCache<TPrivilege, TRole>>()));
        }
        else
        {
            services.TryAddScoped<IRoleAndPermissionAuthorizationRepository<TPrivilege, TRole>, TRepository>();
        }
        
        services.AddScoped<IGlobalAuthorizationApiService<TPrivilege>, RoleAndPermissionAuthorizationService<TPrivilege, TRole>>();
        services.AddScoped(_ => (IGlobalAuthorizationService<TPrivilege>)_.GetRequiredService<IGlobalAuthorizationApiService<TPrivilege>>());

        return services;
    }
}
