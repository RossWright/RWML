using RossWright.MetalCommand.Internal.Commands;
using RossWright.MetalCommand.Tests.Infrastructure;
using System.Text.Json;

namespace RossWright.MetalCommand.Tests;

[Collection("FileSystem")]
public class LoadConCommandTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _originalDir;
    private readonly ConsoleApplication _app;
    private readonly TestConsole _console;

    public LoadConCommandTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _originalDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_tempDir);

        (_app, _console) = CommandFixture.Build(configure: (commands, _) =>
        {
            commands.Add<LoadConCommand>();
        });
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDir);
    }

    private void WriteContextFile(string name, Dictionary<string, string> data)
    {
        var path = Path.Combine(_tempDir, $"{name}.mcc.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data));
    }

    [Fact]
    public async Task LoadConCommand_LoadsFile_MergesContext()
    {
        WriteContextFile("default", new() { ["env"] = "staging", ["region"] = "us-east" });

        await _app.Execute("loadcon");

        _app.Context["env"].ShouldBe("staging");
        _app.Context["region"].ShouldBe("us-east");
    }

    [Fact]
    public async Task LoadConCommand_DefaultName_LoadsDefaultFile()
    {
        WriteContextFile("default", new() { ["key"] = "value" });

        await _app.Execute("loadcon");

        _app.Context["key"].ShouldBe("value");
    }

    [Fact]
    public async Task LoadConCommand_MissingFile_ReturnsFail()
    {
        await _app.Execute("loadcon", "nonexistent");

        _console.ErrorLines.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task LoadConCommand_DoesNotClearExistingKeys()
    {
        _app.Context["existing"] = "preserved";
        WriteContextFile("default", new() { ["new"] = "added" });

        await _app.Execute("loadcon");

        _app.Context["existing"].ShouldBe("preserved");
        _app.Context["new"].ShouldBe("added");
    }

    [Fact]
    public async Task LoadConCommand_OverwritesExistingKeyWithFileValue()
    {
        _app.Context["env"] = "old";
        WriteContextFile("default", new() { ["env"] = "new" });

        await _app.Execute("loadcon");

        _app.Context["env"].ShouldBe("new");
    }

    [Fact]
    public async Task LoadConCommand_EmptyFile_ReturnsOk()
    {
        var path = Path.Combine(_tempDir, "default.mcc.json");
        File.WriteAllText(path, "{}");

        await _app.Execute("loadcon");

        _console.Lines.ShouldContain("No context entries found in file.");
    }

    [Fact]
    public async Task LoadConCommand_NullFile_ReturnsOk()
    {
        var path = Path.Combine(_tempDir, "default.mcc.json");
        File.WriteAllText(path, "null");

        await _app.Execute("loadcon");

        _console.Lines.ShouldContain("No context entries found in file.");
    }
}
