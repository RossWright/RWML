using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

/// <summary>Extension method for registering the built-in <c>migrate</c> command.</summary>
public static class AddMigrateCommandExtension
{
    /// <summary>
    /// Adds the built-in <c>migrate [env]</c> command, which runs
    /// <c>Database.MigrateAsync()</c> against the selected environment.
    /// </summary>
    /// <typeparam name="TDBCTX">The <see cref="DbContext"/> type to migrate.</typeparam>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Optional delegate to customise invocations and pre/post callbacks.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddMigrateCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Action<MigrateCommandOptions<TDBCTX>>? configure = null)
        where TDBCTX : DbContext
    {
        var registry = DataCommandRegistryHelper.EnsureRegistry(builder);
        var opts = registry.GetOrCreate<MigrateCommandOptions<TDBCTX>>(typeof(MigrateCommand<TDBCTX>));
        configure?.Invoke(opts);
        builder.Services.AddSingleton(opts);
        builder.Commands.Add(typeof(MigrateCommand<TDBCTX>));
        return builder;
    }
}

/// <summary>
/// Configuration options for <see cref="MigrateCommand{TDBCTX}"/>.
/// </summary>
public class MigrateCommandOptions<TDBCTX> : CommandOptions
    where TDBCTX : DbContext
{
    /// <summary>Callback invoked before the migration runs.</summary>
    public Func<DataCommandContext<TDBCTX>, Task>? PreMigration { get; set; }

    /// <summary>Callback invoked after the migration completes.</summary>
    public Func<DataCommandContext<TDBCTX>, Task>? PostMigration { get; set; }
}


