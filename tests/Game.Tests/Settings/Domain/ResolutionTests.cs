using Game.Game.Settings.Domain;
using NUnit.Framework;

namespace Game.Tests.Settings.Domain;

[TestFixture]
public class ResolutionTests
{
    [TestCase(1920, 1080, "16:9")]
    [TestCase(1366, 768, "16:9")]
    [TestCase(1920, 1200, "16:10")]
    [TestCase(1024, 768, "4:3")]
    [TestCase(1280, 1024, "5:4")]
    [TestCase(2560, 1080, "21:9")]
    [TestCase(3440, 1440, "21:9")]
    [TestCase(5120, 1440, "32:9")]
    public void Aspect_IsTheNearestCommonName(int width, int height, string aspect)
    {
        Assert.That(new Resolution(width, height).Aspect(), Is.EqualTo(aspect));
    }

    [Test]
    public void ToString_ShowsTheSizeAndAspect()
    {
        Assert.That(new Resolution(1920, 1080).ToString(), Is.EqualTo("1920 × 1080 (16:9)"));
    }
}
