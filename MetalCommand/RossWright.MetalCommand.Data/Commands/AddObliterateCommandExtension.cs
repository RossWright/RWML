using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

/// <summary>Extension method for registering the built-in <c>obliterate</c> command.</summary>
public static class AddObliterateCommandExtension
{
    /// <summary>
    /// Adds the built-in <c>obliterate env</c> command, which drops all FK constraints,
    /// tables, and stored procedures. The environment argument is required and explicitly typed;
    /// protected environments are blocked.
    /// </summary>
    /// <typeparam name="TDBCTX">The <see cref="DbContext"/> type whose database will be obliterated.</typeparam>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Optional delegate to customise the command's invocation tokens.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static IConsoleApplicationBuilder AddObliterateCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Action<ObliterateCommandOptions>? configure = null)
        where TDBCTX : DbContext
    {
        var registry = DataCommandRegistryHelper.EnsureRegistry(builder);
        var opts = registry.GetOrCreate<ObliterateCommandOptions>(typeof(ObliterateCommand<TDBCTX>));
        configure?.Invoke(opts);
        builder.Commands.Add(typeof(ObliterateCommand<TDBCTX>));
        return builder;
    }
}

/// <summary>
/// Configuration options for <see cref="ObliterateCommand{TDBCTX}"/>.
/// </summary>
public class ObliterateCommandOptions : CommandOptions { }
