using System;
using System.Collections.Generic;

namespace Game.Game.Common.Repository;

public interface ICollectionRepository<TKey, TValue> where TKey : notnull
{
    IRepositoryIndex<TKey, TValue, TIndexKey> AddIndex<TIndexKey>(Func<TValue, TIndexKey> keySelector) where TIndexKey : notnull;
    void Update(TKey key, TValue value);
    void UpdateAll(Dictionary<TKey, TValue> states);
    TValue? GetOne(TKey key);
    Dictionary<TKey, TValue> GetAll();
    void DeleteAll();
}
