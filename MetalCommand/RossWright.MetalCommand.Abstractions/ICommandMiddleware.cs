namespace RossWright.MetalCommand;

/// <summary>
/// Participates in the command execution pipeline for attribute-driven
/// <see cref="ICommand"/> commands.
/// </summary>
/// <remarks>
/// <para>
/// Middleware is executed in registration order (i.e. the first registered middleware
/// is the outermost wrapper). Call <paramref name="next"/> to pass control to the next
/// middleware in the chain; the innermost step calls
/// <see cref="ICommand.ExecuteAsync"/> and sets <see cref="CommandContext.Result"/>.
/// </para>
/// <para>
/// Middleware is resolved from DI on every command execution, so constructor injection
/// of scoped or transient services is fully supported.
/// </para>
/// <para>
/// Register middleware via
/// <see cref="IConsoleApplicationBuilder.AddMiddleware{TMiddleware}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class TimingMiddleware : ICommandMiddleware
/// {
///     public async Task InvokeAsync(CommandContext context, Func&lt;CommandContext, Task&gt; next)
///     {
///         var sw = Stopwatch.StartNew();
///         await next(context);
///         context.Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
///     }
/// }
/// </code>
/// </example>
public interface ICommandMiddleware
{
    /// <summary>
    /// Executes this middleware. Call <paramref name="next"/> to invoke the next
    /// middleware (or the command itself if this is the last middleware).
    /// </summary>
    /// <param name="context">The execution context for the current command.</param>
    /// <param name="next">
    /// The next step in the pipeline. Must be called to continue execution.
    /// </param>
    Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next);
}
