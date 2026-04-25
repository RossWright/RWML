using System.Linq.Expressions;

namespace RossWright;

/// <summary>
/// Extension methods for <see cref="IQueryable{T}"/> that add conditional filtering, nullable skip/take, and direction-aware ordering.
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Skips a specified number of elements, but only when <paramref name="count"/> is non-<see langword="null"/> and positive.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="count">The number of elements to skip, or <see langword="null"/> to skip none.</param>
    /// <returns>The original query unchanged when <paramref name="count"/> is <see langword="null"/> or zero; otherwise the skipped query.</returns>
    public static IQueryable<TSource> Skip<TSource>(this IQueryable<TSource> source, int? count) =>
        count.HasValue && count > 0 ? Queryable.Skip(source, count.Value) : source;

    /// <summary>
    /// Takes a specified number of elements, but only when <paramref name="count"/> is non-<see langword="null"/> and positive.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="count">The maximum number of elements to return, or <see langword="null"/> to take all.</param>
    /// <returns>The original query unchanged when <paramref name="count"/> is <see langword="null"/> or zero; otherwise the limited query.</returns>
    public static IQueryable<TSource> Take<TSource>(this IQueryable<TSource> source, int? count) =>
        count.HasValue && count > 0 ? Queryable.Take(source, count.Value) : source;

    /// <summary>
    /// Applies a filter predicate only when <paramref name="flag"/> is <see langword="true"/>, with an optional alternative predicate for the <see langword="false"/> branch.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="flag">When <see langword="true"/>, <paramref name="predicate"/> is applied; when <see langword="false"/>, <paramref name="elsePredicate"/> is applied if provided.</param>
    /// <param name="predicate">The expression applied when <paramref name="flag"/> is <see langword="true"/>.</param>
    /// <param name="elsePredicate">An optional expression applied when <paramref name="flag"/> is <see langword="false"/>.</param>
    /// <returns>The filtered query, or the original query if no predicate applies.</returns>
    public static IQueryable<TSource> WhereIf<TSource>(
        this IQueryable<TSource> source,
        bool flag, Expression<Func<TSource, bool>> predicate,
            Expression<Func<TSource, bool>>? elsePredicate = null) =>
            flag
                ? source.Where(predicate)
                : (elsePredicate is not null
                    ? source.Where(elsePredicate)
                    : source);

    /// <summary>
    /// Applies a filter predicate only when <paramref name="fieldType"/> is non-<see langword="null"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="fieldType">A reference value; the filter is applied only when this is not <see langword="null"/>.</param>
    /// <param name="predicate">The expression to apply when <paramref name="fieldType"/> is non-<see langword="null"/>.</param>
    /// <returns>The filtered query, or the original query when <paramref name="fieldType"/> is <see langword="null"/>.</returns>
    public static IQueryable<TSource> WhereIfNotNull<TSource>(
            this IQueryable<TSource> source,
            object? fieldType, Expression<Func<TSource, bool>> predicate) =>
            fieldType != null ? source.Where(predicate) : source;

    /// <summary>
    /// Applies a filter predicate only when <paramref name="fieldType"/> is non-<see langword="null"/> and contains at least one element.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="fieldType">A collection; the filter is applied only when this is non-null and non-empty.</param>
    /// <param name="predicate">The expression to apply when the collection is non-empty.</param>
    /// <returns>The filtered query, or the original query when the collection is null or empty.</returns>
    public static IQueryable<TSource> WhereIfNotNullOrEmpty<TSource>(
            this IQueryable<TSource> source,
            System.Collections.ICollection fieldType, Expression<Func<TSource, bool>> predicate) =>
            fieldType?.Count > 0 ? source.Where(predicate) : source;

    /// <summary>
    /// Orders a query ascending or descending based on a direction flag.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <typeparam name="TKey">The ordering key type.</typeparam>
    /// <param name="source">The queryable source.</param>
    /// <param name="keySelector">The expression that identifies the sort key.</param>
    /// <param name="isAscending"><see langword="true"/> for ascending order; <see langword="false"/> for descending.</param>
    /// <returns>An <see cref="IOrderedQueryable{T}"/> sorted by <paramref name="keySelector"/> in the specified direction.</returns>
    public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(
        this IQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool isAscending) =>
        isAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);

    /// <summary>
    /// Applies a secondary sort ascending or descending based on a direction flag.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <typeparam name="TKey">The ordering key type.</typeparam>
    /// <param name="source">An already-ordered queryable.</param>
    /// <param name="keySelector">The expression that identifies the secondary sort key.</param>
    /// <param name="isAscending"><see langword="true"/> for ascending order; <see langword="false"/> for descending.</param>
    /// <returns>An <see cref="IOrderedQueryable{T}"/> with the secondary sort applied.</returns>
    public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(
        this IOrderedQueryable<TSource> source, Expression<Func<TSource, TKey>> keySelector, bool isAscending) =>
        isAscending ? source.ThenBy(keySelector) : source.ThenByDescending(keySelector);
}

