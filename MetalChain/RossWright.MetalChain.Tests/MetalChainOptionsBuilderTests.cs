using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace RossWright.MetalChain.Tests;

public class MetalChainOptionsBuilderTests
{
    [Fact] public void HappyPath()
    {
        MetalChainOptionsBuilder builder = new();
        builder.SetDiscoveredConcreteTypesForTesting(
        [
            typeof(BasicCommand.Handler),
            typeof(BasicCommand.Request),
            typeof(BasicQuery.Handler),
            typeof(BasicQuery.Request),
            typeof(InterceptingCommand.Handler),
            typeof(InterceptingCommand.Request),
            typeof(InterceptingQuery.Handler<>),
            typeof(InterceptingQuery.Request<>),
            typeof(OpenCommand.Request<>),
            typeof(OpenCommand.Handler<>)
        ]);
        ServiceCollection serviceCollection = new();
        builder.Initialize(serviceCollection);

        var registryDescriptor = serviceCollection.First(_ => _.ServiceType == typeof(IMetalChainRegistry));
        registryDescriptor.ImplementationInstance.ShouldNotBeNull();
        registryDescriptor.ImplementationInstance.ShouldBeOfType<MetalChainRegistry>();
        var registry = (MetalChainRegistry)registryDescriptor.ImplementationInstance;

        Assert.Collection(registry._commandHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(BasicCommand.Request)),
            _ => _.ShouldBe(typeof(InterceptingCommand.Request)),
            _ => _.ShouldBe(typeof(OpenCommand.Request<>)));

        Assert.Collection(registry._queryHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(BasicQuery.Request)),
            _ => _.ShouldBe(typeof(InterceptingQuery.Request<>)));
    }

    [Fact] public void LateRegistry()
    {
        MetalChainOptionsBuilder builder = new();
        builder.SetDiscoveredConcreteTypesForTesting(
        [
            typeof(BasicCommand.Handler),
            typeof(BasicCommand.Request),
            typeof(BasicQuery.Handler),
            typeof(BasicQuery.Request),
        ]);
        ServiceCollection serviceCollection = new();
        builder.Initialize(serviceCollection);

        var registryDescriptor = serviceCollection.First(_ => _.ServiceType == typeof(IMetalChainRegistry));
        registryDescriptor.ImplementationInstance.ShouldNotBeNull();
        registryDescriptor.ImplementationInstance.ShouldBeOfType<MetalChainRegistry>();
        var registry = (MetalChainRegistry)registryDescriptor.ImplementationInstance;
        Assert.Collection(registry._commandHandlers.Keys,
            _ => _.ShouldBe(typeof(BasicCommand.Request)));
        Assert.Collection(registry._queryHandlers.Keys,
            _ => _.ShouldBe(typeof(BasicQuery.Request)));

        serviceCollection.AddMetalChainHandlers(typeof(InterceptingCommand.Handler));        
        Assert.Collection(registry._commandHandlers.Keys.OrderBy(_ => _.FullName),
            _ => _.ShouldBe(typeof(BasicCommand.Request)),
            _ => _.ShouldBe(typeof(InterceptingCommand.Request)));
        Assert.Collection(registry._queryHandlers.Keys,
            _ => _.ShouldBe(typeof(BasicQuery.Request)));
    }

    [Fact] public async Task BuilderToMediatorIntegration()
    {
        MetalChainOptionsBuilder builder = new();
        builder.SetDiscoveredConcreteTypesForTesting(
        [
            typeof(BasicCommand.Handler)
        ]);
        ServiceCollection serviceCollection = new();
        builder.Initialize(serviceCollection);
        var provider = serviceCollection.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        await mediator.Send(new BasicCommand.Request(123));
        BasicCommand.Handler.LastValue.ShouldBe(123);
    }

    [Fact]
    public void AddMetalChain_CalledTwice_DoesNotDoubleRegisterHandlers()
    {
        var services = new ServiceCollection();

        var builder1 = new MetalChainOptionsBuilder();
        builder1.SetDiscoveredConcreteTypesForTesting([typeof(BasicCommand.Handler)]);
        builder1.Initialize(services);

        // Second call — should reuse the existing registry, not create a second one
        var builder2 = new MetalChainOptionsBuilder();
        builder2.SetDiscoveredConcreteTypesForTesting([typeof(BasicCommand.Handler)]);
        builder2.Initialize(services);

        services.Count(d => d.ServiceType == typeof(IMediator)).ShouldBe(1);
        services.Count(d => d.ServiceType == typeof(IMetalChainRegistry)).ShouldBe(1);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        mediator.HasHandlerFor<BasicCommand.Request>().ShouldBeTrue();
    }

    [Fact]
    public async Task AddMetalChain_CalledTwice_HandlerInvokedOnlyOnce()
    {
        var services = new ServiceCollection();

        var builder1 = new MetalChainOptionsBuilder();
        builder1.SetDiscoveredConcreteTypesForTesting([typeof(BasicCommand.Handler)]);
        builder1.Initialize(services);

        var builder2 = new MetalChainOptionsBuilder();
        builder2.SetDiscoveredConcreteTypesForTesting([typeof(BasicCommand.Handler)]);
        builder2.Initialize(services);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new BasicCommand.Request(999));
        BasicCommand.Handler.LastValue.ShouldBe(999);
    }
}