using RossWright.MetalCommand.Http.Commands;

namespace RossWright.MetalCommand.Http.Tests.Commands;

/// <summary>
/// Tests for AddPingCommand extension method.
/// </summary>
public class AddPingCommandExtensionTests
{
    [Fact]
    public void AddPingCommand_ValidBuilder_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddPingCommand();

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddPingCommand_ValidBuilder_AddsPingCommandToCommands()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var commands = Substitute.For<ICommandCollection>();
        builder.Commands.Returns(commands);

        // Act
        builder.AddPingCommand();

        // Assert
        commands.Received(1).Add(typeof(PingCommand));
    }
}
