using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

public static class AddLoadDataCommandExtension
{
    public static IConsoleApplicationBuilder AddLoadDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        CommandDescriptor commandDescriptor,
        Func<LoadDataCommandContext<TDBCTX>, Task> loadData, 
        string? loadFilepath = null)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<LoadDataCommand<TDBCTX>>(commandDescriptor, loadData, loadFilepath ?? string.Empty);
        return builder;
    }

    public static IConsoleApplicationBuilder AddLoadDataCommand<TDBCTX>(
        this IConsoleApplicationBuilder builder,
        Func<LoadDataCommandContext<TDBCTX>, Task> loadData, 
        string? loadFilepath = null)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<LoadDataCommand<TDBCTX>>(loadData, loadFilepath ?? string.Empty);
        return builder;
    }
}

