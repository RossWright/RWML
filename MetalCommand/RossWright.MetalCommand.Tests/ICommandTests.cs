namespace RossWright.MetalCommand.Tests;

public class ICommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidConsoleAndCancellationToken_ReturnsCommandResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var cancellationToken = CancellationToken.None;
        command.ExecuteAsync(console, cancellationToken).Returns(CommandResult.Ok());

        // Act
        var result = await command.ExecuteAsync(console, cancellationToken);

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsOkResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Ok());

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.ExitApplication.ShouldBeFalse();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Fail());

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeFalse();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailResultWithMessage()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var errorMessage = "Command failed due to invalid input";
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Fail(errorMessage));

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeFalse();
        result.Message.ShouldBe(errorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExitResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Exit());

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailAndExitResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.FailAndExit());

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailAndExitResultWithMessage()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var errorMessage = "Fatal error occurred";
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.FailAndExit(errorMessage));

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBe(errorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancelledToken_CanBeCancelled()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var cts = new CancellationTokenSource();
        cts.Cancel();
        command.ExecuteAsync(console, cts.Token).Returns(Task.FromCanceled<CommandResult>(cts.Token));

        // Act & Assert
        await Should.ThrowAsync<TaskCanceledException>(async () =>
            await command.ExecuteAsync(console, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentConsoleInstances_AcceptsAnyConsole()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console1 = Substitute.For<IConsole>();
        var console2 = Substitute.For<IConsole>();
        command.ExecuteAsync(console1, CancellationToken.None).Returns(CommandResult.Ok());
        command.ExecuteAsync(console2, CancellationToken.None).Returns(CommandResult.Fail());

        // Act
        var result1 = await command.ExecuteAsync(console1, CancellationToken.None);
        var result2 = await command.ExecuteAsync(console2, CancellationToken.None);

        // Assert
        result1.Success.ShouldBeTrue();
        result2.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_CalledMultipleTimes_CanReturnDifferentResults()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None)
            .Returns(CommandResult.Ok(), CommandResult.Fail(), CommandResult.Exit());

        // Act
        var result1 = await command.ExecuteAsync(console, CancellationToken.None);
        var result2 = await command.ExecuteAsync(console, CancellationToken.None);
        var result3 = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result1.Success.ShouldBeTrue();
        result1.ExitApplication.ShouldBeFalse();
        result2.Success.ShouldBeFalse();
        result2.ExitApplication.ShouldBeFalse();
        result3.Success.ShouldBeTrue();
        result3.ExitApplication.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsException_PropagatesException()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var exception = new InvalidOperationException("Command execution failed");
        command.ExecuteAsync(console, CancellationToken.None).Returns<Task<CommandResult>>(_ => throw exception);

        // Act & Assert
        var thrown = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await command.ExecuteAsync(console, CancellationToken.None));
        thrown.Message.ShouldBe("Command execution failed");
    }

    [Fact]
    public async Task ExecuteAsync_WithCustomCommandResult_ReturnsCustomResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var customResult = new CommandResult { Success = true, ExitApplication = true, Message = "Custom" };
        command.ExecuteAsync(console, CancellationToken.None).Returns(customResult);

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.ExitApplication.ShouldBeTrue();
        result.Message.ShouldBe("Custom");
    }

    [Fact]
    public async Task ExecuteAsync_ReceivesCorrectParameters()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var cancellationToken = new CancellationToken();
        command.ExecuteAsync(console, cancellationToken).Returns(CommandResult.Ok());

        // Act
        await command.ExecuteAsync(console, cancellationToken);

        // Assert
        await command.Received(1).ExecuteAsync(console, cancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_WithImplicitBoolResult_ReturnsExpectedResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        CommandResult implicitTrue = true;
        command.ExecuteAsync(console, CancellationToken.None).Returns(implicitTrue);

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        result.ExitApplication.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTaskType_ConfirmsAsyncContract()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Ok());

        // Act
        var task = command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        var result = await task;
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithDefaultCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, default).Returns(CommandResult.Ok());

        // Act
        var result = await command.ExecuteAsync(console, default);

        // Assert
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithNewCancellationToken_ExecutesSuccessfully()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var token = new CancellationToken();
        command.ExecuteAsync(console, token).Returns(CommandResult.Ok());

        // Act
        var result = await command.ExecuteAsync(console, token);

        // Assert
        result.Success.ShouldBeTrue();
        await command.Received(1).ExecuteAsync(console, token);
    }

    [Fact]
    public async Task ExecuteAsync_ConsoleParameter_IsUsedInExecution()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(Arg.Any<IConsole>(), Arg.Any<CancellationToken>()).Returns(CommandResult.Ok());

        // Act
        await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        await command.Received(1).ExecuteAsync(Arg.Is<IConsole>(c => c == console), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CancellationTokenParameter_IsUsedInExecution()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var cts = new CancellationTokenSource();
        command.ExecuteAsync(Arg.Any<IConsole>(), Arg.Any<CancellationToken>()).Returns(CommandResult.Ok());

        // Act
        await command.ExecuteAsync(console, cts.Token);

        // Assert
        await command.Received(1).ExecuteAsync(Arg.Any<IConsole>(), Arg.Is<CancellationToken>(t => t == cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyMessage_ReturnsResultWithEmptyMessage()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Fail(string.Empty));

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.Message.ShouldNotBeNull();
        result.Message.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_MultipleMockedCommands_OperateIndependently()
    {
        // Arrange
        var command1 = Substitute.For<ICommand>();
        var command2 = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command1.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Ok());
        command2.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Fail());

        // Act
        var result1 = await command1.ExecuteAsync(console, CancellationToken.None);
        var result2 = await command2.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result1.Success.ShouldBeTrue();
        result2.Success.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsCompletedTask_WhenResultIsImmediate()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(Task.FromResult(CommandResult.Ok()));

        // Act
        var task = command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        task.IsCompleted.ShouldBeTrue();
        var result = await task;
        result.Success.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WithLongRunningOperation_CompletesAsynchronously()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(async callInfo =>
        {
            await Task.Delay(1);
            return CommandResult.Ok();
        });

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
    }
}
