using System.Collections.Concurrent;
using System.Collections.Immutable;
namespace RossWright.MetalChain;

internal class ConcurrentDictionaryOfLists<TKey, TValue> 
    where TKey : notnull
{
    private ConcurrentDictionary<TKey, ImmutableList<TValue>> _collection = new();

    public ICollection<TKey> Keys => _collection.Keys;

    public bool ContainsKey(TKey key) => _collection.ContainsKey(key);
    public bool ContainsKey(TKey key, TValue value) => 
        _collection.TryGetValue(key, out var list) ? list.Contains(value) : false;

    // Returns an immutable snapshot — safe to enumerate without extra locking.
    public IEnumerable<TValue> GetValuesOrEmptySet(TKey key) =>
        _collection.TryGetValue(key, out var values) ? values : ImmutableList<TValue>.Empty;

    // Lock-free, atomic updates using immutable lists.
    public void Add(TKey key, TValue value) => 
        _collection.AddOrUpdate(key, [value], (_, list) => list.Add(value));

    // Remove is implemented with optimistic CAS (TryUpdate) loops to avoid races.
    public void Remove(TKey key, TValue value)
    {
        while (true)
        {
            if (!_collection.TryGetValue(key, out var list)) return;

            var newList = list.Remove(value);

            if (newList.Count == 0)
            {
                // Try to replace the current list with the empty one, then attempt removal.
                if (_collection.TryUpdate(key, newList, list))
                {
                    // best-effort remove of the key when the list is empty
                    _collection.TryRemove(key, out _);
                    return;
                }
            }
            else
            {
                if (_collection.TryUpdate(key, newList, list))
                    return;
            }

            // If TryUpdate failed someone else changed the list; retry.
        }
    }
}
