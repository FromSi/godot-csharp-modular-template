using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Example.Domain;
using Game.Game.Example.Domain.Observer;
using Game.Game.Example.Service;
using Moq;
using NUnit.Framework;

namespace Game.Tests.Example.Service;

[TestFixture]
public class NoteServiceTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0);

    private ISingleRepository<NoteState> _repository = null!;
    private Mock<IRandomGeneratorService> _random = null!;
    private Mock<IClockService> _clock = null!;
    private NoteService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new SingleRepository<NoteState>();
        _repository.Update(new NoteState());

        _random = new Mock<IRandomGeneratorService>();
        _clock = new Mock<IClockService>();
        _clock.Setup(c => c.Now).Returns(Now);

        _service = new NoteService(_repository, _random.Object, _clock.Object);
    }

    [Test]
    public void SetText_UpdatesTextAndStampsClockTime()
    {
        Assert.That(_service.GetChangedAt(), Is.Null);

        _service.SetText("hello");

        Assert.That(_service.Get(), Is.EqualTo("hello"));
        Assert.That(_service.GetChangedAt(), Is.EqualTo(Now));
    }

    [Test]
    public void RandomNumber_UsesRandomGenerator()
    {
        _random.Setup(r => r.RandiRange(0, 999_999)).Returns(4242);

        Assert.That(_service.RandomNumber(), Is.EqualTo(4242));
    }

    [Test]
    public void Subscribe_NotifiesObserverWhenStateChanges()
    {
        var observer = new Mock<INoteStateObserver>();
        _service.Subscribe(observer.Object);

        _service.SetText("hi");

        observer.Verify(o => o.OnTextChanged("hi", Now), Times.Once);
    }

    [Test]
    public void Unsubscribe_StopsNotifications()
    {
        var observer = new Mock<INoteStateObserver>();
        _service.Subscribe(observer.Object);
        _service.Unsubscribe(observer.Object);

        _service.SetText("hi");

        observer.Verify(o => o.OnTextChanged(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
    }
}
