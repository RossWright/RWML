namespace RossWright.MetalCommand;

/// <summary>
/// Carries all context for a single command execution through the middleware pipeline.
/// Available to every <see cref="ICommandMiddleware"/> and set by the framework before
/// <see cref="ICommand.ExecuteAsync"/> is called.
/// </summary>
public sealed class CommandContext
{
    /// <summary>The command instance that will be (or has been) executed.</summary>
    public required ICommand Command { get; init; }

    /// <summary>The console for the current session.</summary>
    public required IConsole Console { get; init; }

    /// <summary>
    /// The session-wide key/value context dictionary shared across all commands
    /// (equivalent to <see cref="ICommandExecutor.Context"/>).
    /// </summary>
    public required IDictionary<string, string> SessionContext { get; init; }

    /// <summary>Cancellation token for the current command execution.</summary>
    public required CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Argument values resolved and bound to the command's <c>[Arg]</c> properties,
    /// keyed by property name. Set by the framework after argument binding completes;
    /// available to middleware that runs after binding.
    /// </summary>
    public IReadOnlyDictionary<string, object?> BoundArgs { get; internal set; } =
        new Dictionary<string, object?>();

    /// <summary>
    /// The result of the command execution. Set by the framework after
    /// <see cref="ICommand.ExecuteAsync"/> returns; available to middleware that
    /// runs after the command has executed.
    /// </summary>
    public CommandResult Result { get; internal set; }
}
