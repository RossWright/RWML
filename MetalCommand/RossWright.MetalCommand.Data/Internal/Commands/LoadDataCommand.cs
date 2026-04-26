namespace RossWright.MetalCommand.Data.Commands;

[Command("LoadData", "LoadData", "ld",
    HelpBrief = "loads the database with test data",
    Category = "Database Operations")]
internal class LoadDataCommand<TDBCTX> : ICommand
    where TDBCTX : DbContext
{
    public LoadDataCommand(
        IDatabaseContextFactory<TDBCTX> dbCtxFac,
        LoadDataCommandOptions<TDBCTX> opts)
        => (_dbCtxFac, _loadData, _loadFilepath) = (dbCtxFac, opts.LoadData!, opts.LoadFilepath);

    private readonly IDatabaseContextFactory<TDBCTX> _dbCtxFac;
    private readonly Func<LoadDataCommandContext<TDBCTX>, Task> _loadData;
    private readonly string? _loadFilepath;

    [EnvironmentArg(EnvironmentPolicy.Dangerous,
        HelpDetail = "The environment of the database to load data into")]
    public string Environment { get; set; } = null!;

    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        using var dbCtx = _dbCtxFac.GetContext(Environment);
        await console.AnnounceAsync($"Loading test data into {Environment} database", async () =>
        {
            await _loadData(new LoadDataCommandContext<TDBCTX>(_loadFilepath)
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
