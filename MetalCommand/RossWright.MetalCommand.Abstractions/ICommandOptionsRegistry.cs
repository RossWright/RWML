namespace RossWright.MetalCommand;

/// <summary>
/// Provides per-command option overrides (invocations, environment policy) that are
/// registered at application-builder time and read at descriptor-build and middleware
/// execution time.
/// </summary>
public interface ICommandOptionsRegistry
{
    /// <summary>
    /// Returns the <see cref="CommandOptions"/> for the given command type,
    /// or <see langword="null"/> if none were registered.
    /// </summary>
    CommandOptions? Get(Type commandType);
}

/// <summary>
/// Options that can be overridden for a pre-packaged command at registration time.
/// </summary>
public class CommandOptions
{
    /// <summary>
    /// When set, replaces the command's default invocation tokens.
    /// The first element becomes the canonical name; the remainder are aliases.
    /// </summary>
    public string[]? Invocations { get; set; }

    /// <summary>
    /// When set, overrides the <see cref="EnvironmentPolicy"/> declared on the
    /// command's <c>[EnvironmentArg]</c> property.
    /// </summary>
    public EnvironmentPolicy? EnvironmentPolicy { get; set; }
}
