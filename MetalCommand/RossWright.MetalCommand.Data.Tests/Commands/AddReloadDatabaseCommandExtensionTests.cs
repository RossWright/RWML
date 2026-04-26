using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests.Commands;

/// <summary>
/// Tests for AddReloadDatabaseCommand extension method.
/// </summary>
public class AddReloadDatabaseCommandExtensionTests
{
    [Fact]
    public void AddReloadDatabaseCommand_NullConfigure_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddReloadDatabaseCommand<TestDbContext>(null);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddReloadDatabaseCommand_WithConfigure_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddReloadDatabaseCommand<TestDbContext>(o => { });

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddReloadDatabaseCommand_NullConfigure_CreatesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddReloadDatabaseCommand<TestDbContext>(null);

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddReloadDatabaseCommand_WithExistingRegistry_ReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        builder.AddReloadDatabaseCommand<TestDbContext>(null);

        var registryCountBefore = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Act - Add another reload database command
        builder.AddReloadDatabaseCommand<TestDbContext>(null);

        var registryCountAfter = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Assert - Should not add another registry
        registryCountAfter.ShouldBe(registryCountBefore);
    }

    [Fact]
    public void AddReloadDatabaseCommand_WithConfigure_InvokesConfigureAction()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var configureInvoked = false;

        // Act
        builder.AddReloadDatabaseCommand<TestDbContext>(o =>
        {
            configureInvoked = true;
        });

        // Assert
        configureInvoked.ShouldBeTrue();
    }

    [Fact]
    public void AddReloadDatabaseCommand_NullConfigure_CreatesOptions()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddReloadDatabaseCommand<TestDbContext>(null);

        // Assert
        var registry = builder.Services
            .Where(d => d.ServiceType == typeof(ICommandOptionsRegistry))
            .Select(d => d.ImplementationInstance as DataCommandOptionsRegistry)
            .FirstOrDefault(r => r != null);

        registry.ShouldNotBeNull();
        var options = registry.Get(typeof(ReloadDatabaseCommand<TestDbContext>));
        options.ShouldNotBeNull();
        options.ShouldBeOfType<ReloadDatabaseCommandOptions>();
    }

    [Fact]
    public void AddReloadDatabaseCommand_WithConfigure_OptionsAreMutable()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddReloadDatabaseCommand<TestDbContext>(o =>
        {
            o.Invocations = ["custom-reload"];
        });

        // Assert
        var registry = builder.Services
            .Where(d => d.ServiceType == typeof(ICommandOptionsRegistry))
            .Select(d => d.ImplementationInstance as DataCommandOptionsRegistry)
            .FirstOrDefault(r => r != null);

        registry.ShouldNotBeNull();
        var options = registry.Get(typeof(ReloadDatabaseCommand<TestDbContext>));
        options.ShouldNotBeNull();
        options.Invocations.ShouldNotBeNull();
        options.Invocations.ShouldBe(["custom-reload"]);
    }

    [Fact]
    public void AddReloadDatabaseCommand_CalledMultipleTimes_ReusesOptions()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act - First call
        builder.AddReloadDatabaseCommand<TestDbContext>(o =>
        {
            o.Invocations = ["first"];
        });

        // Act - Second call
        builder.AddReloadDatabaseCommand<TestDbContext>(o =>
        {
            o.Invocations = ["second"];
        });

        // Assert - Second call should have modified the same options instance
        var registry = builder.Services
            .Where(d => d.ServiceType == typeof(ICommandOptionsRegistry))
            .Select(d => d.ImplementationInstance as DataCommandOptionsRegistry)
            .FirstOrDefault(r => r != null);

        registry.ShouldNotBeNull();
        var options = registry.Get(typeof(ReloadDatabaseCommand<TestDbContext>));
        options.ShouldNotBeNull();
        options.Invocations.ShouldNotBeNull();
        options.Invocations.ShouldBe(["second"]);
    }

    [Fact]
    public void AddReloadDatabaseCommand_WithConfigure_SetsEnvironmentPolicy()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddReloadDatabaseCommand<TestDbContext>(o =>
        {
            o.EnvironmentPolicy = EnvironmentPolicy.Dangerous;
        });

        // Assert
        var registry = builder.Services
            .Where(d => d.ServiceType == typeof(ICommandOptionsRegistry))
            .Select(d => d.ImplementationInstance as DataCommandOptionsRegistry)
            .FirstOrDefault(r => r != null);

        registry.ShouldNotBeNull();
        var options = registry.Get(typeof(ReloadDatabaseCommand<TestDbContext>));
        options.ShouldNotBeNull();
        options.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Dangerous);
    }
}
