namespace RossWright.MetalCommand;

/// <summary>
/// Dispatches commands programmatically. Registered as a singleton in the DI container.
/// Inject into commands that need to call other commands (e.g. a <c>reload</c> command
/// that sequences <c>migrate</c> then <c>load</c>).
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Dispatches a command by its invocation string.
    /// </summary>
    /// <param name="invocation">The command's invocation token (case-insensitive).</param>
    /// <param name="args">Positional arguments passed to the command.</param>
    Task Execute(string invocation, params string[] args);

    /// <summary>
    /// The session-wide key/value context dictionary shared across all commands.
    /// </summary>
    IDictionary<string, string> Context { get; }
}
