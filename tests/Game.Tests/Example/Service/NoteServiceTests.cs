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
    private ISingleRepository<NoteState> _repository = null!;
    private Mock<IRandomGeneratorService> _random = null!;
    private NoteService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new SingleRepository<NoteState>();
        _repository.Update(new NoteState());

        _random = new Mock<IRandomGeneratorService>();

        _service = new NoteService(_repository, _random.Object);
    }

    [Test]
    public void SetText_UpdatesState()
    {
        _service.SetText("hello");

        Assert.That(_service.Get(), Is.EqualTo("hello"));
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

        observer.Verify(o => o.OnTextChanged("hi"), Times.Once);
    }
}
