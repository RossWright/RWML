namespace RossWright.MetalCommand.Http.Internal;

/// <summary>
/// Singleton registry that stores all registered HTTP connection groups and their
/// per-environment entries. Populated during application startup by
/// <c>AddHttpConnections</c> calls and read at runtime by
/// <c>HttpConnectionResolver</c> and <c>EnvironmentAwareHttpClientFactory</c>.
/// </summary>
internal sealed class HttpConnectionRegistry
{
    // Key: group name — empty string for the unnamed (default) group.
    private readonly Dictionary<string, HttpConnectionEntry[]> _groups = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _registeredBaseNames = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The set of registered base connection names.  The unnamed group is represented by
    /// an empty string.  Used by <c>EnvironmentAwareHttpClientFactory</c> for O(1)
    /// interception decisions.
    /// </summary>
    public IReadOnlySet<string> RegisteredBaseNames => _registeredBaseNames;

    /// <summary>
    /// Adds or replaces the entries for <paramref name="groupName"/>.
    /// Called once per <c>AddHttpConnections</c> call during startup.
    /// </summary>
    /// <param name="groupName">
    /// The logical connection group name.  Pass an empty string for the unnamed
    /// (default) group.
    /// </param>
    /// <param name="entries">The per-environment entries for the group.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="entries"/> is empty.
    /// </exception>
    public void Upsert(string groupName, HttpConnectionEntry[] entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Length == 0)
            throw new ArgumentException("At least one environment entry is required.", nameof(entries));

        _groups[groupName] = entries;
        _registeredBaseNames.Add(groupName);
    }

    /// <summary>
    /// Returns the per-environment entries for the specified connection group.
    /// </summary>
    /// <param name="groupName">
    /// The logical connection group name, or <see langword="null"/> / empty string for the
    /// unnamed default group.
    /// </param>
    /// <returns>The array of <see cref="HttpConnectionEntry"/> instances for the group.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no group with the given name has been registered.
    /// </exception>
    public HttpConnectionEntry[] GetEntries(string? groupName)
    {
        var key = groupName ?? string.Empty;
        if (!_groups.TryGetValue(key, out var entries))
            throw new InvalidOperationException(
                $"No HTTP connection group registered for '{(string.IsNullOrEmpty(key) ? "(default)" : key)}'. " +
                "Call AddHttpConnections before using IHttpConnectionResolver.");
        return entries;
    }

    /// <summary>
    /// Returns the default environment name for the specified connection group.
    /// The default is the first entry whose <see cref="HttpConnectionEntry.IsDefault"/>
    /// is <see langword="true"/>, falling back to the first registered entry.
    /// </summary>
    /// <param name="groupName">
    /// The logical connection group name, or <see langword="null"/> / empty string for the
    /// unnamed default group.
    /// </param>
    /// <returns>The default environment name for the group.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the group has not been registered.
    /// </exception>
    public string DefaultEnvironment(string? groupName)
    {
        var entries = GetEntries(groupName);
        return (entries.FirstOrDefault(e => e.IsDefault) ?? entries[0]).Environment;
    }
}
