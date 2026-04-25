namespace RossWright;

/// <summary>
/// Extension methods for <see cref="IEnumerable{T}"/> providing conditional filtering, tree traversal, projection helpers, and ordering utilities.
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Applies a filter only when <paramref name="flag"/> is <see langword="true"/>, with an optional alternative filter for the <see langword="false"/> branch.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="flag">When <see langword="true"/>, <paramref name="ifTrue"/> is applied; when <see langword="false"/>, <paramref name="ifFalse"/> is applied if provided.</param>
    /// <param name="ifTrue">The predicate applied when <paramref name="flag"/> is <see langword="true"/>.</param>
    /// <param name="ifFalse">An optional predicate applied when <paramref name="flag"/> is <see langword="false"/>.</param>
    /// <returns>The filtered sequence, or the original sequence if no predicate applies.</returns>
    public static IEnumerable<TSource> WhereIf<TSource>(
        this IEnumerable<TSource> source, bool flag, 
        Func<TSource, bool> ifTrue,
        Func<TSource, bool>? ifFalse = null) =>
        flag ? source.Where(ifTrue) 
            : (ifFalse is not null
                ? source.Where(ifFalse)
                : source);

    /// <summary>
    /// Concatenates two sequences, treating either as empty when <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="first">The first sequence, or <see langword="null"/>.</param>
    /// <param name="second">The second sequence, or <see langword="null"/>.</param>
    /// <returns>The concatenation of both sequences, substituting an empty sequence for any <see langword="null"/> argument.</returns>
    public static IEnumerable<TSource>? ConcatAllowNull<TSource>(this IEnumerable<TSource>? first, IEnumerable<TSource>? second)
    {
        if (first == null) return second;
        if (second == null) return first;
        return first!.Concat(second!);
    }

    /// <summary>
    /// Projects each element paired with its zero-based index.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <returns>A sequence of <c>(item, index)</c> tuples.</returns>
    public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> source) =>
        source?.Select((item, index) => (item, index)) ?? new (T, int)[0];

    /// <summary>
    /// Executes an <paramref name="action"/> for each element in the sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="coll">The source sequence.</param>
    /// <param name="action">The action to invoke for each element.</param>
    public static void ForEach<T>(this IEnumerable<T> coll, Action<T> action)
    {
        foreach (var item in coll) action(item);
    }
    /// <summary>
    /// Executes an <paramref name="action"/> for each element and yields each element through unchanged.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="coll">The source sequence.</param>
    /// <param name="action">The action to invoke for each element before yielding it.</param>
    /// <returns>The original sequence with the side-effect action applied to each element.</returns>
    public static IEnumerable<T> WithEach<T>(this IEnumerable<T> coll, Action<T> action)
    {
        foreach (var item in coll)
        {
            action(item);
            yield return item;
        }
    }
    /// <summary>
    /// Executes an asynchronous <paramref name="action"/> for each element in the sequence, awaiting each call sequentially.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="coll">The source sequence.</param>
    /// <param name="action">The async delegate to invoke for each element.</param>
    /// <returns>A <see cref="Task"/> that completes after all elements have been processed.</returns>
    public static async Task ForEachAsync<T>(this IEnumerable<T> coll, Func<T, Task> action)
    {
        foreach (var item in coll)
            await action(item);
    }

    /// <summary>
    /// Recursively flattens a tree-shaped hierarchy into a single sequence by depth-first traversal.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The root-level items to traverse.</param>
    /// <param name="selectSubItems">A function that returns the children of a node, or <see langword="null"/> if the node is a leaf.</param>
    /// <param name="select">An optional projection applied to each visited node before yielding it.</param>
    /// <returns>All nodes in the tree, depth-first, with the optional projection applied.</returns>
    public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> items,
        Func<T, IEnumerable<T>?> selectSubItems, Func<T, T>? select = null)
    {
        foreach (var item in items)
        {
            yield return select is null ? item : select(item);
            var subItems = selectSubItems(item);
            if (subItems is not null && subItems.Any())
                foreach (var subItem in subItems.SelectDeep(selectSubItems, select))
                {
                    yield return subItem;
                }
        }
    }

    /// <summary>
    /// Filters out the specified values from the sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The source sequence.</param>
    /// <param name="exclusions">The values to remove. Null values are matched by reference equality.</param>
    /// <returns>A sequence containing all elements not present in <paramref name="exclusions"/>.</returns>
    public static IEnumerable<T> Without<T>(this IEnumerable<T> items, params T[] exclusions) =>
        items.Where(item => !exclusions.Any(exclusion =>
        (exclusion == null && item == null) || exclusion?.Equals(item) == true));

    /// <summary>
    /// Sorts a sequence ascending or descending based on a direction flag.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <typeparam name="TKey">The ordering key type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function that extracts the sort key from an element.</param>
    /// <param name="isAscending"><see langword="true"/> for ascending order; <see langword="false"/> for descending.</param>
    /// <returns>An <see cref="IOrderedEnumerable{T}"/> sorted in the specified direction.</returns>
    public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool isAscending) =>
        isAscending ? source.OrderBy(keySelector) : source.OrderByDescending(keySelector);
    /// <summary>
    /// Applies a secondary sort ascending or descending based on a direction flag.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <typeparam name="TKey">The ordering key type.</typeparam>
    /// <param name="source">An already-ordered sequence.</param>
    /// <param name="keySelector">A function that extracts the secondary sort key.</param>
    /// <param name="isAscending"><see langword="true"/> for ascending order; <see langword="false"/> for descending.</param>
    /// <returns>An <see cref="IOrderedEnumerable{T}"/> with the secondary sort applied.</returns>
    public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(this IOrderedEnumerable<TSource> source, Func<TSource, TKey> keySelector, bool isAscending) =>
        isAscending ? source.ThenBy(keySelector) : source.ThenByDescending(keySelector);

    /// <summary>
    /// Returns the zero-based index of the first element that satisfies a condition, or <c>-1</c> if no element matches.
    /// </summary>
    /// <typeparam name="TSource">The element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="predicate">A function to test each element.</param>
    /// <returns>The index of the first matching element, or <c>-1</c> if no match is found.</returns>
    public static int FirstIndexWhere<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
    {
        var index = 0;
        foreach (var item in source)
        {
            if (predicate.Invoke(item))
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    /// <summary>Compares two sequences for equality regardless of element order.</summary>
    /// <remarks>From https://stackoverflow.com/questions/3669970/compare-two-listt-objects-for-equality-ignoring-order</remarks>
    /// <typeparam name="T">The element type. Must be non-nullable.</typeparam>
    /// <param name="list1">The first sequence.</param>
    /// <param name="list2">The second sequence.</param>
    /// <returns><see langword="true"/> if both sequences contain the same elements with the same multiplicities; otherwise <see langword="false"/>.</returns>
    public static bool ScrambledEquals<T>(this IEnumerable<T> list1, IEnumerable<T> list2)
        where T : notnull
    {
        var cnt = new Dictionary<T, int>();
        foreach (T s in list1)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]++;
            }
            else
            {
                cnt.Add(s, 1);
            }
        }
        foreach (T s in list2)
        {
            if (cnt.ContainsKey(s))
            {
                cnt[s]--;
            }
            else
            {
                return false;
            }
        }
        return cnt.Values.All(c => c == 0);
    }

    /// <summary>
    /// Filters out <see langword="null"/> values from a nullable sequence and returns a non-nullable sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="nulls">A sequence that may contain <see langword="null"/> elements.</param>
    /// <returns>A sequence containing only the non-null elements.</returns>
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> nulls) => nulls.Where(_ => _ != null).Select(_ => _!);

    /// <summary>
    /// Computes an aggregate hash code for the entire sequence by combining each element's hash.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="list">The source sequence.</param>
    /// <returns>An aggregate hash code, or <c>0</c> if <paramref name="list"/> is <see langword="null"/>.</returns>
    public static int GetAggregateHashCode<T>(this IEnumerable<T> list)
    {
        if (list == null) return 0;
        const int seedValue = 0x2D2816FE;
        const int primeNumber = 397;
        return list.Aggregate(seedValue, 
            (current, item) => (current * primeNumber) + 
                               (Equals(item, default(T)) ? 0 : item!.GetHashCode()));
    }

    /// <summary>
    /// Returns <see langword="true"/> if all elements produce the same value when projected by <paramref name="predicate"/>.
    /// An empty sequence is considered all-same.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="items">The source sequence.</param>
    /// <param name="predicate">A function that extracts the comparison value from each element.</param>
    /// <returns><see langword="true"/> if all projected values are equal; otherwise <see langword="false"/>.</returns>
    public static bool AllSame<T>(this IEnumerable<T> items, Func<T,object?> predicate)
    {
        using var enumerator = items.GetEnumerator();
        if (!enumerator.MoveNext()) return true; // Empty collection is considered all same
        var first = predicate(enumerator.Current);
        while (enumerator.MoveNext())
        {
            if (!Equals(first, predicate(enumerator.Current))) return false;
        }
        return true;
    }

    /// <summary>
    /// Projects each element using <paramref name="predicate"/> and returns the result as an array.
    /// </summary>
    /// <typeparam name="TIn">The source element type.</typeparam>
    /// <typeparam name="TOut">The projected element type.</typeparam>
    /// <param name="items">The source sequence.</param>
    /// <param name="predicate">The projection function.</param>
    /// <returns>An array of projected elements.</returns>
    public static TOut[] ToArray<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> predicate) =>
        items.Select(predicate).ToArray();

    /// <summary>
    /// Projects each element using <paramref name="predicate"/> and returns the result as a list.
    /// </summary>
    /// <typeparam name="TIn">The source element type.</typeparam>
    /// <typeparam name="TOut">The projected element type.</typeparam>
    /// <param name="items">The source sequence.</param>
    /// <param name="predicate">The projection function.</param>
    /// <returns>A list of projected elements.</returns>
    public static List<TOut> ToList<TIn, TOut>(this IEnumerable<TIn> items, Func<TIn, TOut> predicate) =>
        items.Select(predicate).ToList();
}