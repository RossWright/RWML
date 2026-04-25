using Microsoft.Extensions.DependencyInjection;
using ActivatorUtilities = RossWright.MetalInjection.ActivatorUtilities;

namespace RossWright.MetalCommand;

internal class CommandExecutor(Type _commandType)
{
    public Type CommandType => _commandType;
    public object[] Parameters { get; set; } = [];
    public CommandDescriptor Descriptor { get; set; } = null!;

    public async Task<TimeSpan> Execute(
        IServiceProvider serviceProvider, 
        IConsole console, 
        string[] rawArgs, 
        IDictionary<string, string> context, 
        ConsoleColor? warningColor, 
        CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var command = (ILegacyCommand)ActivatorUtilities.CreateInstance(
                scope.ServiceProvider, CommandType, Parameters);
        var startTime = DateTime.UtcNow;

        var descriptorCount = command.Descriptor.Args?.Length ?? 0;
        if (rawArgs.Length > descriptorCount)
        {
            if (descriptorCount > 0)
            {
                console.WriteLine($"Warning: Last {rawArgs.Length - descriptorCount} arguments were ignored", warningColor);
            }
            else
            {
                console.WriteLine("Warning: All arguments were ignored", warningColor);
            }
        }

        string[]? argValues = new string[descriptorCount];
        int index = 0;
        foreach (var descriptor in command.Descriptor.Args ?? [])
        {
            if (rawArgs.Length > index)
            {
                if (descriptor.ValidValues?
                    .Any(_ => string.Equals(_, rawArgs[index], StringComparison.InvariantCultureIgnoreCase)) == false)
                {
                    console.WriteErrorLine($"Invalid value for {descriptor.Name}, must be one of {descriptor.ValidValues.CommaListJoin("or")}");
                    argValues = null;
                }
                else if (argValues != null)
                {
                    argValues[index] = rawArgs[index];
                }
            }
            else if (descriptor.UseContextKeyForDefault != null &&
                     context.TryGetValue(descriptor.UseContextKeyForDefault, out var contextValue))
            {
                console.WriteLine($"Using \"{contextValue}\" for {descriptor.Name}", warningColor);
                argValues![index] = contextValue;
            }
            else if (descriptor.IsRequired)
            {
                console.WriteErrorLine($"Missing argument for required parameter: {descriptor.Name}");
                argValues = null;
            }
            else if (argValues != null)
            {
                argValues[index] = descriptor.DefaultValue!;
                if (descriptor.DefaultValue != null)
                {
                    console.WriteLine($"Using \"{descriptor.DefaultValue}\" for {descriptor.Name}", warningColor);
                }
            }
            index++;
        }

        if (argValues != null) await command.Execute(console, argValues, cancellationToken);
        return DateTime.UtcNow - startTime;
    }
}