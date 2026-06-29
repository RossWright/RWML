using Microsoft.Extensions.Logging;
using NSubstitute;

namespace RossWright;

public class UsesLoggerOptionsBuilderTests
{
    [Fact]
    public void UseLogger_SetsLoggerFactory()
    {
        // Arrange
        var builder = new UsesBootstrapLoggerOptionsBuilder("test");
        var loggerFactory = Substitute.For<ILoggerFactory>();

        // Act
        builder.UseBootstrapLogger(loggerFactory);

        // Assert
        builder.GetBootstrapLogger();
        loggerFactory.Received(1).CreateLogger("test");
    }

    [Fact]
    public void UseLogger_WithNull_ReturnsNullBootstrapLogger()
    {
        // Arrange
        var builder = new UsesBootstrapLoggerOptionsBuilder("test");
        var loggerFactory = Substitute.For<ILoggerFactory>();
        builder.UseBootstrapLogger(loggerFactory);

        // Act
        builder.UseBootstrapLogger(null);

        // Assert
        builder.GetBootstrapLogger().ShouldBeNull();
    }

    [Fact]
    public void GetBootstrapLogger_UsesCategory()
    {
        // Arrange
        var moduleName = "TestModule";
        var builder = new UsesBootstrapLoggerOptionsBuilder(moduleName);
        var loggerFactory = Substitute.For<ILoggerFactory>();

        // Act
        builder.UseBootstrapLogger(loggerFactory);
        builder.GetBootstrapLogger();

        // Assert
        loggerFactory.Received(1).CreateLogger(moduleName);
    }

    [Fact]
    public void DoNotUseLogger_ReturnsNullBootstrapLogger()
    {
        // Arrange
        var builder = new UsesBootstrapLoggerOptionsBuilder("test");
        var loggerFactory = Substitute.For<ILoggerFactory>();
        builder.UseBootstrapLogger(loggerFactory);

        // Act
        builder.DoNotUseLogger();

        // Assert
        builder.GetBootstrapLogger().ShouldBeNull();
    }

    [Fact]
    public void DoNotUseLogger_CalledDirectly_ReturnsNullBootstrapLogger()
    {
        // Arrange
        IUsesBootstrapLoggerOptionsBuilder builder = new UsesBootstrapLoggerOptionsBuilder("test");
        var loggerFactory = Substitute.For<ILoggerFactory>();
        builder.UseBootstrapLogger(loggerFactory);

        // Act
        IUsesBootstrapLoggerOptionsBuilderExtensions.DoNotUseLogger(builder);

        // Assert
        builder.GetBootstrapLogger().ShouldBeNull();
    }
}
