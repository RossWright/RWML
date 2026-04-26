using NSubstitute;

namespace RossWright.MetalCommand.Tests;

/// <summary>
/// Tests for <see cref="ICommandMiddleware.InvokeAsync"/> covering basic invocation,
/// short-circuiting, exception handling, and async behavior.
/// </summary>
public class ICommandMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_CallsNext_PipelineContinues()
    {
        // Arrange
        var context = CreateContext();
        var nextCalled = false;
        Task Next(CommandContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var middleware = new BasicMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShortCircuits_NextNotCalled()
    {
        // Arrange
        var context = CreateContext();
        var nextCalled = false;
        Task Next(CommandContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var middleware = new ShortCircuitMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        nextCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task InvokeAsync_NextThrowsException_ExceptionPropagates()
    {
        // Arrange
        var context = CreateContext();
        var expectedException = new InvalidOperationException("Test exception");
        Task Next(CommandContext ctx) => throw expectedException;

        var middleware = new BasicMiddleware();

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await middleware.InvokeAsync(context, Next));
        exception.ShouldBe(expectedException);
    }

    [Fact]
    public async Task InvokeAsync_PassesCorrectContext_ContextReceivedByNext()
    {
        // Arrange
        var context = CreateContext();
        CommandContext? receivedContext = null;
        Task Next(CommandContext ctx)
        {
            receivedContext = ctx;
            return Task.CompletedTask;
        }

        var middleware = new BasicMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        receivedContext.ShouldBe(context);
    }

    [Fact]
    public async Task InvokeAsync_ExecutesBeforeAndAfter_OrderMaintained()
    {
        // Arrange
        var context = CreateContext();
        var executionOrder = new List<string>();
        Task Next(CommandContext ctx)
        {
            executionOrder.Add("Next");
            return Task.CompletedTask;
        }

        var middleware = new OrderTrackingMiddleware(executionOrder);

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        executionOrder.ShouldBe(new[] { "Before", "Next", "After" });
    }

    [Fact]
    public async Task InvokeAsync_ModifiesContextBeforeNext_ChangesVisible()
    {
        // Arrange
        var context = CreateContext();
        string? capturedValue = null;
        Task Next(CommandContext ctx)
        {
            capturedValue = ctx.SessionContext.GetValueOrDefault("key");
            return Task.CompletedTask;
        }

        var middleware = new ContextModifyingMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        capturedValue.ShouldBe("modified");
    }

    [Fact]
    public async Task InvokeAsync_ModifiesContextAfterNext_ChangesPreserved()
    {
        // Arrange
        var context = CreateContext();
        Task Next(CommandContext ctx)
        {
            ctx.SessionContext["inner"] = "innerValue";
            return Task.CompletedTask;
        }

        var middleware = new PostModifyingMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        context.SessionContext["inner"].ShouldBe("innerValue");
        context.SessionContext["outer"].ShouldBe("outerValue");
    }

    private class BasicMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            await next(context);
        }
    }

    private class ShortCircuitMiddleware : ICommandMiddleware
    {
        public Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            // Don't call next - short circuit
            return Task.CompletedTask;
        }
    }

    private class OrderTrackingMiddleware : ICommandMiddleware
    {
        private readonly List<string> _executionOrder;

        public OrderTrackingMiddleware(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            _executionOrder.Add("Before");
            await next(context);
            _executionOrder.Add("After");
        }
    }

    private class ContextModifyingMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            context.SessionContext["key"] = "modified";
            await next(context);
        }
    }

    private class PostModifyingMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            await next(context);
            context.SessionContext["outer"] = "outerValue";
        }
    }

    private static CommandContext CreateContext()
    {
        var command = Substitute.For<ICommand>();
        var console = Substitute.For<IConsole>();
        var sessionContext = new Dictionary<string, string>();

        return new CommandContext
        {
            Command = command,
            Console = console,
            SessionContext = sessionContext,
            CancellationToken = CancellationToken.None
        };
    }
}
