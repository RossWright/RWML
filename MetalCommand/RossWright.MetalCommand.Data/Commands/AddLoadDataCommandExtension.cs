using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

/// <summary>Extension method for registering the built-in <c>load</c> command.</summary>
public static class AddLoadDataCommandExtension
{
    /// <summary>
    /// Adds the built-in <c>load [env]</c> command, which calls the
    /// <see cref="LoadDataCommandOptions{TDBCTX}.LoadData"/> delegate with a
    /// <see cref="LoadDataCommandContext{DBCTX}"/> containing the <see cref="DbContext"/> and
    /// CSV-loading helpers.
    /// </summary>
    /// <typeparam name="TDBCTX">The <see cref="DbContext"/> type to load data into.</typeparam>
    /// <param name="builder">The application builder.</param>
    /// <param name="configure">Required delegate that at minimum sets <see cref="LoadDataCommandOptions{TDBCTX}.LoadData"/>.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="LoadDataCommandOptions{TDBCTX}.LoadData"/> is not set by the delegate.</exception>
    public static IConsoleApplicationBuilder AddLoadDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Action<LoadDataCommandOptions<TDBCTX>> configure)
        where TDBCTX : DbContext
    {
        ArgumentNullException.ThrowIfNull(configure);
        var registry = DataCommandRegistryHelper.EnsureRegistry(builder);
        var opts = registry.GetOrCreate<LoadDataCommandOptions<TDBCTX>>(typeof(LoadDataCommand<TDBCTX>));
        configure(opts);
        if (opts.LoadData is null)
            throw new InvalidOperationException($"{nameof(LoadDataCommandOptions<TDBCTX>.LoadData)} must be set in the configure action.");
        builder.Services.AddSingleton(opts);
        builder.Commands.Add(typeof(LoadDataCommand<TDBCTX>));
        return builder;
    }
}

/// <summary>
/// Configuration options for <see cref="LoadDataCommand{TDBCTX}"/>.
/// </summary>
public class LoadDataCommandOptions<TDBCTX> : CommandOptions
    where TDBCTX : DbContext
{
    /// <summary>Required. The action that seeds data into the database context.</summary>
    public Func<LoadDataCommandContext<TDBCTX>, Task>? LoadData { get; set; }

    /// <summary>Optional path prefix applied when resolving CSV seed files.</summary>
    public string? LoadFilepath { get; set; }
}


