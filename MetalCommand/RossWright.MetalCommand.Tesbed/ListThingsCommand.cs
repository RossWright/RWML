using Microsoft.EntityFrameworkCore;
using RossWright.MetalCommand.Data;

namespace RossWright.MetalCommand.Tesbed;

[Command("list-things", "list-things", "lt", 
    HelpBrief = "List the things in the database",
    HelpDetail = "Lists all the thing records in the database ordered by name")]
public class ListThingsCommand(
    IDatabaseContextFactory<TestbedDbContext> _dbCtxFac) 
    : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign,
        HelpDetail = "The environment to list things from")]
    public string Environment { get; set; } = null!;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var dbCtx = _dbCtxFac.GetContext(Environment);
        var things = await dbCtx.Things.AsNoTracking().OrderBy(t => t.Name).ToListAsync(cancellationToken);
        console.WriteLine($"{things.Count} things found:");
        using var indent = console.Indent();
        foreach (var thing in things)
        {
            console.WriteLine(thing.Name);
        }
        return CommandResult.Ok();
    }
}
