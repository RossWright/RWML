namespace RossWright.MetalCommand.Data.Commands;

internal class ReloadDatabaseCommand<TDBCTX> : ILegacyCommand
    where TDBCTX : DbContext
{
    public ReloadDatabaseCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac, ICommandExecutor commandExecutor) =>
        (_dbCtxFac, _commandExecutor) = (dbCtxFac, commandExecutor);
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;
    private readonly ICommandExecutor _commandExecutor;

    public CommandDescriptor Descriptor => new CommandDescriptor()
    {
        Name = "Reload",
        Invocations = ["Reload", "Nuke"],
        Args = [_dbCtxFac.GetUnprotectedEnvironmentArg(helpDetail: "The environment  of the database to obliterate")],
        HelpBrief = "migrates, clears and loads the database with test data"
    };

    public async Task Execute(IConsole console, string[] args, CancellationToken cancellationToken)
    {
        var environment = console.TryParseEnvironment(_dbCtxFac, args?.FirstOrDefault(), allowProtected: false);
        if (environment == null) return;

        await _commandExecutor.Execute<MigrateCommand<TDBCTX>>(environment);
        console.WriteLine();
        await _commandExecutor.Execute<ClearDataCommand<TDBCTX>>(environment);
        console.WriteLine();
        await _commandExecutor.Execute<LoadDataCommand<TDBCTX>>(environment);
    }
}
