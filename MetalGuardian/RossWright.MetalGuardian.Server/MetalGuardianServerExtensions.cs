using Microsoft.AspNetCore.Builder;

namespace RossWright.MetalGuardian;

/// <summary>
/// Entry-point extension methods for registering MetalGuardian server services.
/// </summary>
public static class MetalGuardianServerExtensions
{
    /// <summary>
    /// Registers MetalGuardian server services (authentication, JWT, OTP, authorization)
    /// into the application's DI container. Configure the services via
    /// <paramref name="optionsBuilder"/>. Requires <c>AddDistributedMemoryCache()</c>
    /// (or a distributed cache) when OTP is enabled.
    /// </summary>
    public static WebApplicationBuilder AddMetalGuardianServer(
        this WebApplicationBuilder appBuilder,
        Action<IMetalGuardianServerOptionBuilder> optionsBuilder)
    {
        MetalGuardianServerOptionBuilder options = new();
        optionsBuilder(options);
        options.InitializeServer(appBuilder.Services, appBuilder.Configuration);
        return appBuilder;
    }
}
