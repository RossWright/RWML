using Microsoft.Data.Sqlite;
using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class ClearDataCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ValidEnvironment_CallsClearDataAction()
    {
        var called = false;
        var console = new TestConsole();
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var command = new ClearDataCommand<TestDbContext>(
                factory,
                new ClearDataCommandOptions<TestDbContext> { ClearData = _ => { called = true; return Task.CompletedTask; } })
            { Environment = "dev" };

            var result = await command.ExecuteAsync(console, CancellationToken.None);
            result.Success.ShouldBeTrue();
        }

        called.ShouldBeTrue();
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_DatabaseDoesNotExist_ReturnsFailureAndWritesError()
    {
        var console = new TestConsole();
        var nonExistentDb = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseSqlite($"DataSource={nonExistentDb}") });
        var command = new ClearDataCommand<TestDbContext>(
            factory,
            new ClearDataCommandOptions<TestDbContext> { ClearData = _ => Task.CompletedTask })
        { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeFalse();
        console.ErrorLines.ShouldHaveSingleItem();
        console.ErrorLines[0].ShouldContain("does not");
    }

    [Fact]
    public async Task ExecuteAsync_ValidEnvironment_DelegateReceivesCorrectContext()
    {
        ClearDataCommandContext<TestDbContext>? capturedCtx = null;
        var console = new TestConsole();
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var command = new ClearDataCommand<TestDbContext>(
                factory,
                new ClearDataCommandOptions<TestDbContext>
                {
                    ClearData = ctx =>
                    {
                        capturedCtx = ctx;
                        return Task.CompletedTask;
                    }
                })
            { Environment = "dev" };

            await command.ExecuteAsync(console, CancellationToken.None);
        }

        capturedCtx.ShouldNotBeNull();
        capturedCtx.Environment.ShouldBe("dev");
        capturedCtx.Console.ShouldNotBeNull();
        capturedCtx.DbContext.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ValidEnvironment_SaveChangesCalledAfterDelegate()
    {
        var delegateExecuted = false;
        var console = new TestConsole();
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var command = new ClearDataCommand<TestDbContext>(
                factory,
                new ClearDataCommandOptions<TestDbContext>
                {
                    ClearData = ctx =>
                    {
                        delegateExecuted = true;
                        ctx.DbContext.Items.Add(new TestItem { Id = "save-test", Name = "pending" });
                        return Task.CompletedTask;
                    }
                })
            { Environment = "dev" };

            var result = await command.ExecuteAsync(console, CancellationToken.None);
            result.Success.ShouldBeTrue();

            // Verify the item added in the delegate was persisted (SaveChangesAsync ran)
            using var verifyCtx = new TestDbContext(
                new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options);
            verifyCtx.Items.Any(i => i.Id == "save-test").ShouldBeTrue();
        }

        delegateExecuted.ShouldBeTrue();
    }

    [Fact]
    public void Descriptor_HasExpectedInvocations()
    {
        var attr = typeof(ClearDataCommand<TestDbContext>).GetCustomAttribute<CommandAttribute>();

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("ClearData");
        attr.Invocations.ShouldContain("cd");
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        var factory = Substitute.For<IDatabaseContextFactory<TestDbContext>>();
        var options = new ClearDataCommandOptions<TestDbContext> { ClearData = _ => Task.CompletedTask };

        var command = new ClearDataCommand<TestDbContext>(factory, options);

        command.ShouldNotBeNull();
    }

    [Fact]
    public async Task Constructor_ValidParameters_StoresFactoryCorrectly()
    {
        var factory = Substitute.For<IDatabaseContextFactory<TestDbContext>>();
        var (testFactory, connection) = DbContextFixture.BuildSqliteFactory("test");
        using (connection)
        {
            var dbContext = testFactory.GetContext("test");
            factory.GetContext(Arg.Any<string>()).Returns(dbContext);
            var options = new ClearDataCommandOptions<TestDbContext> { ClearData = _ => Task.CompletedTask };
            var command = new ClearDataCommand<TestDbContext>(factory, options)
            {
                Environment = "test"
            };
            var console = new TestConsole();

            await command.ExecuteAsync(console, CancellationToken.None);

            factory.Received(1).GetContext("test");
        }
    }

    [Fact]
    public async Task Constructor_ValidParameters_StoresClearDataDelegateCorrectly()
    {
        var called = false;
        var factory = Substitute.For<IDatabaseContextFactory<TestDbContext>>();
        var (testFactory, connection) = DbContextFixture.BuildSqliteFactory("test");
        using (connection)
        {
            var options = new ClearDataCommandOptions<TestDbContext>
            {
                ClearData = ctx =>
                {
                    called = true;
                    return Task.CompletedTask;
                }
            };
            var command = new ClearDataCommand<TestDbContext>(testFactory, options)
            {
                Environment = "test"
            };
            var console = new TestConsole();

            await command.ExecuteAsync(console, CancellationToken.None);

            called.ShouldBeTrue();
        }
    }
}
