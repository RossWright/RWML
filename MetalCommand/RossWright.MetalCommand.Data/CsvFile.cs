using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RossWright.MetalCommand.Data;

/// <summary>
/// Reads a CSV file into a typed collection using CsvHelper. The standard way to load
/// seed data inside an <see cref="LoadDataCommandOptions{DBCTX}.LoadData"/> callback.
/// Extra columns, blank lines, and missing fields are silently ignored.
/// </summary>
/// <typeparam name="T">The row type. Properties are matched to CSV headers by name.</typeparam>
public class CsvFile<T>
{
    /// <summary>
    /// Reads the CSV file at <paramref name="fileName"/>.
    /// When <paramref name="fileName"/> is <see langword="null"/>, defaults to
    /// <c>data\{TypeName}.csv</c> relative to <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    /// <param name="fileName">Optional explicit file path.</param>
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

    /// <summary>The rows read from the CSV file. Empty when the file does not exist.</summary>
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
