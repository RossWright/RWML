namespace RossWright.MetalCommand.Data;

/// <summary>
/// Holds the configuration for a single named database environment registered with
/// <see cref="IDatabaseContextFactoryBuilder"/>.
/// </summary>
public class DatabaseEnvironment
{
    /// <summary>The environment name (e.g. <c>"dev"</c>, <c>"prod"</c>).</summary>
    public string Environment { get; set; } = null!;

    /// <summary>When <see langword="true"/>, <see cref="EnvironmentPolicy"/> rules apply to this environment.</summary>
    public bool IsProtected { get; set; }

    /// <summary>Delegate that configures <see cref="DbContextOptionsBuilder"/> for this environment.</summary>
    public Action<DbContextOptionsBuilder> SetOptions { get; set; } = null!;
}
