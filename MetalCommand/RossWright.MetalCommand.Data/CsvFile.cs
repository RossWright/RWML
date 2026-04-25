using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RossWright.MetalCommand.Data;

public class CsvFile<T>
{
    public CsvFile(string? fileName = null)
    {
        fileName ??= $"data\\{typeof(T).Name}.csv";
        if (!File.Exists(fileName)) Rows = Enumerable.Empty<T>();
        using (TextReader fileReader = File.OpenText(fileName))
        {
            var csv = new CsvReader(fileReader, csvConfig);
            Rows = csv.GetRecords<T>().ToArray();
        }
    }

    public IEnumerable<T> Rows { get; }

    private static CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
    {
        IgnoreReferences = true,
        IgnoreBlankLines = true,
        AllowComments = true,
        HeaderValidated = null,
        MissingFieldFound = null
    };
}
