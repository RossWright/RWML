using RossWright.MetalCommand;

namespace RossWright.MetalGuardian;

/// <summary>
/// Extension methods for registering MetalGuardian client services in a MetalCommand console application.
/// </summary>
public static class MetalGuardianMetalCommandExtension
{
    /// <summary>
    /// Registers MetalGuardian client services on the MetalCommand <paramref name="builder"/>.
    /// Use the <paramref name="setOptions"/> callback to configure authentication endpoints,
    /// password validation, device fingerprinting, and authenticated HTTP clients.
    /// The builder's configuration is available via
    /// <see cref="IMetalGuardianConsoleOptionsBuilder.Configuration"/>.
    /// </summary>
    public static IConsoleApplicationBuilder AddMetalGuardianClient(this IConsoleApplicationBuilder builder,
        Action<IMetalGuardianConsoleOptionsBuilder> setOptions)
    {
        MetalGuardianConsoleOptionsBuilder optionsBuilder = new(builder);
        setOptions(optionsBuilder);
        optionsBuilder.InitializeClient(builder.Services);
        return builder;
    }
}
