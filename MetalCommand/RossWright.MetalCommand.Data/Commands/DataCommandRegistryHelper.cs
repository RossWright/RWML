using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand.Data.Commands;

internal static class DataCommandRegistryHelper
{
    internal static DataCommandOptionsRegistry EnsureRegistry(IConsoleApplicationBuilder builder)
    {
        var existing = builder.Services
            .Where(d => d.ServiceType == typeof(ICommandOptionsRegistry))
            .Select(d => d.ImplementationInstance as DataCommandOptionsRegistry)
            .FirstOrDefault(r => r != null);

        if (existing != null)
            return existing;

        var registry = new DataCommandOptionsRegistry();
        builder.Services.AddSingleton<ICommandOptionsRegistry>(registry);
        return registry;
    }
}
