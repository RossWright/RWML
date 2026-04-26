using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RossWright.MetalCommand.Data;

/// <summary>
/// Context passed to the <see cref="LoadDataCommandOptions{DBCTX}.LoadData"/> callback.
/// Extends <see cref="DataCommandContext{DBCTX}"/> with CSV-loading helpers.
/// </summary>
/// <typeparam name="DBCTX">The <see cref="DbContext"/> type for this load operation.</typeparam>
public class LoadDataCommandContext<DBCTX>(
    string? _loadFilepath)
    : DataCommandContext<DBCTX> 
    where DBCTX : DbContext
{
    /// <summary>
    /// Reads entities from a CSV file and adds them to the <see cref="DataCommandContext{DBCTX}.DbContext"/>.
    /// Returns the loaded rows. When the file does not exist, returns an empty array without error.
    /// </summary>
    /// <typeparam name="TEntity">The entity type. Columns are matched by name (case-insensitive).</typeparam>
    /// <param name="fileName">Path to the CSV file, optionally relative to <see cref="LoadDataCommandOptions{DBCTX}.LoadFilepath"/>.</param>
    /// <param name="resolve">Optional delegate called for each row before it is added (e.g. to set navigation properties).</param>
    /// <returns>The array of loaded entities.</returns>
    public TEntity[] LoadFromCsv<TEntity>(string fileName, Action<TEntity>? resolve = null)
        where TEntity : class, new()
    {
        TEntity[] data = [];

        if (_loadFilepath != null)
        {
            fileName = Path.Combine(_loadFilepath, fileName);
        }

        if (File.Exists(fileName))
        {
            using (TextReader fileReader = File.OpenText(fileName))
            {
                var csv = new CsvReader(fileReader, csvConfig);
                data = csv.GetRecords<TEntity>().ToArray();
            }

            if (resolve != null)
            {
                foreach (var datum in data)
                {
                    resolve(datum);
                }
            }

            DbContext.Set<TEntity>().AddRange(data);
        }
        return data;
    }
    private static CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
    {
        IgnoreReferences = true,
        IgnoreBlankLines = true,
        AllowComments = true,
        HeaderValidated = null,
        MissingFieldFound = null
    };
}