namespace RossWright.MetalCore.Tests;

public class ListLoadLogTests
{
    [Fact]
    public void Log_CapturesEntriesInOrder()
    {
        var log = new ListLoadLog();
        log.LogTrace("first");
        log.LogWarning("second");
        log.LogError("third");

        log.Entries.Count.ShouldBe(3);
        log.Entries[0].Message.ShouldBe("first");
        log.Entries[0].Level.ShouldBe(LogLevel.Trace);
        log.Entries[1].Message.ShouldBe("second");
        log.Entries[1].Level.ShouldBe(LogLevel.Warning);
        log.Entries[2].Message.ShouldBe("third");
        log.Entries[2].Level.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void BeginScope_IncreasesScopeLevel()
    {
        var log = new ListLoadLog();
        log.LogTrace("root");
        using (log.BeginScope())
        {
            log.LogTrace("nested");
        }
        log.LogTrace("after");

        log.Entries[0].ScopeLevel.ShouldBe(0);
        log.Entries[1].ScopeLevel.ShouldBe(1);
        log.Entries[2].ScopeLevel.ShouldBe(0);
    }

    [Fact]
    public void BeginScope_DisposingScope_RestoresScopeLevel()
    {
        var log = new ListLoadLog();
        var scope = log.BeginScope();
        log.LogTrace("inside scope");
        scope.Dispose();
        log.LogTrace("after scope");

        log.Entries[0].ScopeLevel.ShouldBe(1);
        log.Entries[1].ScopeLevel.ShouldBe(0);
    }
}
