using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests.Commands;

/// <summary>
/// Tests for AddLoadDataCommand extension method.
/// </summary>
public class AddLoadDataCommandExtensionTests
{
    [Fact]
    public void AddLoadDataCommand_NullConfigure_ThrowsArgumentNullException()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            builder.AddLoadDataCommand<TestDbContext>(null!));
    }

    [Fact]
    public void AddLoadDataCommand_ConfigureWithoutLoadData_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() =>
            builder.AddLoadDataCommand<TestDbContext>(o => { }));

        ex.Message.ShouldContain("LoadData");
        ex.Message.ShouldContain("must be set");
    }

    [Fact]
    public void AddLoadDataCommand_ValidConfiguration_ReturnsBuilder()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        var result = builder.AddLoadDataCommand<TestDbContext>(o =>
            o.LoadData = _ => Task.CompletedTask);

        // Assert
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddLoadDataCommand_ValidConfiguration_AddsOptionsToServices()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddLoadDataCommand<TestDbContext>(o =>
            o.LoadData = _ => Task.CompletedTask);

        // Assert
        var services = builder.Services;
        services.ShouldContain(d =>
            d.ServiceType == typeof(LoadDataCommandOptions<TestDbContext>) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddLoadDataCommand_ValidConfiguration_InvokesConfigureAction()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var configureInvoked = false;

        // Act
        builder.AddLoadDataCommand<TestDbContext>(o =>
        {
            configureInvoked = true;
            o.LoadData = _ => Task.CompletedTask;
        });

        // Assert
        configureInvoked.ShouldBeTrue();
    }

    [Fact]
    public void AddLoadDataCommand_ValidConfiguration_SetsLoadDataProperty()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        Func<LoadDataCommandContext<TestDbContext>, Task> loadDataFunc = _ => Task.CompletedTask;

        // Act
        builder.AddLoadDataCommand<TestDbContext>(o =>
            o.LoadData = loadDataFunc);

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<LoadDataCommandOptions<TestDbContext>>();
        options.LoadData.ShouldBe(loadDataFunc);
    }

    [Fact]
    public void AddLoadDataCommand_ValidConfiguration_SetsLoadFilepathProperty()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        var filepath = "path/to/files";

        // Act
        builder.AddLoadDataCommand<TestDbContext>(o =>
        {
            o.LoadData = _ => Task.CompletedTask;
            o.LoadFilepath = filepath;
        });

        // Assert
        var serviceProvider = builder.Services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<LoadDataCommandOptions<TestDbContext>>();
        options.LoadFilepath.ShouldBe(filepath);
    }

    [Fact]
    public void AddLoadDataCommand_CreatesOrReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act
        builder.AddLoadDataCommand<TestDbContext>(o =>
            o.LoadData = _ => Task.CompletedTask);

        // Assert
        var registryServices = builder.Services.Where(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry) &&
            d.ImplementationInstance is DataCommandOptionsRegistry);

        registryServices.ShouldNotBeEmpty();
    }

    [Fact]
    public void AddLoadDataCommand_WithExistingRegistry_ReusesRegistry()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();
        builder.AddLoadDataCommand<TestDbContext>(o =>
            o.LoadData = _ => Task.CompletedTask);

        var registryCountBefore = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Act - Add another load data command
        builder.AddLoadDataCommand<TestDbContext>(o =>
            o.LoadData = _ => Task.CompletedTask);

        var registryCountAfter = builder.Services.Count(d =>
            d.ServiceType == typeof(ICommandOptionsRegistry));

        // Assert - Should not add another registry
        registryCountAfter.ShouldBe(registryCountBefore);
    }

    [Fact]
    public void AddLoadDataCommand_ConfigureActionWithNullLoadData_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = ConsoleApplication.CreateBuilder();

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() =>
            builder.AddLoadDataCommand<TestDbContext>(o =>
            {
                o.LoadData = null;
            }));

        ex.Message.ShouldContain("LoadData");
    }

    [Fact]
    public async Task AddLoadDataCommand_RegistersExecutableCommand()
    {
        // Arrange
        var loadDataInvoked = false;
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
                b.Add("dev", o => o.UseInMemoryDatabase(Guid.NewGuid().ToString())))
            .AddLoadDataCommand<TestDbContext>(o =>
                o.LoadData = _ =>
                {
                    loadDataInvoked = true;
                    return Task.CompletedTask;
                })
            .Build();

        // Act
        await app.Execute("loaddata", "dev");

        // Assert
        loadDataInvoked.ShouldBeTrue();
    }
}
