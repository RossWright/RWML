using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RossWright.MetalGuardian;

public static class MetalGuardianBlazorExtensions
{
    public static WebAssemblyHostBuilder AddMetalGuardianClient(this WebAssemblyHostBuilder appBuilder,
        Action<IMetalGuardianBlazorOptionsBuilder> setOptions)
    {
        MetalGuardianBlazorOptionsBuilder optionsBuilder = new(appBuilder);
        setOptions(optionsBuilder);
        optionsBuilder.InitializeClient(appBuilder.Services);
        return appBuilder;
    }

    public static void AddAuthenticatedHttpClient(
        this IMetalGuardianBlazorOptionsBuilder optionsBuilder,
        string? connectionName = null,
        bool isDefault = false) =>
        optionsBuilder.AddAuthenticatedHttpClient(
            ((IMetalGuardianBlazorOptionsBuilderInternal)optionsBuilder)
                .WebAssemblyHostBuilder
                .HostEnvironment
                .BaseAddress, 
            connectionName, 
            isDefault);

    public static void UseBlazorAuthentication(
        this IMetalGuardianBlazorOptionsBuilder optionsBuilder,
        string? blazorAuthenciationConnectionName = null) =>
        ((IOptionsBuilder)optionsBuilder)
            .AddServices(services => services
                .AddCascadingAuthenticationState()
                .AddAuthorizationCore()
                .TryAddScoped<AuthenticationStateProvider>(_ => new MetalGuardianAuthenticationStateProvider(
                    _.GetRequiredService<IMetalGuardianAuthenticationClient>(),
                    blazorAuthenciationConnectionName ??
                    _.GetRequiredService<IBaseAddressRepository>().DefaultConnectionName)));
}
