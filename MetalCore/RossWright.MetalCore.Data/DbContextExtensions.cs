using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace RossWright;

/// <summary>
/// Extension methods for <see cref="DbContext"/>
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Calls <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> and, on a foreign key constraint failure,
    /// invokes <paramref name="onError"/> with a <see cref="ForeignKeyErrorReport"/> describing the violated constraint.
    /// The original exception is always re-thrown.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="onError">A callback invoked with details about the FK constraint violation, if one is detected.</param>
    /// <returns>A <see cref="Task"/> that completes after the save attempt.</returns>
    /// <exception cref="Microsoft.EntityFrameworkCore.DbUpdateException">Re-thrown on any EF update failure.</exception>
    public static async Task SaveChangesAsyncWithFkErrors(this DbContext dbContext, Action<ForeignKeyErrorReport> onError)
    {
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            var innerEx = ex.InnerException;
            if (innerEx != null)
            {
                string msg = innerEx.Message;
                string provider = dbContext.Database.ProviderName?.ToLowerInvariant() ?? string.Empty;
                string? constraintName = null;

                if (provider.Contains("sqlserver"))
                {
                    if (msg.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase))
                    {
                        const string fkPrefix = "FOREIGN KEY constraint \"";
                        int startIndex = msg.IndexOf(fkPrefix, StringComparison.OrdinalIgnoreCase);
                        if (startIndex >= 0)
                        {
                            startIndex += fkPrefix.Length;
                            int endIndex = msg.IndexOf("\"", startIndex);
                            if (endIndex > startIndex)
                            {
                                constraintName = msg.Substring(startIndex, endIndex - startIndex);
                            }
                        }
                    }
                }
                else if (provider.Contains("mysql"))
                {
                    if (msg.Contains("a foreign key constraint fails", StringComparison.OrdinalIgnoreCase))
                    {
                        const string fkPrefix = "CONSTRAINT `";
                        int startIndex = msg.IndexOf(fkPrefix, StringComparison.OrdinalIgnoreCase);
                        if (startIndex >= 0)
                        {
                            startIndex += fkPrefix.Length;
                            int endIndex = msg.IndexOf("`", startIndex);
                            if (endIndex > startIndex)
                            {
                                constraintName = msg.Substring(startIndex, endIndex - startIndex);
                            }
                        }
                    }
                }

                if (constraintName != null)
                {
                    foreach (var entry in ex.Entries)
                    {
                        foreach (var fk in entry.Metadata.GetForeignKeys())
                        {
                            string fkConstraintName = fk.GetConstraintName()!;
                            if (string.Equals(fkConstraintName, constraintName, StringComparison.OrdinalIgnoreCase))
                            {
                                onError(new ForeignKeyErrorReport
                                {
                                    EntityName = entry.Metadata.Name,
                                    ConstraintName = constraintName,
                                    Values = fk.Properties
                                        .ToDictionary(p => p.Name, p => entry.CurrentValues[p])
                                });
                            }
                        }
                    }
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> if the relational database has been created.
    /// </summary>
    /// <param name="dbContext">The database context to check.</param>
    /// <returns><see langword="true"/> if the database exists; otherwise <see langword="false"/>.</returns>
    public static bool DatabaseExists(this DbContext dbContext) =>
        ((RelationalDatabaseCreator)dbContext.Database.GetService<IDatabaseCreator>()).Exists();

    /// <summary>
    /// Returns <see langword="true"/> if the change tracker has any pending changes for entity type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The entity type to check for changes.</typeparam>
    /// <param name="dbCtx">The database context.</param>
    /// <returns><see langword="true"/> if at least one tracked entry of type <typeparamref name="T"/> has unsaved changes; otherwise <see langword="false"/>.</returns>
    public static bool CheckForChangesToAny<T>(this DbContext dbCtx) where T : class
    {
        dbCtx.ChangeTracker.DetectChanges();
        return dbCtx.ChangeTracker.Entries().Any(_ => _.Entity is T);
    }

    /// <summary>
    /// Drops all foreign key constraints, tables, and stored procedures from the database.
    /// <para><strong>SQL Server only. Intended for test teardown — do not call in production.</strong></para>
    /// </summary>
    /// <param name="dbCtx">The database context targeting the database to destroy.</param>
    /// <returns>A <see cref="Task"/> that completes when all objects have been removed.</returns>
    public static async Task Obliterate(this DbContext dbCtx) => await dbCtx.Database.ExecuteSqlRawAsync(
        "DECLARE @sql NVARCHAR(2000)" +
        "WHILE(EXISTS(SELECT 1 from INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_TYPE = 'FOREIGN KEY'))\n" +
        "BEGIN\n" +
        "    SELECT TOP 1 @sql = ('ALTER TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + ']')\n" +
        "    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS\n" +
        "    WHERE CONSTRAINT_TYPE = 'FOREIGN KEY'\n" +
        "    EXEC(@sql)\n" +
        "    PRINT @sql\n" +
        "END\n" +
        "WHILE(EXISTS(SELECT * from INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME NOT IN ('database_firewall_rules', 'ipv6_database_firewall_rules')))\n" +
        "BEGIN\n" +
        "    SELECT TOP 1 @sql = ('DROP TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + ']')\n" +
        "    FROM INFORMATION_SCHEMA.TABLES\n" +
        "    WHERE TABLE_NAME NOT IN ('database_firewall_rules', 'ipv6_database_firewall_rules')\n" +
        "    EXEC(@sql)\n" +
        "    PRINT @sql\n" +
        "END\n" +
        "WHILE EXISTS (SELECT * FROM sys.objects WHERE type = 'P')\n" +
        "BEGIN\n" +
        "    SELECT TOP 1 @sql = 'DROP PROCEDURE [' + SCHEMA_NAME(schema_id) + '].[' + name + ']'\n" +
        "    FROM sys.objects\n" +
        "    WHERE type = 'P';\n" +
        "    EXEC(@sql);\n" +
        "    PRINT @sql;" +
        "\nEND");
}

/// <summary>
/// Describes a foreign key constraint violation detected during a save operation.
/// </summary>
public class ForeignKeyErrorReport
{
    /// <summary>Gets or sets the name of the entity type involved in the FK violation.</summary>
    public string EntityName { get; set; } = null!;
    /// <summary>Gets or sets the database constraint name, if it could be parsed from the exception message.</summary>
    public string? ConstraintName { get; set; } = null!;
    /// <summary>Gets or sets the current property values of the violating entity, keyed by property name.</summary>
    public Dictionary<string, object?> Values { get; set; } = new();
}