using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace RossWright.MetalInjection.Tests;

public class TestServiceInterfaces
{
    [Fact] public void DetectSingleton()
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns([typeof(SingletonByInterfaceService)]);
        var serviceCollection = new ServiceCollection();
        var service = serviceCollection
            .BuildMetalInjectionServiceProvider(_ => _.ScanAssemblies(mockAssembly));
        var obj1 = service.GetService<ISingletonByInterfaceService>();
        obj1.ShouldNotBeNull();
    }
}

public interface ISingletonByInterfaceService 
{ 
}

public class SingletonByInterfaceService : 
    ISingletonByInterfaceService,
    ISingleton<ISingletonByInterfaceService>
{
}
