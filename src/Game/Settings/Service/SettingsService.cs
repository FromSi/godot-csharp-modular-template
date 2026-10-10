using Game.Game.Common.Repository;
using Game.Game.Settings.Domain;
using Game.Game.Settings.Domain.Observer;
using Game.Game.Settings.Enum;
using System.Collections.Generic;
using System.Linq;

namespace Game.Game.Settings.Service;

/// <summary>
/// The game settings: windowed or fullscreen, and the window size — one of
/// <see cref="Resolutions"/> that fits the player's screen (a saved value that is not offered
/// falls back to the default). A change is saved right away.
/// </summary>
public class SettingsService : ISettingsService
{
    public static readonly IReadOnlyList<DisplayMode> DisplayModes =
    [
        DisplayMode.Windowed, DisplayMode.Fullscreen,
    ];
    public static readonly Resolution DefaultResolution = new(1280, 720);

    // The common monitor sizes of every aspect (4:3, 5:4, 16:10, 16:9, 21:9, 32:9), by width then
    // height, as PC games list them. The layout is 16:9; other aspects show it with bars.
    public static readonly IReadOnlyList<Resolution> Resolutions =
    [
        new(1024, 768), new(1280, 720), new(1280, 800), new(1280, 960), new(1280, 1024), new(1366, 768),
        new(1440, 900), new(1600, 900), new(1600, 1200), new(1680, 1050), new(1920, 1080), new(1920, 1200),
        new(2560, 1080), new(2560, 1440), new(2560, 1600), new(2880, 1800), new(3200, 1800), new(3440, 1440),
        new(3840, 1080), new(3840, 1600), new(3840, 2160), new(5120, 1440), new(5120, 2160),
    ];

    private readonly ISingleRepository<SettingsState> _repository;
    private readonly ISettingsStore _store;

    public SettingsService(ISingleRepository<SettingsState> repository, ISettingsStore store)
    {
        _repository = repository;
        _store = store;
    }

    public IReadOnlyList<DisplayMode> GetDisplayModes()
    {
        return DisplayModes;
    }

    public DisplayMode GetDisplayMode()
    {
        var saved = _repository.GetOne().DisplayMode;

        return DisplayModes.Contains(saved) ? saved : DisplayModes[0];
    }

    public bool SetDisplayMode(DisplayMode displayMode)
    {
        if (!DisplayModes.Contains(displayMode) || displayMode == GetDisplayMode())
        {
            return false;
        }

        _repository.GetOne().SetDisplayMode(displayMode);
        _store.Save();

        return true;
    }

    // F11 / Alt+Enter: windowed ↔ fullscreen.
    public DisplayMode ToggleDisplayMode()
    {
        var next = GetDisplayMode() == DisplayMode.Windowed ? DisplayMode.Fullscreen : DisplayMode.Windowed;

        SetDisplayMode(next);

        return next;
    }

    // The sizes no bigger than the screen; the smallest is offered even on a tiny screen.
    public IReadOnlyList<Resolution> GetResolutions(Resolution screen)
    {
        var fitting = Resolutions
            .Where(resolution => resolution.Width <= screen.Width && resolution.Height <= screen.Height)
            .ToList();

        return fitting.Count > 0 ? fitting : [Resolutions[0]];
    }

    // The saved size stays in the file even when this screen is too small for it, so it comes back
    // on a bigger one. Otherwise the default, or the smallest when even that doesn't fit.
    public Resolution GetResolution(Resolution screen)
    {
        var state = _repository.GetOne();
        var saved = new Resolution(state.WindowWidth, state.WindowHeight);
        var offered = GetResolutions(screen);

        if (offered.Contains(saved))
        {
            return saved;
        }

        return offered.Contains(DefaultResolution) ? DefaultResolution : offered[0];
    }

    public bool SetResolution(Resolution resolution)
    {
        var state = _repository.GetOne();

        if (!Resolutions.Contains(resolution)
            || resolution == new Resolution(state.WindowWidth, state.WindowHeight))
        {
            return false;
        }

        state.SetResolution(resolution);
        _store.Save();

        return true;
    }

    public void Subscribe(ISettingsStateObserver observer)
    {
        _repository.GetOne().AddObserver(observer);
    }

    public void Unsubscribe(ISettingsStateObserver observer)
    {
        _repository.GetOne().RemoveObserver(observer);
    }
}
