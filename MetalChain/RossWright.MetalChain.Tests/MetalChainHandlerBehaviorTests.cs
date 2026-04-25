using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalChain.Tests;

// ── Scenario 1: Unhandled Query ───────────────────────────────────────────────

public class MetalChainUnhandledQueryTests
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainUnhandledQueryTests()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [AllowNoHandler]
    private class OptionalQuery : IRequest<string> { }

    [RequireHandler]
    private class StrictQuery : IRequest<string> { }

    private class OptionalQueryHandler : IRequestHandler<OptionalQuery, string>
    {
        public Task<string> Handle(OptionalQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult("handled");
    }

    [Fact]
    public async Task AllowUnhandledQueries_NoHandler_ReturnsDefault()
    {
        var registry = new MetalChainRegistry { AllowUnhandledQueries = true };
        var mediator = new Mediator(_scopeFactory, registry);

        var result = await mediator.Send(new OptionalQuery());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task AllowNoHandlerAttribute_ReturnsDefault_IgnoresGlobalDefault()
    {
        var registry = new MetalChainRegistry(); // strict global default
        var mediator = new Mediator(_scopeFactory, registry);

        var result = await mediator.Send(new OptionalQuery());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task AllowNoHandlerAttribute_WithHandler_ReturnsHandlerResult()
    {
        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(OptionalQueryHandler));
        var mediator = new Mediator(_scopeFactory, registry);

        var result = await mediator.Send(new OptionalQuery());

        result.ShouldBe("handled");
    }

    [Fact]
    public async Task RequireHandlerAttribute_ThrowsEvenWhenAllowUnhandledQueriesSet()
    {
        var registry = new MetalChainRegistry { AllowUnhandledQueries = true };
        var mediator = new Mediator(_scopeFactory, registry);

        await Should.ThrowAsync<MetalChainException>(async () =>
            await mediator.Send(new StrictQuery()));
    }

    [Fact]
    public async Task SendOrDefault_ReturnsDefault_WhenNoHandlerRegistered()
    {
        var registry = new MetalChainRegistry();
        var mediator = new Mediator(_scopeFactory, registry);

        var result = await mediator.SendOrDefault(new OptionalQuery());

        result.ShouldBeNull();
    }

    [Fact]
    public async Task SendOrDefault_ReturnsResult_WhenHandlerRegistered()
    {
        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(OptionalQueryHandler));
        var mediator = new Mediator(_scopeFactory, registry);

        var result = await mediator.SendOrDefault(new OptionalQuery());

        result.ShouldBe("handled");
    }

    [Fact]
    public async Task SendOrDefault_NeverThrows_RegardlessOfAttributeOrGlobalSetting()
    {
        var registry = new MetalChainRegistry(); // strict global default
        var mediator = new Mediator(_scopeFactory, registry);

        // StrictQuery has [RequireHandler] but SendOrDefault bypasses the registry throw path
        var result = await mediator.SendOrDefault(new StrictQuery());

        result.ShouldBeNull();
    }
}

// ── Scenario 2: Unhandled Command ────────────────────────────────────────────

public class MetalChainUnhandledCommandTests
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainUnhandledCommandTests()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    [AllowNoHandler]
    private class OptionalCommand : IRequest { }

    [RequireHandler]
    private class StrictCommand : IRequest { }

    [Fact]
    public async Task AllowUnhandledCommands_NoHandler_CompletesSilently()
    {
        var registry = new MetalChainRegistry { AllowUnhandledCommands = true };
        var mediator = new Mediator(_scopeFactory, registry);

        await mediator.Send(new OptionalCommand());
    }

    [Fact]
    public async Task AllowNoHandlerAttribute_CompletesSilently_IgnoresGlobalDefault()
    {
        var registry = new MetalChainRegistry(); // strict global default
        var mediator = new Mediator(_scopeFactory, registry);

        await mediator.Send(new OptionalCommand());
    }

    [Fact]
    public async Task RequireHandlerAttribute_ThrowsEvenWhenAllowUnhandledCommandsSet()
    {
        var registry = new MetalChainRegistry { AllowUnhandledCommands = true };
        var mediator = new Mediator(_scopeFactory, registry);

        await Should.ThrowAsync<MetalChainException>(async () =>
            await mediator.Send(new StrictCommand()));
    }

    [Fact]
    public async Task ListenerRegistered_SuppressesThrow_WhenNoHandlerExists()
    {
        var registry = new MetalChainRegistry();
        var mediator = new Mediator(_scopeFactory, registry);
        bool listenerCalled = false;
        mediator.Listen<OptionalCommand>((_, _) => { listenerCalled = true; return Task.CompletedTask; });

        // No handler, but a listener is present — should not throw
        await mediator.Send(new OptionalCommand());

        listenerCalled.ShouldBeTrue();
    }

    [Fact]
    public async Task SendOrIgnore_CompletesSilently_WhenNoHandlerRegistered()
    {
        var registry = new MetalChainRegistry();
        var mediator = new Mediator(_scopeFactory, registry);

        await mediator.SendOrIgnore(new StrictCommand());
    }

    [Fact]
    public async Task SendOrIgnore_NeverThrows_RegardlessOfAttributeOrGlobalSetting()
    {
        var registry = new MetalChainRegistry(); // strict global default + [RequireHandler] on type
        var mediator = new Mediator(_scopeFactory, registry);

        // StrictCommand has [RequireHandler] but SendOrIgnore bypasses the throw path
        await mediator.SendOrIgnore(new StrictCommand());
    }
}

