using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace RossWright.MetalChain.Tests;

// ── Assembly scanning runtime behavior ─────────────────────────────────────────
// Uses SetDiscoveredConcreteTypesForTesting to provide explicit types to the
// scanner, verifying that AddMetalChain wires up a real ServiceCollection,
// builds a provider that resolves IMediator, and dispatches correctly.
// (ScanAssemblyContaining cannot be aimed at the test assembly because multiple
// test classes each define their own IRequestHandler<BasicCommand.Request>,
// which would cause a duplicate-handler exception at startup.)

public class MetalChainAssemblyScanningTests
{
    private sealed class ScanCommand : IRequest { }
    private sealed class ScanCommandHandler : IRequestHandler<ScanCommand>
    {
        public static int InvocationCount;
        public Task Handle(ScanCommand request, CancellationToken cancellationToken = default)
        { InvocationCount++; return Task.CompletedTask; }
    }

    private sealed class ScanQuery : IRequest<int> { }
    private sealed class ScanQueryHandler : IRequestHandler<ScanQuery, int>
    {
        public Task<int> Handle(ScanQuery request, CancellationToken cancellationToken = default)
            => Task.FromResult(42);
    }

    [Fact]
    public async Task AddMetalChain_WithDiscoveredCommandHandler_SendSucceeds()
    {
        ScanCommandHandler.InvocationCount = 0;

        var services = new ServiceCollection();
        var builder = new MetalChainOptionsBuilder();
        builder.SetDiscoveredConcreteTypesForTesting([typeof(ScanCommand), typeof(ScanCommandHandler)]);
        builder.Initialize(services);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        await mediator.Send(new ScanCommand());

        ScanCommandHandler.InvocationCount.ShouldBe(1);
    }

    [Fact]
    public async Task AddMetalChain_WithDiscoveredQueryHandler_SendReturnsResponse()
    {
        var services = new ServiceCollection();
        var builder = new MetalChainOptionsBuilder();
        builder.SetDiscoveredConcreteTypesForTesting([typeof(ScanQuery), typeof(ScanQueryHandler)]);
        builder.Initialize(services);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.Send(new ScanQuery());

        result.ShouldBe(42);
    }

    [Fact]
    public void AddMetalChain_HandlerDiscovered_HasHandlerForReturnsTrue()
    {
        var services = new ServiceCollection();
        var builder = new MetalChainOptionsBuilder();
        builder.SetDiscoveredConcreteTypesForTesting([typeof(ScanCommand), typeof(ScanCommandHandler)]);
        builder.Initialize(services);

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        mediator.HasHandlerFor<ScanCommand>().ShouldBeTrue();
        mediator.HasHandlerFor<ScanQuery>().ShouldBeFalse();
    }
}
