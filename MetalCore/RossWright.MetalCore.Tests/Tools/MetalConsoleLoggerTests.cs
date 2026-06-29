using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace RossWright.MetalCore.Tests.Tools;

public class MetalConsoleLoggerTests
{
    [Fact]
    public void IsEnabled_LogLevelNone_ReturnsFalse()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Trace);
        var logger = provider.CreateLogger("test");

        logger.IsEnabled(LogLevel.None).ShouldBeFalse();
    }

    [Fact]
    public void IsEnabled_LevelBelowMinimum_ReturnsFalse()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Warning);
        var logger = provider.CreateLogger("test");

        logger.IsEnabled(LogLevel.Trace).ShouldBeFalse();
    }

    [Fact]
    public void IsEnabled_LevelAtMinimum_ReturnsTrue()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Warning);
        var logger = provider.CreateLogger("test");

        logger.IsEnabled(LogLevel.Warning).ShouldBeTrue();
    }

    [Fact]
    public void IsEnabled_LevelAboveMinimum_ReturnsTrue()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Warning);
        var logger = provider.CreateLogger("test");

        logger.IsEnabled(LogLevel.Error).ShouldBeTrue();
    }

    [Fact]
    public void Log_BelowMinLevel_DoesNotThrow()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Warning);
        var logger = provider.CreateLogger("test");

        logger.Log(LogLevel.Trace, "filtered message");
    }

    [Fact]
    public void Log_AtMinLevel_DoesNotThrow()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Trace);
        var logger = provider.CreateLogger("test");

        logger.Log(LogLevel.Trace, "trace message");
    }

    [Fact]
    public void Log_WithException_DoesNotThrow()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Trace);
        var logger = provider.CreateLogger("test");

        logger.Log(LogLevel.Error, new EventId(), "state", new InvalidOperationException("boom"), (s, _) => s);
    }

    [Fact]
    public void Log_InvalidLogLevel_DoesNotThrow()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Trace);
        var logger = provider.CreateLogger("test");

        logger.Log((LogLevel)999, "unknown level");
    }

    [Fact]
    public void BeginScope_IncrementsAndDecrements_DoesNotThrow()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Trace);
        var logger = provider.CreateLogger("test");

        using var scope = logger.BeginScope("my scope");
        scope.ShouldNotBeNull();
        logger.Log(LogLevel.Trace, "inside scope");
    }

    [Fact]
    public void Provider_SameCategory_ReturnsSameInstance()
    {
        var provider = new MetalConsoleLoggerProvider(LogLevel.Trace);

        var a = provider.CreateLogger("cat");
        var b = provider.CreateLogger("cat");

        a.ShouldBeSameAs(b);
    }

    [Fact]
    public void AddMetalConsoleLogger_RegistersProvider_DoesNotThrow()
    {
        var factory = LoggerFactory.Create(b => b.AddMetalConsoleLogger(LogLevel.Warning));
        var logger = factory.CreateLogger("test");

        logger.ShouldNotBeNull();
        logger.IsEnabled(LogLevel.Warning).ShouldBeTrue();
    }
}
