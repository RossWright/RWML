namespace RossWright.MetalCommand.Data.Commands;

internal class ObliterateCommand<TDBCTX> : ILegacyCommand
    where TDBCTX : DbContext
{
    public ObliterateCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac) => _dbCtxFac = dbCtxFac;
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;

    public CommandDescriptor Descriptor => new CommandDescriptor()
    {
        Name = "Obliterate",
        Invocations = ["Obliterate", "MegaNuke"],
        Args = [_dbCtxFac.GetUnprotectedEnvironmentArg(helpDetail: "The environment  of the database to obliterate")],
        HelpBrief = "completely DESTROYS all tables in the database leaving it empty"
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
                console.WriteErrorLine("Database does not exist");
            }
            else
            {
                await console.AnnounceAsync($"Erasing {environment} database...", () => dbCtx.Obliterate());
            }
        }
    }
}
