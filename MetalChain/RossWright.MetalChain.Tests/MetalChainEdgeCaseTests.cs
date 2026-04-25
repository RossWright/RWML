using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalChain.Tests;

// ── Edge-case and behavioral tests ─────────────────────────────────────────────────────────

public class MetalChainNoHandlerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainNoHandlerTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [Fact]
    public async Task NoHandlerRegistered_Query_ThrowsMetalChainException()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);

        await Should.ThrowAsync<MetalChainException>(async () =>
            await mediator.Send(new BasicQuery.Request(1)));
    }

    [Fact]
    public async Task NoHandlerRegistered_Command_ThrowsMetalChainException()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);

        await Should.ThrowAsync<MetalChainException>(async () =>
            await mediator.Send(new BasicCommand.Request(1)));
    }
}

public class MetalChainHasHandlerForTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainHasHandlerForTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [Fact]
    public void HasHandlerForGenericExtension_DelegatesToNonGeneric()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);

        mediator.HasHandlerFor<BasicCommand.Request>().ShouldBeTrue();
        mediator.HasHandlerFor<BasicQuery.Request>().ShouldBeFalse();
    }

    [Fact]
    public void HasListenerForGenericExtension_ReturnsFalse_WhenNoListenerRegistered()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);

        mediator.HasListenerFor<BasicCommand.Request>().ShouldBeFalse();
    }

    [Fact]
    public void HasListenerForGenericExtension_ReturnsTrue_AfterListenerRegistered()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);

        mediator.Listen<BasicCommand.Request>((_, _) => Task.CompletedTask);

        mediator.HasListenerFor<BasicCommand.Request>().ShouldBeTrue();
    }

    [Fact]
    public void HasListenerForGenericExtension_ReturnsFalse_AfterListenerDisposed()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);

        var subscription = mediator.Listen<BasicCommand.Request>((_, _) => Task.CompletedTask);
        mediator.HasListenerFor<BasicCommand.Request>().ShouldBeTrue();

        subscription.Dispose();
        mediator.HasListenerFor<BasicCommand.Request>().ShouldBeFalse();
    }
}

public class MetalChainCancellationTokenTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainCancellationTokenTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [Fact]
    public async Task CancellationToken_PassedThrough_ToCommandHandler()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(TokenCapturingCommandHandler));
        var mediator = new Mediator(_scopeFactory, registry);
        using var cts = new CancellationTokenSource();

        await mediator.Send(new BasicCommand.Request(1), cts.Token);

        TokenCapturingCommandHandler.LastToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task CancellationToken_PassedThrough_ToQueryHandler()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(TokenCapturingQueryHandler));
        var mediator = new Mediator(_scopeFactory, registry);
        using var cts = new CancellationTokenSource();

        await mediator.Send(new BasicQuery.Request(1), cts.Token);

        TokenCapturingQueryHandler.LastToken.ShouldBe(cts.Token);
    }

    private class TokenCapturingCommandHandler : IRequestHandler<BasicCommand.Request>
    {
        public static CancellationToken LastToken { get; private set; }
        public Task Handle(BasicCommand.Request request, CancellationToken cancellationToken = default)
        {
            LastToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private class TokenCapturingQueryHandler : IRequestHandler<BasicQuery.Request, BasicQuery.Response>
    {
        public static CancellationToken LastToken { get; private set; }
        public Task<BasicQuery.Response> Handle(BasicQuery.Request request, CancellationToken cancellationToken = default)
        {
            LastToken = cancellationToken;
            return Task.FromResult(new BasicQuery.Response { Result = request.Value });
        }
    }
}

public class MetalChainListenerTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainListenerTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [Fact]
    public async Task MultipleListeners_BothReceiveRequest()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        int listener1Calls = 0, listener2Calls = 0;

        mediator.Listen<BasicCommand.Request>((r, ct) => { listener1Calls++; return Task.CompletedTask; });
        mediator.Listen<BasicCommand.Request>((r, ct) => { listener2Calls++; return Task.CompletedTask; });

        await mediator.Send(new BasicCommand.Request(1));

        listener1Calls.ShouldBe(1);
        listener2Calls.ShouldBe(1);
    }

    [Fact]
    public async Task Listener_ReceivesActualRequestInstance()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        var request = new BasicCommand.Request(42);
        BasicCommand.Request? received = null;

        mediator.Listen<BasicCommand.Request>((r, ct) => { received = r; return Task.CompletedTask; });

        await mediator.Send(request);

        received.ShouldBeSameAs(request);
    }

    [Fact]
    public async Task ListenerDispose_StopsInvocation()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        int callCount = 0;

        var subscription = mediator.Listen<BasicCommand.Request>((r, ct) => { callCount++; return Task.CompletedTask; });
        await mediator.Send(new BasicCommand.Request(1));
        callCount.ShouldBe(1);

        subscription.Dispose();
        // After disposal, no handler or listener remains — Send throws by default
        await Should.ThrowAsync<MetalChainException>(async () =>
            await mediator.Send(new BasicCommand.Request(2)));

        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task Listen_ViaIRequestHandlerExtension()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        var handler = new TrackingCommandHandler();

        mediator.Listen<BasicCommand.Request>(handler);
        await mediator.Send(new BasicCommand.Request(99));

        handler.LastValue.ShouldBe(99);
    }

    [Fact]
    public async Task ThrowingListener_HandlerStillCompletes_ExceptionSurfaces()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);

        mediator.Listen<BasicCommand.Request>((_, _) =>
            Task.FromException(new InvalidOperationException("listener threw")));

        await Should.ThrowAsync<Exception>(async () =>
            await mediator.Send(new BasicCommand.Request(77)));

        BasicCommand.Handler.LastValue.ShouldBe(77);
    }

    [Fact]
    public async Task MultipleThrowingListeners_FirstExceptionSurfaces_HandlerStillCompleted()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);

        mediator.Listen<BasicCommand.Request>((_, _) =>
            Task.FromException(new InvalidOperationException("listener 1 threw")));
        mediator.Listen<BasicCommand.Request>((_, _) =>
            Task.FromException(new InvalidOperationException("listener 2 threw")));

        // Task.WhenAll surfaces exceptions but await unwraps to the first one
        await Should.ThrowAsync<Exception>(async () =>
            await mediator.Send(new BasicCommand.Request(88)));

        BasicCommand.Handler.LastValue.ShouldBe(88);
    }

    private class TrackingCommandHandler : IRequestHandler<BasicCommand.Request>
    {
        public int LastValue { get; private set; }
        public Task Handle(BasicCommand.Request request, CancellationToken cancellationToken = default)
        {
            LastValue = request.Value;
            return Task.CompletedTask;
        }
    }
}

