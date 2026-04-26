using NSubstitute;

namespace RossWright;

public class UsesLoggerOptionsBuilderTests
{
    [Fact]
    public void UseLogger_SetsLoadLog()
    {
        // Arrange
        var builder = new UsesLoggerOptionsBuilder("test");
        var loadLog = Substitute.For<ILoadLog>();

        // Act
        builder.UseLogger(loadLog);

        // Assert
        builder.LoadLog.ShouldBe(loadLog);
    }

    [Fact]
    public void UseLogger_WithNull_SetsLoadLogToNull()
    {
        // Arrange
        var builder = new UsesLoggerOptionsBuilder("test");
        var loadLog = Substitute.For<ILoadLog>();
        builder.UseLogger(loadLog);

        // Act
        builder.UseLogger(null);

        // Assert
        builder.LoadLog.ShouldBeNull();
    }

    [Fact]
    public void UseLogger_WithNonNullLoadLog_LoadLogPropertySetsModuleName()
    {
        // Arrange
        var moduleName = "TestModule";
        var builder = new UsesLoggerOptionsBuilder(moduleName);
        var loadLog = Substitute.For<ILoadLog>();

        // Act
        builder.UseLogger(loadLog);
        var result = builder.LoadLog;

        // Assert
        result.ShouldBe(loadLog);
        loadLog.Received(1).ModuleName = moduleName;
    }

    [Fact]
    public void DoNotUseLogger_SetsLoadLogToNull()
    {
        // Arrange
        var builder = new UsesLoggerOptionsBuilder("test");
        var loadLog = Substitute.For<ILoadLog>();
        builder.UseLogger(loadLog);

        // Act
        builder.DoNotUseLogger();

        // Assert
        builder.LoadLog.ShouldBeNull();
    }

    [Fact]
    public void DoNotUseLogger_CalledDirectly_SetsLoadLogToNull()
    {
        // Arrange
        IUsesLoggerOptionsBuilder builder = new UsesLoggerOptionsBuilder("test");
        var loadLog = Substitute.For<ILoadLog>();
        builder.UseLogger(loadLog);

        // Act
        IUsesLoggerOptionsBuilderExtensions.DoNotUseLogger(builder);

        // Assert
        builder.LoadLog.ShouldBeNull();
    }
}
