using Game.Game.Common.Repository;
using Game.Game.Settings.Domain;
using Game.Game.Settings.Domain.Observer;
using Game.Game.Settings.Enum;
using Game.Game.Settings.Service;
using Moq;
using NUnit.Framework;

namespace Game.Tests.Settings.Service;

[TestFixture]
public class SettingsServiceTests
{
    private static readonly Resolution FullHd = new(1920, 1080);
    private static readonly Resolution Uhd = new(3840, 2160);

    private SingleRepository<SettingsState> _repository = null!;
    private Mock<ISettingsStore> _store = null!;
    private SettingsService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new SingleRepository<SettingsState>();
        _repository.Update(new SettingsState());
        _store = new Mock<ISettingsStore>();
        _service = new SettingsService(_repository, _store.Object);
    }

    [Test]
    public void DisplayMode_IsWindowedByDefault_SetSavesAndNotifies_RefusesTheSame()
    {
        var observer = new Mock<ISettingsStateObserver>();
        _service.Subscribe(observer.Object);

        Assert.That(_service.GetDisplayModes(), Is.EqualTo(new[] { DisplayMode.Windowed, DisplayMode.Fullscreen }));
        Assert.That(_service.GetDisplayMode(), Is.EqualTo(DisplayMode.Windowed));
        Assert.That(_service.SetDisplayMode(DisplayMode.Windowed), Is.False);
        Assert.That(_service.SetDisplayMode(DisplayMode.Fullscreen), Is.True);

        Assert.That(_service.GetDisplayMode(), Is.EqualTo(DisplayMode.Fullscreen));
        _store.Verify(s => s.Save(), Times.Once);
        observer.Verify(o => o.OnSettingsChanged(It.IsAny<SettingsState>()), Times.Once);
    }

    [Test]
    public void ToggleDisplayMode_SwitchesBackAndForthAndSaves()
    {
        Assert.That(_service.ToggleDisplayMode(), Is.EqualTo(DisplayMode.Fullscreen));
        Assert.That(_service.ToggleDisplayMode(), Is.EqualTo(DisplayMode.Windowed));
        Assert.That(_service.GetDisplayMode(), Is.EqualTo(DisplayMode.Windowed));
        _store.Verify(s => s.Save(), Times.Exactly(2));
    }

    [Test]
    public void GetDisplayMode_FallsBackToWindowedForAnUnknownSavedOne()
    {
        _repository.GetOne().DisplayMode = (DisplayMode)7;

        Assert.That(_service.GetDisplayMode(), Is.EqualTo(DisplayMode.Windowed));
    }

    [Test]
    public void Resolutions_AreTheCommonSizesThatFitTheScreen_ByWidthThenHeight()
    {
        var offered = _service.GetResolutions(FullHd);

        Assert.That(offered, Has.Count.EqualTo(10));
        Assert.That(offered[0], Is.EqualTo(new Resolution(1024, 768)));
        Assert.That(offered[^1], Is.EqualTo(FullHd));
        Assert.That(offered, Does.Not.Contain(new Resolution(1600, 1200)));
        Assert.That(_service.GetResolutions(new Resolution(1920, 1200))[^1], Is.EqualTo(new Resolution(1920, 1200)));
        Assert.That(_service.GetResolutions(new Resolution(5120, 2160)), Has.Count.EqualTo(23));
        Assert.That(
            SettingsService.Resolutions,
            Is.Ordered.By(nameof(Resolution.Width)).Then.By(nameof(Resolution.Height))
        );
        Assert.That(_service.GetResolution(FullHd), Is.EqualTo(new Resolution(1280, 720)));
    }

    [Test]
    public void Resolutions_KeepTheSmallestOnATinyScreen()
    {
        var tiny = new Resolution(800, 600);

        Assert.That(_service.GetResolutions(tiny), Is.EqualTo(new[] { new Resolution(1024, 768) }));
        Assert.That(_service.GetResolution(tiny), Is.EqualTo(new Resolution(1024, 768)));
    }

    [Test]
    public void GetResolution_FallsBackOnASmallerScreenButKeepsTheSavedSize()
    {
        _service.SetResolution(new Resolution(2560, 1440));

        Assert.That(_service.GetResolution(FullHd), Is.EqualTo(new Resolution(1280, 720)));
        Assert.That(_service.GetResolution(Uhd), Is.EqualTo(new Resolution(2560, 1440)));
    }

    [Test]
    public void SetResolution_ChangesSavesAndNotifies()
    {
        var observer = new Mock<ISettingsStateObserver>();
        _service.Subscribe(observer.Object);

        Assert.That(_service.SetResolution(new Resolution(1920, 1080)), Is.True);

        Assert.That(_service.GetResolution(FullHd), Is.EqualTo(FullHd));
        _store.Verify(s => s.Save(), Times.Once);
        observer.Verify(o => o.OnSettingsChanged(It.IsAny<SettingsState>()), Times.Once);
    }

    [Test]
    public void SetResolution_RefusesTheSameOrAnUnofferedSize()
    {
        Assert.That(_service.SetResolution(new Resolution(1280, 720)), Is.False);
        Assert.That(_service.SetResolution(new Resolution(800, 600)), Is.False);
        _store.Verify(s => s.Save(), Times.Never);
    }

    [Test]
    public void GetResolution_FallsBackToTheBaseSizeForAnUnknownSavedOne()
    {
        _repository.GetOne().WindowWidth = 2304;
        _repository.GetOne().WindowHeight = 1296;

        Assert.That(_service.GetResolution(Uhd), Is.EqualTo(new Resolution(1280, 720)));
    }
}
