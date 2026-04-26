namespace RossWright.MetalCommand.Data.Tests;

/// <summary>
/// Tests for <see cref="ICommandMiddleware.InvokeAsync"/> covering pipeline behavior,
/// context handling, exception propagation, and async operations.
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
    public async Task InvokeAsync_DoesNotCallNext_ShortCircuitsPipeline()
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
    public async Task InvokeAsync_ModifiesContextBeforeNext_ChangesAvailableDownstream()
    {
        // Arrange
        var context = CreateContext();
        var capturedValue = string.Empty;
        Task Next(CommandContext ctx)
        {
            capturedValue = ctx.SessionContext["key"];
            return Task.CompletedTask;
        }

        var middleware = new ModifyBeforeMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        capturedValue.ShouldBe("modified");
    }

    [Fact]
    public async Task InvokeAsync_ModifiesContextAfterNext_ChangesAvailableUpstream()
    {
        // Arrange
        var context = CreateContext();
        context.SessionContext["key"] = "initial";

        Task Next(CommandContext ctx)
        {
            ctx.SessionContext["key"] = "from-next";
            return Task.CompletedTask;
        }

        var middleware = new ModifyAfterMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        context.SessionContext["key"].ShouldBe("after-middleware");
    }

    [Fact]
    public async Task InvokeAsync_NextThrowsException_PropagatesException()
    {
        // Arrange
        var context = CreateContext();
        var exception = new InvalidOperationException("Next failed");
        Task Next(CommandContext ctx)
        {
            throw exception;
        }

        var middleware = new BasicMiddleware();

        // Act & Assert
        var thrown = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await middleware.InvokeAsync(context, Next));
        thrown.Message.ShouldBe("Next failed");
    }

    [Fact]
    public async Task InvokeAsync_MiddlewareThrowsBeforeNext_PropagatesException()
    {
        // Arrange
        var context = CreateContext();
        var nextCalled = false;
        Task Next(CommandContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var middleware = new ThrowBeforeNextMiddleware();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await middleware.InvokeAsync(context, Next));
        nextCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task InvokeAsync_MiddlewareThrowsAfterNext_NextExecutedAndExceptionPropagates()
    {
        // Arrange
        var context = CreateContext();
        var nextCalled = false;
        Task Next(CommandContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        var middleware = new ThrowAfterNextMiddleware();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await middleware.InvokeAsync(context, Next));
        nextCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PassesCorrectContextToNext_SameContextInstance()
    {
        // Arrange
        var context = CreateContext();
        CommandContext? capturedContext = null;
        Task Next(CommandContext ctx)
        {
            capturedContext = ctx;
            return Task.CompletedTask;
        }

        var middleware = new BasicMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        capturedContext.ShouldBeSameAs(context);
    }

    [Fact]
    public async Task InvokeAsync_WithAsyncNext_AwaitsCompletion()
    {
        // Arrange
        var context = CreateContext();
        var tcs = new TaskCompletionSource<bool>();
        var nextStarted = false;
        var nextCompleted = false;

        async Task Next(CommandContext ctx)
        {
            nextStarted = true;
            await tcs.Task;
            nextCompleted = true;
        }

        var middleware = new BasicMiddleware();

        // Act
        var invokeTask = middleware.InvokeAsync(context, Next);
        await Task.Delay(50); // Give next time to start
        nextStarted.ShouldBeTrue();
        nextCompleted.ShouldBeFalse();

        tcs.SetResult(true);
        await invokeTask;

        // Assert
        nextCompleted.ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ChainsMultipleMiddleware_ExecutesInOrder()
    {
        // Arrange
        var context = CreateContext();
        var executionOrder = new List<string>();

        async Task FinalNext(CommandContext ctx)
        {
            executionOrder.Add("final");
            await Task.CompletedTask;
        }

        Func<CommandContext, Task> chain = FinalNext;
        chain = CreateChainedNext("middleware3", executionOrder, chain);
        chain = CreateChainedNext("middleware2", executionOrder, chain);
        chain = CreateChainedNext("middleware1", executionOrder, chain);

        // Act
        await chain(context);

        // Assert
        executionOrder.ShouldBe(new[] { "middleware1-before", "middleware2-before", "middleware3-before", "final", "middleware3-after", "middleware2-after", "middleware1-after" });
    }

    [Fact]
    public async Task InvokeAsync_MiddlewareHandlesException_DoesNotPropagate()
    {
        // Arrange
        var context = CreateContext();
        Task Next(CommandContext ctx)
        {
            throw new InvalidOperationException("Next failed");
        }

        var middleware = new ExceptionHandlingMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        context.SessionContext["exception-handled"].ShouldBe("true");
    }

    [Fact]
    public async Task InvokeAsync_WithCancellationToken_PassesToNextViaContext()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var context = new CommandContext
        {
            Command = Substitute.For<ICommand>(),
            Console = Substitute.For<IConsole>(),
            SessionContext = new Dictionary<string, string>(),
            CancellationToken = cts.Token
        };
        CancellationToken capturedToken = default;

        Task Next(CommandContext ctx)
        {
            capturedToken = ctx.CancellationToken;
            return Task.CompletedTask;
        }

        var middleware = new BasicMiddleware();

        // Act
        await middleware.InvokeAsync(context, Next);

        // Assert
        capturedToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task InvokeAsync_NextCompletedSynchronously_CompletesSuccessfully()
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

    // Helper methods and inner classes

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

    private static Func<CommandContext, Task> CreateChainedNext(string name, List<string> executionOrder, Func<CommandContext, Task> next)
    {
        return async (ctx) =>
        {
            executionOrder.Add($"{name}-before");
            await next(ctx);
            executionOrder.Add($"{name}-after");
        };
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

    private class ModifyBeforeMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            context.SessionContext["key"] = "modified";
            await next(context);
        }
    }

    private class ModifyAfterMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            await next(context);
            context.SessionContext["key"] = "after-middleware";
        }
    }

    private class ThrowBeforeNextMiddleware : ICommandMiddleware
    {
        public Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            throw new InvalidOperationException("Middleware failed before next");
        }
    }

    private class ThrowAfterNextMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            await next(context);
            throw new InvalidOperationException("Middleware failed after next");
        }
    }

    private class ExceptionHandlingMiddleware : ICommandMiddleware
    {
        public async Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            try
            {
                await next(context);
            }
            catch
            {
                context.SessionContext["exception-handled"] = "true";
            }
        }
    }
}
