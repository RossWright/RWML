using System.Text;
using System.Text.Json;
using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

/// <summary>
/// Tests for the <see cref="ConsoleApplicationExtensions.LoadContext"/> builder extension,
/// which loads a <c>.mcc.json</c> context file into the session during <c>Build()</c>.
/// </summary>
[Collection("FileSystem")]
public class LoadContextBuilderTests
{
    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Writes a context file to <see cref="AppContext.BaseDirectory"/> and returns the name
    /// (without extension) so it can be passed to <see cref="ConsoleApplicationExtensions.LoadContext"/>.
    /// </summary>
    private static string WriteContextFile(Dictionary<string, string> ctx)
    {
        var name = $"test-ctx-{Guid.NewGuid():N}";
        var path = Path.Combine(AppContext.BaseDirectory, name + ".mcc.json");
        File.WriteAllText(path, JsonSerializer.Serialize(ctx), Encoding.UTF8);
        return name;
    }

    /// <summary>
    /// Builds a <see cref="ConsoleApplication"/> via the public builder path and captures
    /// anything written to <see cref="System.Console.Out"/> during <c>Build()</c>.
    /// </summary>
    private static (ConsoleApplication App, string CapturedOutput) BuildAndCapture(
        Action<IConsoleApplicationBuilder> configure)
    {
        var original = System.Console.Out;
        var sb = new StringBuilder();
        System.Console.SetOut(new StringWriter(sb));
        try
        {
            var builder = ConsoleApplication.CreateBuilder();
            configure(builder);
            var app = builder.Build();
            return (app, sb.ToString());
        }
        finally
        {
            System.Console.SetOut(original);
        }
    }

    // ── 24.1 — file exists → context loaded before REPL ───────────────────

    [Fact]
    public void LoadContext_FileExists_ContextLoadedBeforeRepl()
    {
        var name = WriteContextFile(new Dictionary<string, string>
        {
            ["env"] = "integration",
            ["tenant"] = "acme"
        });

        var (app, _) = BuildAndCapture(b => b.LoadContext(name));

        app.Context["env"].ShouldBe("integration");
        app.Context["tenant"].ShouldBe("acme");
    }

    // ── 24.2 — file missing + showWarnIfMissing=true → warning written ─────

    [Fact]
    public void LoadContext_FileMissing_ShowWarnIfMissing_WritesWarning()
    {
        var missingName = $"definitely-missing-{Guid.NewGuid():N}";

        var (_, output) = BuildAndCapture(b =>
            b.LoadContext(missingName, showWarnIfMissing: true));

        output.ShouldContain("Warning");
        output.ShouldContain(missingName);
    }

    // ── 24.3 — file missing + showWarnIfMissing=false → silent ────────────

    [Fact]
    public void LoadContext_FileMissing_ShowWarnFalse_NoOutput()
    {
        var missingName = $"definitely-missing-{Guid.NewGuid():N}";

        var (_, output) = BuildAndCapture(b =>
            b.LoadContext(missingName, showWarnIfMissing: false));

        output.ShouldNotContain("Warning");
        output.ShouldNotContain(missingName);
    }
}
