using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

/// <summary>Extension method for registering the built-in <c>clear</c> command.</summary>
public static class AddClearDataCommandExtension
{
    /// <summary>
    /// Adds the built-in <c>clear [env]</c> command, which deletes all data without
    /// dropping the schema. Supply either a <see cref="ClearDataCommandOptions{TDBCTX}.ClearData"/>
    /// callback or a <see cref="ClearDataCommandOptions{TDBCTX}.TableNames"/> list.
    /// </summary>
    /// <typeparam name="TDBCTX">The <see cref="DbContext"/> type to clear data from.</typeparam>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Required delegate that sets either <c>ClearData</c> or <c>TableNames</c>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when neither <c>ClearData</c> nor <c>TableNames</c> is set.</exception>
    public static IConsoleApplicationBuilder AddClearDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Action<ClearDataCommandOptions<TDBCTX>> configure)
        where TDBCTX : DbContext
    {
        ArgumentNullException.ThrowIfNull(configure);
        var registry = DataCommandRegistryHelper.EnsureRegistry(builder);
        var opts = registry.GetOrCreate<ClearDataCommandOptions<TDBCTX>>(typeof(ClearDataCommand<TDBCTX>));
        configure(opts);
        if (opts.ClearData is null && opts.TableNames is null)
            throw new InvalidOperationException(
                $"Either {nameof(ClearDataCommandOptions<TDBCTX>.ClearData)} or {nameof(ClearDataCommandOptions<TDBCTX>.TableNames)} must be set.");
        if (opts.ClearData is null)
            opts.ClearData = MakeClearData<TDBCTX>(opts.TableNames!);
        builder.Services.AddSingleton(opts);
        builder.Commands.Add(typeof(ClearDataCommand<TDBCTX>));
        return builder;
    }

    private static Func<ClearDataCommandContext<TDBCTX>, Task> MakeClearData<TDBCTX>(string[] tableNames)
        where TDBCTX : DbContext => async ctx =>
        {
            foreach (var tableName in tableNames)
                await ctx.ClearTable(tableName);
        };
}

/// <summary>
/// Configuration options for <see cref="ClearDataCommand{TDBCTX}"/>.
/// </summary>
public class ClearDataCommandOptions<TDBCTX> : CommandOptions
    where TDBCTX : DbContext
{
    /// <summary>
    /// Action that clears data from the database context.
    /// Set this or <see cref="TableNames"/>; if both are set, <see cref="ClearData"/> takes precedence.
    /// </summary>
    public Func<ClearDataCommandContext<TDBCTX>, Task>? ClearData { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ClearData"/>: names of tables to truncate.
    /// Ignored when <see cref="ClearData"/> is also set.
    /// </summary>
    public string[]? TableNames { get; set; }
}


