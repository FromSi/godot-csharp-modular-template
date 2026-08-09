namespace Game.Game.Common.Domain.Observer;

/// <summary>
/// Publisher side of the observer pattern: something that lets listeners of type
/// <typeparamref name="TObserver"/> subscribe to its changes.
/// </summary>
public interface IObservableState<in TObserver>
{
    void AddObserver(TObserver observer);
    void RemoveObserver(TObserver observer);
}
