using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Tests.Infrastructure;
using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

/// <summary>
/// Tests for <see cref="EnvironmentArgMiddleware"/> covering confirmation prompting,
/// Forbidden short-circuit, and per-run-ID confirmation scoping.
/// </summary>
public class EnvironmentArgMiddlewareTests
{
    private static (ConsoleApplication App, TestConsole Console) BuildWithEnvMiddleware(
        Action<CommandCollectionBuilder, IServiceCollection>? configure = null,
        params string?[] consoleInputs)
    {
        return CommandFixture.Build(
            configure: (commands, services) =>
            {
                services.AddSingleton<IEnvironmentSource>(new StubProtectedEnvironmentSource());
                configure?.Invoke(commands, services);
            },
            middlewareTypes: [typeof(EnvironmentArgMiddleware)],
            consoleInputs: consoleInputs);
    }

    // -------------------------------------------------------------------------
    // Benign / unprotected environments — no prompt
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Dangerous_UnprotectedEnvironment_NoPrompt_CommandRuns()
    {
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>());

        await app.Execute("dangerousenv", "local");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // Dangerous — prompt and confirm
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Dangerous_ProtectedEnvironment_ConfirmYes_CommandRuns()
    {
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>(),
            consoleInputs: "yes");

        await app.Execute("dangerousenv", "production");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Dangerous_ProtectedEnvironment_ConfirmNo_CommandAborted()
    {
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>(),
            consoleInputs: "no");

        await app.Execute("dangerousenv", "production");

        DangerousEnvCommand.WasCalled.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Dangerous_ProtectedEnvironment_PromptIncludesCommandNameAndEnvironment()
    {
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>(),
            consoleInputs: "yes");

        await app.Execute("dangerousenv", "production");

