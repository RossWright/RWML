using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace RossWright.MetalCommand.Data;

public interface IMySqlConnectionBuilder
{
    DbContextOptionsBuilder DbOpt { get; }
    MySqlDbContextOptionsBuilder MySql { get; }
}

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

    public static void AddMySqlDefault(
        this IDatabaseContextFactoryBuilder builder, 
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? opts = null) => 
        builder.AddMySql(environment, connectionString, opts, isDefault: true, isProtected: false);

    public static void AddMySqlProtected(
        this IDatabaseContextFactoryBuilder builder, 
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? opts = null) => 
        builder.AddMySql(environment, connectionString, opts, isDefault: false, isProtected: true);

    public static void AddMySqlDefaultProtected(
        this IDatabaseContextFactoryBuilder builder, 
        string environment, 
        string connectionString, 
        Action<IMySqlConnectionBuilder>? opts = null) => 
        builder.AddMySql(environment, connectionString, opts, isDefault: true, isProtected: true);


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

    public static void AddMySqlDefaultByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<IMySqlConnectionBuilder>? opts = null) =>
        builder.AddMySqlByConfigurationName(environment, connectionString, opts, isDefault: true, isProtected: false);

    public static void AddMySqlProtectedByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<IMySqlConnectionBuilder>? opts = null) =>
        builder.AddMySqlByConfigurationName(environment, connectionString, opts, isDefault: false, isProtected: true);

    public static void AddMySqlDefaultProtectedByConfigurationName(
        this IDatabaseContextFactoryBuilder builder,
        string environment,
        string connectionString,
        Action<IMySqlConnectionBuilder>? opts = null) =>
        builder.AddMySqlByConfigurationName(environment, connectionString, opts, isDefault: true, isProtected: true);
}