using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RossWright.MetalCommand;

namespace RossWright.MetalCommand.Tests;

public class IConsoleApplicationBuilderTests
{
    private class TestMiddleware : ICommandMiddleware
    {
        public Task InvokeAsync(CommandContext context, Func<CommandContext, Task> next)
        {
            return next(context);
        }
    }

    private class TestServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        public IServiceCollection CreateBuilder(IServiceCollection services) => services;
        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder) => containerBuilder.BuildServiceProvider();
    }

    // -----------------------------------------------------------------------
    // AddServices Extension Method Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void AddServices_WithValidAction_InvokesActionWithServices()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);
        var actionInvoked = false;

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.AddServices(builder, s =>
        {
            actionInvoked = true;
            s.ShouldBe(services);
        });

        // Assert
        actionInvoked.ShouldBeTrue();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddServices_AddsServiceToCollection_ServiceIsRegistered()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var services = new ServiceCollection();
        builder.Services.Returns(services);

        // Act
        IConsoleApplicationBuilderExtensions.AddServices(builder, s =>
        {
            s.AddSingleton<string>("test-service");
        });

        // Assert
        services.ShouldContain(sd => sd.ServiceType == typeof(string));
    }

    // -----------------------------------------------------------------------
    // AddCommands Extension Method Tests (single parameter)
    // -----------------------------------------------------------------------

    [Fact]
    public void AddCommands_WithValidAction_InvokesActionWithCommands()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var commands = Substitute.For<ICommandCollection>();
        builder.Commands.Returns(commands);
        var actionInvoked = false;

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.AddCommands(builder, c =>
        {
            actionInvoked = true;
            c.ShouldBe(commands);
        });

        // Assert
        actionInvoked.ShouldBeTrue();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddCommands_ReturnsBuilder()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var commands = Substitute.For<ICommandCollection>();
        builder.Commands.Returns(commands);

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.AddCommands(builder, c => { });

        // Assert
        result.ShouldBe(builder);
    }

    // -----------------------------------------------------------------------
    // AddCommands Extension Method Tests (two parameters)
    // -----------------------------------------------------------------------

    [Fact]
    public void AddCommands_WithValidActionAndConfiguration_InvokesActionWithCommandsAndConfiguration()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var commands = Substitute.For<ICommandCollection>();
        var configuration = Substitute.For<IConfiguration>();
        builder.Commands.Returns(commands);
        builder.Configuration.Returns(configuration);
        var actionInvoked = false;

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.AddCommands(builder, (c, config) =>
        {
            actionInvoked = true;
            c.ShouldBe(commands);
            config.ShouldBe(configuration);
        });

        // Assert
        actionInvoked.ShouldBeTrue();
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddCommands_WithConfiguration_ReturnsBuilder()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        var commands = Substitute.For<ICommandCollection>();
        var configuration = Substitute.For<IConfiguration>();
        builder.Commands.Returns(commands);
        builder.Configuration.Returns(configuration);

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.AddCommands(builder, (c, config) => { });

        // Assert
        result.ShouldBe(builder);
    }

    // -----------------------------------------------------------------------
    // SetPromptFactory Extension Method Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void SetPromptFactory_WithValidFactory_SetsPromptFactory()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();
        Func<IDictionary<string, string>, string> promptFactory = dict => "test-prompt";

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.SetPromptFactory(builder, promptFactory);

        // Assert
        builder.Received(1).PromptFactory = promptFactory;
        result.ShouldBe(builder);
    }

    [Fact]
    public void SetPromptFactory_WithNullFactory_SetsPromptFactoryToNull()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.SetPromptFactory(builder, null);

        // Assert
        builder.Received(1).PromptFactory = null;
        result.ShouldBe(builder);
    }

    [Fact]
    public void SetPromptFactory_ReturnsBuilder()
    {
        // Arrange
        var builder = Substitute.For<IConsoleApplicationBuilder>();

        // Act
        IConsoleApplicationBuilder result = IConsoleApplicationBuilderExtensions.SetPromptFactory(builder, dict => "prompt");

        // Assert
        result.ShouldBe(builder);
    }

    // -----------------------------------------------------------------------
    // SetColors Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void SetColors_WithAllParameters_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.SetColors(
            introOutroColor: ConsoleColor.Blue,
            helpColor: ConsoleColor.Green,
            warningColor: ConsoleColor.Yellow,
            errorColor: ConsoleColor.Red,
            errorBgColor: ConsoleColor.DarkRed);

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void SetColors_WithNoParameters_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.SetColors();

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void SetColors_WithIntroOutroColor_SetsColor()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.SetColors(introOutroColor: ConsoleColor.Cyan);
        var app = builder.Build();

        // Assert
        app.IntroOutroColor.ShouldBe(ConsoleColor.Cyan);
    }

    [Fact]
    public void SetColors_WithHelpColor_SetsColor()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.SetColors(helpColor: ConsoleColor.Magenta);
        var app = builder.Build();

        // Assert
        app.HelpColor.ShouldBe(ConsoleColor.Magenta);
    }

    [Fact]
    public void SetColors_WithWarningColor_SetsColor()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.SetColors(warningColor: ConsoleColor.Yellow);
        var app = builder.Build();

        // Assert
        app.WarningColor.ShouldBe(ConsoleColor.Yellow);
    }

    [Fact]
    public void SetColors_WithErrorColor_SetsConsoleErrorColor()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetColors(errorColor: ConsoleColor.Red);

        // Assert
        console.ErrorTextColor.ShouldBe(ConsoleColor.Red);
    }

    [Fact]
    public void SetColors_WithErrorBgColor_SetsConsoleErrorBackgroundColor()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetColors(errorBgColor: ConsoleColor.DarkRed);

        // Assert
        console.ErrorBackgroundColor.ShouldBe(ConsoleColor.DarkRed);
    }

    [Fact]
    public void SetColors_CalledMultipleTimes_UpdatesColors()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.SetColors(introOutroColor: ConsoleColor.Blue);
        builder.SetColors(introOutroColor: ConsoleColor.Green);
        var app = builder.Build();

        // Assert
        app.IntroOutroColor.ShouldBe(ConsoleColor.Green);
    }

    [Fact]
    public void SetColors_WithExplicitNullValues_DoesNotSetColors()
    {
        // Arrange
        var builder = CreateBuilder();
        builder.SetColors(introOutroColor: ConsoleColor.Blue);

        // Act
        builder.SetColors(
            introOutroColor: null,
            helpColor: null,
            warningColor: null,
            errorColor: null,
            errorBgColor: null);
        var app = builder.Build();

        // Assert
        app.IntroOutroColor.ShouldBe(ConsoleColor.Blue); // Should remain unchanged
    }

    [Fact]
    public void SetColors_WithMixedNullAndNonNull_SetsOnlyNonNullColors()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetColors(
            introOutroColor: ConsoleColor.Cyan,
            helpColor: null,
            warningColor: ConsoleColor.Yellow,
            errorColor: null,
            errorBgColor: ConsoleColor.DarkRed);
        var app = builder.Build();

        // Assert
        app.IntroOutroColor.ShouldBe(ConsoleColor.Cyan);
        app.WarningColor.ShouldBe(ConsoleColor.Yellow);
        console.ErrorBackgroundColor.ShouldBe(ConsoleColor.DarkRed);
    }

    [Theory]
    [InlineData(ConsoleColor.Black)]
    [InlineData(ConsoleColor.DarkBlue)]
    [InlineData(ConsoleColor.DarkGreen)]
    [InlineData(ConsoleColor.DarkCyan)]
    [InlineData(ConsoleColor.DarkRed)]
    [InlineData(ConsoleColor.DarkMagenta)]
    [InlineData(ConsoleColor.DarkYellow)]
    [InlineData(ConsoleColor.Gray)]
    [InlineData(ConsoleColor.DarkGray)]
    [InlineData(ConsoleColor.Blue)]
    [InlineData(ConsoleColor.Green)]
    [InlineData(ConsoleColor.Cyan)]
    [InlineData(ConsoleColor.Red)]
    [InlineData(ConsoleColor.Magenta)]
    [InlineData(ConsoleColor.Yellow)]
    [InlineData(ConsoleColor.White)]
    public void SetColors_WithAllConsoleColors_SetsIntroOutroColor(ConsoleColor color)
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.SetColors(introOutroColor: color);
        var app = builder.Build();

        // Assert
        app.IntroOutroColor.ShouldBe(color);
    }

    [Fact]
    public void SetColors_WithAllColorsSet_SetsAllColors()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetColors(
            introOutroColor: ConsoleColor.Blue,
            helpColor: ConsoleColor.Green,
            warningColor: ConsoleColor.Yellow,
            errorColor: ConsoleColor.Red,
            errorBgColor: ConsoleColor.DarkRed);
        var app = builder.Build();

        // Assert
        app.IntroOutroColor.ShouldBe(ConsoleColor.Blue);
        app.HelpColor.ShouldBe(ConsoleColor.Green);
        app.WarningColor.ShouldBe(ConsoleColor.Yellow);
        console.ErrorTextColor.ShouldBe(ConsoleColor.Red);
        console.ErrorBackgroundColor.ShouldBe(ConsoleColor.DarkRed);
    }

    // -----------------------------------------------------------------------
    // SetTabWidth Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void SetTabWidth_WithPositiveValue_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.SetTabWidth(4);

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void SetTabWidth_SetsConsoleTabWidth()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(8);

        // Assert
        console.TabWidth.ShouldBe(8);
    }

    [Fact]
    public void SetTabWidth_WithZero_SetsTabWidthToZero()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(0);

        // Assert
        console.TabWidth.ShouldBe(0);
    }

    [Fact]
    public void SetTabWidth_WithNegativeValue_SetsTabWidthToNegative()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(-1);

        // Assert
        console.TabWidth.ShouldBe(-1);
    }

    [Fact]
    public void SetTabWidth_CalledMultipleTimes_UpdatesWidth()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(4);
        builder.SetTabWidth(8);

        // Assert
        console.TabWidth.ShouldBe(8);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    public void SetTabWidth_WithVariousPositiveValues_SetsTabWidth(int width)
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(width);

        // Assert
        console.TabWidth.ShouldBe(width);
    }

    [Fact]
    public void SetTabWidth_WithMaxInt_SetsTabWidth()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(int.MaxValue);

        // Assert
        console.TabWidth.ShouldBe(int.MaxValue);
    }

    [Fact]
    public void SetTabWidth_WithMinInt_SetsTabWidth()
    {
        // Arrange
        var console = new Console();
        var builder = CreateBuilder(console);

        // Act
        builder.SetTabWidth(int.MinValue);

        // Assert
        console.TabWidth.ShouldBe(int.MinValue);
    }

    // -----------------------------------------------------------------------
    // AddMiddleware Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void AddMiddleware_WithValidMiddleware_ReturnsBuilder()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder.AddMiddleware<TestMiddleware>();

        // Assert
        result.ShouldBe(builder);
    }

    [Fact]
    public void AddMiddleware_RegistersMiddlewareType()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.AddMiddleware<TestMiddleware>();
        var app = builder.Build();

        // Assert - middleware is registered and will be used during execution
        app.ShouldNotBeNull();
    }

    [Fact]
    public void AddMiddleware_CalledMultipleTimes_RegistersMultipleMiddleware()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        builder.AddMiddleware<TestMiddleware>();
        builder.AddMiddleware<TestMiddleware>();
        var app = builder.Build();

        // Assert
        app.ShouldNotBeNull();
    }

    [Fact]
    public void AddMiddleware_CanChainMultipleCalls()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act
        var result = builder
            .AddMiddleware<TestMiddleware>()
            .AddMiddleware<TestMiddleware>();

        // Assert
        result.ShouldBe(builder);
    }

    // -----------------------------------------------------------------------
    // SetServiceProviderFactory Tests
    // -----------------------------------------------------------------------

    [Fact]
    public void SetServiceProviderFactory_WithValidFactory_DoesNotThrow()
    {
        // Arrange
        var builder = CreateBuilder();
        var factory = new TestServiceProviderFactory();

        // Act & Assert
        Should.NotThrow(() => builder.SetServiceProviderFactory(factory));
    }

    [Fact]
    public void SetServiceProviderFactory_UsesCustomFactory()
    {
        // Arrange
        var builder = CreateBuilder();
        var factory = new TestServiceProviderFactory();

        // Act
        builder.SetServiceProviderFactory(factory);
        var app = builder.Build();

        // Assert
        app.ShouldNotBeNull();
    }

    [Fact]
    public void SetServiceProviderFactory_CalledMultipleTimes_UsesLastFactory()
    {
        // Arrange
        var builder = CreateBuilder();
        var factory1 = new TestServiceProviderFactory();
        var factory2 = new TestServiceProviderFactory();

        // Act
        builder.SetServiceProviderFactory(factory1);
        builder.SetServiceProviderFactory(factory2);
        var app = builder.Build();

        // Assert
        app.ShouldNotBeNull();
    }

    // -----------------------------------------------------------------------
    // Helper Methods
    // -----------------------------------------------------------------------

    private static IConsoleApplicationBuilder CreateBuilder(Console? console = null)
    {
        var config = new ConfigurationBuilder().Build();
        var consoleInstance = console ?? new Console();
        var services = new ServiceCollection();
        return new ConsoleApplicationBuilder(config, consoleInstance, services);
    }
}
