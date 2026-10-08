using Game.Game.Common.Domain;
using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using NUnit.Framework;

namespace Game.Tests.Common.Service;

[TestFixture]
public class NewGameServiceTests
{
    [Test]
    public void Create_ResetsEverySlotToItsFreshState()
    {
        var usedRepository = new SingleRepository<IdState>();
        usedRepository.Update(new IdState { Counter = 99 });
        var emptyRepository = new SingleRepository<IdState>();

        var service = new NewGameService(
        [
            new StateSlot<IdState>(usedRepository, () => new IdState()),
            new StateSlot<IdState>(emptyRepository, () => new IdState { Counter = 10 }),
        ]);

        service.Create();

        Assert.That(usedRepository.GetOne().Counter, Is.EqualTo(1));
        Assert.That(emptyRepository.GetOne().Counter, Is.EqualTo(10));
    }
}
