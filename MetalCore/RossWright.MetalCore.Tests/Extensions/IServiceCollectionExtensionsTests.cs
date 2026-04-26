using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace RossWright;

public class IServiceCollectionExtensionsTests
{
    // ── HasService<T> ──────────────────────────────────────────────────────────────
    [Fact]
    public void HasService_RegisteredService_ReturnsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestServiceImpl>();

        // Act
        var result = services.HasService<ITestService>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasService_UnregisteredService_ReturnsFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.HasService<ITestService>();

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void HasService_MultipleServicesOfSameType_ReturnsTrue()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestServiceImpl>();
        services.AddSingleton<ITestService, AnotherTestServiceImpl>();

        // Act
        var result = services.HasService<ITestService>();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void HasService_ChecksServiceTypeNotImplementationType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestServiceImpl>();

        // Act
        var hasInterface = services.HasService<ITestService>();
        var hasImplementation = services.HasService<TestServiceImpl>();

        // Assert
        hasInterface.ShouldBeTrue();
        hasImplementation.ShouldBeFalse();
    }

    [Fact]
    public void HasService_EmptyServiceCollection_ReturnsFalse()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.HasService<ITestService>();

        // Assert
        result.ShouldBeFalse();
    }

    // ── AddScopedAlias<TService, TAliasOf> ────────────────────────────────────────
    [Fact]
    public void AddScopedAlias_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestServiceImpl>();

        // Act
        var result = services.AddScopedAlias<ITestService, TestServiceImpl>();

        // Assert
        result.ShouldBeSameAs(services);
    }

    [Fact]
    public void AddScopedAlias_RegistersServiceWithScopedLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestServiceImpl>();

        // Act
        services.AddScopedAlias<ITestService, TestServiceImpl>();

        // Assert
        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ITestService));
        descriptor.ShouldNotBeNull();
        descriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddScopedAlias_UsesImplementationFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestServiceImpl>();

        // Act
        services.AddScopedAlias<ITestService, TestServiceImpl>();

        // Assert
        var descriptor = services.First(d => d.ServiceType == typeof(ITestService));
        descriptor.ImplementationFactory.ShouldNotBeNull();
        descriptor.ImplementationType.ShouldBeNull();
        descriptor.ImplementationInstance.ShouldBeNull();
    }

    [Fact]
    public void AddScopedAlias_FactoryCastsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScopedAlias<ITestService, TestServiceImpl>();
        var descriptor = services.First(d => d.ServiceType == typeof(ITestService));
        
        var mockProvider = Substitute.For<IServiceProvider>();
        var impl = new TestServiceImpl();
        mockProvider.GetService(typeof(TestServiceImpl)).Returns(impl);

        // Act
        var result = descriptor.ImplementationFactory!(mockProvider);

        // Assert
        result.ShouldBeSameAs(impl);
    }

    [Fact]
    public void AddScopedAlias_AddsServiceDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestServiceImpl>();

        // Act
        services.AddScopedAlias<ITestService, TestServiceImpl>();

        // Assert
        services.HasService<ITestService>().ShouldBeTrue();
    }

    [Fact]
    public void AddScopedAlias_CanBeCalledMultipleTimesForDifferentAliases()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestServiceImpl>();

        // Act
        services.AddScopedAlias<ITestService, TestServiceImpl>();
        services.AddScopedAlias<IAnotherTestService, TestServiceImpl>();

        // Assert
        services.HasService<ITestService>().ShouldBeTrue();
        services.HasService<IAnotherTestService>().ShouldBeTrue();
    }

    [Fact]
    public void AddScopedAlias_WithoutPriorRegistration_AddsDescriptor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddScopedAlias<ITestService, TestServiceImpl>();

        // Assert
        services.HasService<ITestService>().ShouldBeTrue();
    }

    [Fact]
    public void AddScopedAlias_FactoryUsesGetRequiredService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScopedAlias<ITestService, TestServiceImpl>();
        var descriptor = services.First(d => d.ServiceType == typeof(ITestService));
        
        var mockProvider = Substitute.For<IServiceProvider>();
        var impl = new TestServiceImpl();
        mockProvider.GetService(typeof(TestServiceImpl)).Returns(impl);

        // Act
        var result = descriptor.ImplementationFactory!(mockProvider);

        // Assert
        mockProvider.Received(1).GetService(typeof(TestServiceImpl));
        result.ShouldBeSameAs(impl);
    }

    [Fact]
    public void AddScopedAlias_WithMultipleInterfaces_CastsToCorrectType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScopedAlias<ITestService, TestServiceImpl>();
        var descriptor = services.First(d => d.ServiceType == typeof(ITestService));
        
        var mockProvider = Substitute.For<IServiceProvider>();
        var impl = new TestServiceImpl();
        mockProvider.GetService(typeof(TestServiceImpl)).Returns(impl);

        // Act
        var result = descriptor.ImplementationFactory!(mockProvider) as ITestService;

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeSameAs(impl);
    }

    private interface ITestService { }
    private interface IAnotherTestService { }
    private class TestServiceImpl : ITestService, IAnotherTestService { }
    private class AnotherTestServiceImpl : ITestService { }
}

public class AddServicesTests
{
    [Fact]
    public void AddServices_MultipleConfigurators_AllApplied()
    {
        var services = new ServiceCollection();
        var configurators = new List<Action<IServiceCollection>>
        {
            s => s.AddSingleton<IFooService, FooImpl>(),
            s => s.AddSingleton<IBarService, BarImpl>(),
        };
        services.AddServices(configurators);
        services.HasService<IFooService>().ShouldBeTrue();
        services.HasService<IBarService>().ShouldBeTrue();
    }

    [Fact]
    public void AddServices_EmptyList_ServiceCollectionUnchanged()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFooService, FooImpl>();
        var initialCount = services.Count;
        services.AddServices(new List<Action<IServiceCollection>>());
        services.Count.ShouldBe(initialCount);
    }

    private interface IFooService { }
    private interface IBarService { }
    private class FooImpl : IFooService { }
    private class BarImpl : IBarService { }
}
