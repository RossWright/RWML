using RossWright.MetalCommand.Data;

namespace RossWright.MetalCommand.Tesbed;

[Command("add-thing", "add-thing", "at",
    HelpBrief = "Add a thing to the database",
    HelpDetail = "Adds a new thing record with the given name to the database")]
public class AddThingCommand(
    IDatabaseContextFactory<TestbedDbContext> _dbCtxFac)
    : ICommand
{
    [Arg(Name = "name", IsRequired = true, HelpDetail = "The name of the thing to add")]
    public string Name { get; set; } = null!;

    [EnvironmentArg(EnvironmentPolicy.Dangerous,
        HelpDetail = "The environment to add the thing to")]
    public string Environment { get; set; } = null!;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var dbCtx = _dbCtxFac.GetContext(Environment);
        var thing = new Thing { Name = Name };
        dbCtx.Things.Add(thing);
        await dbCtx.SaveChangesAsync(cancellationToken);
        console.WriteLine($"Added thing '{thing.Name}' ({thing.ThingId})");
        return CommandResult.Ok();
    }
}
