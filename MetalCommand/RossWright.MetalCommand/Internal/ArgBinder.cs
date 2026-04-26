using System.Globalization;
using System.Reflection;

namespace RossWright.MetalCommand;

internal static class ArgBinder
{
    /// <summary>
    /// Parses <paramref name="rawArgs"/>, matches values to <paramref name="descriptor"/> args,
    /// applies context and default fallbacks, validates, converts, and sets properties on
    /// <paramref name="command"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if binding succeeded; <see langword="false"/> if any error was
    /// written to <paramref name="console"/> (caller should not invoke ExecuteAsync).
    /// </returns>
    public static bool TryBind(
        ICommand command,
        CommandDescriptor descriptor,
        string[] rawArgs,
        IDictionary<string, string> context,
        IConsole console,
        ConsoleColor? warningColor,
        IServiceProvider serviceProvider,
        out IReadOnlyDictionary<string, object?> boundArgs)
    {
        var bound = new Dictionary<string, object?>();
        boundArgs = bound;

        var args = descriptor.Args ?? [];
        bool success = true;

        // --- Phase 1: parse named tokens out of rawArgs ---
        // Named tokens: "--PropertyName value"  or  "--property-name value"
        // Boolean flag: bare "--PropertyName" (no following non-flag token) → true
        var namedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var positionalTokens = new List<string>();

        for (int i = 0; i < rawArgs.Length; i++)
        {
            var token = rawArgs[i];
            if (token.StartsWith("--"))
            {
                // Normalise: replace hyphens after -- with nothing so "--my-arg" == "--myarg" == "myarg"
                var key = token[2..].Replace("-", "");
                bool nextIsValue = i + 1 < rawArgs.Length && !rawArgs[i + 1].StartsWith("--");
                if (nextIsValue)
                {
                    namedValues[key] = rawArgs[++i];
                }
                else
                {
                    // Boolean flag — value resolved later during binding
                    namedValues[key] = "true";
                }
            }
            else
            {
                positionalTokens.Add(token);
            }
        }

        // --- Phase 2: match each descriptor arg ---
        int positionalIndex = 0;

        foreach (var arg in args)
        {
            if (arg.PropertyName is null || arg.PropertyType is null)
                continue; // safety guard — should never happen for new-style args

            // Skip env args — they are handled exclusively in Phase 3
            var argProp = command.GetType().GetProperty(arg.PropertyName,
                BindingFlags.Public | BindingFlags.Instance);
            if (argProp?.GetCustomAttribute<EnvironmentArgAttribute>() != null)
                continue;

            string? rawValue = null;

            // Named match (AllowNamed or bare boolean flag) — checked first
            if (arg.AllowNamed || arg.PropertyType == typeof(bool))
            {
                // Match by PropertyName (normalised) or display Name (normalised)
                var normalised = arg.PropertyName.Replace("-", "");
                var displayNormalised = arg.Name?.Replace("-", "") ?? normalised;

                if (namedValues.TryGetValue(normalised, out var nv) ||
                    namedValues.TryGetValue(displayNormalised, out nv))
                {
                    rawValue = nv;
                    // Remove so it doesn't also fill a positional slot
                    namedValues.Remove(normalised);
                    namedValues.Remove(displayNormalised);
                }
            }

            // Positional match — if named didn't resolve
            if (rawValue is null && positionalIndex < positionalTokens.Count)
            {
                rawValue = positionalTokens[positionalIndex++];
            }

            // Context fallback
            if (rawValue is null && arg.UseContextKeyForDefault is not null &&
                context.TryGetValue(arg.UseContextKeyForDefault, out var contextValue))
            {
                console.WriteLine($"Using \"{contextValue}\" for {arg.Name}", warningColor);
                rawValue = contextValue;
            }

            // Default fallback
            if (rawValue is null && arg.DefaultValue is not null)
            {
                rawValue = arg.DefaultValue;
                console.WriteLine($"Using \"{rawValue}\" for {arg.Name}", warningColor);
            }

            // Required check
            if (rawValue is null)
            {
                if (arg.IsRequired)
                {
                    console.WriteErrorLine($"Missing argument for required parameter: {arg.Name}");
                    success = false;
                }
                // Optional with no value — leave property at its default; record null
                bound[arg.PropertyName] = null;
                continue;
            }

            // ValidValues check (case-insensitive)
            if (arg.ValidValues is { Length: > 0 } &&
                !arg.ValidValues.Any(v => string.Equals(v, rawValue, StringComparison.OrdinalIgnoreCase)))
            {
                console.WriteErrorLine(
                    $"Invalid value \"{rawValue}\" for {arg.Name}. Must be one of: {string.Join(", ", arg.ValidValues)}");
                success = false;
                continue;
            }

            // Type conversion
            if (!TryConvert(rawValue, arg.PropertyType, out var converted))
            {
                console.WriteErrorLine(
                    $"Cannot convert \"{rawValue}\" to {arg.PropertyType.Name} for argument {arg.Name}.");
                success = false;
                continue;
            }

            // Property set
            var property = command.GetType().GetProperty(arg.PropertyName,
                BindingFlags.Public | BindingFlags.Instance);
            property?.SetValue(command, converted);

            bound[arg.PropertyName] = converted;
        }

        // --- Phase 3: bind [EnvironmentArg]-decorated properties ---
        var envArgProperties = command.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<EnvironmentArgAttribute>() != null && p.CanWrite)
            .ToArray();

