namespace RossWright.MetalCommand;

/// <summary>
/// Represents a single named environment returned by <see cref="IEnvironmentSource.Environments"/>.
/// </summary>
public sealed class EnvironmentEntry
{
    /// <summary>The environment name (e.g. <c>"local"</c>, <c>"prod"</c>).</summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// When <see langword="true"/>, <see cref="EnvironmentPolicy.Dangerous"/> commands prompt for
    /// confirmation and <see cref="EnvironmentPolicy.Forbidden"/> commands refuse to execute against
    /// this environment.
    /// </summary>
    public bool IsProtected { get; init; }
}
