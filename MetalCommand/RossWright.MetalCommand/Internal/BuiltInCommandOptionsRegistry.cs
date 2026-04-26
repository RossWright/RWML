using RossWright.MetalCommand.Internal.Commands;

namespace RossWright.MetalCommand.Internal;

/// <summary>
/// Implements <see cref="ICommandOptionsRegistry"/> for the five commands that are
/// built into every <see cref="ConsoleApplication"/>. Translates per-command
/// invocation arrays from <see cref="BuiltInCommandOptions"/> into the
/// <see cref="CommandOptions"/> shape that <see cref="CommandCollectionBuilder"/> expects.
/// </summary>
internal sealed class BuiltInCommandOptionsRegistry(BuiltInCommandOptions options) : ICommandOptionsRegistry
{
    private readonly Dictionary<Type, CommandOptions> _map = new()
    {
        [typeof(ListContextCommand)] = new CommandOptions { Invocations = options.ListContextInvocations },
        [typeof(SetContextCommand)]  = new CommandOptions { Invocations = options.SetContextInvocations },
        [typeof(SaveConCommand)]     = new CommandOptions { Invocations = options.SaveContextInvocations },
        [typeof(LoadConCommand)]     = new CommandOptions { Invocations = options.LoadContextInvocations },
        [typeof(DupConCommand)]      = new CommandOptions { Invocations = options.DupConInvocations },
    };

    /// <inheritdoc/>
    public CommandOptions? Get(Type commandType) => _map.GetValueOrDefault(commandType);
}
