using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

public class SetContextCommandTests
{
    private readonly ConsoleApplication _app;
    private readonly TestConsole _console;

    public SetContextCommandTests()
    {
        (_app, _console) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<SetContextCommand>();
        });
    }

    [Fact]
    public async Task SetContextCommand_SetsKeyValueInContext()
    {
        await _app.Execute("setcontext", "env", "production");

        _app.Context["env"].ShouldBe("production");
    }

    [Fact]
    public async Task SetContextCommand_PrintsKeyValue()
    {
        await _app.Execute("setcontext", "env", "production");

        _console.Lines.ShouldContain("env = production");
    }

    [Fact]
    public async Task SetContextCommand_OverwritesExistingKey()
    {
        _app.Context["env"] = "staging";

        await _app.Execute("setcontext", "env", "production");

        _app.Context["env"].ShouldBe("production");
    }
}