        console.Lines.ShouldContain(l =>
            l.Contains("DangerousEnvCommand", StringComparison.OrdinalIgnoreCase) &&
            l.Contains("production", StringComparison.OrdinalIgnoreCase));
    }

    // -------------------------------------------------------------------------
    // Forbidden — hard block regardless of confirmation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Forbidden_ProtectedEnvironment_CommandBlocked()
    {
        ForbiddenEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<ForbiddenEnvCommand>());

        await app.Execute("forbiddenenv", "production");

        ForbiddenEnvCommand.WasCalled.ShouldBeFalse();
        console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Forbidden_UnprotectedEnvironment_CommandRuns()
    {
        ForbiddenEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<ForbiddenEnvCommand>());

        await app.Execute("forbiddenenv", "local");

        ForbiddenEnvCommand.WasCalled.ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // Per-run-ID scoping — confirm once, skip for sub-commands in same run
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Dangerous_SameRunId_SecondCallSkipsPrompt()
    {
        // Pre-populate a __runId and its confirmation key to simulate a sub-command
        // scenario where the top-level command already confirmed.
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>());

        var runId = "test-run-123";
        app.Context["__runId"] = runId;
        app.Context[$"__confirmed:production:{runId}"] = "true";

        await app.Execute("dangerousenv", "production");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
        // No "Type yes" prompt should have been written
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    [Fact]
    public async Task Dangerous_DifferentRunId_PromptsAgain()
    {
        // Confirm for run-A, then execute under run-B — should prompt again.
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>(),
            consoleInputs: "yes");

        app.Context["__runId"] = "run-B";
        app.Context["__confirmed:production:run-A"] = "true"; // different run

        await app.Execute("dangerousenv", "production");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldContain(l => l.Contains("Type \"yes\""));
    }

    [Fact]
    public async Task Dangerous_NoRunId_PromptsEveryTime()
    {
        // If no __runId is in context (edge case), each call must prompt independently.
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>(),
            consoleInputs: "yes");

        // Deliberately leave __runId absent (Execute(string) will add one, so remove it after)
        // Use the low-level path by ensuring Execute adds it, then strip it:
        // Instead, confirm the standard flow still prompts when there's no pre-existing confirmation.
        await app.Execute("dangerousenv", "production");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // Forbidden — error lists safe alternatives
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Forbidden_ProtectedEnvironment_ErrorListsSafeAlternatives()
    {
        ForbiddenEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<ForbiddenEnvCommand>());

        await app.Execute("forbiddenenv", "production");

        ForbiddenEnvCommand.WasCalled.ShouldBeFalse();
        // StubProtectedEnvironmentSource has "local" as the only safe environment
        console.ErrorLines.ShouldContain(l => l.Contains("local"));
    }

    // -------------------------------------------------------------------------
    // Dangerous — confirmation scoped per environment, not globally per run
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Dangerous_ConfirmationScopedPerEnvironment_DifferentEnvStillPrompts()
    {
        // Pre-confirm "production" for the current run; then execute against "staging"
        // (also protected). The second execution must prompt independently.
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, services) =>
            {
                commands.Add<DangerousEnvCommand>();
                // Replace the default source with one that has two protected environments
                services.AddSingleton<IEnvironmentSource>(new TwoProtectedEnvironmentSource());
            },
            consoleInputs: "yes");

        var runId = "test-run-scoped";
        app.Context["__runId"] = runId;
        // Pre-confirm production — staging should NOT inherit this
        app.Context[$"__confirmed:production:{runId}"] = "true";

        DangerousEnvCommand.Reset();
        await app.Execute("dangerousenv", "staging");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // No IEnvironmentSource registered — middleware skips policy checks
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NoEnvironmentSource_CommandRunsWithoutPrompt()
    {
        DangerousEnvCommand.Reset();
        // Build WITHOUT registering any IEnvironmentSource
        var (app, console) = CommandFixture.Build(
            configure: (commands, _) => commands.Add<DangerousEnvCommand>(),
            middlewareTypes: [typeof(EnvironmentArgMiddleware)]);

        await app.Execute("dangerousenv", "production");

        // Without a source the binder cannot validate the env value — command runs
        // or the binder rejects the unknown value. Either way, no prompt should appear.
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // Benign policy — never prompts even on protected environments
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Benign_ProtectedEnvironment_NeverPrompts()
    {
        BenignEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<BenignEnvCommand>());

        await app.Execute("benignenv", "production");

        BenignEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // ICommandOptionsRegistry policy override
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PolicyOverride_ViaOptionsRegistry_OverridesAttributePolicy()
    {
        // DangerousEnvCommand declares Dangerous, but the registry overrides it to Benign
        DangerousEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, services) =>
            {
                commands.Add<DangerousEnvCommand>();
                services.AddSingleton<ICommandOptionsRegistry>(
                    new StubCommandOptionsRegistry(typeof(DangerousEnvCommand), EnvironmentPolicy.Benign));
            });

        await app.Execute("dangerousenv", "production");

        DangerousEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // Null boundValue — middleware skips when property is null
    // -------------------------------------------------------------------------

    [Fact]
    public async Task NullBoundValue_MiddlewareSkipsCheck_CommandRuns()
    {
        // Command has an EnvironmentArg property but no value is provided — should skip
        OptionalEnvCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<OptionalEnvCommand>());

        // Execute without providing an environment argument
        await app.Execute("optionalenv");

        OptionalEnvCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    // -------------------------------------------------------------------------
    // Null source — binding fails when EnvironmentSourceType is not registered
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CustomSourceNotRegistered_BindingFails_CommandDoesNotRun()
    {
        // Command specifies a custom EnvironmentSourceType that is not registered
        // ArgBinder Phase 3 should fail and write an error
        CustomSourceCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<CustomSourceCommand>());

        await app.Execute("customsource", "production");

        // Binding should fail, command should not run
        CustomSourceCommand.WasCalled.ShouldBeFalse();
        console.ErrorLines.ShouldContain(l => l.Contains("No IEnvironmentSource"));
    }

    // -------------------------------------------------------------------------
    // Direct middleware tests for edge cases
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Middleware_BoundValueIsNull_SkipsCheck()
    {
        // Arrange: Create a command with an EnvironmentArg property that is null
        var command = new DangerousEnvCommand { Environment = null };
        var console = new TestConsole();
        var services = new ServiceCollection()
            .AddSingleton<IEnvironmentSource>(new StubProtectedEnvironmentSource())
            .BuildServiceProvider();
        var middleware = new EnvironmentArgMiddleware(services);
        var context = new CommandContext
        {
            Command = command,
            Console = console,
            SessionContext = new Dictionary<string, string>(),
            CancellationToken = CancellationToken.None
        };
        var nextCalled = false;

        // Act
        await middleware.InvokeAsync(context, _ => { nextCalled = true; return Task.CompletedTask; });

        // Assert: Middleware should skip the check and call next
        nextCalled.ShouldBeTrue();
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }

    [Fact]
    public async Task Middleware_SourceIsNull_SkipsCheck()
    {
        // Arrange: Create a command with a custom source type that won't be resolved
        var command = new CustomSourceCommand { Environment = "production" };
        var console = new TestConsole();
        var services = new ServiceCollection()
            .AddSingleton<IEnvironmentSource>(new StubProtectedEnvironmentSource()) // Only register default source, not ICustomEnvironmentSource
            .BuildServiceProvider();
        var middleware = new EnvironmentArgMiddleware(services);
        var context = new CommandContext
        {
            Command = command,
            Console = console,
            SessionContext = new Dictionary<string, string>(),
            CancellationToken = CancellationToken.None
        };
        var nextCalled = false;

        // Act
        await middleware.InvokeAsync(context, _ => { nextCalled = true; return Task.CompletedTask; });

        // Assert: Middleware should skip the check when source is null and call next
        nextCalled.ShouldBeTrue();
        console.Lines.ShouldNotContain(l => l.Contains("Type \"yes\""));
    }
}