        foreach (var prop in envArgProperties)
        {
            var attr = prop.GetCustomAttribute<EnvironmentArgAttribute>()!;

            var sourceType = attr.EnvironmentSourceType ?? typeof(IEnvironmentSource);
            var source = (IEnvironmentSource?)serviceProvider.GetService(sourceType);
            if (source == null)
            {
                console.WriteErrorLine(
                    $"No IEnvironmentSource is registered for argument '{prop.Name.ToLowerInvariant()}'.");
                success = false;
                continue;
            }

            string? rawValue = null;
            var normalised = prop.Name.Replace("-", "");

            if (namedValues.TryGetValue(normalised, out var nv))
            {
                rawValue = nv;
                namedValues.Remove(normalised);
            }
            else if (positionalIndex < positionalTokens.Count)
            {
                rawValue = positionalTokens[positionalIndex++];
            }

            if (rawValue is null)
            {
                rawValue = source.DefaultEnvironment;
                console.WriteLine($"Using \"{rawValue}\" for {prop.Name.ToLowerInvariant()}", warningColor);
            }

            // Validate against known environment names
            if (!source.Environments.Any(e =>
                    string.Equals(e.Name, rawValue, StringComparison.OrdinalIgnoreCase)))
            {
                var valid = source.Environments.Select(e => e.Name);
                console.WriteErrorLine(
                    $"Invalid value \"{rawValue}\" for {prop.Name.ToLowerInvariant()}. " +
                    $"Must be one of: {string.Join(", ", valid)}");
                success = false;
                continue;
            }

            // Resolve to canonical casing from source
            var canonicalName = source.Environments
                .First(e => string.Equals(e.Name, rawValue, StringComparison.OrdinalIgnoreCase))
                .Name;

            prop.SetValue(command, canonicalName);
            bound[prop.Name] = canonicalName;
        }

        // Warn about unrecognised named args (typos / misspellings)
        foreach (var leftover in namedValues.Keys)
        {
            console.WriteLine($"Warning: unrecognised named argument --{leftover} was ignored.", warningColor);
        }

        // Warn about excess positional args
        int unconsumedPositional = positionalTokens.Count - positionalIndex;
        if (unconsumedPositional > 0)
        {
            string plural = unconsumedPositional == 1 ? "argument was" : "arguments were";
            console.WriteLine(
                $"Warning: Last {unconsumedPositional} positional {plural} ignored.", warningColor);
        }

        return success;
    }

    private static bool TryConvert(string rawValue, Type targetType, out object? result)
    {
        result = null;
        try
        {
            if (targetType.IsEnum)
            {
                result = Enum.Parse(targetType, rawValue, ignoreCase: true);
                return true;
            }

            if (targetType == typeof(Guid))
            {
                result = Guid.Parse(rawValue);
                return true;
            }

            if (targetType == typeof(DateTime))
            {
                result = DateTime.Parse(rawValue, CultureInfo.InvariantCulture);
                return true;
            }

            result = Convert.ChangeType(rawValue, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
