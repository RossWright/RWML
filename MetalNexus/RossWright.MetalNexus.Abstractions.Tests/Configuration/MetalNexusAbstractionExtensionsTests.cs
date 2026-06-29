using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalNexus.Internal;
using RossWright.MetalNexus.Schema;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Configuration;

public class MetalNexusAbstractionExtensionsTests
{
    [Fact]
    public void AddMetalNexusEndpoints_WhenRegistryExists_CallsAddEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = Substitute.For<IMetalNexusRegistry>();
        services.AddSingleton(registry);
        var types = new[] { typeof(string), typeof(int) };

        // Act
        services.AddMetalNexusEndpoints(types);

        // Assert
        registry.Received(1).AddEndpoints(types);
    }

    [Fact]
    public void AddMetalNexusEndpoints_WhenRegistryDoesNotExist_AddsMetalNexusPreLoads()
    {
        // Arrange
        var services = new ServiceCollection();
        var types = new[] { typeof(string), typeof(int) };

        // Act
        services.AddMetalNexusEndpoints(types);

        // Assert
        var preLoadsService = services.FirstOrDefault(s => s.ServiceType == typeof(MetalNexusPreLoads));
        preLoadsService.ShouldNotBeNull();
        preLoadsService.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        preLoadsService.ImplementationInstance.ShouldBeOfType<MetalNexusPreLoads>();
    }

    [Fact]
    public void AddMetalNexusEndpoints_WhenRegistryExistsButNotAsInstance_AddsMetalNexusPreLoads()
    {
        // Arrange
        var services = new ServiceCollection();
        var registry = Substitute.For<IMetalNexusRegistry>();
        services.AddSingleton<IMetalNexusRegistry>(_ => registry);
        var types = new[] { typeof(string), typeof(int) };

        // Act
        services.AddMetalNexusEndpoints(types);

        // Assert
        var preLoadsService = services.FirstOrDefault(s => s.ServiceType == typeof(MetalNexusPreLoads));
        preLoadsService.ShouldNotBeNull();
        preLoadsService.Lifetime.ShouldBe(ServiceLifetime.Singleton);
        preLoadsService.ImplementationInstance.ShouldBeOfType<MetalNexusPreLoads>();
        registry.DidNotReceive().AddEndpoints(Arg.Any<Type[]>());
    }
}
