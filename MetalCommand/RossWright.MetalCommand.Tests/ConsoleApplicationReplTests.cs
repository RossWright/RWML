using System.Text;
using System.Text.Json;
using RossWright.MetalCommand.Tests.Infrastructure;
using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

/// <summary>
/// Tests for the <see cref="ConsoleApplication.RunAsync"/> REPL loop, covering
/// termination, blank input, command-as-args pre-execution, context bootstrap,
/// and EOF handling.
/// </summary>
/// <remarks>
/// Placed in the "FileSystem" collection because <see cref="ConsoleApplication.RunAsync"/>
/// calls <see cref="Directory.SetCurrentDirectory"/> and must not run in parallel with
/// other tests that depend on the process-wide current working directory.
/// </remarks>
[Collection("FileSystem")]
/// and EOF handling.
/// </summary>
public class ConsoleApplicationReplTests : IDisposable
{
    private readonly string _originalDir = Directory.GetCurrentDirectory();

    public void Dispose() => Directory.SetCurrentDirectory(_originalDir);

    // ── Helpers ────────────────────────────────────────────────────────────

    private static (ConsoleApplication App, TestConsole Console) BuildRepl(
        Action<CommandCollectionBuilder>? configure = null,
        params string?[] consoleInputs)
    {
        var (app, console) = CommandFixture.Build(
            configure: (commands, _) =>
            {
                commands.Add<GreetCommand>();
                commands.Add<ReadContextCommand>();
                configure?.Invoke(commands);
            },
            consoleInputs: consoleInputs);
        return (app, console);
    }

    /// <summary>Encodes a context dictionary as Base64 JSON for --ctx bootstrap.</summary>
    private static string EncodeContext(Dictionary<string, string> ctx)
    {
        var json = JsonSerializer.Serialize(ctx);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private static readonly TimeSpan _replTimeout = TimeSpan.FromSeconds(5);

    // ── 23.1 — "exit" terminates the loop ─────────────────────────────────

    [Fact]
    public async Task RunAsync_ExitCommand_TerminatesLoop()
    {
        GreetCommand.Reset();
        var (app, console) = BuildRepl(consoleInputs: "exit");

        var task = app.RunAsync();
        await task.WaitAsync(_replTimeout);

        task.IsCompleted.ShouldBeTrue();
        console.Lines.ShouldContain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_ByeCommand_TerminatesLoop()
    {
        var (app, console) = BuildRepl(consoleInputs: "bye");

        var task = app.RunAsync();
        await task.WaitAsync(_replTimeout);

        task.IsCompleted.ShouldBeTrue();
        console.Lines.ShouldContain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_QuitCommand_TerminatesLoop()
    {
        var (app, console) = BuildRepl(consoleInputs: "quit");

        var task = app.RunAsync();
        await task.WaitAsync(_replTimeout);

        task.IsCompleted.ShouldBeTrue();
        console.Lines.ShouldContain("Goodbye!");
    }

    // ── 23.2 — null input (EOF) terminates cleanly ────────────────────────

    [Fact]
    public async Task RunAsync_NullInput_TerminatesLoop()
    {
        // TestConsole returns null from ReadLine when the queue is empty.
        var (app, _) = BuildRepl(consoleInputs: Array.Empty<string?>());

        var task = app.RunAsync();
        await task.WaitAsync(_replTimeout);

        task.IsCompleted.ShouldBeTrue();
    }

    // ── 23.3 — command passed as startup args executes then continues ──────

    [Fact]
    public async Task RunAsync_CommandAsArgs_ExecutesImmediately()
    {
        GreetCommand.Reset();
        // Feed "exit" so the REPL terminates after executing the initial command.
        var (app, console) = BuildRepl(consoleInputs: "exit");

        var task = app.RunAsync("greet");
        await task.WaitAsync(_replTimeout);

        GreetCommand.WasCalled.ShouldBeTrue();
        console.Lines.ShouldContain("Goodbye!");
    }

    // ── 23.4 — --ctx startup arg populates context before first command ────

    [Fact]
    public async Task RunAsync_DupConBootstrap_CtxArg_PopulatesContext()
    {
        ReadContextCommand.Reset();
        var blob = EncodeContext(new Dictionary<string, string> { ["env"] = "staging" });

        // Feed "readctx" to read the context key, then "exit".
        var (app, _) = BuildRepl(configure: null, "readctx", "exit");

        var task = app.RunAsync("--ctx", blob);
        await task.WaitAsync(_replTimeout);

        ReadContextCommand.LastEnv.ShouldBe("staging");
    }

    // ── 23.5 — blank input is silently ignored ────────────────────────────

    [Fact]
    public async Task RunAsync_BlankInput_DoesNotCallAnyCommand()
    {
        var (app, console) = BuildRepl(configure: null, "", "exit");

        var task = app.RunAsync();
        await task.WaitAsync(_replTimeout);

        console.ErrorLines.ShouldBeEmpty();
    }
}
