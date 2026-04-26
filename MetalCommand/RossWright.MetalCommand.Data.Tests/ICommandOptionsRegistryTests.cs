using NSubstitute;
using RossWright.MetalCommand;
using Shouldly;
using Xunit;

namespace RossWright.MetalCommand.Data.Tests;

public class ICommandOptionsRegistryTests
{
    [Fact]
    public void Get_WithRegisteredCommandType_ReturnsCommandOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            Invocations = ["test", "t"],
            EnvironmentPolicy = EnvironmentPolicy.Forbidden
        };
        registry.Get(commandType).Returns(expectedOptions);

        // Act
        var result = registry.Get(commandType);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBe(expectedOptions);
    }

    [Fact]
    public void Get_WithUnregisteredCommandType_ReturnsNull()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        registry.Get(commandType).Returns((CommandOptions?)null);

        // Act
        var result = registry.Get(commandType);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Get_WithDifferentCommandTypes_ReturnsDifferentOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType1 = typeof(TestCommand);
        var commandType2 = typeof(AnotherTestCommand);
        var options1 = new CommandOptions { Invocations = ["cmd1"] };
        var options2 = new CommandOptions { Invocations = ["cmd2"] };
        registry.Get(commandType1).Returns(options1);
        registry.Get(commandType2).Returns(options2);

        // Act
        var result1 = registry.Get(commandType1);
        var result2 = registry.Get(commandType2);

        // Assert
        result1.ShouldBe(options1);
        result2.ShouldBe(options2);
    }

    [Fact]
    public void Get_WithCommandOptionsOnlyInvocations_ReturnsOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            Invocations = ["invoke", "i"]
        };
        registry.Get(commandType).Returns(expectedOptions);

        // Act
        var result = registry.Get(commandType);

        // Assert
        result.ShouldNotBeNull();
        result.Invocations.ShouldBe(["invoke", "i"]);
        result.EnvironmentPolicy.ShouldBeNull();
    }

    [Fact]
    public void Get_WithCommandOptionsOnlyEnvironmentPolicy_ReturnsOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            EnvironmentPolicy = EnvironmentPolicy.Dangerous
        };
        registry.Get(commandType).Returns(expectedOptions);

        // Act
        var result = registry.Get(commandType);

        // Assert
        result.ShouldNotBeNull();
        result.Invocations.ShouldBeNull();
        result.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Dangerous);
    }

    [Fact]
    public void Get_WithEmptyCommandOptions_ReturnsOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions();
        registry.Get(commandType).Returns(expectedOptions);

        // Act
        var result = registry.Get(commandType);

        // Assert
        result.ShouldNotBeNull();
        result.Invocations.ShouldBeNull();
        result.EnvironmentPolicy.ShouldBeNull();
    }

    [Fact]
    public void Get_WithAllEnvironmentPolicyValues_ReturnsCorrectOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var benignOptions = new CommandOptions { EnvironmentPolicy = EnvironmentPolicy.Benign };
        var dangerousOptions = new CommandOptions { EnvironmentPolicy = EnvironmentPolicy.Dangerous };
        var forbiddenOptions = new CommandOptions { EnvironmentPolicy = EnvironmentPolicy.Forbidden };
        registry.Get(typeof(TestCommand)).Returns(benignOptions);
        registry.Get(typeof(AnotherTestCommand)).Returns(dangerousOptions);
        registry.Get(typeof(YetAnotherTestCommand)).Returns(forbiddenOptions);

        // Act
        var benignResult = registry.Get(typeof(TestCommand));
        var dangerousResult = registry.Get(typeof(AnotherTestCommand));
        var forbiddenResult = registry.Get(typeof(YetAnotherTestCommand));

        // Assert
        benignResult.ShouldNotBeNull();
        benignResult.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Benign);
        dangerousResult.ShouldNotBeNull();
        dangerousResult.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Dangerous);
        forbiddenResult.ShouldNotBeNull();
        forbiddenResult.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Forbidden);
    }

    [Fact]
    public void Get_CalledMultipleTimes_ReturnsSameOptions()
    {
        // Arrange
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions { Invocations = ["test"] };
        registry.Get(commandType).Returns(expectedOptions);

        // Act
        var result1 = registry.Get(commandType);
        var result2 = registry.Get(commandType);

        // Assert
        result1.ShouldBe(expectedOptions);
        result2.ShouldBe(expectedOptions);
    }

    private class TestCommand
    {
    }

    private class AnotherTestCommand
    {
    }

    private class YetAnotherTestCommand
    {
    }
}
