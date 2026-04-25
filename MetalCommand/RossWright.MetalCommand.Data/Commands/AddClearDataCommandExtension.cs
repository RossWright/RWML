using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

public static class AddClearDataCommandExtension
{
    public static IConsoleApplicationBuilder AddClearDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        CommandDescriptor commandDescriptor,
        Func<ClearDataCommandContext<TDBCTX>, Task> clearData)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ClearDataCommand<TDBCTX>>(commandDescriptor, clearData);
        return builder;
    }

    public static IConsoleApplicationBuilder AddClearDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Func<ClearDataCommandContext<TDBCTX>, Task> clearData)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ClearDataCommand<TDBCTX>>(clearData);
        return builder;
    }

    public static IConsoleApplicationBuilder AddClearDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        CommandDescriptor commandDescriptor, 
        params string[] tableNames) 
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ClearDataCommand<TDBCTX>>(commandDescriptor, MakeClearData<TDBCTX>(tableNames));
        return builder;
    }

    public static IConsoleApplicationBuilder AddClearDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        params string[] tableNames)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ClearDataCommand<TDBCTX>>(MakeClearData<TDBCTX>(tableNames));
        return builder;
    }

    private static Func<ClearDataCommandContext<TDBCTX>, Task> MakeClearData<TDBCTX>(string[] tableNames)
        where TDBCTX : DbContext => new(async _ =>
        {
            foreach (var tableName in tableNames)
            {
                await _.ClearTable(tableName);
            }
        });
}

