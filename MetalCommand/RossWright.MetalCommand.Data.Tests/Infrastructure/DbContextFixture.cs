using Microsoft.Data.Sqlite;

namespace RossWright.MetalCommand.Data.Tests.Infrastructure;

internal static class DbContextFixture
{
    public static TestDbContext BuildInMemory(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    public static TestDbContext BuildSqlite()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new TestDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }

    /// <summary>
    /// Builds a real DatabaseContextFactory wired to in-memory databases,
    /// required by TryParseEnvironment which hard-casts to the internal type.
    /// Keys are lowercase to match the .ToLower() lookup in the extension.
    /// </summary>
    public static DatabaseContextFactory<TestDbContext> BuildFactory(
        string defaultEnvironment,
        params DatabaseEnvironment[] environments)
        => new(environments, defaultEnvironment);

    public static DatabaseContextFactory<TestDbContext> BuildDefaultFactory()
    {
        var envs = new[]
        {
            new DatabaseEnvironment { Environment = "dev",  IsProtected = false, SetOptions = b => b.UseInMemoryDatabase("dev") },
            new DatabaseEnvironment { Environment = "prod", IsProtected = true,  SetOptions = b => b.UseInMemoryDatabase("prod") },
        };
        return BuildFactory("dev", envs);
    }

    /// <summary>
    /// Builds a DatabaseContextFactory backed by a shared in-memory SQLite database that
    /// is pre-created so DatabaseExists() returns true. Suitable for ClearData/Obliterate tests.
    /// </summary>
    public static (DatabaseContextFactory<TestDbContext> Factory, SqliteConnection Connection) BuildSqliteFactory(
        string environment = "dev",
        bool isProtected = false)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        // Pre-create schema so DatabaseExists() returns true.
        var seedOptions = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;
        using var seed = new TestDbContext(seedOptions);
        seed.Database.EnsureCreated();

        var envs = new[]
        {
            new DatabaseEnvironment
            {
                Environment = environment,
                IsProtected = isProtected,
                SetOptions = b => b.UseSqlite(connection)
            }
        };
        return (BuildFactory(environment, envs), connection);
    }
}
