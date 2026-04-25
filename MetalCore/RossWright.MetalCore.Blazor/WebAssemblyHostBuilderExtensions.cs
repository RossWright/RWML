using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright;

/// <summary>
/// Fluent extension methods on <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder"/>
/// and <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHost"/> for chaining Blazor WASM host setup.
/// </summary>
public static class WebAssemblyHostBuilderExtensions
{
    /// <summary>
    /// Registers Blazor root components via a fluent callback, returning the builder for further chaining.
    /// </summary>
    /// <param name="builder">The <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder"/>.</param>
    /// <param name="addRootComponents">An action that registers root components on <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder.RootComponents"/>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static WebAssemblyHostBuilder AddRootComponents(this WebAssemblyHostBuilder builder,
        Action<RootComponentMappingCollection> addRootComponents)
    {
        addRootComponents(builder.RootComponents);
        return builder;
    }

    /// <summary>
    /// Registers DI services via a fluent callback, returning the builder for further chaining.
    /// </summary>
    /// <param name="builder">The <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder"/>.</param>
    /// <param name="addServices">An action that registers services on <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHostBuilder.Services"/>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static WebAssemblyHostBuilder AddServices(this WebAssemblyHostBuilder builder,
        Action<IServiceCollection> addServices)
    {
        addServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Configures the built <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHost"/> via a fluent callback, returning the host for further chaining.
    /// </summary>
    /// <param name="app">The built <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHost"/>.</param>
    /// <param name="useApp">An action that configures the host after it is built.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebAssemblyHost UseApp(this WebAssemblyHost app,
        Action<WebAssemblyHost> useApp)
    {
        useApp(app);
        return app;
    }

    /// <summary>
    /// Runs an async initialization step before starting the <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHost"/>.
    /// Useful for pre-loading services or data before the first Blazor render.
    /// </summary>
    /// <param name="app">The built <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHost"/>.</param>
    /// <param name="butFirst">An async delegate executed before <see cref="Microsoft.AspNetCore.Components.WebAssembly.Hosting.WebAssemblyHost.RunAsync"/> is called.</param>
    /// <returns>A <see cref="Task"/> that completes when the host shuts down.</returns>
    public static async Task RunAsync(this WebAssemblyHost app, 
        Func<WebAssemblyHost, Task> butFirst)
    {
        await butFirst(app);
        await app.RunAsync();
    }
}
