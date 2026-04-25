using RossWright.MetalCommand.Data.Commands;

namespace RossWright.MetalCommand.Data;

public static class AddObliterateCommandExtension
{
    public static IConsoleApplicationBuilder AddObliterateCommand<TDBCTX>(this IConsoleApplicationBuilder builder)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ObliterateCommand<TDBCTX>>();
        return builder;
    }

    public static IConsoleApplicationBuilder AddObliterateCommand<TDBCTX>(this IConsoleApplicationBuilder builder,
        CommandDescriptor commandDescriptor)
        where TDBCTX : DbContext
    {
        builder.Commands.Add<ObliterateCommand<TDBCTX>>(commandDescriptor);
        return builder;
    }    
}
