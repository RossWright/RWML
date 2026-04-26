using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand.Data;

/// <summary>
/// Extension methods for registering an environment-aware <see cref="IDatabaseContextFactory{TDBCTX}"/>
/// with a <see cref="IConsoleApplicationBuilder"/>.
/// </summary>
public static class AddDatabaseContextFactoryExtensions
{
    /// <summary>
    /// Registers a scoped <see cref="IDatabaseContextFactory{TDBCTX}"/> and automatically adds
    /// <see cref="EnvironmentArgMiddleware"/> to enforce environment policies.
    /// </summary>
    /// <typeparam name="TDBCTX">The <see cref="DbContext"/> type to manage.</typeparam>
    /// <param name="appBuilder">The application builder.</param>
    /// <param name="buildDatabaseContextFactory">Delegate that registers one or more named environments.</param>
    /// <returns>The same <paramref name="appBuilder"/> for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no environments are registered.</exception>
    public static IConsoleApplicationBuilder AddDatabaseContextFactory<TDBCTX>(
        this IConsoleApplicationBuilder appBuilder,
        Action<IDatabaseContextFactoryBuilder> buildDatabaseContextFactory)
        where TDBCTX : DbContext
    {
        DatabaseContextFactoryBuilder builder = new(appBuilder.Configuration);
        buildDatabaseContextFactory(builder);
        if (builder.DatabaseEnvironments.Count == 0)
            throw new InvalidOperationException("At least one database environment must be added.");

        appBuilder.Services.AddScoped<IDatabaseContextFactory<TDBCTX>>(_ =>
            new DatabaseContextFactory<TDBCTX>(
                builder.DatabaseEnvironments,
                builder.DefaultEnvironment ?? builder.DatabaseEnvironments[0].Environment));

        appBuilder.Services.AddScoped<IEnvironmentSource>(sp =>
            sp.GetRequiredService<IDatabaseContextFactory<TDBCTX>>());

        appBuilder.AddMiddleware<EnvironmentArgMiddleware>();

        return appBuilder;
    }

    private sealed class DatabaseContextFactoryBuilder(
        IConfiguration configuration) 
        : IDatabaseContextFactoryBuilder
    {
        public IConfiguration Configuration { get; } = configuration;

        public List<DatabaseEnvironment> DatabaseEnvironments { get; } = new();
        public string? DefaultEnvironment { get; private set; }

        public void Add(string environment, Action<DbContextOptionsBuilder> opts, bool isDefault = false, bool isProtected = false)
        {
            DatabaseEnvironments.Add(new()
            {
                Environment = environment,
                IsProtected = isProtected,
                SetOptions = opts
            });
            if (DefaultEnvironment == null || isDefault) DefaultEnvironment = environment;
        }
    }
}