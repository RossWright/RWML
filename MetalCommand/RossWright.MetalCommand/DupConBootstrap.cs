using System.Text;
using System.Text.Json;

namespace RossWright.MetalCommand;

/// <summary>
/// Extracts MetalCommand bootstrap args from the process argument list before the REPL opens.
/// </summary>
public static class DupConBootstrap
{
    /// <summary>
    /// Extracts MetalCommand bootstrap args from the process argument list before the REPL opens.
    /// Returns the cleaned arg list, any preloaded context entries, and an optional initial command.
    /// </summary>
    public static (string[] CleanArgs, IReadOnlyDictionary<string, string> Context, string? InitialCommand)
        TryExtract(string[] args)
    {
        if (args.Length == 0)
            return ([], new Dictionary<string, string>(), null);

        // Legacy mode: first arg doesn't start with '--'
        if (!args[0].StartsWith("--"))
        {
            var command = string.Join(' ', args);
            return ([], new Dictionary<string, string>(), command);
        }

        var cleanArgs = new List<string>();
        var context = new Dictionary<string, string>();
        string? initialCommand = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals("--ctx", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                context = ResolveContext(args[i + 1]);
                i++; // skip value
            }
            else if (args[i].Equals("--cmd", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                initialCommand = args[i + 1];
                i++; // skip value
            }
            else
            {
                cleanArgs.Add(args[i]);
            }
        }

        return (cleanArgs.ToArray(), context, initialCommand);
    }

    private static Dictionary<string, string> ResolveContext(string value)
    {
        // 1. Try Base64 → UTF-8 JSON
        try
        {
            var bytes = Convert.FromBase64String(value);
            var json = Encoding.UTF8.GetString(bytes);
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (result != null)
                return result;
        }
        catch { /* not base64 JSON */ }

        // 2. Try as literal file path
        if (File.Exists(value))
        {
            try
            {
                var json = File.ReadAllText(value);
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (result != null)
                    return result;
            }
            catch { /* invalid JSON */ }
        }

        // 3. Try <value>.mcc.json in cwd
        var cwdFile = Path.Combine(Directory.GetCurrentDirectory(), $"{value}.mcc.json");
        if (File.Exists(cwdFile))
        {
            try
            {
                var json = File.ReadAllText(cwdFile);
                var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (result != null)
                    return result;
            }
            catch { /* invalid JSON */ }
        }

        // 4. None succeeded — warn and return empty
        System.Console.Error.WriteLine($"[MetalCommand] Warning: could not resolve --ctx value '{value}'. Continuing with empty context.");
        return [];
    }
}
