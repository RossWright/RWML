namespace RossWright.MetalCommand.Data;

/// <summary>
/// Context passed to the <see cref="ClearDataCommandOptions{DBCTX}.ClearData"/> callback.
/// Extends <see cref="DataCommandContext{DBCTX}"/> with a table-truncation helper.
/// </summary>
/// <typeparam name="DBCTX">The <see cref="DbContext"/> type for this clear operation.</typeparam>
public class ClearDataCommandContext<DBCTX> 
    : DataCommandContext<DBCTX> where DBCTX : DbContext
{
    /// <summary>
    /// Executes <c>DELETE FROM <paramref name="tableName"/></c> and writes a progress line.
    /// Handles missing tables gracefully by reporting "Table Not Found" instead of throwing.
    /// </summary>
    /// <param name="tableName">The table to delete all rows from.</param>
    public Task ClearTable(string tableName)
    {
        string conclusion = "Done!";
        return Console.AnnounceAsync($"Clearing {tableName}", async () =>
        {
            try
            {
                string sql = $"DELETE FROM {tableName}";
                await DbContext.Database.ExecuteSqlRawAsync(sql);
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("Invalid object name") || ex.Message.Contains("Cannot find the object"))
                    conclusion = "Table Not Found";
                else
                    conclusion = ex.ToBetterString();
            }
        }, () => conclusion);
    }
}
