using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests.Commands;

/// <summary>
/// Tests for AddMigrateCommand extension method.
/// </summary>
public class AddMigrateCommandExtensionTests
{
    [Fact]
    public void AddMigrateCommand_NullConfigureAction_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddMigrateCommand<TestDbContext>(null);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddMigrateCommand_NullConfigureAction_AddsOptionsToServices()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddMigrateCommand<TestDbContext>(null);

        // Assert
        var services = builder.Services;
        services.ShouldContain(d =>
            d.ServiceType == typeof(MigrateCommandOptions<TestDbContext>) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddMigrateCommand_NullConfigureAction_CreatesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddMigrateCommand<TestDbContext>(null);

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddMigrateCommand_ValidConfiguration_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddMigrateCommand<TestDbContext>(o => { });

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddMigrateCommand_ValidConfiguration_AddsOptionsToServices()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddMigrateCommand<TestDbContext>(o => { });

        // Assert
        var services = builder.Services;
        services.ShouldContain(d =>
            d.ServiceType == typeof(MigrateCommandOptions<TestDbContext>) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddMigrateCommand_ValidConfiguration_InvokesConfigureAction()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var configureInvoked = false;

        // Act
        builder.AddMigrateCommand<TestDbContext>(o =>
        {
            configureInvoked = true;
        });

        // Assert
        configureInvoked.ShouldBeTrue();
    }

    [Fact]
    public void AddMigrateCommand_ValidConfiguration_SetsPreMigrationProperty()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        Func<DataCommandContext<TestDbContext>, Task> preMigrationFunc = _ => Task.CompletedTask;

        // Act
        builder.AddMigrateCommand<TestDbContext>(o =>
            o.PreMigration = preMigrationFunc);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<MigrateCommandOptions<TestDbContext>>();
        options.PreMigration.ShouldBe(preMigrationFunc);
    }

    [Fact]
    public void AddMigrateCommand_ValidConfiguration_SetsPostMigrationProperty()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        Func<DataCommandContext<TestDbContext>, Task> postMigrationFunc = _ => Task.CompletedTask;

        // Act
        builder.AddMigrateCommand<TestDbContext>(o =>
            o.PostMigration = postMigrationFunc);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<MigrateCommandOptions<TestDbContext>>();
        options.PostMigration.ShouldBe(postMigrationFunc);
    }

    [Fact]
    public void AddMigrateCommand_CreatesOrReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddMigrateCommand<TestDbContext>(null);

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddMigrateCommand_WithExistingRegistry_ReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        builder.AddMigrateCommand<TestDbContext>(null);

        var registryCountBefore = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Act - Add another migrate command
        builder.AddMigrateCommand<TestDbContext>(null);

        var registryCountAfter = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Assert - Should not add another registry
        registryCountAfter.ShouldBe(registryCountBefore);
    }

    [Fact]
    public void AddMigrateCommand_ValidConfiguration_ConfiguresMultipleProperties()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        Func<DataCommandContext<TestDbContext>, Task> preMigrationFunc = _ => Task.CompletedTask;
        Func<DataCommandContext<TestDbContext>, Task> postMigrationFunc = _ => Task.CompletedTask;

        // Act
        builder.AddMigrateCommand<TestDbContext>(o =>
        {
            o.PreMigration = preMigrationFunc;
            o.PostMigration = postMigrationFunc;
        });

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<MigrateCommandOptions<TestDbContext>>();
        options.PreMigration.ShouldBe(preMigrationFunc);
        options.PostMigration.ShouldBe(postMigrationFunc);
    }
}
