using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RossWright.MetalCommand.Internal;
using RossWright.MetalCommand.Internal.Commands;
using System.ComponentModel;

namespace RossWright.MetalCommand;

/// <summary>
/// Default implementation of <see cref="IConsoleApplicationBuilder"/>.
/// </summary>
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
        _services.AddLogging();
        _loggingBuilder = new LoggingBuilder(_services);
    }
    private readonly IConfiguration _configuration;
    private readonly Console _console;
    private readonly IServiceCollection _services;
    private readonly CommandCollectionBuilder _commandCollectionBuilder = new();

    private ConsoleColor? _introOutroColor;
    private ConsoleColor? _helpColor;
    private ConsoleColor? _warningColor;
    private readonly List<Type> _middlewareTypes = new();

    /// <inheritdoc />
    public IConfiguration Configuration => _configuration;
    /// <inheritdoc />
    public IServiceCollection Services => _services;
    /// <inheritdoc />
    public IConsole Console => _console;
    /// <inheritdoc />
    public ICommandCollection Commands => _commandCollectionBuilder;
    /// <inheritdoc />
    public Func<IDictionary<string, string>, string>? PromptFactory { get; set; }
    /// <inheritdoc />
    public IConsoleApplicationBuilder SetTabWidth(int width)
    {
        _console.TabWidth = width;
        return this;
    }
    /// <inheritdoc />
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
    /// <inheritdoc />
    public IConsoleApplicationBuilder AddMiddleware<TMiddleware>() where TMiddleware : class, ICommandMiddleware
    {
        _middlewareTypes.Add(typeof(TMiddleware));
        return this;
    }

    /// <inheritdoc />
    public void SetServiceProviderFactory(IServiceProviderFactory<IServiceCollection> serviceProviderFactory) =>
        _serviceProviderFactory = serviceProviderFactory;
    private IServiceProviderFactory<IServiceCollection>? _serviceProviderFactory;

    internal BuiltInCommandOptions BuiltInCommandOptions { get; } = new();
    internal string? LoadContextName { get; set; }
    internal bool ShowWarnIfContextMissing { get; set; } = true;

    /// <inheritdoc />
    public ILoggingBuilder Logging => _loggingBuilder;
    private readonly ILoggingBuilder _loggingBuilder;

    /// <summary>
    /// Builds the configured console application.
    /// </summary>
    /// <returns>The configured <see cref="ConsoleApplication"/>.</returns>
    public ConsoleApplication Build()
    {
        // Register built-in commands
        _commandCollectionBuilder.Add(typeof(ListContextCommand));
        _commandCollectionBuilder.Add(typeof(SetContextCommand));
        _commandCollectionBuilder.Add(typeof(SaveConCommand));
        _commandCollectionBuilder.Add(typeof(LoadConCommand));
        _commandCollectionBuilder.Add(typeof(DupConCommand));

        // Register BuiltInCommandOptionsRegistry so CommandCollectionBuilder can resolve invocation overrides
        var registry = new BuiltInCommandOptionsRegistry(BuiltInCommandOptions);
        _services.AddSingleton<ICommandOptionsRegistry>(registry);

        // Register DupCon-specific DI dependencies from BuiltInCommandOptions
        var dupConOpts = new DupConCommandOptions
        {
            DefaultForwardContext = BuiltInCommandOptions.DupConDefaultForwardContext,
            WindowsLaunchMode = BuiltInCommandOptions.DupConWindowsLaunchMode,
            TerminalLauncher = BuiltInCommandOptions.DupConTerminalLauncher,
        };
        _services.AddSingleton(dupConOpts);

        if (BuiltInCommandOptions.DupConTerminalLauncher is not null)
            _services.AddSingleton<IDupConTerminalLauncher>(BuiltInCommandOptions.DupConTerminalLauncher);
        else if (_services.All(d => d.ServiceType != typeof(IDupConTerminalLauncher)))
            _services.AddSingleton<IDupConTerminalLauncher>(
                new PlatformTerminalLauncher(BuiltInCommandOptions.DupConWindowsLaunchMode));

        var consoleApp = new ConsoleApplication(
            Console,
            Services,
            _serviceProviderFactory,
            (CommandCollectionBuilder)Commands,
            _middlewareTypes)
        {
            Configuration = Configuration,
            MakePrompt = PromptFactory
        };
        if (_introOutroColor != null) consoleApp.IntroOutroColor = _introOutroColor;
        if (_helpColor != null) consoleApp.HelpColor = _helpColor;
        if (_warningColor != null) consoleApp.WarningColor = _warningColor;

        if (LoadContextName != null)
        {
            var basePath = AppContext.BaseDirectory;
            var name = LoadContextName;
            var path = Path.IsPathRooted(name) ? name : Path.Combine(basePath, name);

            // Try exact path first, then with .mcc.json appended
            string? resolved = File.Exists(path) ? path
                : File.Exists(path + ".mcc.json") ? path + ".mcc.json"
                : null;

            if (resolved == null)
            {
                if (ShowWarnIfContextMissing)
                    _console.WriteLine(
                        $"Warning: context file \"{name}\" not found — starting with empty context.",
                        ConsoleColor.Yellow);
            }
            else
            {
                try
                {
                    var json = File.ReadAllText(resolved);
                    var loaded = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, string>>(json);
                    if (loaded != null)
                        foreach (var (k, v) in loaded)
                            consoleApp.Context[k] = v;
                }
                catch (Exception ex)
                {
                    _console.WriteLine(
                        $"Warning: failed to load context file \"{resolved}\": {ex.Message} — starting with empty context.",
                        ConsoleColor.Yellow);
                }
            }
        }

        return consoleApp;
    }
}

file sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
{
    public IServiceCollection Services { get; } = services;
}
