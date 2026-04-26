namespace RossWright.MetalCommand.Internal.Commands;

[Command("ListContext", "listcontext", HelpBrief = "Lists all key/value pairs in the current session context", Category = "Console")]
internal class ListContextCommand(IDictionary<string, string> context) : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        var entries = context
            .Where(x => !x.Key.StartsWith("__", StringComparison.Ordinal))
            .OrderBy(x => x.Key)
            .ToList();

        if (entries.Count == 0)
        {
            console.WriteLine("Context is empty.");
            return Task.FromResult(CommandResult.Ok());
        }

        foreach (var (key, value) in entries)
            console.WriteLine($"{key} = {value}");

        return Task.FromResult(CommandResult.Ok());
    }
}
