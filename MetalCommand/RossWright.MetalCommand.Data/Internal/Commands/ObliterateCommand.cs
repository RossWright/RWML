namespace RossWright.MetalCommand.Data.Commands;

[Command("Obliterate", "Obliterate", "MegaNuke",
    HelpBrief = "completely DESTROYS all tables in the database leaving it empty",
    Category = "Database Operations")]
internal class ObliterateCommand<TDBCTX> : ICommand
    where TDBCTX : DbContext
{
    [EnvironmentArg(EnvironmentPolicy.Forbidden,
        HelpDetail = "The environment of the database to obliterate")]
    public string Environment { get; set; } = null!;

    public ObliterateCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac) => _dbCtxFac = dbCtxFac;
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var dbCtx = _dbCtxFac.GetContext(Environment);
        if (!dbCtx.DatabaseExists())
        {
            console.WriteErrorLine("Database does not exist");
            return CommandResult.Fail();
        }
        await console.AnnounceAsync($"Erasing {Environment} database...", () => dbCtx.Obliterate());
        return CommandResult.Ok();
    }
}
