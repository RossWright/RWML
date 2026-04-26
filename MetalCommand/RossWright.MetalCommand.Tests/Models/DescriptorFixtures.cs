namespace RossWright.MetalCommand.Tests.Models;

// ---------------------------------------------------------------------------
// Fixtures used by CommandDescriptorFactoryTests
// ---------------------------------------------------------------------------

[Command("NoArg", HelpBrief = "No args brief", HelpDetail = "No args detail")]
internal class NoArgCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

[Command("Foo", "foo", "f", HelpBrief = "Foo brief", HelpDetail = "Foo detail")]
internal class ExplicitInvocationsCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Three [Arg] properties with no explicit Order — declaration order is expected.</summary>
[Command("DeclarationOrder")]
internal class DeclarationOrderCommand : ICommand
{
    [Arg] public string? Alpha { get; set; }
    [Arg] public string? Beta { get; set; }
    [Arg] public string? Gamma { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Three [Arg] properties with explicit Order values set out of declaration sequence.</summary>
[Command("ExplicitOrder")]
internal class ExplicitOrderCommand : ICommand
{
    [Arg(Order = 2)] public string? Third { get; set; }
    [Arg(Order = 0)] public string? First { get; set; }
    [Arg(Order = 1)] public string? Second { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Exercises ArgAttribute property propagation: Name, IsRequired, DefaultValue, ValidValues, ContextKey.</summary>
[Command("PropagationTest")]
internal class PropagationCommand : ICommand
{
    [Arg(Name = "custom")] public string? NamedProp { get; set; }

    [Arg] public string? MyProp { get; set; }

    [Arg(IsRequired = true)] public string? RequiredProp { get; set; }

    [Arg(DefaultValue = "default-val")] public string? DefaultProp { get; set; }

    [Arg(ValidValues = ["a", "b"])] public string? ValidValuesProp { get; set; }

    [Arg(ContextKey = "ctx-key")] public string? ContextProp { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>A type with NO [Command] attribute — used to verify that the factory throws.</summary>
internal class NoAttributeCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>Command with a Category specified — verifies Category propagation.</summary>
[Command("Cat", Category = "Tools")]
internal class CategorisedCommand : ICommand
{
    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}

/// <summary>
/// One regular [Arg] and one [EnvironmentArg] property — verifies that
/// [EnvironmentArg] descriptors are appended after [Arg] descriptors.
/// </summary>
[Command("MixedArg")]
internal class MixedArgCommand : ICommand
{
    [Arg] public string? Name { get; set; }
    [EnvironmentArg] public string? Environment { get; set; }

    public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        => Task.FromResult(CommandResult.Ok());
}
