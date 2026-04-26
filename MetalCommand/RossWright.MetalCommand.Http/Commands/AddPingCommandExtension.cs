using RossWright.MetalCommand.Http.Commands;

namespace RossWright.MetalCommand.Http;

/// <summary>
/// Extension methods for registering the built-in <c>ping</c> command.
/// </summary>
public static class AddPingCommandExtension
{
    /// <summary>
    /// Adds the built-in <c>ping</c> command to the application.
    /// The command sends a GET request to the active HTTP environment and reports
    /// the status code and latency.
    /// </summary>
    /// <param name="builder">The <see cref="IConsoleApplicationBuilder"/> to configure.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddPingCommand(this IConsoleApplicationBuilder builder)
    {
        builder.Commands.Add(typeof(PingCommand));
        return builder;
    }
}
