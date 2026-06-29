namespace RossWright.MetalCommand;

/// <summary>
/// Extension methods for <see cref="ConsoleApplication"/> and <see cref="IConsoleApplicationBuilder"/>.
/// </summary>
public static class ConsoleApplicationExtensions
{
    /// <summary>
    /// Applies a configuration action to the application and returns it for chaining.
    /// </summary>
    /// <param name="app">The application to configure.</param>
    /// <param name="useApp">The configuration action.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static ConsoleApplication UseApp(this ConsoleApplication app, Action<ConsoleApplication> useApp)
    {
        useApp(app);
        return app;
    }

    /// <summary>
    /// Pre-populates a session context key/value pair before the application starts.
    /// </summary>
    /// <param name="app">The console application.</param>
    /// <param name="contextName">The context key.</param>
    /// <param name="value">The value to store.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static ConsoleApplication AddContext(this ConsoleApplication app, string contextName, string value)
    {
        app.Context.Add(contextName, value);
        return app;
    }

    /// <summary>
    /// Customizes the invocation tokens (and DupCon behaviour) for the five commands that are
    /// built into every <see cref="ConsoleApplication"/>: <c>listcontext</c>, <c>setcontext</c>,
    /// <c>savecon</c>, <c>loadcon</c>, and <c>dupcon</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.CustomizeBuiltInCommands(o =>
    /// {
    ///     o.SaveContextInvocations = ["sc"];
    ///     o.LoadContextInvocations = ["lc"];
    ///     o.DupConInvocations      = ["dup", "dc"];
    /// });
    /// </code>
    /// </example>
    public static IConsoleApplicationBuilder CustomizeBuiltInCommands(
        this IConsoleApplicationBuilder builder,
        Action<BuiltInCommandOptions> configure)
    {
        configure(((ConsoleApplicationBuilder)builder).BuiltInCommandOptions);
        return builder;
    }

    /// <summary>Builds and returns the <see cref="ConsoleApplication"/> from the configured builder.</summary>
    /// <param name="builder">The configured application builder.</param>
    /// <returns>A ready-to-run <see cref="ConsoleApplication"/>.</returns>
    public static ConsoleApplication Build(this IConsoleApplicationBuilder builder) => 
        ((ConsoleApplicationBuilder)builder).Build();

    /// <summary>
    /// Loads a context file into the session before the application starts.
    /// The <paramref name="name"/> is resolved relative to <see cref="AppContext.BaseDirectory"/>;
    /// the extension <c>.mcc.json</c> is appended automatically if the exact path is not found.
    /// If neither path exists a warning is printed and the application starts with an empty context.
    /// </summary>
    /// <param name="builder">The configured application builder.</param>
    /// <param name="name">File name or path. Defaults to <c>"default"</c>.</param>
    /// <param name="showWarnIfMissing">Whether to print a warning when no context file is found.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder LoadContext(
        this IConsoleApplicationBuilder builder,
        string name = "default", bool showWarnIfMissing = true)
    {
        var b = (ConsoleApplicationBuilder)builder;
        b.LoadContextName = name;
        b.ShowWarnIfContextMissing = showWarnIfMissing;
        return builder;
    }
}
