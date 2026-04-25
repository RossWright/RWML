using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace RossWright.MetalInjection;

/// <summary>
/// Extension methods that wire MetalInjection into an ASP.NET Core <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class MetalInjectionServerExtensions
{
    /// <summary>
    /// Adds MetalInjection to a <see cref="WebApplicationBuilder"/>, performing assembly scanning,
    /// configuring the service provider factory for property injection, and registering discovered
    /// hosted services. Automatically registers
    /// <see cref="Microsoft.AspNetCore.Components.InjectAttribute"/> as an alternate inject attribute
    /// and wires up MetalInjection property injection for MVC controllers.
    /// </summary>
    /// <param name="appBuilder">The web application builder to configure.</param>
    /// <param name="setOptions">An optional delegate to configure MetalInjection options.</param>
    /// <returns>The <paramref name="appBuilder"/> for chaining.</returns>
    public static WebApplicationBuilder AddMetalInjection(this WebApplicationBuilder appBuilder,
        Action<IMetalInjectionOptionsBuilder>? setOptions = null)
    {
        var optionsBuilder = MetalInjectionOptionsBuilder.Create(InternalKey.Value);
        optionsBuilder.SetAlternateInjectAttribute<Microsoft.AspNetCore.Components.InjectAttribute>(_ => _.Key);
        if (setOptions != null) setOptions(optionsBuilder);
        optionsBuilder.InitializeServices(appBuilder.Services, appBuilder.Configuration);
        appBuilder.Host.UseServiceProviderFactory(optionsBuilder.CreateServiceProviderFactory());
        appBuilder.Services.AddSingleton<IControllerActivator, MetalInjectionControllerActivator>();

        foreach (var hostedServiceType in optionsBuilder.ConsideredTypes
            .Where(_ => _.HasAttribute<HostedServiceAttribute>() &&
                        typeof(IHostedService).IsAssignableFrom(_)))
        {
            appBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHostedService), hostedServiceType));
        }

        return appBuilder;
    }
}