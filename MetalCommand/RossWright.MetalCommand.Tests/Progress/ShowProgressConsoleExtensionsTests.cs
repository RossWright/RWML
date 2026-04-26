namespace RossWright.MetalCommand.Tests;

public class ShowProgressConsoleExtensionsTests
{
    [Fact]
    public void ShowProgress_SyncGenericOverload_CallsAsyncVersionAndReturnsResult()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };
        var expectedResult = 42;

        // Act
        var result = console.ShowProgress<int>(_ => expectedResult, indicators);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void ShowProgress_SyncGenericOverload_WithNullIndicators_UsesDefaultIndicators()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var expectedResult = "result";

        // Act
        var result = console.ShowProgress<string>(_ => expectedResult, null);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public void ShowProgress_SyncGenericOverload_CallsBodyWithProgressAction()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };
        Action<double>? capturedProgress = null;

        // Act
        console.ShowProgress<int>(progress =>
        {
            capturedProgress = progress;
            return 123;
        }, indicators);

        // Assert
        capturedProgress.ShouldNotBeNull();
    }

    [Fact]
    public void ShowProgress_SyncGenericOverload_ReturnsResultFromBody()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };

        // Act
        var result = console.ShowProgress<double>(_ => 3.14, indicators);

        // Assert
        result.ShouldBe(3.14);
    }

    [Fact]
    public async Task ShowProgress_AsyncNonGenericOverload_CompletesSuccessfully()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };
        var wasCalled = false;

        // Act
        await console.ShowProgress(async _ =>
        {
            wasCalled = true;
            await Task.CompletedTask;
        }, indicators);

        // Assert
        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShowProgress_AsyncNonGenericOverload_WithNullIndicators_UsesDefaultIndicators()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var wasCalled = false;

        // Act
        await console.ShowProgress(async _ =>
        {
            wasCalled = true;
            await Task.CompletedTask;
        }, null);

        // Assert
        wasCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task ShowProgress_AsyncNonGenericOverload_CallsBodyWithProgressAction()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };
        Action<double>? capturedProgress = null;

        // Act
        await console.ShowProgress(async progress =>
        {
            capturedProgress = progress;
            await Task.CompletedTask;
        }, indicators);

        // Assert
        capturedProgress.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShowProgress_AsyncNonGenericOverload_HandlesAsyncOperation()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };
        var counter = 0;

        // Act
        await console.ShowProgress(async _ =>
        {
            await Task.Delay(10);
            counter++;
        }, indicators);

        // Assert
        counter.ShouldBe(1);
    }

    [Fact]
    public async Task ShowProgress_AsyncNonGenericOverload_AllowsProgressUpdates()
    {
        // Arrange
        var console = Substitute.For<IConsole>();
        var disposable = Substitute.For<IDisposable>();
        console.HideCursor().Returns(disposable);
        var indicator = Substitute.For<IProgressIndicator>();
        indicator.Width.Returns(5);
        indicator.Output(Arg.Any<double>()).Returns("test");
        var indicators = new[] { indicator };

        // Act
        await console.ShowProgress(async progress =>
        {
            progress(0.5);
            await Task.Delay(10);
            progress(1.0);
        }, indicators);

        // Assert
        indicator.Received(1).Output(0.5);
        indicator.Received(1).Output(1.0);
    }
}
