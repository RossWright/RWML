namespace RossWright.MetalCommand.Tests.Models;

// ---------------------------------------------------------------------------
// Fixtures used by MiddlewarePipelineTests
// ---------------------------------------------------------------------------

[Command("Tracked")]
internal class TrackedCommand : ICommand
{
    [Arg(IsRequired = true)] public string? Value { get; set; }

    public static readonly string Marker = "command";

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        console.WriteLine(Marker);
        return Task.FromResult(CommandResult.Ok());
    }
}

[Command("NoArgsTracked")]
internal class NoArgsTrackedCommand : ICommand
{
    public static readonly string Marker = "command";

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        console.WriteLine(Marker);
        return Task.FromResult(CommandResult.Ok());
    }
}

internal class LabelMiddleware(string _label, IConsole _console) : ICommandMiddleware
{
    public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
    {
        _console.WriteLine($"{_label}-before");
        await next(context);
        _console.WriteLine($"{_label}-after");
    }
}

internal class FirstMiddleware(IConsole _console) : ICommandMiddleware
{
    public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
    {
        _console.WriteLine("mw1-before");
        await next(context);
        _console.WriteLine("mw1-after");
    }
}

internal class SecondMiddleware(IConsole _console) : ICommandMiddleware
{
    public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
    {
        _console.WriteLine("mw2-before");
        await next(context);
        _console.WriteLine("mw2-after");
    }
}

internal class ShortCircuitMiddleware : ICommandMiddleware
{
    public Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        => Task.CompletedTask;
}

internal class ResultModifyingMiddleware : ICommandMiddleware
{
    public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
    {
        await next(context);
        context.Result = CommandResult.Fail("modified");
    }
}

// ---------------------------------------------------------------------------
// Fixtures used by EnvironmentArgMiddlewareTests
// ---------------------------------------------------------------------------

internal class StubProtectedEnvironmentSource : IEnvironmentSource
{
    public string DefaultEnvironment => "local";
    public EnvironmentEntry[] Environments =>
    [
        new() { Name = "local",      IsProtected = false },
        new() { Name = "production", IsProtected = true  },
    ];
}

[Command("DangerousEnvCommand", "dangerousenv")]
internal class DangerousEnvCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Dangerous)]
    public string? Environment { get; set; }

    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(CommandResult.Ok());
    }
}

[Command("ForbiddenEnvCommand", "forbiddenenv")]
internal class ForbiddenEnvCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Forbidden)]
    public string? Environment { get; set; }

    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>Command with Benign policy — must never prompt even for protected environments.</summary>
[Command("BenignEnvCommand", "benignenv")]
internal class BenignEnvCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign)]
    public string? Environment { get; set; }

    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Environment source with two protected environments ("production" and "staging")
/// and one safe environment ("local") — used to test per-env confirmation scoping.
/// </summary>
internal class TwoProtectedEnvironmentSource : IEnvironmentSource
{
    public string DefaultEnvironment => "local";
    public EnvironmentEntry[] Environments =>
    [
        new() { Name = "local",      IsProtected = false },
        new() { Name = "production", IsProtected = true  },
        new() { Name = "staging",    IsProtected = true  },
    ];
}

/// <summary>
/// Stub ICommandOptionsRegistry that returns a fixed EnvironmentPolicy override
/// for a specific command type.
/// </summary>
internal class StubCommandOptionsRegistry(Type commandType, EnvironmentPolicy policy)
    : ICommandOptionsRegistry
{
    public CommandOptions? Get(Type type)
        => type == commandType ? new CommandOptions { EnvironmentPolicy = policy } : null;
}

// ---------------------------------------------------------------------------
// Fixtures used by EnvironmentArgBindingIntegrationTests
// ---------------------------------------------------------------------------

/// <summary>
/// Like DangerousEnvCommand but also captures the resolved environment value
/// so tests can assert what EnvironmentArgMiddleware set on the property.
/// </summary>
[Command("DangerousEnvCapture", "dangerousenv-capture")]
internal class DangerousEnvCaptureCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Dangerous)]
    public string? Environment { get; set; }

    public static string? LastEnvironment { get; private set; }
    public static void Reset() => LastEnvironment = null;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        LastEnvironment = Environment;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Command with both a regular [Arg] and an [EnvironmentArg] to verify that
/// ArgBinder handles [Arg] while EnvironmentArgMiddleware handles [EnvironmentArg].
/// </summary>
[Command("MixedArgAndEnv", "mixed")]
internal class MixedArgAndEnvCommand : ICommand
{
    [Arg(IsRequired = true)]
    public string? Name { get; set; }

    [EnvironmentArg(EnvironmentPolicy.Dangerous)]
    public string? Environment { get; set; }

    public static string? LastName { get; private set; }
    public static string? LastEnvironment { get; private set; }
    public static void Reset() { LastName = null; LastEnvironment = null; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        LastName = Name;
        LastEnvironment = Environment;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Command with a custom EnvironmentSourceType that is not registered.
/// Used to test the middleware skips checks when source cannot be resolved.
/// </summary>
internal interface ICustomEnvironmentSource : IEnvironmentSource { }

[Command("CustomSourceCommand", "customsource")]
internal class CustomSourceCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Dangerous, EnvironmentSourceType = typeof(ICustomEnvironmentSource))]
    public string? Environment { get; set; }

    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(CommandResult.Ok());
    }
}

/// <summary>
/// Command where the Environment property is optional and may be null.
/// Used to test the middleware skips checks when boundValue is null.
/// </summary>
[Command("OptionalEnvCommand", "optionalenv")]
internal class OptionalEnvCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Dangerous)]
    public string? Environment { get; set; }

    public static bool WasCalled { get; private set; }
    public static void Reset() => WasCalled = false;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        WasCalled = true;
        return Task.FromResult(CommandResult.Ok());
    }
}
