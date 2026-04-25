using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Data;

public interface ISqlServerConnectionBuilder
{
    DbContextOptionsBuilder DbOpt { get; }
    SqlServerDbContextOptionsBuilder SqlServer { get; }
}

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

    public static void AddSqlServerDefault(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServer(environment, connectionString, opts, isDefault: true, isProtected: false);

    public static void AddSqlServerProtected(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServer(environment, connectionString, opts, isDefault: false, isProtected: true);

    public static void AddSqlServerDefaultProtected(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<ISqlServerConnectionBuilder>? opts = null) =>
        builder.AddSqlServer(environment, connectionString, opts, isDefault: true, isProtected: true);


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
