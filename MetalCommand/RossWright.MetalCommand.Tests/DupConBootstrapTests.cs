using System.Text;
using System.Text.Json;

namespace RossWright.MetalCommand.Tests;

[Collection("FileSystem")]
public class DupConBootstrapTests
{
    [Fact]
    public void TryExtract_EmptyArgs_ReturnsAllEmpty()
    {
        var (cleanArgs, context, initialCommand) = DupConBootstrap.TryExtract([]);

        cleanArgs.ShouldBeEmpty();
        context.ShouldBeEmpty();
        initialCommand.ShouldBeNull();
    }

    [Fact]
    public void TryExtract_LegacyMode_WholeStringIsCommand()
    {
        var (cleanArgs, context, initialCommand) = DupConBootstrap.TryExtract(["nuke", "prod"]);

        initialCommand.ShouldBe("nuke prod");
        context.ShouldBeEmpty();
        cleanArgs.ShouldBeEmpty();
    }

    [Fact]
    public void TryExtract_CtxBlob_PopulatesContext()
    {
        var dict = new Dictionary<string, string> { ["env"] = "staging", ["region"] = "us-east" };
        var blob = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(dict));

        var (_, context, _) = DupConBootstrap.TryExtract(["--ctx", blob]);

        context["env"].ShouldBe("staging");
        context["region"].ShouldBe("us-east");
    }

    [Fact]
    public void TryExtract_CtxFile_PopulatesContext()
    {
        var dict = new Dictionary<string, string> { ["key"] = "value" };
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(dict));

            var (_, context, _) = DupConBootstrap.TryExtract(["--ctx", path]);

            context["key"].ShouldBe("value");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryExtract_CtxShortName_ResolvesFile()
    {
        var dict = new Dictionary<string, string> { ["tier"] = "staging" };
        var cwd = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(cwd, "staging.mcc.json");
        try
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(dict));

            var (_, context, _) = DupConBootstrap.TryExtract(["--ctx", "staging"]);

            context["tier"].ShouldBe("staging");
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void TryExtract_CtxUnresolvable_ReturnsEmptyContext()
    {
        var (_, context, _) = DupConBootstrap.TryExtract(["--ctx", "!!this_cannot_resolve!!"]);

        context.ShouldBeEmpty();
    }

    [Fact]
    public void TryExtract_CmdFlag_ExtractsInitialCommand()
    {
        var (_, _, initialCommand) = DupConBootstrap.TryExtract(["--cmd", "nuke prod"]);

        initialCommand.ShouldBe("nuke prod");
    }

    [Fact]
    public void TryExtract_BothFlags_ExtractsBoth()
    {
        var dict = new Dictionary<string, string> { ["x"] = "1" };
        var blob = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(dict));

        var (_, context, initialCommand) = DupConBootstrap.TryExtract(["--ctx", blob, "--cmd", "ping"]);

        context["x"].ShouldBe("1");
        initialCommand.ShouldBe("ping");
    }

    [Fact]
    public void TryExtract_StripsBothFlagsFromCleanArgs()
    {
        var blob = Convert.ToBase64String(Encoding.UTF8.GetBytes("{}"));

        var (cleanArgs, _, _) = DupConBootstrap.TryExtract(["--ctx", blob, "--cmd", "ping"]);

        cleanArgs.ShouldBeEmpty();
    }

    [Fact]
    public void TryExtract_UnknownFlags_LeftInCleanArgs()
    {
        var (cleanArgs, _, _) = DupConBootstrap.TryExtract(["--env", "prod"]);

        cleanArgs.ShouldBe(["--env", "prod"]);
    }

    [Fact]
    public void TryExtract_UnknownFlagAlongsideCtx_PreservesUnknownFlagAndPopulatesContext()
    {
        var dict = new Dictionary<string, string> { ["env"] = "staging" };
        var blob = Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(dict));

        var (cleanArgs, context, _) = DupConBootstrap.TryExtract(["--verbose", "--ctx", blob]);

        cleanArgs.ShouldBe(["--verbose"]);
        context["env"].ShouldBe("staging");
    }

    [Fact]
    public void TryExtract_CtxFileWithInvalidJson_ReturnsEmptyContext()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, "not valid json {{{{");

            var (_, context, _) = DupConBootstrap.TryExtract(["--ctx", path]);

            context.ShouldBeEmpty();
        }
        finally
        {
            File.Delete(path);
        }
    }
}
