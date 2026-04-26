namespace RossWright.MetalCommand.Data;

/// <summary>
/// Configures per-environment <see cref="DbContext"/> options for an
/// <see cref="IDatabaseContextFactory{TDBCTX}"/>.
/// Obtain via the delegate passed to
/// <see cref="AddDatabaseContextFactoryExtensions.AddDatabaseContextFactory{TDBCTX}"/>.
/// </summary>
public interface IDatabaseContextFactoryBuilder
{
    /// <summary>The application configuration, available for resolving connection strings.</summary>
    IConfiguration Configuration { get; }

    /// <summary>
    /// Registers a named database environment.
    /// </summary>
    /// <param name="environment">The environment name (e.g. <c>"dev"</c>, <c>"prod"</c>).</param>
    /// <param name="opts">Delegate that configures <see cref="DbContextOptionsBuilder"/> for this environment.</param>
    /// <param name="isDefault">When <see langword="true"/>, this environment is selected when none is specified by the user.</param>
    /// <param name="isProtected">When <see langword="true"/>, <see cref="EnvironmentPolicy"/> rules apply.</param>
    void Add(string environment, Action<DbContextOptionsBuilder> opts, 
        bool isDefault = false, bool isProtected = false);
}

/// <summary>
/// Convenience extension methods for <see cref="IDatabaseContextFactoryBuilder"/>.
/// </summary>
public static class IDatabaseContextFactoryBuilderExtensions
{
    /// <summary>Registers the environment and marks it as the default (non-protected).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="opts">Delegate that configures <see cref="DbContextOptionsBuilder"/>.</param>
    public static void AddDefault(this IDatabaseContextFactoryBuilder builder, 
        string environment, Action<DbContextOptionsBuilder> opts) =>
        builder.Add(environment, opts, isDefault: true, isProtected: false);

    /// <summary>Registers the environment and marks it as protected (non-default).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="opts">Delegate that configures <see cref="DbContextOptionsBuilder"/>.</param>
    public static void AddProtected(this IDatabaseContextFactoryBuilder builder, 
        string environment, Action<DbContextOptionsBuilder> opts) =>
        builder.Add(environment, opts, isDefault: false, isProtected: true);

    /// <summary>Registers the environment and marks it as both default and protected.</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="opts">Delegate that configures <see cref="DbContextOptionsBuilder"/>.</param>
    public static void AddDefaultProtected(this IDatabaseContextFactoryBuilder builder, 
        string environment, Action<DbContextOptionsBuilder> opts) =>
        builder.Add(environment, opts, isDefault: true, isProtected: true);
}
