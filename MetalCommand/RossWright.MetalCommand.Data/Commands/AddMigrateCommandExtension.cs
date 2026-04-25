using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

public static class AddMigrateCommandExtension
{
    public static IConsoleApplicationBuilder AddMigrateCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Func<DataCommandContext<TDBCTX>, Task>? preMigration = null,
        Func<DataCommandContext<TDBCTX>, Task>? postMigration = null)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<MigrateCommand<TDBCTX>>((preMigration, postMigration));
        return builder;
    }

    public static IConsoleApplicationBuilder AddMigrateCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        CommandDescriptor commandDescriptor,
        Func<DataCommandContext<TDBCTX>, Task>? preMigration = null,
        Func<DataCommandContext<TDBCTX>, Task>? postMigration = null)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<MigrateCommand<TDBCTX>>(commandDescriptor, (preMigration, postMigration));
        return builder;
    }
}

