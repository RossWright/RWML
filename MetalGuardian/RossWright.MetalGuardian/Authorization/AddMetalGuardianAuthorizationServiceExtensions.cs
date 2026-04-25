using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authorization.Support;

namespace RossWright.MetalGuardian.Authorization;

public static class AddMetalGuardianAuthorizationServiceExtensions
{
    public static IServiceCollection AddMetalGuardianGlobalAuthorization<TPrivilege, TGlobalAuthorizationApiService>(this IServiceCollection services, string? connectionName = null) 
        where TGlobalAuthorizationApiService : class, IGlobalAuthorizationApiService<TPrivilege>
    {
        if (!typeof(TPrivilege).IsSimpleType()) throw new MetalGuardianException(
            $"{typeof(TPrivilege).Name} is too complex to use as a privilege type. Use something like an int, string or enum.");
        
        services.TryAddScoped<IGlobalAuthorizationApiService<TPrivilege>, TGlobalAuthorizationApiService>();
        
        services.AddScoped<IGlobalAuthorizationService<TPrivilege>>(_ => new GlobalAuthorizationClientService<TPrivilege>(
            _.GetRequiredService<IGlobalAuthorizationApiService<TPrivilege>>(), connectionName));
        
        services.AddScoped(_ => (IAuthenticationAuthorizationConnection)_.GetRequiredService<IAuthorizationContext<TPrivilege>>());
        
        return services;
    }

    public static IServiceCollection AddMetalGuardianEntityAuthorization<TPrivilege, TMetalGuardianAdvancedAuthorizationApiService>(this IServiceCollection services, string? connectionName = null)
    where TMetalGuardianAdvancedAuthorizationApiService : class, IEntityAuthorizationApiService<TPrivilege>
    {
        if (!typeof(TPrivilege).IsSimpleType()) throw new MetalGuardianException(
            $"{typeof(TPrivilege).Name} is too complex to use as a privilege type. Use something like an int, string or enum.");

        services.TryAddScoped<IEntityAuthorizationApiService<TPrivilege>, TMetalGuardianAdvancedAuthorizationApiService>();
        services.TryAddScoped<IGlobalAuthorizationApiService<TPrivilege>>(_ => _.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>());

        services.AddScoped<IEntityAuthorizationService<TPrivilege>>(_ => new EntityAuthorizationClientService<TPrivilege>(
            _.GetRequiredService<IEntityAuthorizationApiService<TPrivilege>>(), connectionName));
        services.AddScoped<IGlobalAuthorizationService<TPrivilege>>(_ => _.GetRequiredService<IEntityAuthorizationService<TPrivilege>>());

        services.AddScoped(_ => (IAuthenticationAuthorizationConnection)_.GetRequiredService<IEntityAuthorizationService<TPrivilege>>());

        return services;
    }
}
