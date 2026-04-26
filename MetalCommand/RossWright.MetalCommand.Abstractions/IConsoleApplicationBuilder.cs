using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand;

/// <summary>
/// Configures a <see cref="ConsoleApplication"/> before it is built. Provides access to
/// commands, DI services, configuration, and pipeline options.
/// </summary>
public interface IConsoleApplicationBuilder : IOptionsBuilder
{
    /// <summary>Registers <see cref="ICommand"/> implementations with the application.</summary>
    ICommandCollection Commands { get; }

    /// <summary>Pre-built application configuration from <c>appsettings.json</c> and <c>appsettings.dev.json</c>.</summary>
    IConfiguration Configuration { get; }

    /// <summary>The shared <see cref="IConsole"/> instance used by the application.</summary>
    IConsole Console { get; }

    /// <summary>
    /// Optional factory that builds the prompt string from the current session context.
    /// When <see langword="null"/> the default prompt (<c>&gt;</c>) is used.
    /// </summary>
    Func<IDictionary<string, string>, string>? PromptFactory { get; set; }

    /// <summary>The DI service collection used to register services consumed by commands.</summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Overrides the default colors used for intro/outro messages, help text, warnings, and errors.
    /// Pass <see langword="null"/> for any role to keep the current default.
    /// </summary>
    /// <param name="introOutroColor">Color for startup and goodbye messages.</param>
    /// <param name="helpColor">Color for help output.</param>
    /// <param name="warningColor">Color for warning messages.</param>
    /// <param name="errorColor">Foreground color for error messages.</param>
    /// <param name="errorBgColor">Background color for error messages.</param>
    /// <returns>This builder for chaining.</returns>
    IConsoleApplicationBuilder SetColors(ConsoleColor? introOutroColor = null, ConsoleColor? helpColor = null, ConsoleColor? warningColor = null, ConsoleColor? errorColor = null, ConsoleColor? errorBgColor = null);

    /// <summary>
    /// Sets the number of spaces per indent level used by <see cref="IConsole.Indent"/>.
    /// Defaults to <c>5</c>.
    /// </summary>
    /// <param name="width">Number of spaces per indent level.</param>
    /// <returns>This builder for chaining.</returns>
    IConsoleApplicationBuilder SetTabWidth(int width);

    /// <summary>
    /// Registers a middleware type to participate in the execution pipeline for
    /// attribute-driven <see cref="ICommand"/> commands. Middleware is executed in
    /// registration order and resolved from DI on each command invocation.
    /// </summary>
    IConsoleApplicationBuilder AddMiddleware<TMiddleware>() where TMiddleware : class, ICommandMiddleware;

    void SetServiceProviderFactory(IServiceProviderFactory<IServiceCollection> serviceProviderFactory);
}

/// <summary>
/// Fluent extension methods for <see cref="IConsoleApplicationBuilder"/>.
/// </summary>
public static class IConsoleApplicationBuilderExtensions
{
    /// <summary>
    /// Registers DI services via a fluent delegate.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="addServices">Delegate that configures <see cref="IServiceCollection"/>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddServices(this IConsoleApplicationBuilder builder,
    Action<IServiceCollection> addServices)
    {
        addServices(builder.Services);
        return builder;
    }

    /// <summary>
    /// Registers commands via a fluent delegate.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="addCommands">Delegate that registers commands on <see cref="ICommandCollection"/>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddCommands(this IConsoleApplicationBuilder builder, 
        Action<ICommandCollection> addCommands)
    {
        addCommands(builder.Commands);
        return builder;
    }

    /// <summary>
    /// Registers commands via a fluent delegate that also receives the application configuration.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="addCommands">Delegate that registers commands on <see cref="ICommandCollection"/> with access to <see cref="IConfiguration"/>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddCommands(this IConsoleApplicationBuilder builder,
        Action<ICommandCollection, IConfiguration> addCommands)
    {
        addCommands(builder.Commands, builder.Configuration);
        return builder;
    }

    /// <summary>
    /// Sets the delegate used to build the prompt string from the current session context.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="promptFactory">
    /// A delegate that receives the session context dictionary and returns a prompt string.
    /// Pass <see langword="null"/> to use the default prompt.
    /// </param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder SetPromptFactory(this IConsoleApplicationBuilder builder, 
        Func<IDictionary<string, string>, string>? promptFactory)
    {
        builder.PromptFactory = promptFactory;
        return builder;
    }

}