using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RossWright.MetalGuardian.Authentication;

namespace RossWright.MetalGuardian;

internal interface IMetalGuardianBlazorOptionsBuilderInternal
{
    WebAssemblyHostBuilder WebAssemblyHostBuilder { get; }
}

internal class MetalGuardianBlazorOptionsBuilder : 
    MetalGuardianClientOptionsBuilder, 
    IMetalGuardianBlazorOptionsBuilder
{
    public MetalGuardianBlazorOptionsBuilder(WebAssemblyHostBuilder builder)  =>
        WebAssemblyHostBuilder = builder;
    public WebAssemblyHostBuilder WebAssemblyHostBuilder { get; }
    public string HostBaseAddress => WebAssemblyHostBuilder.HostEnvironment.BaseAddress;
    public IConfiguration Configuration => WebAssemblyHostBuilder.Configuration;

    public void UseDeviceFingerprinting() => UseDeviceFingerprinting<DeviceFingerprintService>();

    public override void InitializeClient(IServiceCollection services)
    {
        services.AddBrowserLocalStorage();
        services.TryAddScoped<IAuthenticationTokenStorage, BlazorAuthenticationTokenRepository>();
        base.InitializeClient(services);
    }
}
