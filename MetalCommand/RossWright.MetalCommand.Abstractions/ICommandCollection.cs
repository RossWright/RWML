namespace RossWright.MetalCommand;

/// <summary>
/// Registers <see cref="ICommand"/> implementations with the application builder.
/// Obtained via <see cref="IConsoleApplicationBuilder.Commands"/>.
/// </summary>
public interface ICommandCollection : IAssemblyScanningOptionsBuilder
{
    /// <summary>Registers a command by type parameter.</summary>
    /// <typeparam name="TCOMMAND">The command type to register.</typeparam>
    /// <returns>This collection for chaining.</returns>
    ICommandCollection Add<TCOMMAND>() where TCOMMAND : class, ICommand;

    /// <summary>Registers a command by its <see cref="Type"/>.</summary>
    /// <param name="commandType">The command type to register. Must implement <see cref="ICommand"/>.</param>
    /// <returns>This collection for chaining.</returns>
    ICommandCollection Add(Type commandType);
}
