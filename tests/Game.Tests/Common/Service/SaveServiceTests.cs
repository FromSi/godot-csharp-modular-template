using Game.Game.Common.Domain;
using Game.Game.Common.Enum;
using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Common.Service.FileHandler;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;

namespace Game.Tests.Common.Service;

[TestFixture]
public class SaveServiceTests
{
    private const string Path = "user://saves/game.save";

    private class Note
    {
        public string Text { get; set; } = "";
    }

    private ISingleRepository<IdState> _idRepository = null!;
    private ISingleRepository<Note> _noteRepository = null!;
    private Mock<IJsonStateFileHandlerService> _fileHandler = null!;
    private SaveService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _idRepository = new SingleRepository<IdState>();
        _idRepository.Update(new IdState());
        _noteRepository = new SingleRepository<Note>();
        _noteRepository.Update(new Note());

        _fileHandler = new Mock<IJsonStateFileHandlerService>();
        _service = new SaveService(Path, _fileHandler.Object, new List<IStateSlot>
        {
            new StateSlot<IdState>(_idRepository, () => new IdState()),
            new StateSlot<Note>(_noteRepository, () => new Note()),
        });
    }

    [Test]
    public void Save_StoresEverySlotInOrder()
    {
        _idRepository.GetOne().Counter = 7;
        _noteRepository.GetOne().Text = "saved";

        List<object?>? stored = null;
        _fileHandler
            .Setup(h => h.Store(It.IsAny<List<object?>>(), Path, true))
            .Callback<List<object?>, string, bool>((data, _, _) => stored = data)
            .Returns(FileError.Ok);

        Assert.That(_service.Save(), Is.True);

        Assert.That(stored, Has.Count.EqualTo(2));
        Assert.That(((IdState)stored![0]!).Counter, Is.EqualTo(7));
        Assert.That(((Note)stored[1]!).Text, Is.EqualTo("saved"));
    }

    [Test]
    public void Save_IsFalseWhenTheFileIsNotWritten()
    {
        _fileHandler
            .Setup(h => h.Store(It.IsAny<List<object?>>(), Path, true))
            .Returns(FileError.AccessDenied);

        Assert.That(_service.Save(), Is.False);
    }

    [Test]
    public void Load_RestoresEverySlot()
    {
        _fileHandler
            .Setup(h => h.Load(Path))
            .Returns([new IdState { Counter = 5 }, new Note { Text = "restored" }]);

        Assert.That(_service.Load(), Is.True);
        Assert.That(_idRepository.GetOne().Counter, Is.EqualTo(5));
        Assert.That(_noteRepository.GetOne().Text, Is.EqualTo("restored"));
    }

    [Test]
    public void Load_RejectsMissingIncompleteOrMismatchedFileWithoutTouchingState()
    {
        _idRepository.GetOne().Counter = 42;

        _fileHandler.Setup(h => h.Load(Path)).Returns([]);
        Assert.That(_service.Load(), Is.False);

        _fileHandler.Setup(h => h.Load(Path)).Returns([new IdState { Counter = 1 }]);
        Assert.That(_service.Load(), Is.False);

        _fileHandler.Setup(h => h.Load(Path)).Returns([new IdState { Counter = 1 }, new IdState()]);
        Assert.That(_service.Load(), Is.False);

        _fileHandler.Setup(h => h.Load(Path)).Returns([new IdState { Counter = 1 }, null]);
        Assert.That(_service.Load(), Is.False);

        Assert.That(_idRepository.GetOne().Counter, Is.EqualTo(42));
    }

    [Test]
    public void CanLoad_OnlyForAFileThatMatchesTheSlots()
    {
        _idRepository.GetOne().Counter = 42;

        _fileHandler.Setup(h => h.Load(Path)).Returns([]);
        Assert.That(_service.CanLoad(), Is.False);

        _fileHandler.Setup(h => h.Load(Path)).Returns([new IdState { Counter = 1 }, new IdState()]);
        Assert.That(_service.CanLoad(), Is.False);

        _fileHandler.Setup(h => h.Load(Path)).Returns([new IdState { Counter = 1 }, new Note()]);
        Assert.That(_service.CanLoad(), Is.True);

        Assert.That(_idRepository.GetOne().Counter, Is.EqualTo(42));
    }
}
