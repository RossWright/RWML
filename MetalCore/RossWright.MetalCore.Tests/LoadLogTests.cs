namespace RossWright;

public class ConsoleLoadLogTests
{
    [Fact] public void LogTrace_DoesNotThrow()
    {
        var log = new ConsoleLoadLog();
        log.LogTrace("trace message");
    }

    [Fact] public void LogWarning_DoesNotThrow()
    {
        var log = new ConsoleLoadLog();
        log.LogWarning("warning message");
    }

    [Fact] public void LogError_DoesNotThrow()
    {
        var log = new ConsoleLoadLog();
        log.LogError("error message");
    }

    [Fact] public void BeginScope_ReturnsDisposable()
    {
        var log = new ConsoleLoadLog();
        var scope = log.BeginScope();
        scope.ShouldNotBeNull();
        scope.Dispose();
    }

    [Fact] public void BeginScope_NestedScopes_DoNotThrow()
    {
        var log = new ConsoleLoadLog();
        using var outer = log.BeginScope();
        using var inner = log.BeginScope();
        log.LogTrace("nested message");
    }
}

public class ListLoadLogTests
{
    [Fact] public void Log_CapturesEntriesInOrder()
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

    [Fact] public void BeginScope_IncreasesScopeLevel()
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

    [Fact] public void BeginScope_DisposingScope_RestoresScopeLevel()
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

public class ThrowExceptionOnLogErrorTests
{
    [Fact] public void Log_Error_ThrowsMetalCoreException()
    {
        var log = new ThrowExceptionOnLogError();
        Should.Throw<MetalCoreException>(() => log.LogError("something broke"));
    }

    [Fact] public void Log_Warning_WithThrowOnWarn_ThrowsMetalCoreException()
    {
        var log = new ThrowExceptionOnLogError(throwOnWarn: true);
        Should.Throw<MetalCoreException>(() => log.LogWarning("warning message"));
    }

    [Fact] public void Log_Warning_WithoutThrowOnWarn_ForwardsToInner()
    {
        var inner = new ListLoadLog();
        var log = new ThrowExceptionOnLogError(inner, throwOnWarn: false);
        log.LogWarning("warning message");
        inner.Entries.ShouldHaveSingleItem();
        inner.Entries[0].Level.ShouldBe(LogLevel.Warning);
    }

    [Fact] public void Log_Trace_PassesThroughToInner()
    {
        var inner = new ListLoadLog();
        var log = new ThrowExceptionOnLogError(inner);
        log.LogTrace("trace message");
        inner.Entries.ShouldHaveSingleItem();
        inner.Entries[0].Level.ShouldBe(LogLevel.Trace);
    }

    [Fact] public void BeginScope_WithInnerLog_DelegatesToInner()
    {
        var inner = new ListLoadLog();
        var log = new ThrowExceptionOnLogError(inner);
        var scope = log.BeginScope();
        scope.ShouldNotBeNull();
        scope.Dispose();
    }

    [Fact] public void BeginScope_WithoutInnerLog_ReturnsNoOpDisposable()
    {
        var log = new ThrowExceptionOnLogError();
        var scope = log.BeginScope();
        scope.ShouldNotBeNull();
        scope.Dispose();
    }
}
