using System.Text.Json;

namespace RossWright.MetalCommand.Internal.Commands;

[Command("SaveCon", "savecon", HelpBrief = "Saves the current context to a .mcc.json file", Category = "Console")]
internal class SaveConCommand(IDictionary<string, string> context) : ICommand
{
    [Arg(Name = "name", IsRequired = false)]
    public string Name { get; set; } = "default";

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), $"{Name}.mcc.json");
        var json = JsonSerializer.SerializeToUtf8Bytes(context,
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllBytesAsync(path, json, cancellationToken);
        console.WriteLine($"Context saved to {path}");
        return CommandResult.Ok();
    }
}
