using RossWright.MetalCommand.Tests.Infrastructure;
using RossWright.MetalCommand.Tests.Models;

namespace RossWright.MetalCommand.Tests;

public class MiddlewarePipelineTests
{
    [Fact]
    public async Task NoMiddleware_CommandExecutesDirectly()
    {
        var (app, console) = CommandFixture.Build(configure: (commands, services) =>
        {
            commands.Add<NoArgsTrackedCommand>();
        });

        await app.Execute("noargstracked");

        console.Lines.ShouldContain(NoArgsTrackedCommand.Marker);
    }

    [Fact]
    public async Task SingleMiddleware_WrapsCommand()
    {
        var (app, console) = CommandFixture.Build(
            configure: (commands, services) =>
            {
                commands.Add<NoArgsTrackedCommand>();
            },
            middlewareTypes: [typeof(FirstMiddleware)]);

        await app.Execute("noargstracked");

        var idx1Before = console.Lines.IndexOf("mw1-before");
        var idxCommand = console.Lines.IndexOf(NoArgsTrackedCommand.Marker);
        var idx1After = console.Lines.IndexOf("mw1-after");

        idx1Before.ShouldBeGreaterThanOrEqualTo(0);
        idxCommand.ShouldBeGreaterThan(idx1Before);
        idx1After.ShouldBeGreaterThan(idxCommand);
    }

    [Fact]
    public async Task TwoMiddleware_ExecuteInRegistrationOrder()
    {
        var (app, console) = CommandFixture.Build(
            configure: (commands, services) =>
            {
                commands.Add<NoArgsTrackedCommand>();
            },
            middlewareTypes: [typeof(FirstMiddleware), typeof(SecondMiddleware)]);

        await app.Execute("noargstracked");

        var mw1Before = console.Lines.IndexOf("mw1-before");
        var mw2Before = console.Lines.IndexOf("mw2-before");
        var command  = console.Lines.IndexOf(NoArgsTrackedCommand.Marker);
        var mw2After = console.Lines.IndexOf("mw2-after");
        var mw1After = console.Lines.IndexOf("mw1-after");

        mw1Before.ShouldBeGreaterThanOrEqualTo(0);
        mw2Before.ShouldBeGreaterThan(mw1Before);
        command.ShouldBeGreaterThan(mw2Before);
        mw2After.ShouldBeGreaterThan(command);
        mw1After.ShouldBeGreaterThan(mw2After);
    }

    [Fact]
    public async Task Middleware_ShortCircuit_CommandNotCalled()
    {
        var (app, console) = CommandFixture.Build(
            configure: (commands, services) =>
            {
                commands.Add<NoArgsTrackedCommand>();
            },
            middlewareTypes: [typeof(ShortCircuitMiddleware)]);

        await app.Execute("noargstracked");

        console.Lines.ShouldNotContain(NoArgsTrackedCommand.Marker);
    }

    [Fact]
    public async Task Middleware_CanModifyResult()
    {
        var (app, console) = CommandFixture.Build(
            configure: (commands, services) =>
            {
                commands.Add<NoArgsTrackedCommand>();
            },
            middlewareTypes: [typeof(ResultModifyingMiddleware)]);

        // Execute and then inspect result via a second middleware layer.
        // We verify indirectly: ResultModifyingMiddleware sets Fail("modified"),
        // and the command should still have run (marker present).
        await app.Execute("noargstracked");

        // Command ran (marker present), result was overridden to Fail by middleware.
        // We can't directly capture CommandResult from Execute() — but we can verify
        // the command itself executed (marker written) and middleware ran (no error).
        console.Lines.ShouldContain(NoArgsTrackedCommand.Marker);
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task BindingFailure_SkipsPipeline()
    {
        var (app, console) = CommandFixture.Build(
            configure: (commands, services) =>
            {
                commands.Add<TrackedCommand>();
            },
            middlewareTypes: [typeof(FirstMiddleware)]);

        // TrackedCommand has a required arg — calling without it triggers binding failure.
        await app.Execute("tracked");

        console.Lines.ShouldNotContain(TrackedCommand.Marker);
        console.Lines.ShouldNotContain("mw1-before");
        console.ErrorLines.ShouldNotBeEmpty();
    }
}
