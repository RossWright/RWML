using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RossWright.MetalNexus;

/// <summary>
/// Extension methods for registering the MetalNexus client in a Blazor WebAssembly application.
/// </summary>
public static class MetalNexusBlazorExtensions
{
    /// <summary>
    /// Registers the MetalNexus client and the JavaScript interop script loader required by
    /// the <c>FileInput</c> component with the Blazor WebAssembly host.
    /// </summary>
    /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/> to register services into.</param>
    /// <param name="buildOptions">A delegate that configures the MetalNexus client options.</param>
    /// <returns>The same <see cref="WebAssemblyHostBuilder"/> for chaining.</returns>
    public static WebAssemblyHostBuilder AddMetalNexusClient(this WebAssemblyHostBuilder builder,
        Action<IMetalNexusClientOptionsBuilder> buildOptions)
    {
        builder.Services.AddMetalNexusClient(buildOptions);
        builder.Services.AddJsScriptLoader();
        return builder;
    }

    /// <summary>
    /// Registers the default <see cref="HttpClient"/> connection, pre-configured with the
    /// Blazor application's <see cref="IWebAssemblyHostEnvironment.BaseAddress"/>.
    /// </summary>
    /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/> to register into.</param>
    /// <param name="configureClient">An optional delegate for additional <see cref="HttpClient"/> configuration.</param>
    /// <returns>The same <see cref="WebAssemblyHostBuilder"/> for chaining.</returns>
    public static WebAssemblyHostBuilder AddHttpClient(this WebAssemblyHostBuilder builder,
        Action<HttpClient>? configureClient = null) =>
        AddHttpClient(builder, Microsoft.Extensions.Options.Options.DefaultName, configureClient);

    /// <summary>
    /// Registers a named <see cref="HttpClient"/> connection, pre-configured with the
    /// Blazor application's <see cref="IWebAssemblyHostEnvironment.BaseAddress"/>.
    /// </summary>
    /// <param name="builder">The <see cref="WebAssemblyHostBuilder"/> to register into.</param>
    /// <param name="connectionName">The name used to identify this connection.</param>
    /// <param name="configureClient">An optional delegate for additional <see cref="HttpClient"/> configuration.</param>
    /// <returns>The same <see cref="WebAssemblyHostBuilder"/> for chaining.</returns>
    public static WebAssemblyHostBuilder AddHttpClient(this WebAssemblyHostBuilder builder,
        string connectionName, Action<HttpClient>? configureClient = null)
    {
        builder.Services.AddHttpClient(connectionName, _ =>
        {
            _.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            if (configureClient != null) configureClient(_);
        });
        return builder;
    }
}
