namespace RossWright.MetalCommand;

/// <summary>
/// Provides the list of named environments and identifies the default.
/// Implement this interface to supply environment choices to commands decorated with
/// <see cref="EnvironmentArgAttribute"/>. Register the implementation in DI so
/// <see cref="EnvironmentArgAttribute.EnvironmentSourceType"/> (or the single registered
/// instance) can be resolved.
/// </summary>
public interface IEnvironmentSource
{
    /// <summary>The environment name used when no explicit value is supplied by the user.</summary>
    string DefaultEnvironment { get; }

    /// <summary>All available environments, including their protection status.</summary>
    EnvironmentEntry[] Environments { get; }
}
