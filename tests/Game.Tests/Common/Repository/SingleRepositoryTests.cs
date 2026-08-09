using Game.Game.Common.Repository;
using NUnit.Framework;

namespace Game.Tests.Common.Repository;

[TestFixture]
public class SingleRepositoryTests
{
    private class Sample
    {
        public int Value { get; set; }
    }

    [Test]
    public void GetOne_ReturnsUpdatedState()
    {
        var repository = new SingleRepository<Sample>();
        var state = new Sample { Value = 42 };

        repository.Update(state);

        Assert.That(repository.GetOne(), Is.SameAs(state));
    }

    [Test]
    public void GetOne_ThrowsWhenNotInitialized()
    {
        var repository = new SingleRepository<Sample>();

        Assert.Throws<System.InvalidOperationException>(() => repository.GetOne());
    }
}
