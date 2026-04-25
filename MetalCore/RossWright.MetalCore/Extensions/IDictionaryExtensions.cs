namespace RossWright;

/// <summary>
/// Extension methods for <see cref="IDictionary{TKey,TValue}"/> and dictionary-of-collections patterns.
/// </summary>
public static class IDictionaryExtensions
{
    /// <summary>
    /// Returns the value for <paramref name="key"/>, or <paramref name="defaultValue"/> if the key is not present.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dict">The source dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="defaultValue">The fallback value returned when <paramref name="key"/> is absent. Defaults to <see langword="default"/>(<typeparamref name="TValue"/>).</param>
    /// <returns>The stored value, or <paramref name="defaultValue"/>.</returns>
    public static TValue? GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue? defaultValue = default(TValue)) =>
        dict.TryGetValue(key, out var value) ? value : defaultValue;
    
    /// <summary>
    /// Returns a new dictionary containing all entries except the one with <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dict">The source dictionary.</param>
    /// <param name="key">The key to exclude.</param>
    /// <returns>A new <see cref="IDictionary{TKey,TValue}"/> without the specified key.</returns>
    public static IDictionary<TKey, TValue> WithoutKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
         where TKey : notnull =>
        dict.Where(_ => !key.Equals(_.Key)).ToDictionary();

    /// <summary>
    /// Returns a new dictionary excluding all entries whose key matches <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dict">The source dictionary.</param>
    /// <param name="predicate">A function that returns <see langword="true"/> for keys to exclude.</param>
    /// <returns>A new <see cref="IDictionary{TKey,TValue}"/> without the matching keys.</returns>
    public static IDictionary<TKey, TValue> WithoutKey<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, bool> predicate)
        where TKey : notnull =>
        dict.Where(_ => !predicate(_.Key)).ToDictionary();

    /// <summary>
    /// Returns a new dictionary excluding all entries for which <paramref name="predicate"/> returns <see langword="true"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dict">The source dictionary.</param>
    /// <param name="predicate">A function that receives each key–value pair and returns <see langword="true"/> for entries to exclude.</param>
    /// <returns>A new <see cref="IDictionary{TKey,TValue}"/> without the matching entries.</returns>
    public static IDictionary<TKey, TValue> Without<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, bool> predicate)
         where TKey : notnull =>
        dict.Where(_ => !predicate(_.Key, _.Value)).ToDictionary();

    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> to a <see cref="Dictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="stream">The source key–value pair sequence.</param>
    /// <returns>A new <see cref="IDictionary{TKey,TValue}"/> containing all pairs.</returns>
    public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> stream)
         where TKey : notnull =>
        stream.ToDictionary(_ => _.Key, _ => _.Value);

    /// <summary>
    /// Creates a shallow copy of an <see cref="IDictionary{TKey,TValue}"/> as a new <see cref="Dictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="stream">The source dictionary to copy.</param>
    /// <returns>A new <see cref="IDictionary{TKey,TValue}"/> containing the same entries.</returns>
    public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IDictionary<TKey, TValue> stream)
        where TKey : notnull =>
        stream.ToDictionary(_ => _.Key, _ => _.Value);
        
    /// <summary>
    /// Copies all entries from this dictionary into <paramref name="target"/>, overwriting any existing values with the same key.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="original">The source dictionary.</param>
    /// <param name="target">The destination dictionary to receive the copied entries.</param>
    public static void CopyTo<TKey, TValue>(this IDictionary<TKey, TValue> original, IDictionary<TKey, TValue> target)
        where TKey : notnull
    {
        foreach (var kvp in original)
            target[kvp.Key] = kvp.Value;
    }
    
    /// <summary>
    /// Copies all entries from this <see cref="Dictionary{TKey,TValue}"/> into <paramref name="target"/>, overwriting any existing values with the same key.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="original">The source dictionary.</param>
    /// <param name="target">The destination dictionary to receive the copied entries.</param>
    public static void CopyTo<TKey, TValue>(this Dictionary<TKey, TValue> original, Dictionary<TKey, TValue> target)
        where TKey : notnull
    {
        foreach (var kvp in original)
            target[kvp.Key] = kvp.Value;
    }

    /// <summary>
    /// Removes all entries from the dictionary for which <paramref name="predicate"/> returns <see langword="true"/>.
    /// The dictionary is modified in place.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="dict">The dictionary to modify.</param>
    /// <param name="predicate">A function that receives each key–value pair and returns <see langword="true"/> for entries to remove.</param>
    public static void RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> dict, Func<TKey, TValue, bool> predicate)
    {
        List<TKey> killList = new();
        foreach(var kvp in dict)
        {
            if (predicate(kvp.Key, kvp.Value))
            {
                killList.Add(kvp.Key);
            }
        }
        foreach(var key in killList)
        {
            dict.Remove(key);
        }
    }

    #region Dictionary<TKey, IList<TValue>>
    /// <summary>
    /// Appends <paramref name="value"/> to the list stored at <paramref name="key"/>, creating a new list if the key is absent.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="key">The key identifying the target list.</param>
    /// <param name="value">The value to append.</param>
    public static void AddToList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictOfLists, TKey key, TValue value)
        where TKey : notnull
    {
        if (!dictOfLists.TryGetValue(key, out var list))
            dictOfLists.Add(key, new List<TValue> { value });
        else
            list.Add(value);
    }

    /// <summary>
    /// Returns the list stored at <paramref name="key"/>, or <see langword="null"/> if the key is absent or the list is empty.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The stored list, or <see langword="null"/> if not found or empty.</returns>
    public static IList<TValue>? GetList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictOfLists, TKey key)
        where TKey : notnull
    {
        if (dictOfLists.TryGetValue(key, out var list))
            return list.Any() ? list : null;
        else
            return null;
    }

    /// <summary>
    /// Removes <paramref name="value"/> from the list stored at <paramref name="key"/>. Does nothing if the key is absent.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="key">The key identifying the target list.</param>
    /// <param name="value">The value to remove.</param>
    public static void RemoveFromList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictOfLists, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictOfLists.TryGetValue(key, out var list))
        {
            list.Remove(value);
            if (list.Count == 0) dictOfLists.Remove(key);
        }
    }

    /// <summary>Returns <see langword="true"/> if any list in the dictionary contains at least one element.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <returns><see langword="true"/> if any nested list is non-empty.</returns>
    public static bool AnyInAnyList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictOfLists)
        where TKey : notnull => dictOfLists.Any(_ => _.Value.Any());

    /// <summary>Returns <see langword="true"/> if any list in the dictionary contains at least one element matching <paramref name="predicate"/>.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="predicate">The condition to test each element against.</param>
    /// <returns><see langword="true"/> if any element in any nested list satisfies <paramref name="predicate"/>.</returns>
    public static bool AnyInAnyList<TKey, TValue>(this IDictionary<TKey, IList<TValue>> dictOfLists, Func<TValue, bool> predicate)
        where TKey : notnull => dictOfLists.Any(_ => _.Value.Any(predicate));
    #endregion

    #region Dictionary<TKey, List<TValue>>
    /// <summary>
    /// Appends <paramref name="value"/> to the <see cref="List{T}"/> stored at <paramref name="key"/>, creating a new list if the key is absent.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="key">The key identifying the target list.</param>
    /// <param name="value">The value to append.</param>
    public static void AddToList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictOfLists, TKey key, TValue value)
    where TKey : notnull
    {
        if (!dictOfLists.TryGetValue(key, out var list))
            dictOfLists.Add(key, new List<TValue> { value });
        else
            list.Add(value);
    }

    /// <summary>
    /// Returns the <see cref="List{T}"/> stored at <paramref name="key"/>, or <see langword="null"/> if the key is absent or the list is empty.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The stored list, or <see langword="null"/> if not found or empty.</returns>
    public static IList<TValue>? GetList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictOfLists, TKey key)
        where TKey : notnull
    {
        if (dictOfLists.TryGetValue(key, out var list))
            return list.Any() ? list : null;
        else
            return null;
    }

    /// <summary>
    /// Removes <paramref name="value"/> from the <see cref="List{T}"/> stored at <paramref name="key"/>. Does nothing if the key is absent.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="key">The key identifying the target list.</param>
    /// <param name="value">The value to remove.</param>
    public static void RemoveFromList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictOfLists, TKey key, TValue value)
        where TKey : notnull
    {
        if (dictOfLists.TryGetValue(key, out var list))
        {
            list.Remove(value);
            if (list.Count == 0) dictOfLists.Remove(key);
        }
    }

    /// <summary>Returns <see langword="true"/> if any list in the dictionary contains at least one element.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <returns><see langword="true"/> if any nested list is non-empty.</returns>
    public static bool AnyInAnyList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictOfLists)
        where TKey : notnull => dictOfLists.Any(_ => _.Value.Any());

    /// <summary>Returns <see langword="true"/> if any list in the dictionary contains at least one element matching <paramref name="predicate"/>.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The list element type.</typeparam>
    /// <param name="dictOfLists">The dictionary of lists.</param>
    /// <param name="predicate">The condition to test each element against.</param>
    /// <returns><see langword="true"/> if any element in any nested list satisfies <paramref name="predicate"/>.</returns>
    public static bool AnyInAnyList<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictOfLists, Func<TValue, bool> predicate)
        where TKey : notnull => dictOfLists.Any(_ => _.Value.Any(predicate));
    #endregion

    #region Dictionary<TKey, HashSet<TValue>>
    /// <summary>
    /// Adds <paramref name="value"/> to the <see cref="HashSet{T}"/> stored at <paramref name="key"/>, creating a new set if the key is absent.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The set element type.</typeparam>
    /// <param name="dictOfSets">The dictionary of hash sets.</param>
    /// <param name="key">The key identifying the target set.</param>
    /// <param name="value">The value to add.</param>
    /// <returns><see langword="true"/> if the value was new and added; <see langword="false"/> if it was already present.</returns>
    public static bool AddToSet<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictOfSets, TKey key, TValue value)
        where TKey : notnull
    {
        if (!dictOfSets.TryGetValue(key, out var set))
        {
            dictOfSets.Add(key, new HashSet<TValue> { value });
            return true;
        }
        return set.Add(value);
    }

    /// <summary>
    /// Returns the <see cref="HashSet{T}"/> stored at <paramref name="key"/>, or <see langword="null"/> if the key is absent or the set is empty.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The set element type.</typeparam>
    /// <param name="dictOfSets">The dictionary of hash sets.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The stored set, or <see langword="null"/> if not found or empty.</returns>
    public static ISet<TValue>? GetSet<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictOfSets, TKey key)
        where TKey : notnull
    {
        if (dictOfSets.TryGetValue(key, out var set))
            return set.Any() ? set : null;
        else
            return null;
    }

    /// <summary>
    /// Removes <paramref name="value"/> from the <see cref="HashSet{T}"/> stored at <paramref name="key"/>.
    /// Optionally removes the entire set entry when it becomes empty.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The set element type.</typeparam>
    /// <param name="dictOfSets">The dictionary of hash sets.</param>
    /// <param name="key">The key identifying the target set.</param>
    /// <param name="value">The value to remove.</param>
    /// <param name="removeEmptySet">When <see langword="true"/>, removes the key entirely if the set becomes empty after removal.</param>
    /// <returns>The set after the operation, or <see langword="null"/> if the key was not found.</returns>
    public static HashSet<TValue>? RemoveFromSet<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictOfSets, TKey key, TValue value, bool removeEmptySet = false)
        where TKey : notnull
    {
        if (dictOfSets.TryGetValue(key, out var set))
            set.Remove(value);
        if (removeEmptySet && set?.Any() == false)
            dictOfSets.Remove(key);
        return set;
    }

    /// <summary>Returns <see langword="true"/> if any set in the dictionary contains at least one element.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The set element type.</typeparam>
    /// <param name="dictOfSets">The dictionary of hash sets.</param>
    /// <returns><see langword="true"/> if any nested set is non-empty.</returns>
    public static bool AnyInAnySet<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictOfSets)
        where TKey : notnull => dictOfSets.Any(_ => _.Value.Any());

    /// <summary>Returns <see langword="true"/> if any set in the dictionary contains at least one element matching <paramref name="predicate"/>.</summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The set element type.</typeparam>
    /// <param name="dictOfSets">The dictionary of hash sets.</param>
    /// <param name="predicate">The condition to test each element against.</param>
    /// <returns><see langword="true"/> if any element in any nested set satisfies <paramref name="predicate"/>.</returns>
    public static bool AnyInAnySet<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictOfSets, Func<TValue, bool> predicate)
        where TKey : notnull => dictOfSets.Any(_ => _.Value.Any(predicate));
    #endregion
}

