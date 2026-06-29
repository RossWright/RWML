using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace RossWright.MetalGuardian;

/// <summary>
/// Extension methods for registering the MetalGuardian Blazor WASM client.
/// </summary>
public static class MetalGuardianBlazorExtensions
{
    /// <summary>
    /// Registers the MetalGuardian client for a Blazor WebAssembly application.
    /// Call this on <see cref="WebAssemblyHostBuilder"/> in <c>Program.cs</c> and use
    /// the <paramref name="setOptions"/> callback to configure connections, HTTP clients,
    /// and optional features such as device fingerprinting.
    /// If OTP support is required, also call <c>services.AddDistributedMemoryCache()</c>.
    /// </summary>
    public static WebAssemblyHostBuilder AddMetalGuardianClient(this WebAssemblyHostBuilder appBuilder,
        Action<IMetalGuardianBlazorOptionsBuilder> setOptions)
    {
        MetalGuardianBlazorOptionsBuilder optionsBuilder = new(appBuilder);
        setOptions(optionsBuilder);
        optionsBuilder.InitializeClient(appBuilder.Services);
        return appBuilder;
    }

    /// <summary>
    /// Registers an authenticated <see cref="HttpClient"/> using the Blazor host's base address.
    /// Convenience overload that reads the base address from the <see cref="IWebAssemblyHostEnvironment"/>.
    /// </summary>
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

    /// <summary>
    /// Wires up Blazor's <see cref="AuthenticationStateProvider"/> to the MetalGuardian authentication client,
    /// enabling <c>AuthorizeView</c> and cascading authentication state throughout the component tree.
    /// </summary>
    public static void UseBlazorAuthentication(
        this IMetalGuardianBlazorOptionsBuilder optionsBuilder,
        string? blazorAuthenciationConnectionName = null) =>
        ((IOptionsBuilder)optionsBuilder)
            .AddServices(services => services
                .AddCascadingAuthenticationState()
                .AddAuthorizationCore()
                .TryAddScoped<AuthenticationStateProvider>(_ => new MetalGuardianAuthenticationStateProvider(
                    _.GetRequiredService<IMetalGuardianAuthenticationClient>(),
                    _.GetRequiredService<ILogger<MetalGuardianAuthenticationStateProvider>>(),
                    blazorAuthenciationConnectionName ??
                    _.GetRequiredService<IBaseAddressRepository>().DefaultConnectionName)));
}
