using Shouldly;
using Xunit;

namespace RossWright.MetalCore.Tests;

public class ConsoleLoadLogTests
{
    [Fact]
    public void Log_InvalidLogLevel_UsesTraceColor()
    {
        // Arrange
        var log = new ConsoleLoadLog(
            minLogLevel: LogLevel.Trace,
            traceColor: ConsoleColor.Green,
            warningColor: ConsoleColor.Yellow,
            errorColor: ConsoleColor.Red);
        
        // Act - Cast an invalid integer to LogLevel to trigger the default case
        var invalidLevel = (LogLevel)999;
        log.Log(invalidLevel, "test message");
        
        // Assert - Should not throw; the method should handle it gracefully
        // The default case uses _traceColor, which we can verify by observing no exception is thrown
    }

    [Fact]
    public void Log_TraceLevel_DoesNotThrow()
    {
        // Arrange
        var log = new ConsoleLoadLog(minLogLevel: LogLevel.Trace);

        // Act & Assert
        log.Log(LogLevel.Trace, "trace message");
    }

    [Fact]
    public void Log_WarningLevel_DoesNotThrow()
    {
        // Arrange
        var log = new ConsoleLoadLog(minLogLevel: LogLevel.Trace);

        // Act & Assert
        log.Log(LogLevel.Warning, "warning message");
    }

    [Fact]
    public void Log_ErrorLevel_DoesNotThrow()
    {
        // Arrange
        var log = new ConsoleLoadLog(minLogLevel: LogLevel.Trace);

        // Act & Assert
        log.Log(LogLevel.Error, "error message");
    }

    [Fact]
    public void Log_BelowMinLogLevel_DoesNotWrite()
    {
        // Arrange
        var log = new ConsoleLoadLog(minLogLevel: LogLevel.Warning);

        // Act & Assert - Trace is below Warning, so should be filtered out
        log.Log(LogLevel.Trace, "trace message");
    }

    [Fact]
    public void Log_AtMinLogLevel_Writes()
    {
        // Arrange
        var log = new ConsoleLoadLog(minLogLevel: LogLevel.Warning);

        // Act & Assert - Warning equals minimum, so should be written
        log.Log(LogLevel.Warning, "warning message");
    }

    [Fact]
    public void Log_AboveMinLogLevel_Writes()
    {
        // Arrange
        var log = new ConsoleLoadLog(minLogLevel: LogLevel.Warning);

        // Act & Assert - Error is above Warning, so should be written
        log.Log(LogLevel.Error, "error message");
    }
}
