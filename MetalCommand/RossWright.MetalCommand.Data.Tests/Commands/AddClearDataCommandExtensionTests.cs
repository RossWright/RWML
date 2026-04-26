using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests.Commands;

/// <summary>
/// Tests for AddClearDataCommand extension method.
/// </summary>
public class AddClearDataCommandExtensionTests
{
    [Fact]
    public void AddClearDataCommand_NullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            builder.AddClearDataCommand<TestDbContext>(null!));
    }

    [Fact]
    public void AddClearDataCommand_BothClearDataAndTableNamesNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
            builder.AddClearDataCommand<TestDbContext>(o => { }));

        exception.Message.ShouldContain("Either ClearData or TableNames must be set.");
    }

    [Fact]
    public void AddClearDataCommand_WithTableNames_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = new[] { "Table1" });

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddClearDataCommand_WithTableNames_AddsOptionsToServices()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = new[] { "Table1" });

        // Assert
        var services = builder.Services;
        services.ShouldContain(d =>
            d.ServiceType == typeof(ClearDataCommandOptions<TestDbContext>) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddClearDataCommand_WithTableNames_InvokesConfigureAction()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var configureInvoked = false;

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
        {
            configureInvoked = true;
            o.TableNames = new[] { "Table1" };
        });

        // Assert
        configureInvoked.ShouldBeTrue();
    }

    [Fact]
    public void AddClearDataCommand_WithTableNames_SetsClearDataFromTableNames()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var tableNames = new[] { "Table1", "Table2" };

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = tableNames);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<ClearDataCommandOptions<TestDbContext>>();
        options.ClearData.ShouldNotBeNull();
        options.TableNames.ShouldBe(tableNames);
    }

    [Fact]
    public void AddClearDataCommand_WithClearData_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddClearDataCommand<TestDbContext>(o =>
            o.ClearData = _ => Task.CompletedTask);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddClearDataCommand_WithClearData_AddsOptionsToServices()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.ClearData = _ => Task.CompletedTask);

        // Assert
        var services = builder.Services;
        services.ShouldContain(d =>
            d.ServiceType == typeof(ClearDataCommandOptions<TestDbContext>) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddClearDataCommand_WithClearData_DoesNotOverwriteClearData()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        Func<ClearDataCommandContext<TestDbContext>, Task> clearDataFunc = _ => Task.CompletedTask;

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.ClearData = clearDataFunc);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<ClearDataCommandOptions<TestDbContext>>();
        options.ClearData.ShouldBe(clearDataFunc);
    }

    [Fact]
    public void AddClearDataCommand_WithBothClearDataAndTableNames_UsesClearData()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        Func<ClearDataCommandContext<TestDbContext>, Task> clearDataFunc = _ => Task.CompletedTask;

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
        {
            o.ClearData = clearDataFunc;
            o.TableNames = new[] { "Table1" };
        });

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<ClearDataCommandOptions<TestDbContext>>();
        options.ClearData.ShouldBe(clearDataFunc);
    }

    [Fact]
    public void AddClearDataCommand_CreatesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = new[] { "Table1" });

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddClearDataCommand_WithExistingRegistry_ReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = new[] { "Table1" });

        var registryCountBefore = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Act - Add another clear data command
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = new[] { "Table2" });

        var registryCountAfter = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Assert - Should not add another registry
        registryCountAfter.ShouldBe(registryCountBefore);
    }

    [Fact]
    public void AddClearDataCommand_WithMultipleTableNames_ConfiguresCorrectly()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var tableNames = new[] { "Table1", "Table2", "Table3" };

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
            o.TableNames = tableNames);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<ClearDataCommandOptions<TestDbContext>>();
        options.TableNames.ShouldBe(tableNames);
        options.ClearData.ShouldNotBeNull();
    }

    [Fact]
    public void AddClearDataCommand_ConfiguresCommandOptions()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddClearDataCommand<TestDbContext>(o =>
        {
            o.TableNames = new[] { "Table1" };
            o.Invocations = new[] { "custom-clear" };
            o.EnvironmentPolicy = EnvironmentPolicy.Dangerous;
        });

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<ClearDataCommandOptions<TestDbContext>>();
        options.Invocations.ShouldBe(new[] { "custom-clear" });
        options.EnvironmentPolicy.ShouldBe(EnvironmentPolicy.Dangerous);
    }
}
