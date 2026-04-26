using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;
using NSubstitute;

namespace RossWright.MetalCommand.Data.Tests;

public class ObliterateCommandTests
{
    [Fact]
    public void Constructor_StoresFactory()
    {
        var factory = Substitute.For<IDatabaseContextFactory<TestDbContext>>();

        var command = new ObliterateCommand<TestDbContext>(factory);

        command.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_DatabaseDoesNotExist_WritesError()
    {
        var console = new TestConsole();
        // Use a file path that doesn't exist so DatabaseExists() returns false
        var nonExistentDb = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseSqlite($"DataSource={nonExistentDb}") });
        var command = new ObliterateCommand<TestDbContext>(factory) { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeFalse();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("does not exist");
    }

    [Fact]
    public async Task ExecuteAsync_SqliteConnection_ThrowsOrAnnounces()
    {
        // Obliterate uses SQL Server-specific T-SQL; SQLite in-memory always reports the db as
        // existing once the connection is open, so this test verifies the SQLite error is surfaced.
        var console = new TestConsole();
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        var envs = new[]
        {
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseSqlite(connection) }
        };
        var factory = DbContextFixture.BuildFactory("dev", envs);
        var command = new ObliterateCommand<TestDbContext>(factory) { Environment = "dev" };

        await Should.ThrowAsync<Exception>(() => command.ExecuteAsync(console, CancellationToken.None));
    }

    [Fact]
    public void Descriptor_HasForbiddenEnvironmentPolicy()
    {
        var prop = typeof(ObliterateCommand<TestDbContext>).GetProperty(nameof(ObliterateCommand<TestDbContext>.Environment));
        var attr = prop?.GetCustomAttribute<EnvironmentArgAttribute>();

        attr.ShouldNotBeNull();
        attr!.Policy.ShouldBe(EnvironmentPolicy.Forbidden);
    }

    [Fact]
    public void Descriptor_HasExpectedInvocations()
    {
        var attr = typeof(ObliterateCommand<TestDbContext>).GetCustomAttribute<CommandAttribute>();

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("Obliterate");
        attr.Invocations.ShouldContain("MegaNuke");
    }

    [Fact]
    public async Task ExecuteAsync_DatabaseExists_ObliteratesAndReturnsOk()
    {
        var console = new TestConsole();
        var dbName = $"TestDb_{Guid.NewGuid():N}";
        var connectionString = $"Server=(localdb)\\mssqllocaldb;Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true";

        try
        {
            // Create the database first
            var setupFactory = DbContextFixture.BuildFactory("dev",
                new DatabaseEnvironment
                {
                    Environment = "dev",
                    IsProtected = false,
                    SetOptions = b => b.UseSqlServer(connectionString)
                });
            using (var setupCtx = setupFactory.GetContext("dev"))
            {
                setupCtx.Database.EnsureCreated();
            }

            // Now test obliterate
            var factory = DbContextFixture.BuildFactory("dev",
                new DatabaseEnvironment
                {
                    Environment = "dev",
                    IsProtected = false,
                    SetOptions = b => b.UseSqlServer(connectionString)
                });
            var command = new ObliterateCommand<TestDbContext>(factory) { Environment = "dev" };

            var result = await command.ExecuteAsync(console, CancellationToken.None);

            result.Success.ShouldBeTrue();
            console.Lines.ShouldContain(line => line.Contains("Erasing dev database"));
        }
        finally
        {
            // Clean up - drop the database
            try
            {
                var cleanupFactory = DbContextFixture.BuildFactory("dev",
                    new DatabaseEnvironment
                    {
                        Environment = "dev",
                        IsProtected = false,
                        SetOptions = b => b.UseSqlServer(connectionString)
                    });
                using var cleanupCtx = cleanupFactory.GetContext("dev");
                cleanupCtx.Database.EnsureDeleted();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
