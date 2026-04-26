using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalCommand.Internal;

namespace RossWright.MetalCommand.Tests;

public class CommandCollectionBuilderTests
{
    [Command("test")]
    private class TestCommand : ICommand
    {
        public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        {
            return Task.FromResult(CommandResult.Ok());
        }
    }

    [Command("another-test")]
    private class AnotherTestCommand : ICommand
    {
        public Task<CommandResult> ExecuteAsync(IConsole console, CancellationToken cancellationToken)
        {
            return Task.FromResult(CommandResult.Ok());
        }
    }

    private class NonCommandType
    {
    }

    [Fact]
    public void Add_Generic_AddsCommandAndReturnsThis()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();

        // Act
        var result = builder.Add<TestCommand>();

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void Add_Generic_SupportsFluentChaining()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();

        // Act
        var result = builder
            .Add<TestCommand>()
            .Add<TestCommand>();

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void Add_Generic_ActuallyAddsCommandToCollection()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IEnumerable<ICommandOptionsRegistry>))
            .Returns(Enumerable.Empty<ICommandOptionsRegistry>());

        // Act
        builder.Add<TestCommand>();
        var descriptors = builder.GetCommandDescriptors(serviceProvider).ToList();

        // Assert
        descriptors.ShouldNotBeEmpty();
        descriptors.ShouldContain(d => d.CommandType == typeof(TestCommand));
    }

    [Fact]
    public void Add_Generic_CanAddMultipleCommandTypes()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IEnumerable<ICommandOptionsRegistry>))
            .Returns(Enumerable.Empty<ICommandOptionsRegistry>());

        // Act
        builder
            .Add<TestCommand>()
            .Add<AnotherTestCommand>();
        var descriptors = builder.GetCommandDescriptors(serviceProvider).ToList();

        // Assert
        descriptors.Count.ShouldBeGreaterThanOrEqualTo(2);
        descriptors.ShouldContain(d => d.CommandType == typeof(TestCommand));
        descriptors.ShouldContain(d => d.CommandType == typeof(AnotherTestCommand));
    }

    [Fact]
    public void Add_Generic_CanAddSameCommandMultipleTimes()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IEnumerable<ICommandOptionsRegistry>))
            .Returns(Enumerable.Empty<ICommandOptionsRegistry>());

        // Act
        builder
            .Add<TestCommand>()
            .Add<TestCommand>();
        var allDescriptors = builder.GetCommandDescriptors(serviceProvider).ToList();

        // Assert - GetCommandDescriptors uses DistinctBy, so only one should appear
        var testCommandDescriptors = allDescriptors.Where(d => d.CommandType == typeof(TestCommand)).ToList();
        testCommandDescriptors.Count.ShouldBe(1);
    }

    [Fact]
    public void Add_TypeValidCommand_AddsCommandAndReturnsThis()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();
        var commandType = typeof(TestCommand);

        // Act
        var result = builder.Add(commandType);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void Add_TypeNotImplementingICommand_ThrowsArgumentException()
    {
        // Arrange
        var builder = new CommandCollectionBuilder();
        var invalidType = typeof(NonCommandType);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => builder.Add(invalidType));
        exception.Message.ShouldContain("must implement ICommand");
        exception.ParamName.ShouldBe("commandType");
    }
}
