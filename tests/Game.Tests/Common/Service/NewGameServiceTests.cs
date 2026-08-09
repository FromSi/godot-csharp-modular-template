using Game.Game.Common.Domain;
using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Example.Domain;
using NUnit.Framework;

namespace Game.Tests.Common.Service;

[TestFixture]
public class NewGameServiceTests
{
    [Test]
    public void Create_ResetsRepositoriesToFreshState()
    {
        var idRepository = new SingleRepository<IdState>();
        idRepository.Update(new IdState { Counter = 99 });
        var noteRepository = new SingleRepository<NoteState>();
        noteRepository.Update(new NoteState { Text = "old" });

        var service = new NewGameService(idRepository, noteRepository);

        service.Create();

        Assert.That(idRepository.GetOne().Counter, Is.EqualTo(1));
        Assert.That(noteRepository.GetOne().Text, Is.EqualTo(""));
    }
}
