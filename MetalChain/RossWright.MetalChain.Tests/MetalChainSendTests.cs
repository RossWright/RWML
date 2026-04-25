using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalChain.Tests;

public class MetalChainSendTests
{
    public MetalChainSendTests()
    {
        _serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceScope.ServiceProvider.Returns(_serviceProvider);
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scopeFactory.CreateScope().Returns(serviceScope);
    }
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    [Fact] public async Task BasicCommand_HappyPath()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);
        await mediator.Send(new BasicCommand.Request(123));
        BasicCommand.Handler.LastValue.ShouldBe(123);
    }

    [Fact] public async Task BasicQuery_HappyPath()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicQuery.Handler));
        var mediator = new Mediator(_scopeFactory, registry);
        var response = await mediator.Send(new BasicQuery.Request(1));
        response.Result.ShouldBe(2);
        BasicQuery.Handler.LastValue.ShouldBe(1);
    }

    [Fact] public async Task InterceptingCommand_HappyPath()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(
            typeof(BasicCommand.Handler),
            typeof(InterceptingCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);
        _serviceProvider.GetService(typeof(IMediator)).Returns(mediator);
        await mediator.Send(new InterceptingCommand.Request(456, new BasicCommand.Request(123)));
        BasicCommand.Handler.LastValue.ShouldBe(123);
        InterceptingCommand.Handler.LastValue.ShouldBe(456);
    }

    [Fact] public async Task InterceptingQuery_HappyPath()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(
            typeof(BasicQuery.Handler),
            typeof(InterceptingQuery.Handler<>));
        registry._queryHandlers.Keys.ShouldContain(typeof(InterceptingQuery.Request<>));
        var mediator = new Mediator(_scopeFactory, registry);

        _serviceProvider.GetService(typeof(IMediator)).Returns(mediator);
        var response = await mediator.Send(
            new InterceptingQuery.Request<BasicQuery.Response>(456, new BasicQuery.Request(1)));
        response.Result.ShouldBe(2);
        BasicQuery.Handler.LastValue.ShouldBe(1);
        InterceptingQuery.Handler<BasicQuery.Response>.LastValue.ShouldBe(456);
    }

    [Fact] public async Task OpenCommand_HappyPath()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(OpenCommand.Handler<>));
        registry._commandHandlers.Keys.ShouldContain(typeof(OpenCommand.Request<>));
        var mediator = new Mediator(_scopeFactory, registry);

        _serviceProvider.GetService(typeof(IMediator)).Returns(mediator);
        await mediator.Send(new OpenCommand.Request<MyThing>(new MyThing()));
        OpenCommand.Handler<MyThing>.LastValue.ShouldBeOfType<MyThing>();
    }

    public class MyThing { }

    [Fact] public async Task UntypedSend_Query_ReturnsTypedResponseAsObject()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicQuery.Handler));
        var mediator = new Mediator(_scopeFactory, registry);

        object? result = await mediator.Send((object)new BasicQuery.Request(5));

        result.ShouldBeOfType<BasicQuery.Response>();
        ((BasicQuery.Response)result!).Result.ShouldBe(6);
    }

    [Fact] public async Task UntypedSend_Command_ReturnsNull()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);

        object? result = await mediator.Send((object)new BasicCommand.Request(7));

        result.ShouldBeNull();
    }

    [Fact] public async Task Send_CommandWithNoHandler_ThrowsExpectedException()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        await Should.ThrowAsync<MetalChainException>(async () => await mediator.Send(new UnhandledCommand()));
    }

    [Fact] public async Task Send_QueryWithNoHandler_ThrowsExpectedException()
    {
        MetalChainRegistry registry = new();
        var mediator = new Mediator(_scopeFactory, registry);
        await Should.ThrowAsync<MetalChainException>(async () => await mediator.Send(new UnhandledQuery()));
    }

    [Fact] public async Task Send_WhenCommandHandlerThrows_PropagatesException()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(ThrowingCommand.Handler));
        var mediator = new Mediator(_scopeFactory, registry);
        await Should.ThrowAsync<InvalidOperationException>(async () => await mediator.Send(new ThrowingCommand.Request()));
    }

    public sealed record UnhandledCommand : IRequest;
    public sealed record UnhandledQuery : IRequest<int>;

    public static class ThrowingCommand
    {
        public sealed record Request : IRequest;
        public sealed class Handler : IRequestHandler<Request>
        {
            public Task Handle(Request request, CancellationToken cancellationToken = default) =>
                Task.FromException(new InvalidOperationException("handler threw"));
        }
    }
}

