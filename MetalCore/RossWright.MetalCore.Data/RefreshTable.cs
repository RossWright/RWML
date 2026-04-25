using Microsoft.EntityFrameworkCore;

namespace RossWright;

/// <summary>
/// Extension methods for synchronizing a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against an incoming data set,
/// inserting new records, updating existing ones, and optionally removing absent ones.
/// </summary>
public static class RefreshTableExtensions
{
    private static bool IsSame(IHasId o, IHasId n) => o.Id.Equals(n.Id);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/>,
    /// matching records by <see cref="IHasId.Id"/> and copying all members via <c>CopyTo</c>.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type; must implement <see cref="IHasId"/>.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type; must implement <see cref="IHasId"/>.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        IEnumerable<INENTITY>? newData,
        bool deleteSourceEntities = false)
        where DBENTITY : class, IHasId, new()
        where INENTITY : class, IHasId =>
        RefreshTable(dbSet, _ => _.ToListAsync(), newData, IsSame, inEntity =>
        {
            var dbEntity = new DBENTITY();
            inEntity.CopyTo(dbEntity);
            return dbEntity;
        }, (o, n) => n.CopyTo(o), deleteSourceEntities);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/>,
    /// matching records by <see cref="IHasId.Id"/> and applying a custom <paramref name="update"/> delegate.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type; must implement <see cref="IHasId"/>.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type; must implement <see cref="IHasId"/>.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="update">An action applied to each matched pair of existing and incoming entities.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        IEnumerable<INENTITY>? newData,
        Action<DBENTITY, INENTITY> update,
        bool deleteSourceEntities = false)
        where DBENTITY : class, IHasId, new()
        where INENTITY : class, IHasId =>
        RefreshTable(dbSet, _ => _.ToListAsync(), newData, IsSame, inEntity =>
        {
            var dbEntity = new DBENTITY();
            update(dbEntity, inEntity);
            return dbEntity;
        }, update, deleteSourceEntities);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/>,
    /// matching records by <see cref="IHasId.Id"/> using a custom <paramref name="fetchOldData"/> query and a custom <paramref name="update"/> delegate.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type; must implement <see cref="IHasId"/>.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type; must implement <see cref="IHasId"/>.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="fetchOldData">A delegate that queries the existing rows from the <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="update">An action applied to each matched pair of existing and incoming entities.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        Func<DbSet<DBENTITY>, Task<List<DBENTITY>>> fetchOldData,
        IEnumerable<INENTITY>? newData,
        Action<DBENTITY, INENTITY> update,
        bool deleteSourceEntities = false)
        where DBENTITY : class, IHasId, new()
        where INENTITY : class, IHasId =>
        RefreshTable(dbSet, fetchOldData, newData, IsSame, inEntity =>
        {
            var dbEntity = new DBENTITY();
            update(dbEntity, inEntity);
            return dbEntity;
        }, update, deleteSourceEntities);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/>,
    /// using a custom <paramref name="isSame"/> predicate to match records and copying all members via <c>CopyTo</c>.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="isSame">A predicate that returns <see langword="true"/> when an existing and an incoming entity represent the same record.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        IEnumerable<INENTITY>? newData,
        Func<DBENTITY, INENTITY, bool> isSame,
        bool deleteSourceEntities = false)
        where DBENTITY : class, new()
        where INENTITY : class =>
        RefreshTable(dbSet, _ => _.ToListAsync(), newData, isSame, inEntity =>
        {
            var dbEntity = new DBENTITY();
            inEntity.CopyTo(dbEntity);
            return dbEntity;
        }, (o, n) => n.CopyTo(o), deleteSourceEntities);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/>,
    /// using a custom <paramref name="isSame"/> predicate and a custom <paramref name="update"/> delegate.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="isSame">A predicate that returns <see langword="true"/> when an existing and an incoming entity represent the same record.</param>
    /// <param name="update">An action applied to each matched pair of existing and incoming entities.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        IEnumerable<INENTITY>? newData,
        Func<DBENTITY, INENTITY, bool> isSame,
        Action<DBENTITY, INENTITY> update,
        bool deleteSourceEntities = false)
        where DBENTITY : class, new()
        where INENTITY : class =>
        RefreshTable(dbSet, _ => _.ToListAsync(), newData, isSame, inEntity =>
        {
            var dbEntity = new DBENTITY();
            update(dbEntity, inEntity);
            return dbEntity;
        }, update, deleteSourceEntities);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/>,
    /// using a custom <paramref name="fetchOldData"/> query, a custom <paramref name="isSame"/> predicate, and a custom <paramref name="update"/> delegate.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="fetchOldData">A delegate that queries the existing rows from the <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="isSame">A predicate that returns <see langword="true"/> when an existing and an incoming entity represent the same record.</param>
    /// <param name="update">An action applied to each matched pair of existing and incoming entities.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        Func<DbSet<DBENTITY>, Task<List<DBENTITY>>> fetchOldData,
        IEnumerable<INENTITY>? newData,
        Func<DBENTITY, INENTITY, bool> isSame,
        Action<DBENTITY, INENTITY> update,
        bool deleteSourceEntities = false)
        where DBENTITY : class, new()
        where INENTITY : class =>
        RefreshTable(dbSet, fetchOldData, newData, isSame, inEntity =>
        {
            var dbEntity = new DBENTITY();
            update(dbEntity, inEntity);
            return dbEntity;
        }, update, deleteSourceEntities);

    /// <summary>
    /// Synchronizes a <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> against <paramref name="newData"/> with full control
    /// over data fetching, record matching, entity creation, and entity update.
    /// All other <c>RefreshTable</c> overloads delegate to this one.
    /// </summary>
    /// <typeparam name="DBENTITY">The EF entity type.</typeparam>
    /// <typeparam name="INENTITY">The incoming data type.</typeparam>
    /// <param name="dbSet">The target <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="fetchOldData">A delegate that queries the existing rows from the <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/>.</param>
    /// <param name="newData">The incoming data to sync; pass <see langword="null"/> to skip all changes.</param>
    /// <param name="isSame">A predicate that returns <see langword="true"/> when an existing and an incoming entity represent the same record.</param>
    /// <param name="add">A factory that creates a new <typeparamref name="DBENTITY"/> from an incoming entity with no matching record.</param>
    /// <param name="update">An action applied to each matched pair of existing and incoming entities.</param>
    /// <param name="deleteSourceEntities">When <see langword="true"/>, records absent from <paramref name="newData"/> are removed from the set.</param>
    /// <returns>An <see cref="IRefreshResult"/> reporting the number of adds, updates, and deletes.</returns>
    public static async Task<IRefreshResult> RefreshTable<DBENTITY, INENTITY>(
        this DbSet<DBENTITY> dbSet,
        Func<DbSet<DBENTITY>, Task<List<DBENTITY>>> fetchOldData,
        IEnumerable<INENTITY>? newData,
        Func<DBENTITY, INENTITY, bool> isSame,
        Func<INENTITY, DBENTITY> add,
        Action<DBENTITY, INENTITY> update,
        bool deleteSourceEntities)
        where DBENTITY : class
        where INENTITY : class
    {
        var result = new RefreshResult();

        if (newData is null) return result;

        var oldData = await fetchOldData(dbSet);
        if (oldData.Count == 0)
        {
            result.Adds = newData.Count();
            dbSet.AddRange(newData.Select(add));
        }
        else
        {
            var deletes = new List<DBENTITY>();
            foreach (var dbEntity in oldData)
            {
                var newRow = newData.FirstOrDefault(inEntity => isSame(dbEntity, inEntity));
                if (newRow == null)
                {
                    deletes.Add(dbEntity);
                }
                else if (update != null)
                {
                    result.Updates++;
                    update(dbEntity, newRow);
                }
            }
            foreach (var inEntity in newData)
            {
                if (!oldData.Any(dbEntity => isSame(dbEntity, inEntity)))
                {
                    result.Adds++;
                    dbSet.Add(add(inEntity));
                }
            }
            if (deleteSourceEntities)
            {
                result.Deletes = deletes.Count;
                dbSet.RemoveRange(deletes);
            }
        }
        return result;
    }

    private class RefreshResult : IRefreshResult
    {
        public int Adds { get; set; }
        public int Updates { get; set; }
        public int Deletes { get; set; }
    }
}

/// <summary>
/// Reports the outcome of a <see cref="RefreshTableExtensions"/> sync operation.
/// </summary>
public interface IRefreshResult
{
    /// <summary>Gets the number of entities that were inserted.</summary>
    int Adds { get; }
    /// <summary>Gets the number of entities that were updated.</summary>
    int Updates { get; }
    /// <summary>Gets the number of entities that were deleted.</summary>
    int Deletes { get; }
}