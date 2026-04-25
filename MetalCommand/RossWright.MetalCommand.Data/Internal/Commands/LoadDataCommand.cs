namespace RossWright.MetalCommand.Data.Commands;

internal class LoadDataCommand<TDBCTX>(
    IDatabaseContextFactory<TDBCTX> _dbCtxFac,
    Func<LoadDataCommandContext<TDBCTX>, Task> _loadData, 
    string? _loadFilepath) : ILegacyCommand
    where TDBCTX : DbContext
{
    public CommandDescriptor Descriptor => new CommandDescriptor()
    {
        Name = "LoadData",
        Invocations = ["LoadData", "ld"],
        Args = [_dbCtxFac.GetEnvironmentArg(helpDetail: "The environment of the database to obliterate")],
        HelpBrief = "loads the database with test data"
    };

    public async Task Execute(IConsole console, string[] args, CancellationToken cancellationToken)
    {
        var environment = console.TryParseEnvironment(_dbCtxFac, args?.FirstOrDefault(), allowProtected: true);
        if (environment == null) return;

        using (var dbCtx = _dbCtxFac.GetContext(environment))
        {
            if (dbCtx == null)
            {
                console.WriteErrorLine($"Unable to create Database Context for {environment}");
            }
            else
            {
                await console.AnnounceAsync($"Loading test data into {environment} database", async () =>
                {
                    await _loadData(new LoadDataCommandContext<TDBCTX>(_loadFilepath)
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

