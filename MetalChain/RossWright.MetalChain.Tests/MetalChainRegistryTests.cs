using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;

namespace RossWright.MetalChain.Tests;

public class MetalChainMediatorTests
{
    public MetalChainMediatorTests()
    {
        var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        var serviceScope = Substitute.For<IServiceScope>();
        serviceProvider = Substitute.For<IServiceProvider>();
        serviceScopeFactory.CreateScope().Returns(serviceScope);
        serviceScope.ServiceProvider.Returns(serviceProvider);
        registry = Substitute.For<IMetalChainRegistry>();
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        token = tokenSource.Token;
        mediator = new Mediator(serviceScopeFactory, registry);
    }
    IServiceProvider serviceProvider;
    IMetalChainRegistry registry;
    CancellationToken token;
    Mediator mediator;

    [Fact] public async Task Send_HappyPath()
    {

        object request = new BasicCommand.Request(123);
        registry.Handle(Arg.Is(serviceProvider), Arg.Is(request), Arg.Is(token))
                .Returns((object?)null); 
        var response = await mediator.Send(request, token);
        response.ShouldBeNull();
    }
    
    [Fact] public async Task SendCommand_HappyPath()
    {
        registry.Handle(Arg.Is(serviceProvider), Arg.Any<BasicCommand.Request>(), Arg.Is(token))
                .Returns((object?)null);
        await mediator.Send(new BasicCommand.Request(123), token);
    }

    [Fact] public async Task SendQuery_HappyPath()
    {
        var expectedResponse = new BasicQuery.Response { Result = 654 };
        registry.Handle(Arg.Is(serviceProvider), Arg.Any<BasicQuery.Request>(), Arg.Is(token))
            .Returns(expectedResponse);        
        var actualResponse = await mediator.Send(new BasicQuery.Request(123), token);
        actualResponse.ShouldBeOfType<BasicQuery.Response>();
        actualResponse.ShouldBe(expectedResponse);
    }

    [Fact] public async Task Listener_HappyPath()
    {
        Func<object, CancellationToken, Task> theFunc = null!;
        registry
            .When(_ => _.AddListener(
                Arg.Is(typeof(BasicCommand.Request)),
                Arg.Any<Func<object, CancellationToken, Task>>()))
            .Do(_ => theFunc = _.Arg<Func<object, CancellationToken, Task>>());

        bool wasCalled = false;
        var sub = mediator.Listen<BasicCommand.Request>((r, ct) => { wasCalled = true; return Task.CompletedTask; });
        sub.ShouldBeOfType<Mediator.ListenerSubscription<BasicCommand.Request>>();
        ((Mediator.ListenerSubscription<BasicCommand.Request>)sub)._listener.ShouldBe(theFunc);
        registry.Received(1).AddListener(
            Arg.Is(typeof(BasicCommand.Request)),
            Arg.Any<Func<object, CancellationToken, Task>>());

        await theFunc(new BasicCommand.Request(123), token);
        wasCalled.ShouldBeTrue();

        sub.Dispose();
        registry.Received(1).RemoveListener(
            Arg.Is(typeof(BasicCommand.Request)),
            Arg.Is(theFunc));
    }
}

public class MetalChainRegistryTests
{
    [Fact] public void HappyPath_HandlerRegistration()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(
            typeof(BasicCommand.Handler),
            typeof(BasicCommand.Request),
            typeof(BasicQuery.Handler),
            typeof(BasicQuery.Request),
            typeof(InterceptingCommand.Handler),
            typeof(InterceptingCommand.Request),
            typeof(InterceptingQuery.Handler<>),
            typeof(InterceptingQuery.Request<>),
            typeof(OpenCommand.Request<>),
            typeof(OpenCommand.Handler<>));

