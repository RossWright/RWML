namespace RossWright.MetalCommand.Data.Commands;

internal class ClearDataCommand<TDBCTX> : ILegacyCommand
    where TDBCTX : DbContext
{
    public ClearDataCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac,
        Func<ClearDataCommandContext<TDBCTX>, Task> clearData) =>
        (_dbCtxFac, _clearData) = (dbCtxFac, clearData);
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;
    private readonly Func<ClearDataCommandContext<TDBCTX>, Task> _clearData;

    public CommandDescriptor Descriptor => new CommandDescriptor()
    {
        Name = "ClearData",
        Invocations = ["ClearData", "cd"],
        Args = [_dbCtxFac.GetUnprotectedEnvironmentArg(helpDetail: "The environment  of the database to clear")],
        HelpBrief = "completely clears all data leaving schema intact",
        HelpDetail = "This command deletes every row from each table leaving the database schema intact\n" +
            "This is useful for resetting the state of the database completely without losing structure."
    };

    public async Task Execute(IConsole console, string[] args, CancellationToken cancellationToken)
    {
        var environment = console.TryParseEnvironment(_dbCtxFac, args?.FirstOrDefault(), allowProtected: false);
        if (environment == null) return;

        using (var dbCtx = _dbCtxFac.GetContext(environment))
        {
            if (dbCtx == null)
            {
                console.WriteErrorLine($"Unable to create Database Context for {environment}");
            }
            else if (!dbCtx.DatabaseExists())
            {
                console.WriteErrorLine("Database does not exits");
            }
            else
            {
                await console.AnnounceAsync($"Clearing data from {environment} environment database", async () =>
                {
                    await _clearData(new ClearDataCommandContext<TDBCTX>
                    {
                        Console = console,
                        Environment = environment,
                        DbContext = dbCtx
                    });
                    await dbCtx.SaveChangesAsync();
                });
            }
        }
    }
}

