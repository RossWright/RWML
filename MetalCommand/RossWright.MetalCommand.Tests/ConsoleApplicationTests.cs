using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;
using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

[Collection("FileSystem")]
public class ConsoleApplicationTests
{
    [Fact]
    public async Task Execute_KnownInvocation_CallsCommand()
    {
        GreetCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<GreetCommand>();
        });

        await app.Execute("greet");

        GreetCommand.WasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_UnknownInvocation_WritesError()
    {
        var (app, console) = CommandFixture.Build();

        await app.Execute("nope");

        console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task DuplicateInvocations_AreReportedAndExcluded()
    {
        var (app, console) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<ClashACommand>();
            commands.Add<ClashBCommand>();
        });

        // The constructor should have written an error about the conflicting invocation.
        console.ErrorLines.ShouldContain(l => l.Contains("clash"));

        // Neither command should be reachable under the clashing invocation.
        await app.Execute("clash");

        // "clash" was excluded — Execute writes its own "No command found" error.
        console.ErrorLines.ShouldContain(l => l.Contains("clash") || l.Contains("No command"));
    }

    [Fact]
    public async Task Context_IsSharedAcrossCalls()
    {
        ReadContextCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<ReadContextCommand>();
        });

        app.Context["env"] = "staging";

        await app.Execute("readctx");

        ReadContextCommand.LastEnv.ShouldBe("staging");
    }

    [Fact]
    public async Task AddContext_SetsValueBeforeRun()
    {
        ReadContextCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<ReadContextCommand>();
        });

        app.AddContext("env", "production");

        await app.Execute("readctx");

        ReadContextCommand.LastEnv.ShouldBe("production");
    }

    // -------------------------------------------------------------------------
    // Execute invocation case-insensitivity
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_MixedCaseInvocation_CallsCommand()
    {
        // Regression: Execute stored invocations lowercased but looked them up without
        // lowercasing the caller's string, so "Greet" would not match "greet".
        GreetCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<GreetCommand>();
        });

        await app.Execute("Greet");

        GreetCommand.WasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task Execute_UpperCaseInvocation_CallsCommand()
    {
        GreetCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<GreetCommand>();
        });

        await app.Execute("GREET");

        GreetCommand.WasCalled.ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // EnvironmentArg routing through Execute
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_WithEnvironmentArg_PassesEnvironmentToCommand()
    {
        // Regression: when ReloadDatabaseCommand called Execute("Migrate", environment),
        // the environment positional token was consumed by Phase 2 and Phase 3 fell back
        // to the default. Verify the value reaches the command.
        EnvCaptureCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, services) =>
        {
            commands.Add<EnvCaptureCommand>();
            services.AddSingleton<IEnvironmentSource>(new StubEnvironmentSource());
        });

        await app.Execute("envcapture", "test");

        EnvCaptureCommand.LastEnvironment.ShouldBe("test");
    }

    [Fact]
    public async Task Execute_WithEnvironmentArgOmitted_UsesDefaultEnvironment()
    {
        EnvCaptureCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, services) =>
        {
            commands.Add<EnvCaptureCommand>();
            services.AddSingleton<IEnvironmentSource>(new StubEnvironmentSource());
        });

        await app.Execute("envcapture");

        EnvCaptureCommand.LastEnvironment.ShouldBe("local");
    }

    // -------------------------------------------------------------------------
    // Alternate invocations
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("m")]
    [InlineData("go")]
    [InlineData("multi")]
    public async Task Execute_AlternateInvocation_CallsCommand(string invocation)
    {
        MultiInvocationCommand.Reset();
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
            commands.Add<MultiInvocationCommand>());

        await app.Execute(invocation);

        MultiInvocationCommand.WasCalled.ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // Unknown invocation — exactly one error
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_UnknownInvocation_WritesExactlyOneError()
    {
        var (app, console) = CommandFixture.Build();

        await app.Execute("totallybogus");

        console.ErrorLines.Count.ShouldBe(1);
    }

    // -------------------------------------------------------------------------
    // CancellationToken propagation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Execute_CancellationToken_CanBeCancelled()
    {
        var (app, _) = CommandFixture.Build(configure: (commands, _) =>
            commands.Add<SlowCommand>());

        // Use reflection to cancel the app's internal CTS
        var ctsField = typeof(ConsoleApplication)
            .GetField("_currentCommandCancellationTokenSource",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var executeTask = app.Execute("slow");

        // Give the command a moment to start, then cancel
        await Task.Delay(50);
        ((CancellationTokenSource)ctsField.GetValue(app)!).Cancel();

        // Task must complete within a reasonable timeout (not hang)
        var completedInTime = await Task.WhenAny(executeTask, Task.Delay(3000)) == executeTask;
        completedInTime.ShouldBeTrue();
    }

    // -------------------------------------------------------------------------
    // Built-in commands through Execute pipeline
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BuiltIn_SetContext_ThroughExecute_UpdatesExistingKey()
    {
        var (app, console) = CommandFixture.Build(configure: (commands, _) =>
            commands.Add<SetContextCommand>());

        app.Context["env"] = "staging";

        await app.Execute("setcontext", "env", "production");

        app.Context["env"].ShouldBe("production");
    }

    [Fact]
    public async Task BuiltIn_ListContext_HidesDoubleUnderscorePrefixedKeys()
    {
        var (app, console) = CommandFixture.Build(configure: (commands, _) =>
            commands.Add<ListContextCommand>());

        app.Context["env"] = "prod";
        app.Context["__runId"] = "hidden";

        await app.Execute("listcontext");

        console.Lines.ShouldContain(l => l.Contains("env"));
        console.Lines.ShouldNotContain(l => l.Contains("__runId"));
    }

    // -------------------------------------------------------------------------
    // RunAsync tests
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData("help")]
    [InlineData("man")]
    [InlineData("h")]
    public async Task RunAsync_HelpCommand_ShowsHelp(string helpInvocation)
    {
        var (app, console) = CommandFixture.Build(
            (commands, _) => commands.Add<GreetCommand>(),
            null,
            helpInvocation, "exit");

        await app.RunAsync();

        console.Lines.ShouldContain(l => l.Contains("Greet") || l.Contains("help", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RunAsync_HelpWithCommandArgument_ShowsCommandHelp()
    {
        var (app, console) = CommandFixture.Build(
            (commands, _) => commands.Add<GreetCommand>(),
            null,
            "help greet", "exit");

        await app.RunAsync();

        console.Lines.ShouldContain(l => l.Contains("Greet"));
    }

    [Fact]
    public async Task RunAsync_UnknownCommand_WritesError()
    {
        var (app, console) = CommandFixture.Build(
            null,
            null,
            "unknowncmd", "exit");

        await app.RunAsync();

        console.ErrorLines.ShouldContain(l => l.Contains("Unknown Command"));
    }

    [Fact]
    public async Task RunAsync_CommandWithExitAndFailure_ExitsWithErrorMessage()
    {
        var (app, console) = CommandFixture.Build(
            (commands, _) => commands.Add<FailAndExitCommand>(),
            null,
            "failexit");

        await app.RunAsync();

        console.ErrorLines.ShouldContain(l => l.Contains("Something went wrong"));
        console.Lines.ShouldContain(l => l.Contains("Goodbye"));
    }

    [Fact]
    public async Task RunAsync_CommandThrowsTaskCanceledException_WritesAbortMessage()
    {
        var (app, console) = CommandFixture.Build(
            (commands, _) => commands.Add<ThrowTaskCanceledCommand>(),
            null,
            "throwcancel", "exit");

        await app.RunAsync();

        console.ErrorLines.ShouldContain(l => l.Contains("Command Aborted"));
    }

    [Fact]
    public async Task RunAsync_CommandThrowsException_WritesErrorDetails()
    {
        var (app, console) = CommandFixture.Build(
            (commands, _) => commands.Add<ThrowExceptionCommand>(),
            null,
            "throwex", "exit");

        await app.RunAsync();

        console.ErrorLines.ShouldContain(l => l.Contains("Test exception"));
    }
}
