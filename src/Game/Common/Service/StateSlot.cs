using Game.Game.Common.Repository;
using System;

namespace Game.Game.Common.Service;

/// <summary>
/// An <see cref="IStateSlot"/> over a single-state repository: saves what is in it, restores a
/// loaded state into it, and resets it to a fresh state built by <c>createFresh</c> (new game).
/// </summary>
public class StateSlot<T> : IStateSlot where T : class
{
    private readonly ISingleRepository<T> _repository;
    private readonly Func<T> _createFresh;

    public StateSlot(ISingleRepository<T> repository, Func<T> createFresh)
    {
        _repository = repository;
        _createFresh = createFresh;
    }

    public Type StateType => typeof(T);

    public object Current()
    {
        return _repository.GetOne();
    }

    public void Restore(object state)
    {
        _repository.Delete();
        _repository.Update((T)state);
    }

    public void Reset()
    {
        _repository.Delete();
        _repository.Update(_createFresh());
    }
}
