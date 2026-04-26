using Microsoft.Extensions.DependencyInjection;
using ActivatorUtilities = RossWright.MetalInjection.ActivatorUtilities;

namespace RossWright.MetalCommand;

internal class CommandExecutor(Type _commandType)
{
    public Type CommandType => _commandType;
    public CommandDescriptor Descriptor { get; set; } = null!;

    public async Task<(TimeSpan Elapsed, CommandResult Result)> Execute(
        IServiceProvider serviceProvider, 
        IConsole console, 
        string[] rawArgs, 
        IDictionary<string, string> context, 
        ConsoleColor? warningColor,
        IReadOnlyList<Type> middlewareTypes,
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var startTime = DateTime.UtcNow;

        var command = (ICommand)ActivatorUtilities.CreateInstance(scope.ServiceProvider, CommandType);
        if (!ArgBinder.TryBind(command, Descriptor, rawArgs, context, console, warningColor, scope.ServiceProvider, out var boundArgs))
            return (DateTime.UtcNow - startTime, CommandResult.Fail());

        var ctx = new CommandContext
        {
            Command = command,
            Console = console,
            SessionContext = context,
            CancellationToken = cancellationToken,
            BoundArgs = boundArgs
        };

        // Build pipeline innermost-first: last registered middleware wraps first.
        Func<CommandContext, Task> pipeline = async c =>
        {
            c.Result = await c.Command.ExecuteAsync(c.Console, c.CancellationToken);
        };

        foreach (var middlewareType in middlewareTypes.Reverse())
        {
            var next = pipeline;
            var mw = (ICommandMiddleware)ActivatorUtilities.CreateInstance(
                scope.ServiceProvider, middlewareType);
            pipeline = c => mw.InvokeAsync(c, next);
        }

        await pipeline(ctx);
        return (DateTime.UtcNow - startTime, ctx.Result);
    }
}