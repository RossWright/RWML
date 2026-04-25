using Microsoft.AspNetCore.Builder;

namespace RossWright.MetalGuardian;

public static class MetalGuardianServerExtensions
{
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
