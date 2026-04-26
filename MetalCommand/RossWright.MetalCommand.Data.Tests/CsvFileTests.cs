using RossWright.MetalCommand.Data.Tests.Infrastructure;

namespace RossWright.MetalCommand.Data.Tests;

public class CsvFileTests
{
    private static string WriteTempCsv<T>(params string[] dataRows) where T : class
    {
        var dir = Path.GetTempPath();
        var path = Path.Combine(dir, $"{Guid.NewGuid()}.csv");
        var header = string.Join(",", typeof(T).GetProperties().Select(p => p.Name));
        File.WriteAllLines(path, [header, .. dataRows]);
        return path;
    }

    [Fact]
    public void FileNotFound_ThrowsFileNotFoundException()
    {
        // CsvFile<T> has a bug: when the file does not exist it sets Rows = Empty
        // but then unconditionally calls File.OpenText which throws.
        // This test documents the actual (buggy) behavior.
        var act = () => new CsvFile<TestItem>("nonexistent_file_that_does_not_exist.csv");

        Should.Throw<FileNotFoundException>(act);
    }

    [Fact]
    public void FileFound_RowsDeserializedCorrectly()
    {
        var path = WriteTempCsv<TestItem>("1,Alpha", "2,Beta");

        var csv = new CsvFile<TestItem>(path);

        var rows = csv.Rows.ToList();
        rows.Count.ShouldBe(2);
        rows[0].Id.ShouldBe("1");
        rows[0].Name.ShouldBe("Alpha");
        rows[1].Id.ShouldBe("2");
        rows[1].Name.ShouldBe("Beta");
    }

    [Fact]
    public void BlankLines_AreIgnored()
    {
        var dir = Path.GetTempPath();
        var path = Path.Combine(dir, $"{Guid.NewGuid()}.csv");
        File.WriteAllLines(path, ["Id,Name", "1,Alpha", "", "2,Beta", "", "3,Gamma"]);

        var csv = new CsvFile<TestItem>(path);

        csv.Rows.Count().ShouldBe(3);
    }

    [Fact]
    public void DefaultFileName_FollowsTypeNamePattern()
    {
        // When no fileName is provided, the constructor builds "data\{TypeName}.csv".
        // Since the "data" directory doesn't exist in the test output directory,
        // it throws DirectoryNotFoundException (a subclass of IOException).
        // Either way the path contains the expected type name.
        var ex = Should.Throw<IOException>(() => new CsvFile<TestItem>());

        ex.Message.ShouldContain("TestItem");
    }
}
