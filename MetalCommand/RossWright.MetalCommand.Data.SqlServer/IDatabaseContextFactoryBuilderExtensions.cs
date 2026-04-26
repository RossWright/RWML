using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Data;

/// <summary>
/// Provides access to the underlying <see cref="DbContextOptionsBuilder"/> and
/// <see cref="SqlServerDbContextOptionsBuilder"/> within an <see cref="AddSqlServer"/> callback.
/// </summary>
public interface ISqlServerConnectionBuilder
{
    /// <summary>The base EF Core options builder.</summary>
    DbContextOptionsBuilder DbOpt { get; }
    /// <summary>The SQL Server-specific options builder.</summary>
    SqlServerDbContextOptionsBuilder SqlServer { get; }
}

/// <summary>
/// Extension methods for <see cref="IDatabaseContextFactoryBuilder"/> that register
/// SQL Server database environments.
/// </summary>
public static class IDatabaseContextFactoryBuilderExtensions
{
    private sealed class SqlServerConnectionBuilder(
        DbContextOptionsBuilder dbOpt, 
        SqlServerDbContextOptionsBuilder sqlServer)
        : ISqlServerConnectionBuilder
    {
        public DbContextOptionsBuilder DbOpt { get; } = dbOpt;
        public SqlServerDbContextOptionsBuilder SqlServer { get; } = sqlServer;
    }

    /// <summary>
    /// Registers a SQL Server environment using a literal connection string.
    /// Sets a default command timeout of 300 seconds.
    /// </summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name (e.g. <c>"dev"</c>).</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="SqlServerOptsBuilder">Optional delegate for additional SQL Server/EF options.</param>
    /// <param name="isDefault">When <see langword="true"/>, used when no environment is specified.</param>
    /// <param name="isProtected">When <see langword="true"/>, environment-policy rules apply.</param>
    public static void AddSqlServer(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? SqlServerOptsBuilder = null,
        bool isDefault = false,
        bool isProtected = false) =>
        builder.Add(environment,
            _ => _.UseSqlServer(connectionString,
                                opt =>
                                {
                                    opt.CommandTimeout(300);
                                    if (SqlServerOptsBuilder != null)
                                        SqlServerOptsBuilder(new SqlServerConnectionBuilder(_, opt));
                                }),
            isDefault,
            isProtected);

    /// <summary>Registers a SQL Server environment as the default (non-protected).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="opts">Optional delegate for additional SQL Server/EF options.</param>
    public static void AddSqlServerDefault(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServer(environment, connectionString, opts, isDefault: true, isProtected: false);

    /// <summary>Registers a SQL Server environment as protected (non-default).</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="opts">Optional delegate for additional SQL Server/EF options.</param>
    public static void AddSqlServerProtected(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServer(environment, connectionString, opts, isDefault: false, isProtected: true);

    /// <summary>Registers a SQL Server environment as both default and protected.</summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="opts">Optional delegate for additional SQL Server/EF options.</param>
    public static void AddSqlServerDefaultProtected(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServer(environment, connectionString, opts, isDefault: true, isProtected: true);


    /// <summary>
    /// Registers a SQL Server environment using a connection string resolved from
    /// <c>IConfiguration.GetConnectionString(connectionStringName)</c>.
    /// </summary>
    /// <param name="builder">The factory builder.</param>
    /// <param name="environment">The environment name.</param>
    /// <param name="connectionStringName">The key under <c>ConnectionStrings</c> in configuration.</param>
    /// <param name="SqlServerOptsBuilder">Optional delegate for additional SQL Server/EF options.</param>
    /// <param name="isDefault">When <see langword="true"/>, used when no environment is specified.</param>
    /// <param name="isProtected">When <see langword="true"/>, environment-policy rules apply.</param>
    /// <exception cref="InvalidOperationException">Thrown when the named connection string is not found in configuration.</exception>
    public static void AddSqlServerByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionStringName,
        Action<ISqlServerConnectionBuilder>? SqlServerOptsBuilder = null,
        bool isDefault = false,
        bool isProtected = false)
    {
        var connectionString = builder.Configuration.GetConnectionString(connectionStringName);
        if (connectionString == null)
            throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");
        builder.AddSqlServer(environment, connectionString, SqlServerOptsBuilder, isDefault, isProtected);
    }

    public static void AddSqlServerDefaultByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServerByConfigurationName(environment, connectionString, opts, isDefault: true, isProtected: false);

    public static void AddSqlServerProtectedByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServerByConfigurationName(environment, connectionString, opts, isDefault: false, isProtected: true);

    public static void AddSqlServerDefaultProtectedByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServerByConfigurationName(environment, connectionString, opts, isDefault: true, isProtected: true);
}
