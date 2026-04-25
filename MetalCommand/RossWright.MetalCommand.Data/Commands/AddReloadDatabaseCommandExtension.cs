using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

public static class AddReloadDatabaseCommandExtension
{
    public static IConsoleApplicationBuilder AddReloadDatabaseCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder) where TDBCTX : DbContext
    {
        builder.Commands.Add<ReloadDatabaseCommand<TDBCTX>>();
        return builder;
    }

    public static IConsoleApplicationBuilder AddReloadDatabaseCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder, CommandDescriptor commandDescriptor)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ReloadDatabaseCommand<TDBCTX>>(commandDescriptor);
        return builder;
    }
}
