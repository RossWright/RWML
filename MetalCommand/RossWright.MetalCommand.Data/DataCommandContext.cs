namespace RossWright.MetalCommand.Data;

/// <summary>
/// Provides the base context passed to data command callbacks such as
/// <see cref="LoadDataCommandOptions{DBCTX}.LoadData"/> and
/// <see cref="MigrateCommandOptions{DBCTX}.PreMigration"/>.
/// </summary>
/// <typeparam name="DBCTX">The <see cref="DbContext"/> type for this operation.</typeparam>
public class DataCommandContext<DBCTX> where DBCTX : DbContext
{
    /// <summary>The console for the current session.</summary>
    public IConsole Console { get; set; } = null!;

    /// <summary>The environment name the command is running against (e.g. <c>"dev"</c>).</summary>
    public string Environment { get; set; } = null!;

    /// <summary>The <typeparamref name="DBCTX"/> instance for the selected environment.</summary>
    public DBCTX DbContext { get; set; } = null!;
}

