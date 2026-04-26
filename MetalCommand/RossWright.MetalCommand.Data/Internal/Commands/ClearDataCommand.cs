namespace RossWright.MetalCommand.Data.Commands;

[Command("ClearData", "ClearData", "cd",
    HelpBrief = "completely clears all data leaving schema intact",
    HelpDetail = "This command deletes every row from each table leaving the database schema intact\n" +
        "This is useful for resetting the state of the database completely without losing structure.",
    Category = "Database Operations")]
internal class ClearDataCommand<TDBCTX> : ICommand
    where TDBCTX : DbContext
{
    [EnvironmentArg(EnvironmentPolicy.Forbidden,
        HelpDetail = "The environment of the database to clear")]
    public string Environment { get; set; } = null!;

    public ClearDataCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac,
        ClearDataCommandOptions<TDBCTX> opts) =>
        (_dbCtxFac, _clearData) = (dbCtxFac, opts.ClearData!);
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;
    private readonly Func<ClearDataCommandContext<TDBCTX>, Task> _clearData;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var dbCtx = _dbCtxFac.GetContext(Environment);
        if (!dbCtx.DatabaseExists())
        {
            console.WriteErrorLine("Database does not exits");
            return CommandResult.Fail();
        }
        await console.AnnounceAsync($"Clearing data from {Environment} environment database", async () =>
        {
            await _clearData(new ClearDataCommandContext<TDBCTX>
            {
                Console = console,
                Environment = Environment,
                DbContext = dbCtx
            });
            await dbCtx.SaveChangesAsync();
        });
        return CommandResult.Ok();
    }
}
