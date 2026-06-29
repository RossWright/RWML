using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Data;

/// <summary>
/// Provides access to the underlying <see cref="DbContextOptionsBuilder"/> and
/// <see cref="MySqlDbContextOptionsBuilder"/> within an <c>AddMySql</c> callback.
/// </summary>
public interface IMySqlConnectionBuilder
{
    /// <summary>The base EF Core options builder.</summary>
    DbContextOptionsBuilder DbOpt { get; }
    /// <summary>The MySQL-specific options builder.</summary>
    MySqlDbContextOptionsBuilder MySql { get; }
}

/// <summary>
/// Extension methods for <see cref="IDatabaseContextFactoryBuilder"/> that register
/// MySQL (Pomelo) database environments.
/// </summary>
public static class IDatabaseContextFactoryBuilderExtensions
{
    private sealed class MySqlConnectionBuilder(
        DbContextOptionsBuilder dbOpt, 
        MySqlDbContextOptionsBuilder mySql)
        : IMySqlConnectionBuilder
    {
        public DbContextOptionsBuilder DbOpt { get; } = dbOpt;
        public MySqlDbContextOptionsBuilder MySql { get; } = mySql;
    }

    /// <summary>
    /// Registers a MySQL environment using a literal connection string.
    /// Sets a default command timeout of 300 seconds.
    /// </summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"dev"</c>).</param>
    /// <param name="connectionString">The MySQL connection string.</param>
    /// <param name="mySqlOptsBuilder">Optional delegate for additional MySQL/EF options.</param>
    /// <param name="isDefault">When <see langword="true"/>, used when no environment is specified.</param>
    /// <param name="isProtected">When <see langword="true"/>, environment-policy rules apply.</param>
    public static void AddMySql(
        this IDatabaseContextFactoryBuilder builder,
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? mySqlOptsBuilder = null, 
        bool isDefault = false, 
        bool isProtected = false) =>
        builder.Add(environment, 
            _ => _.UseMySql(connectionString, 
                            ServerVersion.AutoDetect(connectionString), 
                            opt =>
                            {
                                opt.CommandTimeout(300);
                                if (mySqlOptsBuilder != null) 
                                    mySqlOptsBuilder(new MySqlConnectionBuilder(_, opt));
                            }), 
            isDefault, 
            isProtected);

    /// <summary>Registers a MySQL environment as the default (non-protected).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The MySQL connection string.</param>
    /// <param name="opts">Optional delegate for additional MySQL/EF options.</param>
    public static void AddMySqlDefault(
        this IDatabaseContextFactoryBuilder builder, 
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? opts = null) => 
        builder.AddMySql(environment, connectionString, opts, isDefault: true, isProtected: false);

    /// <summary>Registers a MySQL environment as protected (non-default).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The MySQL connection string.</param>
    /// <param name="opts">Optional delegate for additional MySQL/EF options.</param>
    public static void AddMySqlProtected(
        this IDatabaseContextFactoryBuilder builder, 
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? opts = null) => 
        builder.AddMySql(environment, connectionString, opts, isDefault: false, isProtected: true);

    /// <summary>Registers a MySQL environment as both default and protected.</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The MySQL connection string.</param>
    /// <param name="opts">Optional delegate for additional MySQL/EF options.</param>
    public static void AddMySqlDefaultProtected(
        this IDatabaseContextFactoryBuilder builder, 
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? opts = null) => 
        builder.AddMySql(environment, connectionString, opts, isDefault: true, isProtected: true);


    /// <summary>
    /// Registers a MySQL environment using a connection string resolved from
    /// <c>IConfiguration.GetConnectionString(connectionStringName)</c>.
    /// </summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionStringName">The key under <c>ConnectionStrings</c> in configuration.</param>
    /// <param name="mySqlOptsBuilder">Optional delegate for additional MySQL/EF options.</param>
    /// <param name="isDefault">When <see langword="true"/>, used when no environment is specified.</param>
    /// <param name="isProtected">When <see langword="true"/>, environment-policy rules apply.</param>
    /// <exception cref="InvalidOperationException">Thrown when the named connection string is not found in configuration.</exception>
    public static void AddMySqlByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionStringName,
        Action<IMySqlConnectionBuilder>? mySqlOptsBuilder = null,
        bool isDefault = false,
        bool isProtected = false)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionStringName);
        if (connectionString == null)
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
        builder.AddMySql(environment, connectionString, mySqlOptsBuilder, isDefault, isProtected);
    }

    /// <summary>Registers a configuration-backed MySQL environment as the default (non-protected).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The key under <c>ConnectionStrings</c> in configuration.</param>
    /// <param name="opts">Optional delegate for additional MySQL/EF options.</param>
    public static void AddMySqlDefaultByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<IMySqlConnectionBuilder>? opts = null) =>
        builder.AddMySqlByConfigurationName(environment, connectionString, opts, isDefault: true, isProtected: false);

    /// <summary>Registers a configuration-backed MySQL environment as protected (non-default).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The key under <c>ConnectionStrings</c> in configuration.</param>
    /// <param name="opts">Optional delegate for additional MySQL/EF options.</param>
    public static void AddMySqlProtectedByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<IMySqlConnectionBuilder>? opts = null) =>
        builder.AddMySqlByConfigurationName(environment, connectionString, opts, isDefault: false, isProtected: true);

    /// <summary>Registers a configuration-backed MySQL environment as both default and protected.</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The key under <c>ConnectionStrings</c> in configuration.</param>
    /// <param name="opts">Optional delegate for additional MySQL/EF options.</param>
    public static void AddMySqlDefaultProtectedByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<IMySqlConnectionBuilder>? opts = null) =>
        builder.AddMySqlByConfigurationName(environment, connectionString, opts, isDefault: true, isProtected: true);
}
