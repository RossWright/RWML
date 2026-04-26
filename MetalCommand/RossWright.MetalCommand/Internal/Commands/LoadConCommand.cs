using System.Text.Json;

namespace RossWright.MetalCommand.Internal.Commands;

[Command("LoadCon", "loadcon", HelpBrief = "Loads context from a .mcc.json file into the current session", Category = "Console")]
internal class LoadConCommand(IDictionary<string, string> context) : ICommand
{
    [Arg(Name = "name", IsRequired = false)]
    public string Name { get; set; } = "default";

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), $"{Name}.mcc.json");

        if (!File.Exists(path))
        {
            console.WriteErrorLine($"File not found: {path}");
            return CommandResult.Fail($"File not found: {path}");
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        if (loaded == null || loaded.Count == 0)
        {
            console.WriteLine("No context entries found in file.");
            return CommandResult.Ok();
        }

        foreach (var (k, v) in loaded)
            context[k] = v;

        console.WriteLine($"Loaded {loaded.Count} context {(loaded.Count == 1 ? "entry" : "entries")} from {path}");
        return CommandResult.Ok();
    }
}
