using RossWright.MetalCommand;

namespace RossWright.MetalGuardian;

public static class MetalGuardianMetalCommandExtension
{
    public static IConsoleApplicationBuilder AddMetalGuardianClient(this IConsoleApplicationBuilder builder,
        Action<IMetalGuardianConsoleOptionsBuilder> setOptions)
    {
        MetalGuardianConsoleOptionsBuilder optionsBuilder = new(builder);
        setOptions(optionsBuilder);
        optionsBuilder.InitializeClient(builder.Services);
        return builder;
    }
}
