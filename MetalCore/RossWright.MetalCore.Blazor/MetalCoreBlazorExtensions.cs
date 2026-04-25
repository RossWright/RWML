using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RossWright;

/// <summary>
/// Extension methods on <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> for registering MetalCore Blazor services.
/// </summary>
public static class MetalCoreBlazorExtensions
{
    /// <summary>
    /// Registers <see cref="RossWright.IJsScriptLoaderService"/> as a singleton service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddJsScriptLoader(this IServiceCollection services)
    {
        services.TryAddSingleton<IJsScriptLoaderService, JsScriptLoaderService>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="IBrowserLocalStorage"/> as a transient service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddBrowserLocalStorage(this IServiceCollection services)
    {
        services.TryAddTransient<IBrowserLocalStorage, BrowserLocalStorage>();
        return services;
    }
}
