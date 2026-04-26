namespace RossWright.MetalCommand.Data.Tests;

public class ICommandTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsOkResult_Success()
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
    public async Task ExecuteAsync_ReturnsFailResult_FailureWithoutExit()
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
    public async Task ExecuteAsync_ReturnsFailResultWithMessage_FailureWithMessage()
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
    public async Task ExecuteAsync_ReturnsExitResult_SuccessWithExit()
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
    public async Task ExecuteAsync_ReturnsFailAndExitResult_FailureWithExit()
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
    public async Task ExecuteAsync_ReturnsFailAndExitResultWithMessage_FailureWithExitAndMessage()
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
    public async Task ExecuteAsync_WithCancelledToken_ThrowsTaskCanceledException()
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
    public async Task ExecuteAsync_CalledMultipleTimes_ReturnsSequentialResults()
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
    public async Task ExecuteAsync_WithCustomCommandResult_ReturnsCustomValues()
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
    public async Task ExecuteAsync_ReceivesCorrectParameters_VerifiesParameters()
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
    public async Task ExecuteAsync_WithImplicitBoolTrue_ReturnsSuccessResult()
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
    public async Task ExecuteAsync_WithImplicitBoolFalse_ReturnsFailureResult()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        CommandResult implicitFalse = false;
        command.ExecuteAsync(console, CancellationToken.None).Returns(implicitFalse);

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeFalse();
        result.ExitApplication.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WithNoneCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(CommandResult.Ok());

        // Act
        var result = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        await command.Received(1).ExecuteAsync(console, CancellationToken.None);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveCancellationToken_PassesTokenCorrectly()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var cts = new CancellationTokenSource();
        command.ExecuteAsync(console, cts.Token).Returns(CommandResult.Ok());

        // Act
        var result = await command.ExecuteAsync(console, cts.Token);

        // Assert
        result.Success.ShouldBeTrue();
        await command.Received(1).ExecuteAsync(console, cts.Token);
    }

    [Fact]
    public async Task ExecuteAsync_WithOperationCanceledException_PropagatesException()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var cts = new CancellationTokenSource();
        command.ExecuteAsync(console, cts.Token)
            .Returns<Task<CommandResult>>(_ => throw new OperationCanceledException(cts.Token));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await command.ExecuteAsync(console, cts.Token));
    }

    [Fact]
    public async Task ExecuteAsync_CalledSequentially_EachCallCompletes()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        command.ExecuteAsync(Arg.Any<IConsole>(), Arg.Any<CancellationToken>()).Returns(CommandResult.Ok());

        // Act
        var result1 = await command.ExecuteAsync(console, CancellationToken.None);
        var result2 = await command.ExecuteAsync(console, CancellationToken.None);
        var result3 = await command.ExecuteAsync(console, CancellationToken.None);

        // Assert
        result1.Success.ShouldBeTrue();
        result2.Success.ShouldBeTrue();
        result3.Success.ShouldBeTrue();
        await command.Received(3).ExecuteAsync(console, CancellationToken.None);
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
        result.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTask_CompletesAsynchronously()
    {
        // Arrange
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var tcs = new TaskCompletionSource<CommandResult>();
        command.ExecuteAsync(console, CancellationToken.None).Returns(tcs.Task);

        // Act
        var task = command.ExecuteAsync(console, CancellationToken.None);
        task.IsCompleted.ShouldBeFalse();

        tcs.SetResult(CommandResult.Ok());
        var result = await task;

        // Assert
        result.Success.ShouldBeTrue();
    }
}
