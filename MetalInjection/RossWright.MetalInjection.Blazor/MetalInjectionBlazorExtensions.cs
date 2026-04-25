using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RossWright.MetalInjection;

/// <summary>
/// Extension methods that wire MetalInjection into a Blazor WebAssembly <see cref="WebAssemblyHostBuilder"/>.
/// </summary>
public static class MetalInjectionBlazorExtensions
{
    /// <summary>
    /// Adds MetalInjection to a <see cref="WebAssemblyHostBuilder"/>, performing assembly scanning
    /// and configuring the service provider factory for property injection.
    /// Automatically registers <see cref="Microsoft.AspNetCore.Components.InjectAttribute"/> as an
    /// alternate inject attribute so that Blazor's built-in <c>[Inject]</c> is honoured alongside
    /// MetalInjection's own <see cref="InjectAttribute"/>.
    /// </summary>
    /// <param name="hostBuilder">The WebAssembly host builder to configure.</param>
    /// <param name="setOptions">An optional delegate to configure MetalInjection options.</param>
    /// <returns>The <paramref name="hostBuilder"/> for chaining.</returns>
    public static WebAssemblyHostBuilder AddMetalInjection(
        this WebAssemblyHostBuilder hostBuilder,
        Action<IMetalInjectionOptionsBuilder>? setOptions = null)
    {
        var optionsBuilder = MetalInjectionOptionsBuilder.Create(InternalKey.Value);
        optionsBuilder.SetAlternateInjectAttribute<Microsoft.AspNetCore.Components.InjectAttribute>(_ => _.Key);
        if (setOptions != null) setOptions(optionsBuilder);
        optionsBuilder.InitializeServices(hostBuilder.Services, hostBuilder.Configuration);
        hostBuilder.ConfigureContainer(optionsBuilder.CreateServiceProviderFactory());
        return hostBuilder;
    }
}
