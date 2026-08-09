using System;
using System.Collections.Generic;

namespace Game.Game.Common.Domain.Observer;

/// <summary>
/// Base class for domain states that notify listeners about their own changes.
/// A concrete state extends it with a specific observer interface and calls
/// <see cref="Notify"/> whenever something changes.
///
/// The observer list is a private field, so it is never serialized — an
/// <see cref="ObservableState{TObserver}"/> can still be a plain saved POCO.
/// </summary>
public abstract class ObservableState<TObserver> : IObservableState<TObserver>
{
    private readonly List<TObserver> _observers = [];

    public void AddObserver(TObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void RemoveObserver(TObserver observer)
    {
        _observers.Remove(observer);
    }

    protected void Notify(Action<TObserver> notification)
    {
        foreach (var observer in _observers)
        {
            notification(observer);
        }
    }
}
