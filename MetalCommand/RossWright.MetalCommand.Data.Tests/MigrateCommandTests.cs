using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class MigrateCommandTests
{
    [Fact]
    public async Task ExecuteAsync_NullCallbacks_MigratesWithoutError()
    {
        var console = new TestConsole();
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var command = new MigrateCommand<TestDbContext>(factory, new MigrateCommandOptions<TestDbContext>()) { Environment = "dev" };
            var result = await command.ExecuteAsync(console, CancellationToken.None);
            result.Success.ShouldBeTrue();
        }

        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_InvokesPreAndPostCallbacks()
    {
        var order = new List<string>();
        var console = new TestConsole();
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var command = new MigrateCommand<TestDbContext>(factory,
                new MigrateCommandOptions<TestDbContext>
                {
                    PreMigration = _ => { order.Add("pre"); return Task.CompletedTask; },
                    PostMigration = _ => { order.Add("post"); return Task.CompletedTask; }
                })
            { Environment = "dev" };

            await command.ExecuteAsync(console, CancellationToken.None);
        }

        order.ShouldBe(["pre", "post"]);
    }

    [Fact]
    public void Descriptor_HasExpectedInvocations()
    {
        var attr = typeof(MigrateCommand<TestDbContext>).GetCustomAttribute<CommandAttribute>();

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("Migrate");
    }

    [Fact]
    public void Constructor_WithNullCallbacks_CreatesInstance()
    {
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var options = new MigrateCommandOptions<TestDbContext>();

            var command = new MigrateCommand<TestDbContext>(factory, options);

            command.ShouldNotBeNull();
        }
    }

    [Fact]
    public void Constructor_WithPreMigrationCallback_CreatesInstance()
    {
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var options = new MigrateCommandOptions<TestDbContext>
            {
                PreMigration = _ => Task.CompletedTask
            };

            var command = new MigrateCommand<TestDbContext>(factory, options);

            command.ShouldNotBeNull();
        }
    }

    [Fact]
    public void Constructor_WithPostMigrationCallback_CreatesInstance()
    {
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var options = new MigrateCommandOptions<TestDbContext>
            {
                PostMigration = _ => Task.CompletedTask
            };

            var command = new MigrateCommand<TestDbContext>(factory, options);

            command.ShouldNotBeNull();
        }
    }

    [Fact]
    public void Constructor_WithBothCallbacks_CreatesInstance()
    {
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var options = new MigrateCommandOptions<TestDbContext>
            {
                PreMigration = _ => Task.CompletedTask,
                PostMigration = _ => Task.CompletedTask
            };

            var command = new MigrateCommand<TestDbContext>(factory, options);

            command.ShouldNotBeNull();
        }
    }
}