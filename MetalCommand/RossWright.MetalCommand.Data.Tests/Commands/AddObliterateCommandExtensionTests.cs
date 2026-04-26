using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests.Commands;

/// <summary>
/// Tests for AddObliterateCommand extension method.
/// </summary>
public class AddObliterateCommandExtensionTests
{
    [Fact]
    public void AddObliterateCommand_NullConfigureAction_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddObliterateCommand<TestDbContext>(null);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddObliterateCommand_NullConfigureAction_CreatesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddObliterateCommand<TestDbContext>(null);

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddObliterateCommand_ValidConfiguration_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddObliterateCommand<TestDbContext>(o => { });

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddObliterateCommand_ValidConfiguration_InvokesConfigureAction()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var configureInvoked = false;

        // Act
        builder.AddObliterateCommand<TestDbContext>(o =>
        {
            configureInvoked = true;
        });

        // Assert
        configureInvoked.ShouldBeTrue();
    }

    [Fact]
    public void AddObliterateCommand_CreatesOrReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddObliterateCommand<TestDbContext>(null);

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddObliterateCommand_WithExistingRegistry_ReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        builder.AddObliterateCommand<TestDbContext>(null);

        var registryCountBefore = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Act - Add another obliterate command
        builder.AddObliterateCommand<TestDbContext>(null);

        var registryCountAfter = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Assert - Should not add another registry
        registryCountAfter.ShouldBe(registryCountBefore);
    }

    [Fact]
    public void AddObliterateCommand_ConfigureAction_ModifiesOptions()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        ObliterateCommandOptions? capturedOptions = null;

        // Act
        builder.AddObliterateCommand<TestDbContext>(o =>
        {
            capturedOptions = o;
        });

        // Assert
        capturedOptions.ShouldNotBeNull();
    }

    [Fact]
    public void AddObliterateCommand_MultipleCalls_InvokesConfigureActionEachTime()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var invokeCount = 0;

        // Act
        builder.AddObliterateCommand<TestDbContext>(o => invokeCount++);
        builder.AddObliterateCommand<TestDbContext>(o => invokeCount++);

        // Assert
        invokeCount.ShouldBe(2);
    }
}
