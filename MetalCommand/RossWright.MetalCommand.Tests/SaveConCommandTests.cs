using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;
using System.Text.Json;

namespace RossWright.MetalCommand.Tests;

[Collection("FileSystem")]
public class SaveConCommandTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalDir;
    private readonly ConsoleApplication _app;
    private readonly TestConsole _console;

    public SaveConCommandTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempDir);

        (_app, _console) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<SaveConCommand>();
        });

        _app.Context["env"] = "staging";
        _app.Context["region"] = "us-east";
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
    }

    [Fact]
    public async Task SaveConCommand_DefaultName_WritesDefaultFile()
    {
        await _app.Execute("savecon");

        File.Exists(Path.Combine(_tempDir, "default.mcc.json")).ShouldBeTrue();
    }

    [Fact]
    public async Task SaveConCommand_CustomName_WritesNamedFile()
    {
        await _app.Execute("savecon", "staging");

        File.Exists(Path.Combine(_tempDir, "staging.mcc.json")).ShouldBeTrue();
    }

    [Fact]
    public async Task SaveConCommand_FileContainsContextJson()
    {
        await _app.Execute("savecon");

        var json = await File.ReadAllTextAsync(Path.Combine(_tempDir, "default.mcc.json"));
        var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        loaded.ShouldNotBeNull();
        loaded["env"].ShouldBe("staging");
        loaded["region"].ShouldBe("us-east");
    }

    [Fact]
    public async Task SaveThenLoad_RoundTrips_AllContextKeys()
    {
        // Build a second app wired with both save and load to do a full roundtrip
        var (app2, _) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<SaveConCommand>();
            commands.Add<LoadConCommand>();
        });

        app2.Context["env"] = "prod";
        app2.Context["region"] = "eu-west";

        await app2.Execute("savecon", "roundtrip");

        app2.Context.Remove("env");
        app2.Context.Remove("region");

        await app2.Execute("loadcon", "roundtrip");

        app2.Context["env"].ShouldBe("prod");
        app2.Context["region"].ShouldBe("eu-west");
    }
}