// ── Scenario 3: Duplicate Query Handler (always forbidden) ───────────────────

public class MetalChainDuplicateQueryHandlerTests
{
    private class QueryA : IRequest<int> { }

    private class QueryAHandler : IRequestHandler<QueryA, int>
    {
        public Task<int> Handle(QueryA request, CancellationToken cancellationToken = default)
            => Task.FromResult(1);
    }

    private class QueryAHandlerAlt : IRequestHandler<QueryA, int>
    {
        public Task<int> Handle(QueryA request, CancellationToken cancellationToken = default)
            => Task.FromResult(2);
    }

    [Fact]
    public void DuplicateQueryHandler_ThrowsAtRegistration()
    {
        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(QueryAHandler));

        Should.Throw<MetalChainException>(() =>
            registry.AddHandlers(typeof(QueryAHandlerAlt)));
    }

    [Fact]
    public void IgnoreHandler_PreventsConflictingHandlerFromRegistering()
    {
        var registry = new MetalChainRegistry();
        registry.IgnoredHandlers.Add(typeof(QueryAHandlerAlt));
        registry.AddHandlers(typeof(QueryAHandler));

        // Adding the alt handler is now a no-op — no exception thrown
        registry.AddHandlers(typeof(QueryAHandlerAlt));

        registry._queryHandlers[typeof(QueryA)].ShouldBe(typeof(QueryAHandler));
    }

    [Fact]
    public void IgnoreHandler_ExcludesHandler_BeforeFirstRegistration()
    {
        var registry = new MetalChainRegistry();
        registry.IgnoredHandlers.Add(typeof(QueryAHandlerAlt));

        // The ignored handler is skipped even if registered first
        registry.AddHandlers(typeof(QueryAHandlerAlt), typeof(QueryAHandler));

        registry._queryHandlers[typeof(QueryA)].ShouldBe(typeof(QueryAHandler));
    }
}

// ── Scenario 4: Multiple Command Handlers (Multicast Fan-Out) ────────────────

