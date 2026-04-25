using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand;

public interface IConsoleApplicationBuilder : IOptionsBuilder
{
    ICommandCollection Commands { get; }
    IConfiguration Configuration { get; }
    IConsole Console { get; }
    Func<IDictionary<string, string>, string>? PromptFactory { get; set; }
    IServiceCollection Services { get; }

    IConsoleApplicationBuilder SetColors(ConsoleColor? introOutroColor = null, ConsoleColor? helpColor = null, ConsoleColor? warningColor = null, ConsoleColor? errorColor = null, ConsoleColor? errorBgColor = null);
    IConsoleApplicationBuilder SetTabWidth(int width);

    void SetServiceProviderFactory(IServiceProviderFactory<IServiceCollection> serviceProviderFactory);
}

public static class IConsoleApplicationBuilderExtensions
{
    public static IConsoleApplicationBuilder AddServices(this IConsoleApplicationBuilder builder,
    Action<IServiceCollection> addServices)
    {
        addServices(builder.Services);
        return builder;
    }

    public static IConsoleApplicationBuilder AddCommands(this IConsoleApplicationBuilder builder, 
        Action<ICommandCollection> addCommands)
    {
        addCommands(builder.Commands);
        return builder;
    }

    public static IConsoleApplicationBuilder AddCommands(this IConsoleApplicationBuilder builder,
        Action<ICommandCollection, IConfiguration> addCommands)
    {
        addCommands(builder.Commands, builder.Configuration);
        return builder;
    }

    public static IConsoleApplicationBuilder SetPromptFactory(this IConsoleApplicationBuilder builder, 
        Func<IDictionary<string, string>, string>? promptFactory)
    {
        builder.PromptFactory = promptFactory;
        return builder;
    }

}