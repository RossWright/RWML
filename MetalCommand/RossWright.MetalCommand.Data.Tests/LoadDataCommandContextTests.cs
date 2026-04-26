using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class LoadDataCommandContextTests
{
    private const string CsvHeader = "Id,Name";

    private static string WriteTempCsv(params string[] dataRows)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        File.WriteAllLines(path, [CsvHeader, .. dataRows]);
        return path;
    }

    private static LoadDataCommandContext<TestDbContext> BuildContext(TestDbContext db, string? loadFilepath = null)
        => new LoadDataCommandContext<TestDbContext>(loadFilepath)
        {
            DbContext = db,
            Console = new TestConsole(),
            Environment = "dev"
        };

    [Fact]
    public async Task FileFound_RowsLoadedAndAddedToDbSet()
    {
        var path = WriteTempCsv("1,Alpha", "2,Beta");
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        var result = ctx.LoadFromCsv<TestItem>(path);
        await db.SaveChangesAsync();

        result.Length.ShouldBe(2);
        db.Items.Count().ShouldBe(2);
    }

    [Fact]
    public void FileNotFound_ReturnsEmptyArray_NoException()
    {
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        var result = ctx.LoadFromCsv<TestItem>("nonexistent_file_that_does_not_exist.csv");

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task LoadFilepath_PrefixedToFileName()
    {
        var dir = Path.GetTempPath();
        var fileName = $"{Guid.NewGuid()}.csv";
        var fullPath = Path.Combine(dir, fileName);
        File.WriteAllLines(fullPath, [CsvHeader, "3,Gamma"]);

        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db, loadFilepath: dir);

        var result = ctx.LoadFromCsv<TestItem>(fileName);
        await db.SaveChangesAsync();

        result.Length.ShouldBe(1);
        result[0].Name.ShouldBe("Gamma");
    }

    [Fact]
    public void ResolveCallback_InvokedOncePerRow()
    {
        var path = WriteTempCsv("1,Alpha", "2,Beta", "3,Gamma");
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        var resolvedIds = new List<string>();
        ctx.LoadFromCsv<TestItem>(path, item => resolvedIds.Add(item.Id));

        resolvedIds.ShouldBe(["1", "2", "3"], ignoreOrder: true);
    }

    [Fact]
    public void EmptyCsv_ReturnsEmptyArray_NothingAddedToDbSet()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        File.WriteAllLines(path, [CsvHeader]);
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        var result = ctx.LoadFromCsv<TestItem>(path);

        result.ShouldBeEmpty();
        db.Items.Count().ShouldBe(0);
    }

    [Fact]
    public void LoadFilepath_FileNotFound_ReturnsEmptyArray()
    {
        var dir = Path.GetTempPath();
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db, loadFilepath: dir);

        var result = ctx.LoadFromCsv<TestItem>("nonexistent_file.csv");

        result.ShouldBeEmpty();
        db.Items.Count().ShouldBe(0);
    }

    [Fact]
    public async Task ResolveCallback_ModifiesEntity_ModificationsPersisted()
    {
        var path = WriteTempCsv("1,Alpha", "2,Beta");
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        var result = ctx.LoadFromCsv<TestItem>(path, item => item.Name = item.Name.ToUpper());
        await db.SaveChangesAsync();

        result[0].Name.ShouldBe("ALPHA");
        result[1].Name.ShouldBe("BETA");
        db.Items.First(x => x.Id == "1").Name.ShouldBe("ALPHA");
    }

    [Fact]
    public async Task MultipleLoads_AccumulatesData()
    {
        var path1 = WriteTempCsv("1,Alpha");
        var path2 = WriteTempCsv("2,Beta");
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        ctx.LoadFromCsv<TestItem>(path1);
        ctx.LoadFromCsv<TestItem>(path2);
        await db.SaveChangesAsync();

        db.Items.Count().ShouldBe(2);
    }

    [Fact]
    public void SingleRow_LoadsCorrectly()
    {
        var path = WriteTempCsv("1,Alpha");
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);

        var result = ctx.LoadFromCsv<TestItem>(path);

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe("1");
        result[0].Name.ShouldBe("Alpha");
    }

    [Fact]
    public void ResolveCallback_NotCalled_WhenFileNotFound()
    {
        var db = DbContextFixture.BuildInMemory();
        var ctx = BuildContext(db);
        var callbackInvoked = false;

        ctx.LoadFromCsv<TestItem>("nonexistent.csv", item => callbackInvoked = true);

        callbackInvoked.ShouldBeFalse();
    }

    [Fact]
    public async Task LoadFromCsv_AddsToExistingDbSetData()
    {
        var existingItem = new TestItem { Id = "0", Name = "Existing" };
        var db = DbContextFixture.BuildInMemory();
        db.Items.Add(existingItem);
        await db.SaveChangesAsync();

        var path = WriteTempCsv("1,Alpha");
        var ctx = BuildContext(db);
        ctx.LoadFromCsv<TestItem>(path);
        await db.SaveChangesAsync();

        db.Items.Count().ShouldBe(2);
        db.Items.Any(x => x.Id == "0").ShouldBeTrue();
        db.Items.Any(x => x.Id == "1").ShouldBeTrue();
    }
}
