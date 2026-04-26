using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RossWright.MetalCommand;

internal class CommandCollectionBuilder() 
    : AssemblyScanningOptionsBuilder("MetalCommand"),
    ICommandCollection
{
    private readonly List<CommandExecutor> _commands = new();

    public ICommandCollection Add<TCOMMAND>() where TCOMMAND : class, ICommand
    {
        _commands.Add(new CommandExecutor(typeof(TCOMMAND)));
        return this;
    }

    public ICommandCollection Add(Type commandType)
    {
        if (!typeof(ICommand).IsAssignableFrom(commandType))
            throw new ArgumentException($"{commandType.Name} must implement ICommand.", nameof(commandType));
        _commands.Add(new CommandExecutor(commandType));
        return this;
    }

    public IEnumerable<CommandExecutor> GetCommandDescriptors(IServiceProvider serviceProvider)
    {
        _commands.AddRange(DiscoveredConcreteTypes
            .Where(_ => !_.IsGenericType && _.IsAssignableTo(typeof(ICommand)) && _.GetCustomAttribute<CommandAttribute>() != null)
            .Select(_ => new CommandExecutor(_)));

        var optionsRegistries = serviceProvider.GetServices<ICommandOptionsRegistry>().ToList();

        foreach (var command in _commands.DistinctBy(_ => _.CommandType))
        {
            var descriptor = CommandDescriptorFactory.FromAttributes(command.CommandType);

            var opts = optionsRegistries
                .Select(r => r.Get(command.CommandType))
                .FirstOrDefault(o => o?.Invocations is { Length: > 0 });
            if (opts?.Invocations is { Length: > 0 })
            {
                // Clone the descriptor so the cached instance is never mutated
                descriptor = new CommandDescriptor
                {
                    Name = opts.Invocations[0],
                    Invocations = opts.Invocations,
                    Args = descriptor.Args,
                    HelpBrief = descriptor.HelpBrief,
                    HelpDetail = descriptor.HelpDetail,
                    Category = descriptor.Category
                };
            }

            command.Descriptor = descriptor;
            yield return command;
        }
    }
}
