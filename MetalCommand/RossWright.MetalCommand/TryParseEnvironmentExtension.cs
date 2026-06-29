namespace RossWright.MetalCommand;

/// <summary>
/// Parsing helpers for environment names supplied to MetalCommand commands.
/// </summary>
public static class TryParseEnvironmentExtension
{
    /// <summary>
    /// Validates and resolves the environment argument against the given source,
    /// enforcing the specified policy for protected environments.
    /// Returns null if validation fails or the user declines the confirmation prompt.
    /// </summary>
    public static string? TryParseEnvironment(
        this IConsole console,
        IEnvironmentSource source,
        string? environment,
        EnvironmentPolicy policy = EnvironmentPolicy.Dangerous)
    {
        environment ??= source.DefaultEnvironment;

        var entry = source.Environments.FirstOrDefault(
            e => string.Equals(e.Name, environment, StringComparison.OrdinalIgnoreCase));

        if (entry == null)
        {
            var valid = source.Environments
                .Where(e => policy != EnvironmentPolicy.Forbidden || !e.IsProtected)
                .Select(e => e.Name);
            console.WriteErrorLine("Unknown environment" + (valid.Any()
                ? $", try using {valid.CommaListJoin("or")}"
                : ", no valid environments are available for this command"));
            return null;
        }

        if (entry.IsProtected)
        {
            if (policy == EnvironmentPolicy.Forbidden)
            {
                var valid = source.Environments
                    .Where(e => !e.IsProtected)
                    .Select(e => e.Name);
                console.WriteErrorLine("That environment cannot be used with this command" + (valid.Any()
                    ? $", try using {valid.CommaListJoin("or")}"
                    : ", no valid environments are available for this command"));
                return null;
            }

            if (policy == EnvironmentPolicy.Dangerous)
            {
                console.Write("Are you sure? (type \"yes\" to confirm): ");
                if (console.ReadLine() != "yes")
                {
                    console.WriteErrorLine("Command aborted");
                    return null;
                }
            }
        }

        return entry.Name;
    }

    /// <summary>
    /// Bool shim: <c>true</c> maps to <see cref="EnvironmentPolicy.Dangerous"/>,
    /// <c>false</c> maps to <see cref="EnvironmentPolicy.Forbidden"/>.
    /// </summary>
    public static string? TryParseEnvironment(
        this IConsole console,
        IEnvironmentSource source,
        string? environment,
        bool allowProtected) =>
        console.TryParseEnvironment(source, environment,
            allowProtected ? EnvironmentPolicy.Dangerous : EnvironmentPolicy.Forbidden);
}
