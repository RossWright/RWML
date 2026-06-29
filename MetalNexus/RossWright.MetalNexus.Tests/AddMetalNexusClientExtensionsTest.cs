using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalChain;
using Shouldly;

namespace RossWright.MetalNexus.Tests;

public class AddMetalNexusClientExtensionsTest
{
    [Fact] public void DontRegisterMetalChainIfAlreadyRegistered()
    {
        var asm = TestHelper.SetupAssemblyWithTypes(typeof(EmptyCommand));
        ServiceCollection services = new ServiceCollection();
        services.AddMetalChain(_ => _.ScanAssembly(asm));
        services.AddMetalNexusClient(_ => _.ScanAssembly(asm));
        services.ShouldContain(_ => _.ServiceType == typeof(IMediator));
    }

    [Fact] public void RegisterMetalChainIfNotRegistered()
    {
        var asm = TestHelper.SetupAssemblyWithTypes(typeof(EmptyCommand));
        ServiceCollection services = new ServiceCollection();
        services.AddMetalNexusClient(config => config.ScanAssembly(asm));
        services.ShouldContain(_ => _.ServiceType == typeof(IMediator));
    }
}
