using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace RossWright.MetalCommand.Data.Tests;

public class IDatabaseContextFactoryBuilderExtensionsTests
{
    [Fact]
    public void AddSqlServerDefault_CallsAdd_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "dev";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        builder.AddSqlServerDefault(environment, connectionString);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddSqlServerDefault_WithOpts_CallsAdd_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "prod";
        var connectionString = "Server=server;Database=db;";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        // Act
        builder.AddSqlServerDefault(environment, connectionString, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddSqlServerProtected_CallsAdd_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "prod";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        builder.AddSqlServerProtected(environment, connectionString);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddSqlServerProtected_WithOpts_CallsAdd_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "staging";
        var connectionString = "Server=server;Database=db;";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        // Act
        builder.AddSqlServerProtected(environment, connectionString, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddSqlServerDefaultProtected_CallsAdd_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "prod";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        builder.AddSqlServerDefaultProtected(environment, connectionString);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddSqlServerDefaultProtected_WithOpts_CallsAdd_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "production";
        var connectionString = "Server=server;Database=db;";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        // Act
        builder.AddSqlServerDefaultProtected(environment, connectionString, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddSqlServerByConfigurationName_CallsAdd_WithConnectionStringFromConfiguration()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "DefaultConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "dev";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: false);
    }

    [Fact]
    public void AddSqlServerByConfigurationName_WithAllParameters_CallsAdd_WithCorrectFlags()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "ProdConnection";
        var connectionString = "Server=prod;Database=prod;";
        var environment = "prod";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerByConfigurationName(environment, connectionStringName, opts, isDefault: true, isProtected: true);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddSqlServerByConfigurationName_NullConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "MissingConnection";
        var environment = "dev";

        configSection[connectionStringName].Returns((string?)null);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
            builder.AddSqlServerByConfigurationName(environment, connectionStringName));

        exception.Message.ShouldBe($"Connection string '{connectionStringName}' not found in configuration.");
    }

    [Fact]
    public void AddSqlServerDefaultByConfigurationName_CallsAddSqlServerByConfigurationName_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "DefaultConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "dev";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerDefaultByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddSqlServerDefaultByConfigurationName_WithOpts_CallsAddSqlServerByConfigurationName_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "AppConnection";
        var connectionString = "Server=server;Database=db;";
        var environment = "production";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerDefaultByConfigurationName(environment, connectionStringName, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddSqlServerProtectedByConfigurationName_CallsAddSqlServerByConfigurationName_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "ProtectedConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "prod";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerProtectedByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddSqlServerProtectedByConfigurationName_WithOpts_CallsAddSqlServerByConfigurationName_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "SecureConnection";
        var connectionString = "Server=secure;Database=db;";
        var environment = "staging";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerProtectedByConfigurationName(environment, connectionStringName, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddSqlServerDefaultProtectedByConfigurationName_CallsAddSqlServerByConfigurationName_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "DefaultProtectedConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "prod";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerDefaultProtectedByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddSqlServerDefaultProtectedByConfigurationName_WithOpts_CallsAddSqlServerByConfigurationName_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "PrimarySecureConnection";
        var connectionString = "Server=primary;Database=db;";
        var environment = "production";
        Action<ISqlServerConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddSqlServerDefaultProtectedByConfigurationName(environment, connectionStringName, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddMySqlDefault_CallsAddMySql_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "dev";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        builder.AddMySqlDefault(environment, connectionString);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddMySqlDefault_WithOpts_CallsAddMySql_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "prod";
        var connectionString = "Server=server;Database=db;";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        // Act
        builder.AddMySqlDefault(environment, connectionString, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddMySqlProtected_CallsAddMySql_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "prod";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        builder.AddMySqlProtected(environment, connectionString);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddMySqlProtected_WithOpts_CallsAddMySql_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "staging";
        var connectionString = "Server=server;Database=db;";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        // Act
        builder.AddMySqlProtected(environment, connectionString, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddMySqlDefaultProtected_CallsAddMySql_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "prod";
        var connectionString = "Server=localhost;Database=test;";

        // Act
        builder.AddMySqlDefaultProtected(environment, connectionString);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddMySqlDefaultProtected_WithOpts_CallsAddMySql_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var environment = "production";
        var connectionString = "Server=server;Database=db;";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        // Act
        builder.AddMySqlDefaultProtected(environment, connectionString, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddMySqlByConfigurationName_CallsAddMySql_WithConnectionStringFromConfiguration()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "DefaultConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "dev";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: false);
    }

    [Fact]
    public void AddMySqlByConfigurationName_WithAllParameters_CallsAddMySql_WithCorrectFlags()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "ProdConnection";
        var connectionString = "Server=prod;Database=prod;";
        var environment = "prod";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlByConfigurationName(environment, connectionStringName, opts, isDefault: true, isProtected: true);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddMySqlByConfigurationName_NullConnectionString_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "MissingConnection";
        var environment = "dev";

        configSection[connectionStringName].Returns((string?)null);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
            builder.AddMySqlByConfigurationName(environment, connectionStringName));

        exception.Message.ShouldBe($"Connection string '{connectionStringName}' not found in configuration.");
    }

    [Fact]
    public void AddMySqlDefaultByConfigurationName_CallsAddMySqlByConfigurationName_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "DefaultConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "dev";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlDefaultByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddMySqlDefaultByConfigurationName_WithOpts_CallsAddMySqlByConfigurationName_WithDefaultTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "AppConnection";
        var connectionString = "Server=server;Database=db;";
        var environment = "production";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlDefaultByConfigurationName(environment, connectionStringName, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: false);
    }

    [Fact]
    public void AddMySqlProtectedByConfigurationName_CallsAddMySqlByConfigurationName_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "ProtectedConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "prod";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlProtectedByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddMySqlProtectedByConfigurationName_WithOpts_CallsAddMySqlByConfigurationName_WithProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "SecureConnection";
        var connectionString = "Server=secure;Database=db;";
        var environment = "staging";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlProtectedByConfigurationName(environment, connectionStringName, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: false, isProtected: true);
    }

    [Fact]
    public void AddMySqlDefaultProtectedByConfigurationName_CallsAddMySqlByConfigurationName_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "DefaultProtectedConnection";
        var connectionString = "Server=localhost;Database=test;";
        var environment = "prod";

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlDefaultProtectedByConfigurationName(environment, connectionStringName);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }

    [Fact]
    public void AddMySqlDefaultProtectedByConfigurationName_WithOpts_CallsAddMySqlByConfigurationName_WithDefaultAndProtectedTrue()
    {
        // Arrange
        var builder = Substitute.For<IDatabaseContextFactoryBuilder>();
        var configSection = Substitute.For<IConfigurationSection>();
        var configuration = Substitute.For<IConfiguration>();
        var connectionStringName = "PrimarySecureConnection";
        var connectionString = "Server=primary;Database=db;";
        var environment = "production";
        Action<IMySqlConnectionBuilder> opts = _ => { };

        configSection[connectionStringName].Returns(connectionString);
        configuration.GetSection("ConnectionStrings").Returns(configSection);
        builder.Configuration.Returns(configuration);

        // Act
        builder.AddMySqlDefaultProtectedByConfigurationName(environment, connectionStringName, opts);

        // Assert
        builder.Received(1).Add(environment, Arg.Any<Action<DbContextOptionsBuilder>>(), isDefault: true, isProtected: true);
    }
}