        Assert.Collection(registry._commandHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(BasicCommand.Request)),
            _ => _.ShouldBe(typeof(InterceptingCommand.Request)),
            _ => _.ShouldBe(typeof(OpenCommand.Request<>)));

        Assert.Collection(registry._queryHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(BasicQuery.Request)),
            _ => _.ShouldBe(typeof(InterceptingQuery.Request<>)));
    }

    [Fact] public void DuplicateCommandHandlerRegistration_ThrowsMetalChainException()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicCommand.Handler));

        Should.Throw<MetalChainException>(() =>
            registry.AddHandlers(typeof(DuplicateBasicCommandHandler)));
    }
    public class DuplicateBasicCommandHandler : BasicCommand.Handler { }

    [Fact] public void DuplicateQueryHandlerRegistration_ThrowsMetalChainException()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicQuery.Handler));

        Should.Throw<MetalChainException>(() =>
            registry.AddHandlers(typeof(DuplicateBasicQueryHandler)));
    }
    public class DuplicateBasicQueryHandler : BasicQuery.Handler { }


    [Fact] public async Task ListenToUnregisteredCommandRequest()
    {
        MetalChainRegistry registry = new();
        BasicCommand.Request request = new(123);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        Func<object, CancellationToken, Task> handlerFunc = (r, ct) =>
        {
            r.ShouldBe(request);
            ct.ShouldBe(token);
            return Task.CompletedTask;
        };
        registry.AddListener(typeof(BasicCommand.Request), handlerFunc);

        registry.HasHandlerFor(typeof(BasicCommand.Request)).ShouldBeFalse();

        IServiceProvider provider = Substitute.For<IServiceProvider>();

        await registry.Handle(provider, request, token);

        registry.RemoveListener(typeof(BasicCommand.Request), handlerFunc);

        registry.HasHandlerFor(typeof(BasicCommand.Request)).ShouldBeFalse();

        registry._commandHandlers.Keys.ShouldBeEmpty();
        registry._queryHandlers.Keys.ShouldBeEmpty();
    }

    [Fact] public async Task ListenToUnregisteredQueryRequest()
    {
        MetalChainRegistry registry = new();
        BasicQuery.Request request = new(123);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        Func<object, CancellationToken, Task> handlerFunc = (r, ct) =>
        {
            r.ShouldBe(request);
            ct.ShouldBe(token);
            return Task.CompletedTask;
        };
        registry.AddListener(typeof(BasicQuery.Request), handlerFunc);

        registry.HasHandlerFor(typeof(BasicQuery.Request)).ShouldBeFalse();

        IServiceProvider provider = Substitute.For<IServiceProvider>();

        await Should.ThrowAsync<MetalChainException>(() => registry.Handle(provider, request, token));

        registry.RemoveListener(typeof(BasicQuery.Request), handlerFunc);

        registry.HasHandlerFor(typeof(BasicQuery.Request)).ShouldBeFalse();

        registry._commandHandlers.Keys.ShouldBeEmpty();
        registry._queryHandlers.Keys.ShouldBeEmpty();
    }


    [Fact] public async Task ListenToRegisteredQueryRequest()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(BasicQuery.Handler));
        BasicQuery.Request request = new(123);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;
        registry.AddListener(typeof(BasicQuery.Request), (r, ct) =>
        {
            r.ShouldBe(request);
            ((BasicQuery.Request)r).Value.ShouldBe(123);
            ct.ShouldBe(token);
            return Task.CompletedTask;
        });

        IServiceProvider provider = Substitute.For<IServiceProvider>();

        var response = await registry.Handle(provider, request, token);
        response.ShouldBeOfType<BasicQuery.Response>();
        ((BasicQuery.Response)response).Result.ShouldBe(124);
    }

    [Fact] public async Task HandlerUnhandledQueryRequest()
    {
        MetalChainRegistry registry = new();

        BasicQuery.Request request = new(123);
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;

        IServiceProvider provider = Substitute.For<IServiceProvider>();

        await Should.ThrowAsync<MetalChainException>(() => registry.Handle(provider, request, token));
    }

    [Fact] public async Task MultiRequestTypeObject()
    {
        MetalChainRegistry registry = new();
        registry.AddHandlers(typeof(MultiRequestWithExplicitQuery.Handler));

        Assert.Collection(registry._commandHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(MultiRequestWithExplicitQuery.Request)));
        Assert.Collection(registry._commandHandlers
            .GetValuesOrEmptySet(typeof(MultiRequestWithExplicitQuery.Request)),
            _ => _.ShouldBe(typeof(MultiRequestWithExplicitQuery.Handler)));

        Assert.Collection(registry._queryHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(MultiRequestWithExplicitQuery.Request)));
        Assert.Collection(registry._queryHandlers.Values,
            _ => _.ShouldBe(typeof(MultiRequestWithExplicitQuery.Handler)));

        MultiRequestWithExplicitQuery.Request request = new() { Value = 123 };
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken token = tokenSource.Token;

        IServiceProvider provider = Substitute.For<IServiceProvider>();

        var response = await registry.Handle(provider, request, token);
        response.ShouldBeOfType<MultiRequestWithExplicitQuery.Response>();
        ((MultiRequestWithExplicitQuery.Response)response).Result.ShouldBe(124);
        MultiRequestWithExplicitQuery.Handler.LastValueForCommand.ShouldBe(123);
        MultiRequestWithExplicitQuery.Handler.LastValueForQuery.ShouldBe(123);
    }

    public static class MultiRequestWithExplicitQuery
    {
        public class Request : 
            IRequest, 
            IRequest<Response>
        {
            public int Value { get; set; }
        }

        public class Response
        {
            public int Result { get; set; }
        }

        public class Handler : IRequestHandler<Request>, IRequestHandler<Request, Response>
        {
            public static int LastValueForCommand { get; set; }
            public static int LastValueForQuery { get; set; }

            public Task Handle(Request request, CancellationToken cancellationToken)
            {
                LastValueForCommand = request.Value;
                return Task.CompletedTask;
            }

            Task<Response> IRequestHandler<Request, Response>.Handle(Request request, CancellationToken cancellationToken)
            {
                LastValueForQuery = request.Value;
                return Task.FromResult(new Response { Result = request.Value + 1 });
            }
        }
    }
}
