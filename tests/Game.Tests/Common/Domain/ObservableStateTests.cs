using Game.Game.Common.Domain.Observer;
using NUnit.Framework;

namespace Game.Tests.Common.Domain;

[TestFixture]
public class ObservableStateTests
{
    private interface ICountObserver
    {
        void OnChanged(int value);
    }

    private class CountState : ObservableState<ICountObserver>
    {
        public int Value { get; private set; }

        public void Set(int value)
        {
            Value = value;
            Notify(observer => observer.OnChanged(value));
        }
    }

    private class SpyObserver : ICountObserver
    {
        public int Calls { get; private set; }
        public int LastValue { get; private set; } = -1;

        public void OnChanged(int value)
        {
            Calls++;
            LastValue = value;
        }
    }

    [Test]
    public void Notify_CallsSubscribedObserver()
    {
        var state = new CountState();
        var spy = new SpyObserver();
        state.AddObserver(spy);

        state.Set(7);

        Assert.That(spy.Calls, Is.EqualTo(1));
        Assert.That(spy.LastValue, Is.EqualTo(7));
    }

    [Test]
    public void AddObserver_IsIdempotent()
    {
        var state = new CountState();
        var spy = new SpyObserver();
        state.AddObserver(spy);
        state.AddObserver(spy);

        state.Set(1);

        Assert.That(spy.Calls, Is.EqualTo(1));
    }

    [Test]
    public void RemoveObserver_StopsNotifications()
    {
        var state = new CountState();
        var spy = new SpyObserver();
        state.AddObserver(spy);
        state.RemoveObserver(spy);

        state.Set(9);

        Assert.That(spy.Calls, Is.EqualTo(0));
    }
}
