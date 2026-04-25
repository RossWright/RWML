using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace RossWright.MetalCommand.Data;

public class LoadDataCommandContext<DBCTX>(
    string? _loadFilepath)
    : DataCommandContext<DBCTX> 
    where DBCTX : DbContext
{
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