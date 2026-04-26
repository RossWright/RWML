using NSubstitute;

namespace RossWright.MetalCommand.Tests;

public class ICommandOptionsRegistryTests
{
    [Fact]
    public void Get_RegisteredCommandType_ReturnsCommandOptions()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            Invocations = ["test", "t"],
            EnvironmentPolicy = EnvironmentPolicy.Dangerous
        };
        registry.Get(commandType).Returns(expectedOptions);

        var result = registry.Get(commandType);

        result.ShouldNotBeNull();
        result.ShouldBe(expectedOptions);
    }

    [Fact]
    public void Get_UnregisteredCommandType_ReturnsNull()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        registry.Get(commandType).Returns((CommandOptions?)null);

        var result = registry.Get(commandType);

        result.ShouldBeNull();
    }

    [Fact]
    public void Get_DifferentCommandTypes_ReturnsDifferentOptions()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType1 = typeof(TestCommand);
        var commandType2 = typeof(AnotherTestCommand);
        var options1 = new CommandOptions { Invocations = ["test1"] };
        var options2 = new CommandOptions { Invocations = ["test2"] };
        registry.Get(commandType1).Returns(options1);
        registry.Get(commandType2).Returns(options2);

        var result1 = registry.Get(commandType1);
        var result2 = registry.Get(commandType2);

        result1.ShouldBe(options1);
        result2.ShouldBe(options2);
        result1.ShouldNotBe(result2);
    }

    [Fact]
    public void Get_SameCommandTypeCalledTwice_ReturnsSameOptions()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions { Invocations = ["test"] };
        registry.Get(commandType).Returns(expectedOptions);

        var result1 = registry.Get(commandType);
        var result2 = registry.Get(commandType);

        result1.ShouldBe(expectedOptions);
        result2.ShouldBe(expectedOptions);
        result1.ShouldBe(result2);
    }

    [Fact]
    public void Get_WithInvocationsOnly_ReturnsOptionsWithInvocations()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            Invocations = ["cmd", "command", "c"]
        };
        registry.Get(commandType).Returns(expectedOptions);

        var result = registry.Get(commandType);

        result.ShouldNotBeNull();
        result.Invocations.ShouldNotBeNull();
        result.Invocations.Length.ShouldBe(3);
        result.Invocations[0].ShouldBe("cmd");
        result.EnvironmentPolicy.ShouldBeNull();
    }

    [Fact]
    public void Get_WithEnvironmentPolicyOnly_ReturnsOptionsWithEnvironmentPolicy()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            EnvironmentPolicy = EnvironmentPolicy.Benign
        };
        registry.Get(commandType).Returns(expectedOptions);

        var result = registry.Get(commandType);

        result.ShouldNotBeNull();
        result.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Benign);
        result.Invocations.ShouldBeNull();
    }

    [Fact]
    public void Get_WithBothInvocationsAndEnvironmentPolicy_ReturnsCompleteOptions()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            Invocations = ["full"],
            EnvironmentPolicy = EnvironmentPolicy.Forbidden
        };
        registry.Get(commandType).Returns(expectedOptions);

        var result = registry.Get(commandType);

        result.ShouldNotBeNull();
        result.Invocations.ShouldNotBeNull();
        result.Invocations[0].ShouldBe("full");
        result.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Forbidden);
    }

    [Fact]
    public void Get_WithEmptyInvocations_ReturnsOptionsWithEmptyArray()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        var commandType = typeof(TestCommand);
        var expectedOptions = new CommandOptions
        {
            Invocations = []
        };
        registry.Get(commandType).Returns(expectedOptions);

        var result = registry.Get(commandType);

        result.ShouldNotBeNull();
        result.Invocations.ShouldNotBeNull();
        result.Invocations.Length.ShouldBe(0);
    }

    [Fact]
    public void Get_NullCommandType_ReturnsNull()
    {
        var registry = Substitute.For<ICommandOptionsRegistry>();
        registry.Get(null!).Returns((CommandOptions?)null);

        var result = registry.Get(null!);

        result.ShouldBeNull();
    }

    private class TestCommand : ICommand
    {
        public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken) =>
            Task.FromResult(CommandResult.Ok());
    }

    private class AnotherTestCommand : ICommand
    {
        public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken) =>
            Task.FromResult(CommandResult.Ok());
    }
}
