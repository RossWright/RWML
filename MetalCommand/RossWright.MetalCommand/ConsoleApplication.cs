using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand;

/// <summary>
/// The MetalCommand interactive console application. Build an instance with
/// <see cref="CreateBuilder"/> → configure → <c>Build()</c> → <c>RunAsync</c>.
/// </summary>
public class ConsoleApplication : ICommandExecutor
{
    /// <summary>
    /// Creates a new <see cref="IConsoleApplicationBuilder"/> pre-wired with configuration
    /// from <c>appsettings.json</c> and <c>appsettings.dev.json</c> relative to
    /// <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    /// <returns>A new builder ready for command and service registration.</returns>
    public static IConsoleApplicationBuilder CreateBuilder()
    {
        var serviceCollection = new ServiceCollection();
        var console = new Console();

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.dev.json", optional: true, reloadOnChange: true)
            .Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        return new ConsoleApplicationBuilder(configuration, console, serviceCollection);
    }

    internal ConsoleApplication(IConsole console, 
        IServiceCollection serviceCollection, 
        IServiceProviderFactory<IServiceCollection>? serviceProviderFactory,
        CommandCollectionBuilder commandBuilder,
        IReadOnlyList<Type>? middlewareTypes = null) 
    {
        Console = console;
        MiddlewareTypes = middlewareTypes ?? [];
        serviceCollection.AddSingleton<ICommandExecutor>(this);
        serviceCollection.AddSingleton<IDictionary<string, string>>(_ => Context);
        Services = serviceProviderFactory != null 
            ? serviceProviderFactory.CreateServiceProvider(serviceCollection)
            : serviceCollection.BuildServiceProvider();

        var commandExecutors = commandBuilder.GetCommandDescriptors(Services);

        var commandExecutorsByInvocation = commandExecutors
            .SelectMany(_ => _.Descriptor.Invocations
                .Select(i => new KeyValuePair<string, CommandExecutor>(i.ToLower(), _)));
        var dupeInvocations = commandExecutorsByInvocation
            .GroupBy(_ => _.Key)
            .Where(_ => _.Count() > 1)
            .ToList();
        if (dupeInvocations.Any())
        {
            Console.WriteErrorLine("The following commands have conflicting invocations:");
            using (Console.Indent())
            {
                foreach (var group in dupeInvocations)
                {
                    Console.WriteErrorLine($"{group.Key} is used by " +
                        $"{group.Select(_ => $"{_.Value.Descriptor.Name} ({_.Value.CommandType.GetFullGenericName()})").CommaListJoin()}");
                }
            }
        }
        _commandExecutorsByInvocation = commandExecutorsByInvocation
            .GroupBy(_ => _.Key)
            .Where(_ => _.Count() == 1)
            .Select(_ => _.First())
            .ToDictionary();

        var commandCount = _commandExecutorsByInvocation
            .GroupBy(_ => _.Value.Descriptor)
            .Count() + 2;
        Console.WriteLine($"Found {commandCount} commands. Type \"Help\" to get help.", IntroOutroColor);
    }
    private readonly IDictionary<string, CommandExecutor> _commandExecutorsByInvocation;
    public IDictionary<string, string> Context { get; } = new Dictionary<string, string>();
    internal IReadOnlyList<Type> MiddlewareTypes { get; }
    internal Func<IDictionary<string, string>, string>? MakePrompt { get; set; }
    internal ConsoleColor? HelpColor { get; set; }
    internal ConsoleColor? HelpHeaderColor { get; set; }
    internal ConsoleColor? IntroOutroColor { get; set; } = ConsoleColor.DarkGray;
    internal ConsoleColor? WarningColor { get; set; } = ConsoleColor.Yellow;

