using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace RossWright.MetalChain.Tests.Internal;

public class MediatorTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMetalChainRegistry _registry;

    public MediatorTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
        _registry = Substitute.For<IMetalChainRegistry>();
    }

    [Fact]
    public async Task Send_WithValidRequest_DelegatesToUntypedSend()
    {
        // Arrange
        var request = new BasicCommand.Request(123);
        _registry.Handle(_serviceProvider, request, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(null));
        var mediator = new Mediator(_scopeFactory, _registry);

        // Act
        await mediator.Send(request);

        // Assert
        await _registry.Received(1).Handle(_serviceProvider, request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_WithCancellationToken_PassesTokenToUntypedSend()
    {
        // Arrange
        var request = new BasicCommand.Request(456);
        var cancellationToken = new CancellationToken(true);
        _registry.Handle(_serviceProvider, request, cancellationToken)
            .Returns(Task.FromResult<object?>(null));
        var mediator = new Mediator(_scopeFactory, _registry);

        // Act
        await mediator.Send(request, cancellationToken);

        // Assert
        await _registry.Received(1).Handle(_serviceProvider, request, cancellationToken);
    }

    [Fact]
    public void Listen_WithValidListener_CreatesSubscription()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        Func<BasicCommand.Request, CancellationToken, Task> listener = (req, ct) => Task.CompletedTask;

        // Act
        var subscription = mediator.Listen(listener);

        // Assert
        subscription.ShouldNotBeNull();
        subscription.ShouldBeAssignableTo<IDisposable>();
    }

    [Fact]
    public void Listen_WithValidListener_AddsListenerToRegistry()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        Func<BasicCommand.Request, CancellationToken, Task> listener = (req, ct) => Task.CompletedTask;

        // Act
        mediator.Listen(listener);

        // Assert
        _registry.Received(1).AddListener(
            typeof(BasicCommand.Request),
            Arg.Any<Func<object, CancellationToken, Task>>());
    }

    [Fact]
    public void Listen_WhenSubscriptionDisposed_RemovesListenerFromRegistry()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        Func<BasicCommand.Request, CancellationToken, Task> listener = (req, ct) => Task.CompletedTask;
        var subscription = mediator.Listen(listener);

        // Act
        subscription.Dispose();

        // Assert
        _registry.Received(1).RemoveListener(
            typeof(BasicCommand.Request),
            Arg.Any<Func<object, CancellationToken, Task>>());
    }

    [Fact]
    public void Listen_WhenSubscriptionDisposedMultipleTimes_OnlyRemovesOnce()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        Func<BasicCommand.Request, CancellationToken, Task> listener = (req, ct) => Task.CompletedTask;
        var subscription = mediator.Listen(listener);

        // Act
        subscription.Dispose();
        subscription.Dispose();
        subscription.Dispose();

        // Assert
        _registry.Received(1).RemoveListener(
            typeof(BasicCommand.Request),
            Arg.Any<Func<object, CancellationToken, Task>>());
    }

    [Fact]
    public async Task Listen_ListenerInvocation_WrapsTypedListenerCorrectly()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        var tcs = new TaskCompletionSource<bool>();
        BasicCommand.Request? capturedRequest = null;
        CancellationToken capturedToken = default;

        Func<BasicCommand.Request, CancellationToken, Task> listener = (req, ct) =>
        {
            capturedRequest = req;
            capturedToken = ct;
            tcs.SetResult(true);
            return Task.CompletedTask;
        };

        Func<object, CancellationToken, Task>? wrappedListener = null;
        _registry.AddListener(typeof(BasicCommand.Request), Arg.Do<Func<object, CancellationToken, Task>>(l => wrappedListener = l));

        // Act
        mediator.Listen(listener);

        // Assert
        wrappedListener.ShouldNotBeNull();

        // Invoke the wrapped listener
        var testRequest = new BasicCommand.Request(789);
        var testToken = new CancellationToken(false);
        await wrappedListener(testRequest, testToken);
        await tcs.Task;

        capturedRequest.ShouldBe(testRequest);
        capturedToken.ShouldBe(testToken);
    }

    [Fact]
    public async Task Send_WithDefaultCancellationToken_UsesDefaultToken()
    {
        // Arrange
        var request = new BasicCommand.Request(999);
        _registry.Handle(_serviceProvider, request, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(null));
        var mediator = new Mediator(_scopeFactory, _registry);

        // Act
        await mediator.Send(request);

        // Assert
        await _registry.Received(1).Handle(_serviceProvider, request, default(CancellationToken));
    }

    [Fact]
    public async Task Send_WithCommand_DelegatesToUntypedSend()
    {
        // Arrange
        var request = new BasicCommand.Request(555);
        _registry.Handle(_serviceProvider, request, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(null));
        var mediator = new Mediator(_scopeFactory, _registry);

        // Act
        await mediator.Send(request);

        // Assert
        await _registry.Received(1).Handle(_serviceProvider, request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Send_WhenCalled_CreatesServiceScope()
    {
        // Arrange
        var request = new BasicCommand.Request(777);
        _registry.Handle(_serviceProvider, request, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(null));
        var mediator = new Mediator(_scopeFactory, _registry);

        // Act
        await mediator.Send(request);

        // Assert
        _scopeFactory.Received(1).CreateScope();
    }

    [Fact]
    public void Listen_WithDifferentRequestTypes_CreatesMultipleSubscriptions()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        Func<BasicCommand.Request, CancellationToken, Task> listener1 = (req, ct) => Task.CompletedTask;
        Func<TestRequest, CancellationToken, Task> listener2 = (req, ct) => Task.CompletedTask;

        // Act
        var subscription1 = mediator.Listen(listener1);
        var subscription2 = mediator.Listen(listener2);

        // Assert
        subscription1.ShouldNotBeNull();
        subscription2.ShouldNotBeNull();
        _registry.Received(1).AddListener(typeof(BasicCommand.Request), Arg.Any<Func<object, CancellationToken, Task>>());
        _registry.Received(1).AddListener(typeof(TestRequest), Arg.Any<Func<object, CancellationToken, Task>>());
    }

    [Fact]
    public void Listen_WithSameRequestType_CreatesMultipleSubscriptions()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        Func<BasicCommand.Request, CancellationToken, Task> listener1 = (req, ct) => Task.CompletedTask;
        Func<BasicCommand.Request, CancellationToken, Task> listener2 = (req, ct) => Task.CompletedTask;

        // Act
        var subscription1 = mediator.Listen(listener1);
        var subscription2 = mediator.Listen(listener2);

        // Assert
        subscription1.ShouldNotBeNull();
        subscription2.ShouldNotBeNull();
        _registry.Received(2).AddListener(typeof(BasicCommand.Request), Arg.Any<Func<object, CancellationToken, Task>>());
    }

    [Fact]
    public async Task Send_MultipleCallsWithSameRequestType_EachCreatesOwnScope()
    {
        // Arrange
        var request1 = new BasicCommand.Request(111);
        var request2 = new BasicCommand.Request(222);
        _registry.Handle(_serviceProvider, Arg.Any<BasicCommand.Request>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<object?>(null));
        var mediator = new Mediator(_scopeFactory, _registry);

        // Act
        await mediator.Send(request1);
        await mediator.Send(request2);

        // Assert
        _scopeFactory.Received(2).CreateScope();
    }

    [Fact]
    public async Task Listen_ListenerInvocation_ThrowsException_PropagatesCorrectly()
    {
        // Arrange
        var mediator = new Mediator(_scopeFactory, _registry);
        var expectedException = new InvalidOperationException("Test exception");

        Func<BasicCommand.Request, CancellationToken, Task> listener = (req, ct) =>
            throw expectedException;

        Func<object, CancellationToken, Task>? wrappedListener = null;
        _registry.AddListener(typeof(BasicCommand.Request), Arg.Do<Func<object, CancellationToken, Task>>(l => wrappedListener = l));

        // Act
        mediator.Listen(listener);

        // Assert
        wrappedListener.ShouldNotBeNull();

        var testRequest = new BasicCommand.Request(123);
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await wrappedListener(testRequest, default));

        exception.ShouldBe(expectedException);
    }

    private class TestRequest : IRequest { }
}
