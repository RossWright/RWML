using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand;

public class ConsoleApplication : ICommandExecutor
{
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
        CommandCollectionBuilder commandBuilder) 
    {
        Console = console;
        serviceCollection.AddSingleton<ICommandExecutor>(this);
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
        
        _commandExecutorsByCommandType = commandExecutors
            .GroupBy(_ => _.CommandType)
            .Where(_ => _.Count() == 1)
            .ToDictionary(_ => _.Key, _ => _.First());

        var commandCount = _commandExecutorsByInvocation
            .GroupBy(_ => _.Value.Descriptor)
            .Count() + 2;
        Console.WriteLine($"Found {commandCount} commands. Type \"Help\" to get help.", IntroOutroColor);
    }
    private readonly IDictionary<string, CommandExecutor> _commandExecutorsByInvocation;
    private readonly IDictionary<Type, CommandExecutor> _commandExecutorsByCommandType;
    public IDictionary<string, string> Context { get; } = new Dictionary<string, string>();
    internal Func<IDictionary<string, string>, string>? MakePrompt { get; set; }
    internal ConsoleColor? HelpColor { get; set; }
    internal ConsoleColor? IntroOutroColor { get; set; } = ConsoleColor.DarkGray;
    internal ConsoleColor? WarningColor { get; set; } = ConsoleColor.Yellow;

    public async Task Execute(string invocation, params string[] args)
    {
        if (_commandExecutorsByInvocation.TryGetValue(invocation, out var commandExecutor))
        {
            await commandExecutor.Execute(Services, Console, args, Context, WarningColor, _currentCommandCancellationTokenSource.Token);
        }
        else
        {
            Console.WriteErrorLine($"No command found with invocation: {invocation}");
        }
    }

    public async Task Execute<TCommand>(params string[] args) where TCommand : ILegacyCommand
    {
        if (_commandExecutorsByCommandType.TryGetValue(typeof(TCommand), out var commandExecutor))
        {
            await commandExecutor.Execute(Services, Console, args, Context, WarningColor, _currentCommandCancellationTokenSource.Token);
        }
        else
        {
            Console.WriteErrorLine($"No distinct command found with type: {typeof(TCommand).Name}.");
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

        var input = args.Any() ? string.Join(' ', args) : null;
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
                try
                {
                    Console.WriteLine($"Executing {commandExecutor.Descriptor.Name}. Use Ctrl-C to abort (and Ctrl-C again to abort program).", IntroOutroColor);
                    TimeSpan runTime;
                    using (Console.Indent())
                    {
                        runTime = await commandExecutor.Execute(Services, Console, parts.Skip(1).ToArray(), Context, WarningColor, _currentCommandCancellationTokenSource.Token);
                    }
                    Console.ResetLine();
                    Console.WriteLine($"{commandExecutor.Descriptor.Name} completed in {runTime.ToRelativeTime()}.", IntroOutroColor);
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
                    Console.WriteLine(desc.Name, HelpColor);
                    Console.WriteLine(desc.Name.ButAll('-', 10), HelpColor);
                    Console.WriteLine($"Invocations: {desc.Invocations.Where(_ => _ != desc.Name).CommaListJoin("or")}", HelpColor);

                    var text = desc.HelpDetail == null ? desc.HelpBrief : desc.HelpDetail;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        Console.WriteLine(text, HelpColor);
                        Console.WriteLine();
                    }
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
                    Console.WriteErrorLine($"Unknown command \"{args[1]}\"");
                }
            }
            else
            {
                Console.WriteLine("The following commands are available:", HelpColor);
                Console.WriteLine("Help / Man / H [command] - Display all available commands or detailed help for a command", HelpColor);
                Console.WriteLine("Exit / Bye / Quit - End program", HelpColor);
                foreach (var desc in _commandExecutorsByInvocation
                    .GroupBy(_ => _.Value.Descriptor).OrderBy(_ => _.Key.Name).Select(_ => _.Key))
                {
                    Console.Write($"{string.Join(" / ", desc.Invocations)} ", HelpColor);
                    foreach (var arg in desc.Args ?? [])
                    {
                        if (arg.IsRequired)
                        {
                            Console.Write($"{arg.Name} ", HelpColor);
                        }
                        else
                        {
                            Console.Write($"[{arg.Name}] ", HelpColor);
                        }
                    }
                    Console.WriteLine($"- {desc.HelpBrief ?? desc.Name}", HelpColor);
                }
            }
        }
    }
}
