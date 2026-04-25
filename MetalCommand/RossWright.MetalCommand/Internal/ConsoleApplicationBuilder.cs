using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace RossWright.MetalCommand;

[EditorBrowsable(EditorBrowsableState.Never)]
public class ConsoleApplicationBuilder : OptionsBuilder, IConsoleApplicationBuilder
{
    internal ConsoleApplicationBuilder(
        IConfiguration configuration, 
        Console console, 
        IServiceCollection services)
    {
        _configuration = configuration;
        _console = console;
        _services = services;
    }
    private readonly IConfiguration _configuration;
    private readonly Console _console;
    private readonly IServiceCollection _services;
    private readonly CommandCollectionBuilder _commandCollectionBuilder = new();

    private ConsoleColor? _introOutroColor;
    private ConsoleColor? _helpColor;
    private ConsoleColor? _warningColor;

    public IConfiguration Configuration => _configuration;
    public IServiceCollection Services => _services;
    public IConsole Console => _console;
    public ICommandCollection Commands => _commandCollectionBuilder;
    public Func<IDictionary<string, string>, string>? PromptFactory { get; set; }
    public IConsoleApplicationBuilder SetTabWidth(int width)
    {
        _console.TabWidth = width;
        return this;
    }
    public IConsoleApplicationBuilder SetColors(
        ConsoleColor? introOutroColor = null,
        ConsoleColor? helpColor = null,
        ConsoleColor? warningColor = null,
        ConsoleColor? errorColor = null,
        ConsoleColor? errorBgColor = null)
    {
        if (introOutroColor != null) _introOutroColor = introOutroColor;
        if (helpColor != null) _helpColor = helpColor;
        if (warningColor != null) _warningColor = warningColor.Value;
        if (errorColor != null) _console.ErrorTextColor = errorColor.Value;
        if (errorBgColor != null) _console.ErrorBackgroundColor = errorBgColor.Value;
        return this;
    }
    public void SetServiceProviderFactory(IServiceProviderFactory<IServiceCollection> serviceProviderFactory) =>
        _serviceProviderFactory = serviceProviderFactory;
    private IServiceProviderFactory<IServiceCollection>? _serviceProviderFactory;

    public ConsoleApplication Build()
    {
        var consoleApp = new ConsoleApplication(
            Console,
            Services,
            _serviceProviderFactory,
            (CommandCollectionBuilder)Commands)
        {
            Configuration = Configuration,
            MakePrompt = PromptFactory
        };
        if (_introOutroColor != null) consoleApp.IntroOutroColor = _introOutroColor;
        if (_helpColor != null) consoleApp.HelpColor = _helpColor;
        if (_warningColor != null) consoleApp.WarningColor = _warningColor;
        return consoleApp;
    }
}


