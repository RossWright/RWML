using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Tests.Infrastructure;
using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

/// <summary>
/// Cross-cutting integration tests that verify the split of concerns between
/// <see cref="ArgBinder"/> (Phase 2) and <see cref="EnvironmentArgMiddleware"/>:
/// [EnvironmentArg] properties must not be consumed by ArgBinder and must be
/// correctly populated by the middleware before the command executes.
/// </summary>
public class EnvironmentArgBindingIntegrationTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    private static (ConsoleApplication App, TestConsole Console) BuildWithEnvMiddleware(
        Action<CommandCollectionBuilder, IServiceCollection>? configure = null,
        params string?[] consoleInputs) =>
        CommandFixture.Build(
            configure: (commands, services) =>
            {
                services.AddSingleton<IEnvironmentSource>(new StubProtectedEnvironmentSource());
                configure?.Invoke(commands, services);
            },
            middlewareTypes: [typeof(EnvironmentArgMiddleware)],
            consoleInputs: consoleInputs);

    // ── 22.1 — [EnvironmentArg] property set by middleware, not ArgBinder ──

    [Fact]
    public async Task EnvironmentArgProperty_BoundByMiddleware_NotByArgBinder()
    {
        // Arrange — DangerousEnvCaptureCommand captures the value EnvironmentArgMiddleware sets.
        // "local" is unprotected so no prompt is needed.
        DangerousEnvCaptureCommand.Reset();
        var (app, _) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCaptureCommand>());

        // Act
        await app.Execute("dangerousenv-capture", "local");

        // Assert — middleware resolved and set the property; command captured it.
        DangerousEnvCaptureCommand.LastEnvironment.ShouldBe("local");
    }

    // ── 22.2 — [Arg] still bound while [EnvironmentArg] is handled by middleware

    [Fact]
    public async Task EnvironmentArgSkippedByArgBinder_RegularArgStillBound()
    {
        // Arrange — MixedArgAndEnvCommand has one [Arg] (Name) and one [EnvironmentArg] (Environment).
        // ArgBinder must bind Name to "alice"; EnvironmentArgMiddleware must set Environment to "local".
        MixedArgAndEnvCommand.Reset();
        var (app, _) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<MixedArgAndEnvCommand>());

        // Act — "alice" is the positional [Arg] value; "local" is the env token.
        await app.Execute("mixed", "alice", "local");

        // Assert
        MixedArgAndEnvCommand.LastName.ShouldBe("alice");
        MixedArgAndEnvCommand.LastEnvironment.ShouldBe("local");
    }

    // ── 22.3 — Unknown environment → middleware null-skip path ─────────────

    [Fact]
    public async Task ValidValues_EnvArg_UnknownEnvironment_CommandNotReached()
    {
        // Arrange — "unknown" is not in StubProtectedEnvironmentSource.Environments,
        // so EnvironmentArgMiddleware resolves null and skips execution.
        DangerousEnvCaptureCommand.Reset();
        var (app, console) = BuildWithEnvMiddleware(
            configure: (commands, _) => commands.Add<DangerousEnvCaptureCommand>());

        // Act
        await app.Execute("dangerousenv-capture", "unknown");

        // Assert — command body was never reached; property remains null.
        DangerousEnvCaptureCommand.LastEnvironment.ShouldBeNull();
        // An error or warning should have been written explaining the bad environment.
        console.ErrorLines.ShouldNotBeEmpty();
    }
}
