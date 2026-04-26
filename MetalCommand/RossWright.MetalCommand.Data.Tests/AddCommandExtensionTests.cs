using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

/// <summary>
/// Tests for the Add*Command IConsoleApplicationBuilder extension methods.
/// Each test verifies the command is registered and dispatchable via the executor.
/// </summary>
public class AddCommandExtensionTests
{
    private static IConsoleApplicationBuilder BuilderWithDevFactory() =>
        ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
                b.Add("dev", o => o.UseInMemoryDatabase(Guid.NewGuid().ToString())));

    // ── AddMigrateCommand ────────────────────────────────────────────────────

    [Fact]
    public async Task AddMigrateCommand_RegistersCommand_ExecutableByInvocation()
    {
        var (_, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var app = ConsoleApplication.CreateBuilder()
                .AddDatabaseContextFactory<TestDbContext>(b =>
                    b.Add("dev", o => o.UseSqlite(connection)))
                .AddMigrateCommand<TestDbContext>()
                .Build();

            // "migrate" should be registered and execute without "No command found" error
            await app.Execute("migrate", "dev");
        }
    }

    [Fact]
    public async Task AddMigrateCommand_WithCallbacks_RegistersAndCallbacks_AreInvoked()
    {
        var preCalled = false;
        var postCalled = false;
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var app = ConsoleApplication.CreateBuilder()
                .AddDatabaseContextFactory<TestDbContext>(b =>
                    b.Add("dev", o => o.UseSqlite(connection)))
                .AddMigrateCommand<TestDbContext>(o =>
                {
                    o.PreMigration = _ => { preCalled = true; return Task.CompletedTask; };
                    o.PostMigration = _ => { postCalled = true; return Task.CompletedTask; };
                })
                .Build();

            await app.Execute("migrate", "dev");
        }

        preCalled.ShouldBeTrue();
        postCalled.ShouldBeTrue();
    }



    // ── AddLoadDataCommand ────────────────────────────────────────────────────

    [Fact]
    public async Task AddLoadDataCommand_RegistersCommand_LoadDataActionInvoked()
    {
        var called = false;
        var app = BuilderWithDevFactory()
            .AddLoadDataCommand<TestDbContext>(o => o.LoadData = _ => { called = true; return Task.CompletedTask; })
            .Build();

        await app.Execute("loaddata", "dev");

        called.ShouldBeTrue();
    }



    // ── AddClearDataCommand ────────────────────────────────────────────────────

    [Fact]
    public async Task AddClearDataCommand_WithAction_RegistersCommand_ActionInvoked()
    {
        var called = false;
        var (factory, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var app = ConsoleApplication.CreateBuilder()
                .AddDatabaseContextFactory<TestDbContext>(b =>
                    b.Add("dev", o => o.UseSqlite(connection)))
                .AddClearDataCommand<TestDbContext>(o => o.ClearData = _ => { called = true; return Task.CompletedTask; })
                .Build();

            await app.Execute("cleardata", "dev");
        }

        called.ShouldBeTrue();
    }



    [Fact]
    public void AddClearDataCommand_TableNames_BuildsWithoutError()
    {
        var app = BuilderWithDevFactory()
            .AddClearDataCommand<TestDbContext>(o => o.TableNames = ["Items", "Users"])
            .Build();

        app.ShouldNotBeNull();
    }



    // ── AddObliterateCommand ──────────────────────────────────────────────────

    [Fact]
    public void AddObliterateCommand_RegistersCommand_BuildsWithoutError()
    {
        var app = BuilderWithDevFactory()
            .AddObliterateCommand<TestDbContext>()
            .Build();

        app.ShouldNotBeNull();
    }



    // ── AddReloadDatabaseCommand ─────────────────────────────────────────────

    [Fact]
    public void AddReloadDatabaseCommand_RegistersCommand_BuildsWithoutError()
    {
        var app = BuilderWithDevFactory()
            .AddLoadDataCommand<TestDbContext>(o => o.LoadData = _ => Task.CompletedTask)
            .AddClearDataCommand<TestDbContext>(o => o.ClearData = _ => Task.CompletedTask)
            .AddMigrateCommand<TestDbContext>()
            .AddReloadDatabaseCommand<TestDbContext>()
            .Build();

        app.ShouldNotBeNull();
    }


    // ── WithInvocations ──────────────────────────────────────────────────────

    [Fact]
    public async Task WithInvocations_OnLoadDataCommand_CommandExecutableByNewInvocation()
    {
        var called = false;
        var app = BuilderWithDevFactory()
            .AddLoadDataCommand<TestDbContext>(o =>
            {
                o.LoadData = _ => { called = true; return Task.CompletedTask; };
                o.Invocations = ["seed", "s"];
            })
            .Build();

        await app.Execute("seed", "dev");

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task WithInvocations_OnLoadDataCommand_OldInvocationNoLongerWorks()
    {
        var called = false;
        var app = BuilderWithDevFactory()
            .AddLoadDataCommand<TestDbContext>(o =>
            {
                o.LoadData = _ => { called = true; return Task.CompletedTask; };
                o.Invocations = ["seed"];
            })
            .Build();

        await app.Execute("loaddata", "dev");

        called.ShouldBeFalse();
    }

    [Fact]
    public async Task WithInvocations_OnMigrateCommand_CommandExecutableByNewInvocation()
    {
        var (_, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var app = ConsoleApplication.CreateBuilder()
                .AddDatabaseContextFactory<TestDbContext>(b =>
                    b.Add("dev", o => o.UseSqlite(connection)))
                .AddMigrateCommand<TestDbContext>(o => o.Invocations = ["db-up"])
                .Build();

            await app.Execute("db-up", "dev");
        }
    }

    [Fact]
    public async Task WithInvocations_OnClearDataCommand_CommandExecutableByNewInvocation()
    {
        var called = false;
        var (_, connection) = DbContextFixture.BuildSqliteFactory("dev");
        using (connection)
        {
            var app = ConsoleApplication.CreateBuilder()
                .AddDatabaseContextFactory<TestDbContext>(b =>
                    b.Add("dev", o => o.UseSqlite(connection)))
                .AddClearDataCommand<TestDbContext>(o =>
                {
                    o.ClearData = _ => { called = true; return Task.CompletedTask; };
                    o.Invocations = ["purge"];
                })
                .Build();

            await app.Execute("purge", "dev");
        }

        called.ShouldBeTrue();
    }

    // ── WithEnvironmentPolicy ────────────────────────────────────────────────

    [Fact]
    public async Task WithEnvironmentPolicy_Benign_DoesNotBlockProtectedEnvironment()
    {
        var called = false;
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
                b.AddProtected("prod", o => o.UseInMemoryDatabase(Guid.NewGuid().ToString())))
            .AddLoadDataCommand<TestDbContext>(o =>
            {
                o.LoadData = _ => { called = true; return Task.CompletedTask; };
                o.EnvironmentPolicy = EnvironmentPolicy.Benign;
            })
            .Build();

        await app.Execute("loaddata", "prod");

        called.ShouldBeTrue();
    }

    [Fact]
    public async Task WithEnvironmentPolicy_Forbidden_BlocksProtectedEnvironment()
    {
        var called = false;
        var app = ConsoleApplication.CreateBuilder()
            .AddDatabaseContextFactory<TestDbContext>(b =>
                b.AddProtected("prod", o => o.UseInMemoryDatabase(Guid.NewGuid().ToString())))
            .AddLoadDataCommand<TestDbContext>(o =>
            {
                o.LoadData = _ => { called = true; return Task.CompletedTask; };
                o.EnvironmentPolicy = EnvironmentPolicy.Forbidden;
            })
            .Build();

        await app.Execute("loaddata", "prod");

        called.ShouldBeFalse();
    }

}
