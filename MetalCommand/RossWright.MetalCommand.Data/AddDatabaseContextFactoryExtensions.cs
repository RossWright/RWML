using Microsoft.Extensions.DependencyInjection;

namespace RossWright.MetalCommand.Data;

public static class AddDatabaseContextFactoryExtensions
{
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