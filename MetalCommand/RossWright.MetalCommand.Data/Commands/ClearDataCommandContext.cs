namespace RossWright.MetalCommand.Data;

public class ClearDataCommandContext<DBCTX> 
    : DataCommandContext<DBCTX> where DBCTX : DbContext
{
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
