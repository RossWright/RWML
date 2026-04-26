using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

/// <summary>Extension method for registering the built-in <c>reload</c> command.</summary>
public static class AddReloadDatabaseCommandExtension
{
    /// <summary>
    /// Adds the built-in <c>reload [env]</c> command, which obliterates the database,
    /// re-runs migrations, then loads seed data. Requires both <c>AddObliterateCommand</c>
    /// and <c>AddLoadDataCommand</c> to also be registered.
    /// </summary>
    /// <typeparam name="TDBCTX">The <see cref="DbContext"/> type to reload.</typeparam>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Optional delegate to customise the command's invocation tokens.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddReloadDatabaseCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Action<ReloadDatabaseCommandOptions>? configure = null)
        where TDBCTX : DbContext
    {
        var registry = DataCommandRegistryHelper.EnsureRegistry(builder);
        var opts = registry.GetOrCreate<ReloadDatabaseCommandOptions>(typeof(ReloadDatabaseCommand<TDBCTX>));
        configure?.Invoke(opts);
        builder.Commands.Add(typeof(ReloadDatabaseCommand<TDBCTX>));
        return builder;
    }
}

/// <summary>
/// Configuration options for <see cref="ReloadDatabaseCommand{TDBCTX}"/>.
/// </summary>
public class ReloadDatabaseCommandOptions : CommandOptions { }
