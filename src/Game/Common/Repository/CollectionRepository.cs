using System;
using System.Collections.Generic;

namespace Game.Game.Common.Repository;

public class CollectionRepository<TKey, TValue> : ICollectionRepository<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _states = new();
    private readonly List<IRepositoryIndex<TKey, TValue>> _indexes = [];

    public IRepositoryIndex<TKey, TValue, TIndexKey> AddIndex<TIndexKey>(Func<TValue, TIndexKey> keySelector)
        where TIndexKey : notnull
    {
        var index = new RepositoryIndex<TKey, TValue, TIndexKey>(keySelector);

        _indexes.Add(index);

        return index;
    }

    public void Update(TKey key, TValue value)
    {
        if (_states.TryGetValue(key, out var old))
        {
            foreach (var index in _indexes)
            {
                index.Remove(key, old);
            }
        }

        _states[key] = value;

        foreach (var index in _indexes)
        {
            index.Add(key, value);
        }
    }

    public void UpdateAll(Dictionary<TKey, TValue> states)
    {
        _states.Clear();

        foreach (var index in _indexes)
        {
            index.Clear();
        }

        foreach (var (key, value) in states)
        {
            _states[key] = value;

            foreach (var index in _indexes)
            {
                index.Add(key, value);
            }
        }
    }

    public TValue? GetOne(TKey key) {
        return _states.GetValueOrDefault(key);
    }

    public Dictionary<TKey, TValue> GetAll() {
        return new Dictionary<TKey, TValue>(_states);
    }

    public void DeleteAll()
    {
        _states.Clear();

        foreach (var index in _indexes)
        {
            index.Clear();
        }
    }
}
