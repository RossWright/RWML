using NSubstitute;
using Shouldly;
using Xunit;

namespace RossWright.MetalCore.Tests;

public class ThrowExceptionOnLogErrorTests
{
    [Fact]
    public void ModuleName_GetWithNoInnerLog_ReturnsNull()
    {
        // Arrange
        var throwOnLogError = new ThrowExceptionOnLogError();

        // Act
        var result = throwOnLogError.ModuleName;

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void ModuleName_GetWithInnerLog_ReturnsInnerLogModuleName()
    {
        // Arrange
        var innerLog = Substitute.For<ILoadLog>();
        innerLog.ModuleName.Returns("TestModule");
        var throwOnLogError = new ThrowExceptionOnLogError(innerLog);

        // Act
        var result = throwOnLogError.ModuleName;

        // Assert
        result.ShouldBe("TestModule");
    }

    [Fact]
    public void ModuleName_SetWithNoInnerLog_DoesNotThrow()
    {
        // Arrange
        var throwOnLogError = new ThrowExceptionOnLogError();

        // Act
        throwOnLogError.ModuleName = "TestModule";

        // Assert - no exception thrown
        throwOnLogError.ModuleName.ShouldBeNull();
    }

    [Fact]
    public void ModuleName_SetWithInnerLog_SetsInnerLogModuleName()
    {
        // Arrange
        var innerLog = Substitute.For<ILoadLog>();
        var throwOnLogError = new ThrowExceptionOnLogError(innerLog);

        // Act
        throwOnLogError.ModuleName = "TestModule";

        // Assert
        innerLog.Received(1).ModuleName = "TestModule";
    }
}
