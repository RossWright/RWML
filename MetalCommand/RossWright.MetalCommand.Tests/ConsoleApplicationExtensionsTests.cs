using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RossWright.MetalCommand.Internal;

namespace RossWright.MetalCommand.Tests;

public class ConsoleApplicationExtensionsTests
{
    [Fact]
    public void UseApp_ActionIsCalled_ReturnsApp()
    {
        // Arrange
        var testConsole = new Infrastructure.TestConsole();
        var services = new ServiceCollection();
        services.AddSingleton<IConsole>(testConsole);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var commandCollectionBuilder = new CommandCollectionBuilder();
        var app = new ConsoleApplication(testConsole, services, null, commandCollectionBuilder, null)
        {
            Configuration = configuration
        };
        
        var actionWasCalled = false;
        ConsoleApplication? capturedApp = null;

        // Act
        var result = app.UseApp(a =>
        {
            actionWasCalled = true;
            capturedApp = a;
        });

        // Assert
        actionWasCalled.ShouldBeTrue();
        capturedApp.ShouldBeSameAs(app);
        result.ShouldBeSameAs(app);
    }

    [Fact]
    public void UseApp_ModifiesContext_ContextIsUpdated()
    {
        // Arrange
        var testConsole = new Infrastructure.TestConsole();
        var services = new ServiceCollection();
        services.AddSingleton<IConsole>(testConsole);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var commandCollectionBuilder = new CommandCollectionBuilder();
        var app = new ConsoleApplication(testConsole, services, null, commandCollectionBuilder, null)
        {
            Configuration = configuration
        };

        // Act
        app.UseApp(a => a.Context["testKey"] = "testValue");

        // Assert
        app.Context["testKey"].ShouldBe("testValue");
    }

    [Fact]
    public void UseApp_Chaining_AllActionsExecute()
    {
        // Arrange
        var testConsole = new Infrastructure.TestConsole();
        var services = new ServiceCollection();
        services.AddSingleton<IConsole>(testConsole);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var commandCollectionBuilder = new CommandCollectionBuilder();
        var app = new ConsoleApplication(testConsole, services, null, commandCollectionBuilder, null)
        {
            Configuration = configuration
        };

        // Act
        var result = app
            .UseApp(a => a.Context["first"] = "1")
            .UseApp(a => a.Context["second"] = "2")
            .UseApp(a => a.Context["third"] = "3");

        // Assert
        result.Context["first"].ShouldBe("1");
        result.Context["second"].ShouldBe("2");
        result.Context["third"].ShouldBe("3");
        result.ShouldBeSameAs(app);
    }

    [Fact]
    public void CustomizeBuiltInCommands_ConfigureActionIsCalled_ReturnsBuilder()
    {
        // Arrange
        var console = new Console();
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var builder = new ConsoleApplicationBuilder(configuration, console, services);
        
        var actionWasCalled = false;
        BuiltInCommandOptions? capturedOptions = null;

        // Act
        var result = builder.CustomizeBuiltInCommands(o =>
        {
            actionWasCalled = true;
            capturedOptions = o;
        });

        // Assert
        actionWasCalled.ShouldBeTrue();
        capturedOptions.ShouldNotBeNull();
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void CustomizeBuiltInCommands_ModifiesOptions_OptionsArePersisted()
    {
        // Arrange
        var console = new Console();
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var builder = new ConsoleApplicationBuilder(configuration, console, services);

        // Act
        builder.CustomizeBuiltInCommands(o =>
        {
            o.SaveContextInvocations = ["sc", "save"];
            o.LoadContextInvocations = ["lc", "load"];
        });

        // Assert
        builder.BuiltInCommandOptions.SaveContextInvocations.ShouldBe(["sc", "save"]);
        builder.BuiltInCommandOptions.LoadContextInvocations.ShouldBe(["lc", "load"]);
    }

    [Fact]
    public void CustomizeBuiltInCommands_Chaining_AllModificationsApplied()
    {
        // Arrange
        var console = new Console();
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var builder = new ConsoleApplicationBuilder(configuration, console, services);

        // Act
        var result = (ConsoleApplicationBuilder)builder
            .CustomizeBuiltInCommands(o => o.SaveContextInvocations = ["sc"])
            .CustomizeBuiltInCommands(o => o.LoadContextInvocations = ["lc"])
            .CustomizeBuiltInCommands(o => o.DupConInvocations = ["dup"]);

        // Assert
        result.BuiltInCommandOptions.SaveContextInvocations.ShouldBe(["sc"]);
        result.BuiltInCommandOptions.LoadContextInvocations.ShouldBe(["lc"]);
        result.BuiltInCommandOptions.DupConInvocations.ShouldBe(["dup"]);
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void CustomizeBuiltInCommands_MultipleProperties_AllAreModified()
    {
        // Arrange
        var console = new Console();
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var builder = new ConsoleApplicationBuilder(configuration, console, services);

        // Act
        builder.CustomizeBuiltInCommands(o =>
        {
            o.SaveContextInvocations = ["savecontext"];
            o.LoadContextInvocations = ["loadcontext"];
            o.DupConInvocations = ["duplicate"];
            o.ListContextInvocations = ["listcontext"];
            o.SetContextInvocations = ["setcontext"];
        });

        // Assert
        builder.BuiltInCommandOptions.SaveContextInvocations.ShouldBe(["savecontext"]);
        builder.BuiltInCommandOptions.LoadContextInvocations.ShouldBe(["loadcontext"]);
        builder.BuiltInCommandOptions.DupConInvocations.ShouldBe(["duplicate"]);
        builder.BuiltInCommandOptions.ListContextInvocations.ShouldBe(["listcontext"]);
        builder.BuiltInCommandOptions.SetContextInvocations.ShouldBe(["setcontext"]);
    }
}
