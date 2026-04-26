namespace RossWright.MetalCommand.Tests.Models;

// ---------------------------------------------------------------------------
// Fixtures used by ArgBinderTests
// ---------------------------------------------------------------------------

internal enum Status { Active, Inactive, Pending }

[Command("SingleString")]
internal class SingleStringCommand : ICommand
{
    [Arg] public string? Name { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("TwoPositional")]
internal class TwoPositionalCommand : ICommand
{
    [Arg] public string? First { get; set; }
    [Arg] public string? Second { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("AllowNamed")]
internal class AllowNamedCommand : ICommand
{
    [Arg(AllowNamed = true)] public string? Target { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Named arg with a hyphenated property name to test normalisation.</summary>
[Command("HyphenNamed")]
internal class HyphenNamedCommand : ICommand
{
    [Arg(AllowNamed = true)] public string? MyTarget { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("BoolFlag")]
internal class BoolFlagCommand : ICommand
{
    // bool args are always named-capable regardless of AllowNamed
    [Arg] public bool Verbose { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("RequiredArg")]
internal class RequiredArgCommand : ICommand
{
    [Arg(IsRequired = true)] public string? Value { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("DefaultValue")]
internal class DefaultValueCommand : ICommand
{
    [Arg(DefaultValue = "hello")] public string? Value { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("ContextKey")]
internal class ContextKeyCommand : ICommand
{
    [Arg(ContextKey = "env")] public string? Value { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("RequiredWithContext")]
internal class RequiredWithContextCommand : ICommand
{
    [Arg(IsRequired = true, ContextKey = "env")] public string? Value { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("ValidValues")]
internal class ValidValuesCommand : ICommand
{
    [Arg(ValidValues = ["dev", "prod"])] public string? Env { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("TypedArgs")]
internal class TypedArgsCommand : ICommand
{
    [Arg] public string? StringVal { get; set; }
    [Arg] public int IntVal { get; set; }
    [Arg] public double DoubleVal { get; set; }
    [Arg] public bool BoolVal { get; set; }
    [Arg] public Guid GuidVal { get; set; }
    [Arg] public DateTime DateTimeVal { get; set; }
    [Arg] public Status StatusVal { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Second-declared property has Order=0 so it should receive the first positional token.</summary>
[Command("ExplicitOrderBinder")]
internal class ExplicitOrderBinderCommand : ICommand
{
    [Arg(Order = 1)] public string? Second { get; set; }
    [Arg(Order = 0)] public string? First { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Non-bool property with AllowNamed=false — named tokens must not be consumed.</summary>
[Command("NoNamedAllowed")]
internal class NoNamedAllowedCommand : ICommand
{
    [Arg(AllowNamed = false)] public string? Foo { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>
/// Named arg where the display Name differs from the property name — verifies that
/// named tokens are matched against the display Name as well as the property name.
/// </summary>
[Command("DisplayNameArg")]
internal class DisplayNameArgCommand : ICommand
{
    [Arg(Name = "target", AllowNamed = true)] public string? Dest { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

// ---------------------------------------------------------------------------
// EnvironmentArg fixtures
// ---------------------------------------------------------------------------

/// <summary>Minimal IEnvironmentSource with local/test/prod entries; local is default.</summary>
internal class StubEnvironmentSource : IEnvironmentSource
{
    public string DefaultEnvironment => "local";
    public EnvironmentEntry[] Environments =>
    [
        new() { Name = "local",      IsProtected = false },
        new() { Name = "test",       IsProtected = false },
        new() { Name = "production", IsProtected = true  },
    ];
}

/// <summary>Command with a regular [Arg] followed by an [EnvironmentArg] — exercises Phase 2/3 ordering.</summary>
[Command("EnvArgCommand", "envarg")]
internal class EnvArgCommand : ICommand
{
    [Arg(IsRequired = true)] public string? Name { get; set; }

    [EnvironmentArg(EnvironmentPolicy.Benign)]
    public string? Environment { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Command with only an [EnvironmentArg] — exercises default fallback path.</summary>
[Command("EnvOnlyCommand", "envonly")]
internal class EnvOnlyCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign)]
    public string? Environment { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Application-level fixture that captures the environment it was called with.</summary>
[Command("EnvCapture", "envcapture")]
internal class EnvCaptureCommand : ICommand
{
    [EnvironmentArg(EnvironmentPolicy.Benign)]
    public string? Environment { get; set; }

    public static string? LastEnvironment { get; private set; }
    public static void Reset() => LastEnvironment = null;

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
    {
        LastEnvironment = Environment;
        return Task.FromResult(CommandResult.Ok());
    }
}