public class MetalChainIntegrationTests
{
    [Fact]
    public void AddMetalChain_ThenAddHandlers_IMediator_Resolves_WithHandlerRegistered()
    {
        var services = new ServiceCollection();
        services.AddMetalChain();
        services.AddMetalChainHandlers(typeof(BasicCommand.Handler));
        var sp = services.BuildServiceProvider();

        var mediator = sp.GetRequiredService<IMediator>();
        mediator.ShouldNotBeNull();
        mediator.HasHandlerFor<BasicCommand.Request>().ShouldBeTrue();
    }

    [Fact]
    public async Task DuplicateHandlerTypeRegistration_HandlerInvokedOnlyOnce()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);
        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(serviceScope);

        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(CountingCommandHandler));
        registry.AddHandlers(typeof(CountingCommandHandler));
        var mediator = new Mediator(scopeFactory, registry);

        CountingCommandHandler.Reset();
        await mediator.Send(new BasicCommand.Request(1));

        CountingCommandHandler.InvocationCount.ShouldBe(1);
    }

    private class CountingCommandHandler : IRequestHandler<BasicCommand.Request>
    {
        public static int InvocationCount { get; private set; }
        public static void Reset() => InvocationCount = 0;
        public Task Handle(BasicCommand.Request request, CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return Task.CompletedTask;
        }
    }
}

public class MetalChainDisposableTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainDisposableTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    // Bug #5 — ListenerSubscription.Dispose() is not idempotent; throws NRE on second call
    [Fact]
    public void ListenerSubscription_DoubleDispose_DoesNotThrow()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        var subscription = mediator.Listen<BasicCommand.Request>((r, ct) => Task.CompletedTask);

        subscription.Dispose();
        var ex = Record.Exception(() => subscription.Dispose());

        ex.ShouldBeNull();
    }
}

public class MetalChainSendOrIgnoreTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainSendOrIgnoreTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    // Bug #8 — SendOrIgnore only checks HasHandlerFor; silently drops commands that have listeners but no handler
    [Fact]
    public async Task SendOrIgnore_CommandWithListenerOnly_CallsListener()
    {
        MetalChainRegistry registry = new();
        IMediator mediator = new Mediator(_scopeFactory, registry);
        int callCount = 0;

        mediator.Listen<BasicCommand.Request>((r, ct) => { callCount++; return Task.CompletedTask; });
        await mediator.SendOrIgnore(new BasicCommand.Request(1));

        callCount.ShouldBe(1);
    }

    [Fact]
    public async Task SendOrIgnore_WithHandlerAndListener_BothInvoked()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));
        IMediator mediator = new Mediator(_scopeFactory, registry);
        int listenerCallCount = 0;

        mediator.Listen<BasicCommand.Request>((r, ct) => { listenerCallCount++; return Task.CompletedTask; });
        await mediator.SendOrIgnore(new BasicCommand.Request(42));

        BasicCommand.Handler.LastValue.ShouldBe(42);
        listenerCallCount.ShouldBe(1);
    }
}
