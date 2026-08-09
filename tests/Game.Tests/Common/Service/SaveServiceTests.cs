using System.Collections.Generic;
using Game.Game.Common.Domain;
using Game.Game.Common.Enum;
using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Common.Service.FileHandler;
using Game.Game.Example.Domain;
using Moq;
using NUnit.Framework;

namespace Game.Tests.Common.Service;

[TestFixture]
public class SaveServiceTests
{
    private const string Path = "user://saves/game.save";

    private ISingleRepository<IdState> _idRepository = null!;
    private ISingleRepository<NoteState> _noteRepository = null!;
    private Mock<IJsonStateFileHandlerService> _fileHandler = null!;
    private SaveService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _idRepository = new SingleRepository<IdState>();
        _idRepository.Update(new IdState());
        _noteRepository = new SingleRepository<NoteState>();
        _noteRepository.Update(new NoteState());

        _fileHandler = new Mock<IJsonStateFileHandlerService>();
        _service = new SaveService(Path, _fileHandler.Object, _idRepository, _noteRepository);
    }

    [Test]
    public void Save_StoresEveryRepositoryInOrder()
    {
        _noteRepository.GetOne().ChangeText("saved");

        List<object?>? stored = null;
        _fileHandler
            .Setup(h => h.Store(It.IsAny<List<object?>>(), Path, true))
            .Callback<List<object?>, string, bool>((data, _, _) => stored = data)
            .Returns(FileError.Ok);

        _service.Save();

        Assert.That(stored, Is.Not.Null);
        Assert.That(stored!, Has.Count.EqualTo(2));
        Assert.That(stored[0], Is.TypeOf<IdState>());
        Assert.That(((NoteState)stored[1]!).Text, Is.EqualTo("saved"));
    }

    [Test]
    public void Load_RestoresRepositories()
    {
        _fileHandler
            .Setup(h => h.Load(Path))
            .Returns(new List<object?> { new IdState { Counter = 5 }, new NoteState { Text = "restored" } });

        var ok = _service.Load();

        Assert.That(ok, Is.True);
        Assert.That(_idRepository.GetOne().Counter, Is.EqualTo(5));
        Assert.That(_noteRepository.GetOne().Text, Is.EqualTo("restored"));
    }

    [Test]
    public void Load_ReturnsFalseWhenFileMissingOrIncomplete()
    {
        _fileHandler.Setup(h => h.Load(Path)).Returns([]);

        Assert.That(_service.Load(), Is.False);
    }
}
