using System;

namespace Game.Game.Common.Repository;

public class SingleRepository<T> : ISingleRepository<T> where T : class
{
    private T? _state;

    public void Update(T state)
    {
        _state = state;
    }

    public T GetOne()
    {
        return _state ?? throw new InvalidOperationException($"State of type {typeof(T).Name} is not initialized.");
    }

    public void Delete()
    {
        _state = null;
    }
}
