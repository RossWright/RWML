using System.Collections.Concurrent;
using System.Reflection;

namespace RossWright.MetalCommand;

internal static class CommandDescriptorFactory
{
    private static readonly ConcurrentDictionary<Type, CommandDescriptor> _cache = new();

    public static CommandDescriptor FromAttributes(Type commandType) =>
        _cache.GetOrAdd(commandType, BuildDescriptor);

    private static CommandDescriptor BuildDescriptor(Type commandType)
    {
        var cmd = commandType.GetCustomAttribute<CommandAttribute>()
            ?? throw new InvalidOperationException(
                $"{commandType.Name} must be decorated with [Command] to use the attribute-driven ICommand.");

        var invocations = cmd.Invocations.Length > 0
            ? cmd.Invocations
            : [cmd.Name.ToLowerInvariant()];

        var argProperties = commandType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<ArgAttribute>() != null && p.CanWrite)
            .OrderBy(p =>
            {
                var order = p.GetCustomAttribute<ArgAttribute>()!.Order;
                return order >= 0 ? order : int.MaxValue;
            })
            .ThenBy(p => p.MetadataToken)
            .ToArray();

        var args = argProperties.Select(p =>
        {
            var arg = p.GetCustomAttribute<ArgAttribute>()!;
            return new ArgumentDescriptor(
                name: arg.Name ?? p.Name,
                isRequired: arg.IsRequired,
                defaultValue: arg.DefaultValue,
                contextKey: arg.ContextKey,
                validValues: arg.ValidValues,
                helpDetail: arg.HelpDetail,
                allowNamed: arg.AllowNamed,
                propertyName: p.Name,
                propertyType: p.PropertyType
            );
        }).ToArray();

        var envArgProperties = commandType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<EnvironmentArgAttribute>() != null && p.CanWrite)
            .OrderBy(p => p.MetadataToken)
            .ToArray();

        var envArgs = envArgProperties.Select(p =>
        {
            var attr = p.GetCustomAttribute<EnvironmentArgAttribute>()!;
            return new ArgumentDescriptor(
                name: p.Name.ToLowerInvariant(),
                isRequired: false,
                defaultValue: null,  // default resolved at runtime from IEnvironmentSource
                contextKey: null,
                validValues: null,   // valid values resolved at runtime from IEnvironmentSource
                helpDetail: attr.HelpDetail,
                allowNamed: true,
                propertyName: p.Name,
                propertyType: typeof(string)
            );
        }).ToArray();

        return new CommandDescriptor
        {
            Name = cmd.Name,
            Invocations = invocations,
            HelpBrief = cmd.HelpBrief,
            HelpDetail = cmd.HelpDetail,
            Category = cmd.Category,
            Args = [.. args, .. envArgs]
        };
    }
}
