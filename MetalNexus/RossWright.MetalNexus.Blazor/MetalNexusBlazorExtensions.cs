using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RossWright.MetalNexus;

public static class MetalNexusBlazorExtensions
{
    public static WebAssemblyHostBuilder AddMetalNexusClient(this WebAssemblyHostBuilder builder,
        Action<IMetalNexusClientOptionsBuilder> buildOptions)
    {
        builder.Services.AddMetalNexusClient(buildOptions);
        builder.Services.AddJsScriptLoader();
        return builder;
    }

    public static WebAssemblyHostBuilder AddHttpClient(this WebAssemblyHostBuilder builder,
        Action<HttpClient>? configureClient = null) =>
        AddHttpClient(builder, Microsoft.Extensions.Options.Options.DefaultName, configureClient);

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
