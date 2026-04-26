namespace RossWright.MetalCommand.Data.Commands;

[Command("Migrate", "Migrate",
    HelpBrief = "migrates the schema of the specified database",
    Category = "Database Operations")]
internal class MigrateCommand<TDBCTX> : ICommand
    where TDBCTX : DbContext
{
    [EnvironmentArg(EnvironmentPolicy.Dangerous,
        HelpDetail = "The database environment to migrate")]
    public string Environment { get; set; } = null!;

    public MigrateCommand(IDatabaseContextFactory<TDBCTX> dbCtxFac, MigrateCommandOptions<TDBCTX> opts) =>
        (_dbCtxFac, _preMigration, _postMigration) =
        (dbCtxFac, opts.PreMigration, opts.PostMigration);
    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;
    private readonly Func<DataCommandContext<TDBCTX>, Task>? _preMigration;
    private readonly Func<DataCommandContext<TDBCTX>, Task>? _postMigration;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var dbCtx = _dbCtxFac.GetContext(Environment);
        var ctx = new ClearDataCommandContext<TDBCTX>
        {
            Console = console,
            Environment = Environment,
            DbContext = dbCtx
        };
        if (_preMigration != null) await _preMigration.Invoke(ctx);
        await console.AnnounceAsync($"Migrating {Environment} database",
            () => dbCtx.Database.MigrateAsync());
        if (_postMigration != null) await _postMigration.Invoke(ctx);
        return CommandResult.Ok();
    }
}
