using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

public class ListContextCommandTests
{
    private readonly ConsoleApplication _app;
    private readonly TestConsole _console;

    public ListContextCommandTests()
    {
        (_app, _console) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<ListContextCommand>();
        });
    }

    [Fact]
    public async Task ListContextCommand_EmptyContext_PrintsEmptyMessage()
    {
        await _app.Execute("listcontext");

        _console.Lines.ShouldContain("Context is empty.");
    }

    [Fact]
    public async Task ListContextCommand_WithEntries_PrintsAllKeyValuePairs()
    {
        _app.Context["env"] = "staging";
        _app.Context["region"] = "us-east";

        await _app.Execute("listcontext");

        _console.Lines.ShouldContain("env = staging");
        _console.Lines.ShouldContain("region = us-east");
    }

    [Fact]
    public async Task ListContextCommand_WithEntries_PrintsInAlphabeticalOrder()
    {
        _app.Context["zzz"] = "last";
        _app.Context["aaa"] = "first";

        await _app.Execute("listcontext");

        var lines = _console.Lines.ToList();
        var aaaIndex = lines.IndexOf("aaa = first");
        var zzzIndex = lines.IndexOf("zzz = last");
        aaaIndex.ShouldBeLessThan(zzzIndex);
    }
}
