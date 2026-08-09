using System.Collections.Generic;

namespace Game.Game.Common.Repository;

public interface IRepositoryIndex<TKey, TValue>
{
    void Add(TKey key, TValue value);
    void Remove(TKey key, TValue value);
    void Clear();
}

public interface IRepositoryIndex<TKey, TValue, TIndexKey> : IRepositoryIndex<TKey, TValue>
    where TKey : notnull
    where TIndexKey : notnull
{
    Dictionary<TKey, TValue> GetAll(TIndexKey indexKey);
}
