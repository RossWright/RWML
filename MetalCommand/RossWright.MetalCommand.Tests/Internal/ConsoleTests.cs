using NSubstitute;

namespace RossWright.MetalCommand.Tests.Internal;

public class ConsoleTests
{
    // -----------------------------------------------------------------------
    // IConsole.Write Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void IConsole_Write_CalledWithText_InvokesImplementation()
    {
        // Arrange
        var mockConsole = Substitute.For<Console.IConsole>();
        var console = new Console(mockConsole);
        var testText = "test message";

        // Act
        mockConsole.Write(testText);

        // Assert
        mockConsole.Received(1).Write(testText);
    }
}
