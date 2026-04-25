using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace RossWright;

public class ServiceCollectionExtensionTests
{
    // ── HasService<T> / HasService(Type) ──────────────────────────────────────────
    [Fact] public void HasServiceGeneric_RegisteredType_ReturnsTrue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IServiceCollectionTestService, ServiceCollectionTestImpl>();
        services.HasService<IServiceCollectionTestService>().ShouldBeTrue();
    }

    [Fact] public void HasServiceGeneric_UnregisteredType_ReturnsFalse()
    {
        var services = new ServiceCollection();
        services.HasService<IServiceCollectionTestService>().ShouldBeFalse();
    }

    [Fact] public void HasServiceByType_RegisteredType_ReturnsTrue()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IServiceCollectionTestService, ServiceCollectionTestImpl>();
        services.HasService(typeof(IServiceCollectionTestService)).ShouldBeTrue();
    }

    [Fact] public void HasServiceByType_UnregisteredType_ReturnsFalse()
    {
        var services = new ServiceCollection();
        services.HasService(typeof(IServiceCollectionTestService)).ShouldBeFalse();
    }

    // ── AddScopedAlias ────────────────────────────────────────────────────────────
    [Fact] public void AddScopedAlias_AddsDescriptorWithCorrectLifetime()
    {
        var services = new ServiceCollection();
        services.AddScoped<ServiceCollectionTestImpl>();
        services.AddScopedAlias<IServiceCollectionTestService, ServiceCollectionTestImpl>();

        var aliasDescriptor = services.FirstOrDefault(_ => _.ServiceType == typeof(IServiceCollectionTestService));
        aliasDescriptor.ShouldNotBeNull();
        aliasDescriptor.Lifetime.ShouldBe(ServiceLifetime.Scoped);
    }

    [Fact] public void AddScopedAlias_FactoryReturnsAliasedInstance()
    {
        var services = new ServiceCollection();
        services.AddScopedAlias<IServiceCollectionTestService, ServiceCollectionTestImpl>();

        var aliasDescriptor = services.First(_ => _.ServiceType == typeof(IServiceCollectionTestService));
        var impl = new ServiceCollectionTestImpl();
        var mockProvider = Substitute.For<IServiceProvider>();
        mockProvider.GetService(typeof(ServiceCollectionTestImpl)).Returns((object?)impl);

        var result = aliasDescriptor.ImplementationFactory!(mockProvider);
        result.ShouldBeSameAs(impl);
    }

    private interface IServiceCollectionTestService { }
    private class ServiceCollectionTestImpl : IServiceCollectionTestService { }
}

// ── P3-C: AddServices(IEnumerable<Action<IServiceCollection>>) ────────────────
public class AddServicesTests
{
    [Fact] public void AddServices_MultipleConfigurators_AllApplied()
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

    [Fact] public void AddServices_EmptyList_ServiceCollectionUnchanged()
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
