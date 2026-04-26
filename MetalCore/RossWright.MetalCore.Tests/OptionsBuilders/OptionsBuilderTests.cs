using Microsoft.Extensions.DependencyInjection;

namespace RossWright;

public class OptionsBuilderTests
{
    // ── IOptionsBuilder.AddServices(Action<IServiceCollection>) ───────────────────
    [Fact]
    public void AddServices_SingleAction_AddsToInternalList()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        Action<IServiceCollection> action = _ => { };

        // Act
        builder.AddServices(action);

        // Assert
        builder.GetRegistrations().ShouldContain(action);
    }

    [Fact]
    public void AddServices_MultipleActions_AddsAllToInternalList()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        Action<IServiceCollection> action1 = _ => { };
        Action<IServiceCollection> action2 = _ => { };
        Action<IServiceCollection> action3 = _ => { };

        // Act
        builder.AddServices(action1);
        builder.AddServices(action2);
        builder.AddServices(action3);

        // Assert
        var registrations = builder.GetRegistrations();
        registrations.ShouldContain(action1);
        registrations.ShouldContain(action2);
        registrations.ShouldContain(action3);
        registrations.Count.ShouldBe(3);
    }

    [Fact]
    public void AddServices_MultipleCallsWithSameAction_AddsActionMultipleTimes()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        Action<IServiceCollection> action = _ => { };

        // Act
        builder.AddServices(action);
        builder.AddServices(action);

        // Assert
        var registrations = builder.GetRegistrations();
        registrations.Count.ShouldBe(2);
    }

    // ── OptionsBuilder.AddServices(IServiceCollection) ────────────────────────────
    [Fact]
    public void AddServicesProtected_WithRegisteredActions_ExecutesAllActions()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        var services = new ServiceCollection();
        var callCount = 0;

        builder.AddServices(_ => callCount++);
        builder.AddServices(_ => callCount++);
        builder.AddServices(_ => callCount++);

        // Act
        builder.InvokeAddServices(services);

        // Assert
        callCount.ShouldBe(3);
    }

    [Fact]
    public void AddServicesProtected_WithRegisteredActions_PassesServiceCollectionToActions()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        var services = new ServiceCollection();
        IServiceCollection? receivedServices = null;

        builder.AddServices(s => receivedServices = s);

        // Act
        builder.InvokeAddServices(services);

        // Assert
        receivedServices.ShouldBeSameAs(services);
    }

    [Fact]
    public void AddServicesProtected_WithNoRegistrations_DoesNothing()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        var services = new ServiceCollection();
        var initialCount = services.Count;

        // Act
        builder.InvokeAddServices(services);

        // Assert
        services.Count.ShouldBe(initialCount);
    }

    [Fact]
    public void AddServicesProtected_WithServiceRegistration_AddsServiceToCollection()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        var services = new ServiceCollection();

        builder.AddServices(s => s.AddSingleton<ITestService, TestServiceImpl>());

        // Act
        builder.InvokeAddServices(services);

        // Assert
        services.HasService<ITestService>().ShouldBeTrue();
    }

    [Fact]
    public void AddServicesProtected_WithMultipleServiceRegistrations_AddsAllServices()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        var services = new ServiceCollection();

        builder.AddServices(s => s.AddSingleton<ITestService, TestServiceImpl>());
        builder.AddServices(s => s.AddScoped<ITestService2, TestService2Impl>());
        builder.AddServices(s => s.AddTransient<ITestService3, TestService3Impl>());

        // Act
        builder.InvokeAddServices(services);

        // Assert
        services.HasService<ITestService>().ShouldBeTrue();
        services.HasService<ITestService2>().ShouldBeTrue();
        services.HasService<ITestService3>().ShouldBeTrue();
    }

    [Fact]
    public void AddServicesProtected_ExecutesActionsInOrder()
    {
        // Arrange
        var builder = new TestableOptionsBuilder();
        var services = new ServiceCollection();
        var executionOrder = new List<int>();

        builder.AddServices(_ => executionOrder.Add(1));
        builder.AddServices(_ => executionOrder.Add(2));
        builder.AddServices(_ => executionOrder.Add(3));

        // Act
        builder.InvokeAddServices(services);

        // Assert
        executionOrder.ShouldBe(new[] { 1, 2, 3 });
    }

    // ── Test Helper Classes ────────────────────────────────────────────────────────
    private class TestableOptionsBuilder : OptionsBuilder
    {
        public List<Action<IServiceCollection>> GetRegistrations()
        {
            var field = typeof(OptionsBuilder).GetField("_registrations",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (List<Action<IServiceCollection>>)field!.GetValue(this)!;
        }

        public void InvokeAddServices(IServiceCollection services)
        {
            AddServices(services);
        }
    }

    private interface ITestService { }
    private class TestServiceImpl : ITestService { }
    private interface ITestService2 { }
    private class TestService2Impl : ITestService2 { }
    private interface ITestService3 { }
    private class TestService3Impl : ITestService3 { }
}
