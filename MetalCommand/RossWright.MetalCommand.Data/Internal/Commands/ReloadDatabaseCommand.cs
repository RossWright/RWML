namespace RossWright.MetalCommand.Data.Commands;

[Command("Reload", "Reload", "Nuke",
    HelpBrief = "migrates, clears and loads the database with test data",
    Category = "Database Operations")]
internal class ReloadDatabaseCommand<TDBCTX> : ICommand
    where TDBCTX : DbContext
{
    [EnvironmentArg(EnvironmentPolicy.Forbidden,
        HelpDetail = "The environment of the database to reload")]
    public string Environment { get; set; } = null!;

    public ReloadDatabaseCommand(ICommandExecutor commandExecutor) =>
        _commandExecutor = commandExecutor;
    private readonly ICommandExecutor _commandExecutor;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        await _commandExecutor.Execute("Migrate", Environment);
        console.WriteLine();
        await _commandExecutor.Execute("ClearData", Environment);
        console.WriteLine();
        await _commandExecutor.Execute("LoadData", Environment);
        return CommandResult.Ok();
    }
}

