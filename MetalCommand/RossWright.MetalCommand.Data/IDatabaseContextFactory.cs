namespace RossWright.MetalCommand.Data;

/// <summary>
/// Base interface for an environment-aware EF Core context factory. Extends
/// <see cref="IEnvironmentSource"/> so commands decorated with <see cref="EnvironmentArgAttribute"/>
/// can resolve available environments automatically.
/// </summary>
public interface IDatabaseContextFactory : IEnvironmentSource
{
    /// <summary>All registered database environments for this factory.</summary>
    DatabaseEnvironment[] DatabaseEnvironments { get; }
    EnvironmentEntry[] IEnvironmentSource.Environments =>
        DatabaseEnvironments.Select(e => new EnvironmentEntry { Name = e.Environment, IsProtected = e.IsProtected }).ToArray();
}

/// <summary>
/// Creates a <typeparamref name="TDBCTX"/> instance pre-configured for a named environment.
/// Register via <see cref="AddDatabaseContextFactoryExtensions.AddDatabaseContextFactory{TDBCTX}"/>.
/// </summary>
/// <typeparam name="TDBCTX">The <see cref="DbContext"/> type managed by this factory.</typeparam>
public interface IDatabaseContextFactory<TDBCTX> : IDatabaseContextFactory where TDBCTX : DbContext
{
    /// <summary>
    /// Creates a <typeparamref name="TDBCTX"/> configured for <paramref name="environment"/>.
    /// </summary>
    /// <param name="environment">The environment name to use. Defaults to the registered default when <see langword="null"/>.</param>
    /// <returns>A new <typeparamref name="TDBCTX"/> instance.</returns>
    TDBCTX GetContext(string? environment = null);
}


