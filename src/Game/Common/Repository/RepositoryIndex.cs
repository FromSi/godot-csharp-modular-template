using System;
using System.Collections.Generic;

namespace Game.Game.Common.Repository;

public class RepositoryIndex<TKey, TValue, TIndexKey> : IRepositoryIndex<TKey, TValue, TIndexKey>
    where TKey : notnull
    where TIndexKey : notnull
{
    private readonly Dictionary<TIndexKey, Dictionary<TKey, TValue>> _index = new();
    private readonly Func<TValue, TIndexKey> _keySelector;

    public RepositoryIndex(Func<TValue, TIndexKey> keySelector)
    {
        _keySelector = keySelector;
    }

    public void Add(TKey key, TValue value)
    {
        var indexKey = _keySelector(value);
        
        if (!_index.TryGetValue(indexKey, out var bucket))
        {
            bucket = new Dictionary<TKey, TValue>();
            _index[indexKey] = bucket;
        }
        
        bucket[key] = value;
    }

    public void Remove(TKey key, TValue value)
    {
        var indexKey = _keySelector(value);
        
        if (_index.TryGetValue(indexKey, out var bucket))
        {
            bucket.Remove(key);
        }
    }

    public Dictionary<TKey, TValue> GetAll(TIndexKey indexKey)
    {
        var bucket = _index.GetValueOrDefault(indexKey);
        return bucket != null ? new Dictionary<TKey, TValue>(bucket) : new Dictionary<TKey, TValue>();
    }

    public void Clear() {
        _index.Clear();
    }
}
