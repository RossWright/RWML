using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;
using System.Text.Json;

namespace RossWright.MetalCommand.Tests;

public class DupConCommandTests
{
    private static (ConsoleApplication App, TestConsole Console, IDupConTerminalLauncher Launcher)
        BuildApp(
            Action<DupConCommandOptions>? configureOpts = null,
            Dictionary<string, string>? initialContext = null)
    {
        var launcher = Substitute.For<IDupConTerminalLauncher>();

        var opts = new DupConCommandOptions();
        configureOpts?.Invoke(opts);

        var (app, console) = CommandFixture.Build(configure: (commands, services) =>
        {
            services.AddSingleton(opts);
            services.AddSingleton<IDupConTerminalLauncher>(launcher);
            commands.Add<DupConCommand>();
        });

        foreach (var (k, v) in initialContext ?? [])
            app.Context[k] = v;

        return (app, console, launcher);
    }

    [Fact]
    public async Task DupConCommand_NoTerminal_ReturnsFail()
    {
        var (app, console, launcher) = BuildApp();
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await app.Execute("dupcon");

        console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task DupConCommand_LaunchSuccess_ReturnsOk()
    {
        var (app, console, launcher) = BuildApp();
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await app.Execute("dupcon");

        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task DupConCommand_ContextFlag_SerializesContext()
    {
        var (app, _, launcher) = BuildApp(initialContext: new() { ["env"] = "staging" });
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await app.Execute("dupcon", "--context");

        launcher.Received(1).Launch(Arg.Any<string>(), Arg.Is<string>(a => a.Contains("--ctx")));
    }

    [Fact]
    public async Task DupConCommand_DefaultForwardContext_SerializesContext()
    {
        var (app, _, launcher) = BuildApp(
            configureOpts: o => o.DefaultForwardContext = true,
            initialContext: new() { ["env"] = "staging" });
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await app.Execute("dupcon");

        launcher.Received(1).Launch(Arg.Any<string>(), Arg.Is<string>(a => a.Contains("--ctx")));
    }

    [Fact]
    public async Task DupConCommand_CmdFlag_PassesCommand()
    {
        var (app, _, launcher) = BuildApp();
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await app.Execute("dupcon", "--cmd", "ping");

        launcher.Received(1).Launch(Arg.Any<string>(), Arg.Is<string>(a => a.Contains("--cmd \"ping\"")));
    }

    [Fact]
    public async Task DupConCommand_ParentPidAddedToContext()
    {
        var (app, _, launcher) = BuildApp(initialContext: new() { ["env"] = "prod" });
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
        string? capturedArgs = null;
        launcher
            .When(l => l.Launch(Arg.Any<string>(), Arg.Any<string>()))
            .Do(ci => capturedArgs = ci.ArgAt<string>(1));

        await app.Execute("dupcon", "--context");

        capturedArgs.ShouldNotBeNull();
        var base64 = capturedArgs!.Replace("--ctx ", "");
        var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json)!;
        dict.ShouldContainKey("__dupcon_parent_pid");
    }

    [Fact]
    public async Task DupConCommand_NoContextFlag_DoesNotSerialize()
    {
        var (app, _, launcher) = BuildApp(initialContext: new() { ["env"] = "prod" });
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        await app.Execute("dupcon");

        launcher.Received(1).Launch(Arg.Any<string>(), Arg.Is<string>(a => !a.Contains("--ctx")));
    }

    [Fact]
    public async Task DupConCommand_CtxBlob_CanBeDecodedByDupConBootstrap()
    {
        // Arrange: context the command should forward
        var (app, _, launcher) = BuildApp(
            configureOpts: o => o.DefaultForwardContext = false,
            initialContext: new() { ["env"] = "prod", ["region"] = "eu-west" });
        launcher.Launch(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        string? capturedArgs = null;
        launcher
            .When(l => l.Launch(Arg.Any<string>(), Arg.Any<string>()))
            .Do(ci => capturedArgs = ci.ArgAt<string>(1));

        // Act: forward context explicitly
        await app.Execute("dupcon", "--context");

        capturedArgs.ShouldNotBeNull();

        // Split the captured args string back into the array form DupConBootstrap expects
        var bootstrapArgs = capturedArgs!.Split(' ', 2); // ["--ctx", "<blob>"]

        var (_, recovered, _) = DupConBootstrap.TryExtract(bootstrapArgs);

        // Assert: original user keys round-trip correctly
        recovered["env"].ShouldBe("prod");
        recovered["region"].ShouldBe("eu-west");
    }
}
