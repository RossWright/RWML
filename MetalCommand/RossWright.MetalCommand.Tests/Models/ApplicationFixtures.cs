namespace RossWright.MetalCommand.Tests.Models;

// ---------------------------------------------------------------------------
// Fixtures used by ConsoleApplicationTests
// ---------------------------------------------------------------------------

[Command("Greet")]
internal class GreetCommand : ICommand
{
    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        console.WriteLine("Hello!");
        return Task.FromResult(CommandResult.Ok());
    }
}

[Command("Ping")]
internal class PingCommand : ICommand
{
    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        console.WriteLine("Pong!");
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>Two commands that share the "clash" invocation — used to test duplicate detection.</summary>
[Command("ClashA", "clash")]
internal class ClashACommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("ClashB", "clash")]
internal class ClashBCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Reads an [Arg] from context so Context-sharing tests can verify the value reached the command.</summary>
[Command("ReadCtx")]
internal class ReadContextCommand : ICommand
{
    [Arg(ContextKey = "env")] public string? Env { get; set; }

    public static string? LastEnv { get; private set; }
    public static void Reset() => LastEnv = null;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        LastEnv = Env;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>Command with multiple explicit invocations — verifies alternate-alias routing.</summary>
[Command("Multi", "multi", "m", "go")]
internal class MultiInvocationCommand : ICommand
{
    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Command that blocks on the CancellationToken — used to verify that cancellation
/// propagates through the pipeline and does not hang.
/// </summary>
[Command("Slow")]
internal class SlowCommand : ICommand
{
    public async Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
        return CommandResult.Ok();
    }
}

/// <summary>Command that returns FailAndExit result for testing exit handling.</summary>
[Command("FailExit")]
internal class FailAndExitCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        return Task.FromResult(CommandResult.FailAndExit("Something went wrong"));
    }
}

/// <summary>Command that throws TaskCanceledException for testing exception handling.</summary>
[Command("ThrowCancel")]
internal class ThrowTaskCanceledCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        throw new TaskCanceledException("Simulated cancellation");
    }
}

/// <summary>Command that throws a general exception for testing exception handling.</summary>
[Command("ThrowEx")]
internal class ThrowExceptionCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Test exception");
    }
}
