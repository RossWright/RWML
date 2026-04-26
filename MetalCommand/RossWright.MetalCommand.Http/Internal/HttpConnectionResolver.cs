namespace RossWright.MetalCommand.Http.Internal;

/// <summary>
/// Scoped service that implements <see cref="IHttpConnectionResolver"/> and
/// <see cref="IEnvironmentSource"/> for the unnamed (default) connection group.
/// </summary>
/// <remarks>
/// The active environment is resolved in the following priority order:
/// <list type="number">
///   <item><description>The explicit <c>environment</c> parameter passed by the caller.</description></item>
///   <item><description>The environment returned by any registered <see cref="IEnvironmentSource"/> (other than this instance).</description></item>
///   <item><description>The default environment for the connection group as defined in the registry.</description></item>
/// </list>
/// </remarks>
internal sealed class HttpConnectionResolver(
    HttpConnectionRegistry registry,
    IHttpClientFactory realFactory,
    IEnumerable<IEnvironmentSource> environmentSources)
    : IHttpConnectionResolver, IEnvironmentSource
{
    // ── IEnvironmentSource (for the default unnamed group) ─────────────────

    /// <inheritdoc />
    public string DefaultEnvironment => registry.DefaultEnvironment(null);

    /// <inheritdoc />
    public EnvironmentEntry[] Environments =>
        registry.GetEntries(null)
                .Select(e => new EnvironmentEntry { Name = e.Environment, IsProtected = e.IsProtected })
                .ToArray();

    // ── IHttpConnectionResolver ────────────────────────────────────────────

    /// <inheritdoc />
    public string GetClientName(string? environment = null, string? baseConnectionName = null)
    {
        var groupName = string.IsNullOrEmpty(baseConnectionName) ? string.Empty : baseConnectionName;

        // Validate the group exists before anything else.
        var entries = registry.GetEntries(groupName);

        var resolvedEnv = ResolveEnvironment(environment, groupName, entries);

        return string.IsNullOrEmpty(groupName)
            ? $"MetalCommand:{resolvedEnv}"
            : $"MetalCommand:{groupName}:{resolvedEnv}";
    }

    /// <inheritdoc />
    public HttpClient GetClient(string? environment = null, string? baseConnectionName = null)
        => realFactory.CreateClient(GetClientName(environment, baseConnectionName));

    // ── Helpers ────────────────────────────────────────────────────────────

    private string ResolveEnvironment(
        string? requestedEnvironment,
        string groupName,
        HttpConnectionEntry[] entries)
    {
        // 1. Caller supplied an explicit environment — validate and use it.
        if (!string.IsNullOrEmpty(requestedEnvironment))
        {
            var match = entries.FirstOrDefault(
                e => string.Equals(e.Environment, requestedEnvironment, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                throw new InvalidOperationException(
                    $"Environment '{requestedEnvironment}' is not registered for HTTP connection group " +
                    $"'{(string.IsNullOrEmpty(groupName) ? "(default)" : groupName)}'. " +
                    $"Registered environments: {string.Join(", ", entries.Select(e => e.Environment))}.");

            return match.Environment;
        }

        // 2. Ask other IEnvironmentSource registrations (skip self to avoid recursion).
        foreach (var source in environmentSources)
        {
            if (ReferenceEquals(source, this)) continue;

            var envFromSource = source.DefaultEnvironment;
            if (!string.IsNullOrEmpty(envFromSource))
            {
                var match = entries.FirstOrDefault(
                    e => string.Equals(e.Environment, envFromSource, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                    return match.Environment;
            }
        }

        // 3. Fall back to the group's configured default.
        return registry.DefaultEnvironment(string.IsNullOrEmpty(groupName) ? null : groupName);
    }
}
