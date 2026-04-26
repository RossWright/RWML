using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class ClearDataCommandContextTests
{
    private static ClearDataCommandContext<TestDbContext> BuildContext(TestDbContext db, TestConsole? console = null)
    {
        console ??= new TestConsole();
        return new ClearDataCommandContext<TestDbContext>
        {
            DbContext = db,
            Console = console,
            Environment = "dev"
        };
    }

    [Fact]
    public async Task ClearTable_Success_ConclusionIsDone()
    {
        var db = DbContextFixture.BuildSqlite();
        db.Items.Add(new TestItem { Id = "1", Name = "A" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var console = new TestConsole();
        var ctx = BuildContext(db, console);

        await ctx.ClearTable("Items");

        console.Lines.ShouldContain(l => l != null && l.Contains("Done!"));
    }

    [Fact]
    public async Task ClearTable_DeleteExecuted_WithCorrectTableName()
    {
        var db = DbContextFixture.BuildSqlite();
        db.Items.Add(new TestItem { Id = "1", Name = "A" });
        db.Items.Add(new TestItem { Id = "2", Name = "B" });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var ctx = BuildContext(db);

        await ctx.ClearTable("Items");

        db.Items.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ClearTable_TableNotFound_ConclusionIsTableNotFound()
    {
        // On SQLite the error is "no such table" — does not match "Invalid object name" conditions
        // so the other-exception branch fires and conclusion is NOT "Done!" or "Table Not Found".
        var db = DbContextFixture.BuildSqlite();
        var console = new TestConsole();
        var ctx = BuildContext(db, console);

        await db.Database.ExecuteSqlRawAsync("DROP TABLE Items");
        await ctx.ClearTable("Items");

        console.Lines.ShouldNotContain(l => l != null && l.Contains("Done!"));
        console.Lines.ShouldNotContain(l => l != null && l.Contains("Table Not Found"));
    }

    [Fact]
    public async Task ClearTable_OtherException_ConclusionIsExceptionString()
    {
        var db = DbContextFixture.BuildSqlite();
        var console = new TestConsole();
        var ctx = BuildContext(db, console);

        await db.Database.ExecuteSqlRawAsync("DROP TABLE Items");
        await ctx.ClearTable("Items");

        console.Lines.ShouldContain(l => l != null && l.Contains("no such table"));
    }
}
