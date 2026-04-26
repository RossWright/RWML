using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand.Tests.Infrastructure;

internal static class CommandFixture
{
    /// <summary>
    /// Builds a <see cref="ConsoleApplication"/> with the supplied <see cref="TestConsole"/> and
    /// optional command/service registrations.
    /// </summary>
    public static (ConsoleApplication App, TestConsole Console) Build(
        Action<CommandCollectionBuilder, IServiceCollection>? configure = null,
        IReadOnlyList<Type>? middlewareTypes = null,
        params string?[] consoleInputs)
    {
        var testConsole = new TestConsole(consoleInputs);
        var services = new ServiceCollection();
        services.AddSingleton<IConsole>(testConsole);
        IConfiguration configuration = new ConfigurationBuilder().Build();
        var commandCollectionBuilder = new CommandCollectionBuilder();

        configure?.Invoke(commandCollectionBuilder, services);

        var app = new ConsoleApplication(testConsole, services, null, commandCollectionBuilder, middlewareTypes)
        {
            Configuration = configuration
        };
        return (app, testConsole);
    }
}
