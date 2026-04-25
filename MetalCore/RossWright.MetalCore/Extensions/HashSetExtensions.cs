namespace RossWright;

/// <summary>
/// Extension methods for <see cref="HashSet{T}"/>.
/// </summary>
public static class HashSetExtensions
{
    /// <summary>
    /// Adds multiple items to a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type of the hash set.</typeparam>
    /// <param name="set">The target hash set.</param>
    /// <param name="items">The items to add.</param>
    /// <returns><see langword="true"/> if at least one item was new and added; otherwise <see langword="false"/>.</returns>
    public static bool AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
    {
        bool added = false;
        foreach(var item in items)
        {
            added |= set.Add(item);
        }
        return added;
    }

    /// <summary>
    /// Removes multiple items from a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type of the hash set.</typeparam>
    /// <param name="set">The target hash set.</param>
    /// <param name="items">The items to remove.</param>
    /// <returns><see langword="true"/> if at least one item was present and removed; otherwise <see langword="false"/>.</returns>
    public static bool RemoveRange<T>(this HashSet<T> set, IEnumerable<T> items)
    {
        bool removed = false;
        foreach (var item in items)
        {
            removed |= set.Remove(item);
        }
        return removed;
    }
}
