using Microsoft.Extensions.DependencyInjection;
using System.Data;
using ActivatorUtilities = RossWright.MetalInjection.ActivatorUtilities;

namespace RossWright.MetalCommand;

internal class CommandCollectionBuilder() 
    : AssemblyScanningOptionsBuilder("MetalCommand"),
    ICommandCollection
{
    private readonly List<CommandExecutor> _commands = new();
    
    public ICommandCollection Add<TCOMMAND>(params object[] parameters) where TCOMMAND : class, ILegacyCommand =>
        Add<TCOMMAND>(null, parameters);

    public ICommandCollection Add<TCOMMAND>(CommandDescriptor? descriptor = null, params object[] parameters) 
        where TCOMMAND : class, ILegacyCommand
    {
        _commands.Add(new CommandExecutor(typeof(TCOMMAND))
        { 
            Descriptor = descriptor!,
            Parameters = parameters
        });
        return this;
    }

    public IEnumerable<CommandExecutor> GetCommandDescriptors(IServiceProvider serviceProvider)
    {
        _commands.AddRange(DiscoveredConcreteTypes
            .Where(_ => !_.IsGenericType && _.IsAssignableTo(typeof(ILegacyCommand)))
            .Select(_ => new CommandExecutor(_)));

        using var scope = serviceProvider.CreateScope();
        foreach(var command in _commands.DistinctBy(_ => _.CommandType))
        {
            var commandObj = (ILegacyCommand)ActivatorUtilities.CreateInstance(
                scope.ServiceProvider, command.CommandType, command.Parameters ?? []);
            if (command.Descriptor == null)
            {
                command.Descriptor = commandObj.Descriptor;
            }
            else
            { 
                command.Descriptor.Name = command.Descriptor.Name ?? commandObj.Descriptor.Name;
                command.Descriptor.Invocations = command.Descriptor.Invocations ?? commandObj.Descriptor.Invocations;
                command.Descriptor.Args = command.Descriptor.Args ?? commandObj.Descriptor.Args;
                command.Descriptor.HelpBrief = command.Descriptor.HelpBrief ?? commandObj.Descriptor.HelpBrief;
            }
            yield return command;
        }
    }
}