    public async Task Execute(string invocation, params string[] args)
    {
        if (_commandExecutorsByInvocation.TryGetValue(invocation.ToLower(), out var commandExecutor))
        {
            // If called outside of the REPL loop (e.g. programmatically), ensure a run ID exists
            // so that EnvironmentArgMiddleware can scope confirmations correctly.
            Context.TryAdd("__runId", Guid.NewGuid().ToString("N"));
            await commandExecutor.Execute(Services, Console, args, Context, WarningColor, MiddlewareTypes, _currentCommandCancellationTokenSource.Token).ConfigureAwait(false);
        }
        else
        {
            Console.WriteErrorLine($"No command found with invocation: {invocation}");
        }
    }

    public IConsole Console { get; }
    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; internal set; } = null!;

    private CancellationTokenSource _currentCommandCancellationTokenSource = new();

    public async Task RunAsync(params string[] args)
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        bool isJobRunning = false;
        System.Console.CancelKeyPress += OnCancelKeyPress;
        void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = isJobRunning && !_currentCommandCancellationTokenSource.IsCancellationRequested;
            if (e.Cancel) _currentCommandCancellationTokenSource.Cancel();
            Console.WriteError($"Aborting {(e.Cancel ? "Command" : "Process")}...");
        }

        var (cleanArgs, preloadedContext, initialCommand) = DupConBootstrap.TryExtract(args);
        args = cleanArgs;
        foreach (var (k, v) in preloadedContext)
            Context[k] = v;

        var input = initialCommand ?? (args.Any() ? string.Join(' ', args) : null);
        do
        {
            Console.ResetIndent();
            Console.ResetLine();
            if (MakePrompt != null) Console.Write(MakePrompt(Context));
            Console.Write("> ");
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                Console.Write(input);
            }
            else
            {
                input = Console.ReadLine()!;
                if (input == null) break;
                if (string.IsNullOrWhiteSpace(input)) continue;
            }

            var parts = input.SplitAroundQuotes(' ')
                .Where(_ => !string.IsNullOrWhiteSpace(_))
                .Select(_ => _.Trim())
                .ToArray();
            input = null;

            if (parts[0].ToLower().In("exit","bye","quit"))
            {
                Console.WriteLine("Goodbye!");
                break;
            }
            else if (parts[0].ToLower().In("help", "man", "h"))
            {
                ShowHelp(parts.Skip(1).ToArray());
            }
            else if (!_commandExecutorsByInvocation
                .TryGetValue(parts[0].ToLower(), out var commandExecutor))
            {
                Console.WriteErrorLine("Unknown Command");
            }
            else
            {
                _currentCommandCancellationTokenSource = new();
                isJobRunning = true;
                Context["__runId"] = Guid.NewGuid().ToString("N");
                try
                {
                    Console.WriteLine($"Executing {commandExecutor.Descriptor.Name}. Use Ctrl-C to abort (and Ctrl-C again to abort program).", IntroOutroColor);
                    TimeSpan runTime;
                    CommandResult result;
                    using (Console.Indent())
                    {
                        (runTime, result) = await commandExecutor.Execute(Services, Console, parts.Skip(1).ToArray(), Context, WarningColor, MiddlewareTypes, _currentCommandCancellationTokenSource.Token);
                    }
                    Console.ResetLine();
                    Console.WriteLine($"{commandExecutor.Descriptor.Name} completed in {runTime.ToRelativeTime()}.", IntroOutroColor);

                    if (result.ExitApplication)
                    {
                        if (!result.Success && result.Message != null)
                            Console.WriteErrorLine(result.Message);
                        Console.WriteLine("Goodbye!", IntroOutroColor);
                        break;
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.ResetIndent();
                    Console.ResetLine();
                    Console.WriteErrorLine("Command Aborted");
                }
                catch (Exception ex)
                {
                    Console.WriteErrorLine(ex.ToBetterString());
                }
                finally
                {
                    isJobRunning = false;
                    Context.Remove("__runId");
                }
            }
        }
        while (true);
        _currentCommandCancellationTokenSource.Dispose();
    }

    private void ShowHelp(string[] args)
    {
        using (Console.Indent())
        {
            if (args.Any())
            {
                if (_commandExecutorsByInvocation.TryGetValue(args[0], out var helpOnCommand))
                {
                    var desc = helpOnCommand.Descriptor;
                    Console.WriteLine(desc.Name, HelpHeaderColor);

                    var text = desc.HelpDetail == null ? desc.HelpBrief : desc.HelpDetail;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        Console.WriteLineIndented(text, HelpColor);
                    }

                    var aliases = desc.Invocations.Where(_ => _ != desc.Name).ToArray();
                    if (aliases.Any())
                        Console.WriteLine($"Aliases: {aliases.CommaListJoin("or")}", HelpColor);

                    Console.WriteLine("Arguments:", HelpColor);
                    using (Console.Indent())
                    {
                        if (desc.Args?.Any() != true)
                        {
                            Console.WriteLine("No arguments specified", HelpColor);
                        }
                        else
                        {
                            foreach (var arg in desc.Args)
                            {
                                var defaultValue = arg.DefaultValue;
                                if (arg.UseContextKeyForDefault != null)
                                {
                                    Context.TryGetValue(arg.UseContextKeyForDefault, out defaultValue);
                                }
                                Console.WriteLine($"{arg.Name} - " +
                                    $"{(arg.IsRequired ? "Required." : "Optional.")}" +
                                    $"{arg.HelpDetail.PreSpaceIfPresent().EndSentence()}" +
                                    $"{arg.ValidValues.CommaListJoin("or").ToStringIfPresent(_ => $" Valid values are {_}.")}" +
                                    $"{defaultValue.ToStringIfPresent(_ => $" Default value is {_}.")}", HelpColor);
                            }
                        }
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteErrorLine($"Unknown command \"{args[0]}\"");
                }
            }
            else
            {
                var allDescriptors = _commandExecutorsByInvocation
                    .GroupBy(kv => kv.Value.Descriptor)
                    .Select(g => g.Key)
                    .ToList();

                const string consoleCategory    = "Console";
                const string uncategorizedLabel = "Uncategorized";

                // Built-in help/exit entries represented as synthetic descriptors for grouping
                var builtInConsoleEntries = new[]
                {
                    ("Help / Man / H [command]", "Display all available commands or detailed help for a command"),
                    ("Exit / Bye / Quit",         "End program"),
                };

                var grouped = allDescriptors
                    .GroupBy(d => d.Category ?? uncategorizedLabel)
                    .ToDictionary(g => g.Key, g => g.OrderBy(d => d.Name).ToList());

                // Category ordering: Console first, then alpha, Uncategorized last
                var categoryOrder = grouped.Keys
                    .Where(k => k != consoleCategory && k != uncategorizedLabel)
                    .OrderBy(k => k)
                    .Prepend(consoleCategory)
                    .ToList();

                if (grouped.ContainsKey(uncategorizedLabel))
                    categoryOrder.Add(uncategorizedLabel);

                void WriteCommandLine(CommandDescriptor desc)
                {
                    Console.Write("  ");
                    Console.Write($"{string.Join(" / ", desc.Invocations)} ", HelpColor);
                    foreach (var arg in desc.Args ?? [])
                    {
                        Console.Write(arg.IsRequired ? $"{arg.Name} " : $"[{arg.Name}] ", HelpColor);
                    }
                    Console.WriteLine($"- {desc.HelpBrief ?? desc.Name}", HelpColor);
                }

                foreach (var category in categoryOrder)
                {
                    if (!grouped.TryGetValue(category, out var commands) && category == consoleCategory)
                    {
                        // Console category may only have the synthetic built-ins
                        commands = [];
                    }
                    else if (commands == null)
                    {
                        continue;
                    }

                    // Category header: single colored-background line
                    Console.WriteLine();
                    Console.WriteLine($" {category} Commands", ConsoleColor.Yellow);

                    if (category == consoleCategory)
                    {
                        foreach (var (invocation, brief) in builtInConsoleEntries)
                        {
                            Console.WriteLine($"  {invocation} - {brief}", HelpColor);
                        }
                    }

                    foreach (var desc in commands)
                    {
                        WriteCommandLine(desc);
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
