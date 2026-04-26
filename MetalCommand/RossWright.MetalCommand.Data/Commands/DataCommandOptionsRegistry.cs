namespace RossWright.MetalCommand.Data;

/// <summary>
/// Singleton registered in DI that holds <see cref="CommandOptions"/> overrides
/// keyed by command type. Written to during application builder setup;
/// read by <c>EnvironmentArgMiddleware</c> at execution time and by
/// <c>CommandCollectionBuilder</c> during descriptor building.
/// </summary>
public sealed class DataCommandOptionsRegistry : ICommandOptionsRegistry
{
    private readonly Dictionary<Type, CommandOptions> _options = new();

    internal TOptions GetOrCreate<TOptions>(Type commandType) where TOptions : CommandOptions, new()
    {
        if (_options.TryGetValue(commandType, out var existing) && existing is TOptions typed)
            return typed;
        var opts = new TOptions();
        _options[commandType] = opts;
        return opts;
    }

    /// <inheritdoc/>
    public CommandOptions? Get(Type commandType) =>
        _options.GetValueOrDefault(commandType);
}
