using RossWright.MetalCommand.Data.Commands;
using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class LoadDataCommandTests
{
    [Fact]
    public async Task ExecuteAsync_ValidEnvironment_CallsLoadDataAction()
    {
        var called = false;
        var console = new TestConsole();
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase(Guid.NewGuid().ToString()) });
        var command = new LoadDataCommand<TestDbContext>(
            factory,
            new LoadDataCommandOptions<TestDbContext> { LoadData = _ => { called = true; return Task.CompletedTask; } })
        { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeTrue();
        called.ShouldBeTrue();
        console.ErrorLines.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_LoadDelegateCalledWithCorrectEnvironment()
    {
        LoadDataCommandContext<TestDbContext>? capturedCtx = null;
        var console = new TestConsole();
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase(Guid.NewGuid().ToString()) });
        var command = new LoadDataCommand<TestDbContext>(
            factory,
            new LoadDataCommandOptions<TestDbContext>
            {
                LoadData = ctx => { capturedCtx = ctx; return Task.CompletedTask; }
            })
        { Environment = "dev" };

        await command.ExecuteAsync(console, CancellationToken.None);

        capturedCtx.ShouldNotBeNull();
        capturedCtx.Environment.ShouldBe("dev");
        capturedCtx.Console.ShouldNotBeNull();
        capturedCtx.DbContext.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_SaveChanges_CalledAfterLoadDelegate()
    {
        var console = new TestConsole();
        var dbName = Guid.NewGuid().ToString();
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase(dbName) });
        var command = new LoadDataCommand<TestDbContext>(
            factory,
            new LoadDataCommandOptions<TestDbContext>
            {
                LoadData = ctx =>
                {
                    ctx.DbContext.Items.Add(new TestItem { Id = "load-test", Name = "loaded" });
                    return Task.CompletedTask;
                }
            })
        { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);
        result.Success.ShouldBeTrue();

        // Verify the item was persisted by opening a fresh context against the same in-memory db
        var verifyOptions = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(dbName).Options;
        using var verifyCtx = new TestDbContext(verifyOptions);
        verifyCtx.Items.Any(i => i.Id == "load-test").ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_LoadFilepath_UsedByLoadFromCsv()
    {
        // Arrange: write a temp CSV and pass its directory as LoadFilepath
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var csvFile = Path.Combine(tempDir, "TestItem.csv");
        await File.WriteAllTextAsync(csvFile, "Id,Name\r\npath-test,from-csv\r\n");

        TestItem[]? loaded = null;
        var console = new TestConsole();
        var factory = DbContextFixture.BuildFactory("dev",
            new DatabaseEnvironment { Environment = "dev", IsProtected = false, SetOptions = b => b.UseInMemoryDatabase(Guid.NewGuid().ToString()) });
        var command = new LoadDataCommand<TestDbContext>(
            factory,
            new LoadDataCommandOptions<TestDbContext>
            {
                LoadFilepath = tempDir,
                LoadData = ctx =>
                {
                    loaded = ctx.LoadFromCsv<TestItem>("TestItem.csv");
                    return Task.CompletedTask;
                }
            })
        { Environment = "dev" };

        var result = await command.ExecuteAsync(console, CancellationToken.None);

        result.Success.ShouldBeTrue();
        loaded.ShouldNotBeNull();
        loaded!.ShouldHaveSingleItem();
        loaded[0].Id.ShouldBe("path-test");
    }

    [Fact]
    public void Descriptor_HasExpectedInvocations()
    {
        var attr = typeof(LoadDataCommand<TestDbContext>).GetCustomAttribute<CommandAttribute>();

        attr.ShouldNotBeNull();
        attr.Invocations.ShouldContain("LoadData");
        attr.Invocations.ShouldContain("ld");
    }
}