public class MetalChainMulticastCommandTests
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MetalChainMulticastCommandTests()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }

    // ── Request types ──────────────────────────────────────────────────────────

    private class FanOutCommand : IRequest { }

    [AllowMultipleHandlers]
    private class AttributedFanOutCommand : IRequest { }

    [AllowMultipleHandlers(ExecutionMode = MultipleHandlerExecutionMode.SequentialCollectErrors)]
    private class CollectErrorsCommand : IRequest { }

    [AllowMultipleHandlers(ExecutionMode = MultipleHandlerExecutionMode.ParallelCollectErrors)]
    private class ParallelFanOutCommand : IRequest { }

    // ── Tracking handlers ──────────────────────────────────────────────────────

    private class FanOutHandlerA : IRequestHandler<FanOutCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(FanOutCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    private class FanOutHandlerB : IRequestHandler<FanOutCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(FanOutCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    private class AttributedHandlerA : IRequestHandler<AttributedFanOutCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(AttributedFanOutCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    private class AttributedHandlerB : IRequestHandler<AttributedFanOutCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(AttributedFanOutCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    private class ThrowingHandlerA : IRequestHandler<CollectErrorsCommand>
    {
        public Task Handle(CollectErrorsCommand request, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("handler A failed"));
    }

    private class ThrowingHandlerB : IRequestHandler<CollectErrorsCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(CollectErrorsCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.FromException(new InvalidOperationException("handler B failed")); }
    }

    private class FailFastHandlerA : IRequestHandler<FanOutCommand>
    {
        public Task Handle(FanOutCommand request, CancellationToken cancellationToken = default)
            => Task.FromException(new InvalidOperationException("fail fast"));
    }

    private class ParallelHandlerA : IRequestHandler<ParallelFanOutCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(ParallelFanOutCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    private class ParallelHandlerB : IRequestHandler<ParallelFanOutCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(ParallelFanOutCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    [AllowMultipleHandlers(ExecutionMode = MultipleHandlerExecutionMode.ParallelCollectErrors)]
    private class ParallelThrowingCommand : IRequest { }

    private class ParallelThrowingHandlerA : IRequestHandler<ParallelThrowingCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(ParallelThrowingCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.FromException(new InvalidOperationException("parallel A failed")); }
    }

    private class ParallelThrowingHandlerB : IRequestHandler<ParallelThrowingCommand>
    {
        public static int InvocationCount;
        public static void Reset() => InvocationCount = 0;
        public Task Handle(ParallelThrowingCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.FromException(new InvalidOperationException("parallel B failed")); }
    }

    // ── Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public void DefaultBehavior_SecondDistinctCommandHandler_ThrowsAtRegistration()
    {
        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(FanOutHandlerA));

        Should.Throw<MetalChainException>(() => 
            registry.AddHandlers(typeof(FanOutHandlerB)));
    }

    [Fact]
    public async Task AllowMultipleCommandHandlers_BothHandlersInvoked()
    {
        FanOutHandlerA.Reset();
        FanOutHandlerB.Reset();

        var registry = new MetalChainRegistry { AllowMultipleCommandHandlers = true };
        registry.AddHandlers(typeof(FanOutHandlerA), typeof(FanOutHandlerB));
        var mediator = new Mediator(_scopeFactory, registry);

        await mediator.Send(new FanOutCommand());

        FanOutHandlerA.InvocationCount.ShouldBe(1);
        FanOutHandlerB.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task AllowMultipleHandlersAttribute_BothHandlersInvoked_WithoutGlobalOption()
    {
        AttributedHandlerA.Reset();
        AttributedHandlerB.Reset();

        var registry = new MetalChainRegistry(); // global option NOT set
        registry.AddHandlers(typeof(AttributedHandlerA), typeof(AttributedHandlerB));
        var mediator = new Mediator(_scopeFactory, registry);

        await mediator.Send(new AttributedFanOutCommand());

        AttributedHandlerA.InvocationCount.ShouldBe(1);
        AttributedHandlerB.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task SequentialFailFast_StopsOnFirstException()
    {
        FanOutHandlerB.Reset();

        var registry = new MetalChainRegistry { AllowMultipleCommandHandlers = true };
        // FailFastHandlerA throws; FanOutHandlerB should NOT run
        registry.AddHandlers(typeof(FailFastHandlerA), typeof(FanOutHandlerB));
        var mediator = new Mediator(_scopeFactory, registry);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await mediator.Send(new FanOutCommand()));

        FanOutHandlerB.InvocationCount.ShouldBe(0);
    }

    [Fact]
    public async Task SequentialCollectErrors_AllHandlersRun_AggregateExceptionThrown()
    {
        ThrowingHandlerB.Reset();

        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(ThrowingHandlerA), typeof(ThrowingHandlerB));
        var mediator = new Mediator(_scopeFactory, registry);

        var ex = await Should.ThrowAsync<AggregateException>(async () =>
            await mediator.Send(new CollectErrorsCommand()));

        ex.InnerExceptions.Count.ShouldBe(2);
        ThrowingHandlerB.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task ParallelCollectErrors_AllHandlersCalled_AggregateExceptionOnFailure()
    {
        ParallelHandlerA.Reset();
        ParallelHandlerB.Reset();

        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(ParallelHandlerA), typeof(ParallelHandlerB));
        var mediator = new Mediator(_scopeFactory, registry);

        await mediator.Send(new ParallelFanOutCommand());

        ParallelHandlerA.InvocationCount.ShouldBe(1);
        ParallelHandlerB.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task ParallelCollectErrors_BothHandlersThrow_AggregateExceptionContainsBothErrors()
    {
        ParallelThrowingHandlerA.Reset();
        ParallelThrowingHandlerB.Reset();

        var registry = new MetalChainRegistry();
        registry.AddHandlers(typeof(ParallelThrowingHandlerA), typeof(ParallelThrowingHandlerB));
        var mediator = new Mediator(_scopeFactory, registry);

        var ex = await Should.ThrowAsync<AggregateException>(async () =>
            await mediator.Send(new ParallelThrowingCommand()));

        ex.InnerExceptions.Count.ShouldBe(2);
        ParallelThrowingHandlerA.InvocationCount.ShouldBe(1);
        ParallelThrowingHandlerB.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public void IgnoreHandler_ExcludesCommandHandlerFromRegistration()
    {
        var registry = new MetalChainRegistry { AllowMultipleCommandHandlers = true };
        registry.IgnoredHandlers.Add(typeof(FanOutHandlerB));
        registry.AddHandlers(typeof(FanOutHandlerA), typeof(FanOutHandlerB));

        registry._commandHandlers
            .GetValuesOrEmptySet(typeof(FanOutCommand))
            .ShouldHaveSingleItem()
            .ShouldBe(typeof(FanOutHandlerA));
    }
}

// ── Options builder integration ───────────────────────────────────────────────

public class MetalChainOptionsBuilderBehaviorTests
{
    private class ItemQuery : IRequest<string> { }
    private class ItemQueryHandlerPrimary : IRequestHandler<ItemQuery, string>
    {
        public Task<string> Handle(ItemQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult("primary");
    }
    private class ItemQueryHandlerAlt : IRequestHandler<ItemQuery, string>
    {
        public Task<string> Handle(ItemQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult("alt");
    }

    private class ItemCommand : IRequest { }
    private class ItemCommandHandlerA : IRequestHandler<ItemCommand>
    {
        public Task Handle(ItemCommand request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
    private class ItemCommandHandlerB : IRequestHandler<ItemCommand>
    {
        public Task Handle(ItemCommand request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    [Fact]
    public void IgnoreHandler_ExcludesConflictingQueryHandler()
    {
        var builder = new MetalChainOptionsBuilder();
        builder.IgnoreHandler<ItemQueryHandlerAlt>();
        builder.SetDiscoveredConcreteTypesForTesting([
            typeof(ItemQueryHandlerPrimary),
            typeof(ItemQueryHandlerAlt),
        ]);

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        builder.Initialize(services);

        var registry = (MetalChainRegistry)services
            .First(d => d.ServiceType == typeof(IMetalChainRegistry))
            .ImplementationInstance!;

        registry._queryHandlers[typeof(ItemQuery)].ShouldBe(typeof(ItemQueryHandlerPrimary));
    }

    [Fact]
    public void AllowMultipleCommandHandlers_ViaOptionsBuilder_PropagatedToRegistry()
    {
        var builder = new MetalChainOptionsBuilder();
        builder.AllowMultipleCommandHandlers(MultipleHandlerExecutionMode.SequentialCollectErrors);
        builder.SetDiscoveredConcreteTypesForTesting([
            typeof(ItemCommandHandlerA),
            typeof(ItemCommandHandlerB),
        ]);

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        builder.Initialize(services);

        var registry = (MetalChainRegistry)services
            .First(d => d.ServiceType == typeof(IMetalChainRegistry))
            .ImplementationInstance!;

        registry.AllowMultipleCommandHandlers.ShouldBeTrue();
        registry.DefaultCommandExecutionMode.ShouldBe(MultipleHandlerExecutionMode.SequentialCollectErrors);
        registry._commandHandlers
            .GetValuesOrEmptySet(typeof(ItemCommand))
            .Count().ShouldBe(2);
    }

    [Fact]
    public void AllowUnhandledQueries_ViaOptionsBuilder_PropagatedToRegistry()
    {
        var builder = new MetalChainOptionsBuilder();
        builder.AllowUnhandledQueries();
        builder.SetDiscoveredConcreteTypesForTesting([]);

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        builder.Initialize(services);

        var registry = (MetalChainRegistry)services
            .First(d => d.ServiceType == typeof(IMetalChainRegistry))
            .ImplementationInstance!;

        registry.AllowUnhandledQueries.ShouldBeTrue();
    }

    [Fact]
    public void AllowUnhandledCommands_ViaOptionsBuilder_PropagatedToRegistry()
    {
        var builder = new MetalChainOptionsBuilder();
        builder.AllowUnhandledCommands();
        builder.SetDiscoveredConcreteTypesForTesting([]);

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        builder.Initialize(services);

        var registry = (MetalChainRegistry)services
            .First(d => d.ServiceType == typeof(IMetalChainRegistry))
            .ImplementationInstance!;

        registry.AllowUnhandledCommands.ShouldBeTrue();
    }
}
