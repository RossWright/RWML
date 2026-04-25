namespace RossWright.MetalCommand.Data.Commands;

internal class MigrateCommand<TDBCTX> : ILegacyCommand
    where TDBCTX : DbContext
{
    public MigrateCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac,
        (Func<DataCommandContext<TDBCTX>, Task>?,
        Func<DataCommandContext<TDBCTX>, Task>?) actions) =>
        (_dbCtxFac, _preMigration, _postMigration) =
        (dbCtxFac, actions.Item1, actions.Item2);
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;
    private readonly Func<DataCommandContext<TDBCTX>, Task>? _preMigration;
    private readonly Func<DataCommandContext<TDBCTX>, Task>? _postMigration;

    public CommandDescriptor Descriptor => new CommandDescriptor()
    {
        Name = "Migrate",
        Invocations = ["Migrate"],
        Args = [_dbCtxFac.GetEnvironmentArg(helpDetail: "The database environment to migrate")],
        HelpBrief = "migrates the schema of the specified database"
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
                var ctx = new ClearDataCommandContext<TDBCTX>
                {
                    Console = console,
                    Environment = environment,
                    DbContext = dbCtx
                };
                if (_preMigration != null) await _preMigration.Invoke(ctx);
                await console.AnnounceAsync($"Migrating {environment} database",
                    () => dbCtx.Database.MigrateAsync());
                if (_postMigration != null) await _postMigration.Invoke(ctx);
            }
        }
    }
}

