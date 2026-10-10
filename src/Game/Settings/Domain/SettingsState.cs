using Game.Game.Common.Domain.Observer;
using Game.Game.Settings.Domain.Observer;
using Game.Game.Settings.Enum;

namespace Game.Game.Settings.Domain;

/// <summary>
/// The player's settings — kept in their own file, so they survive New Game. Mutate only via
/// <see cref="SetDisplayMode"/> / <see cref="SetResolution"/> so observers are notified; the
/// public setters exist only for JSON.
/// </summary>
public class SettingsState : ObservableState<ISettingsStateObserver>
{
    public DisplayMode DisplayMode { get; set; } = DisplayMode.Windowed;
    public int WindowWidth { get; set; } = 1280;
    public int WindowHeight { get; set; } = 720;

    public void SetDisplayMode(DisplayMode displayMode)
    {
        DisplayMode = displayMode;
        Notify(observer => observer.OnSettingsChanged(this));
    }

    public void SetResolution(Resolution resolution)
    {
        WindowWidth = resolution.Width;
        WindowHeight = resolution.Height;
        Notify(observer => observer.OnSettingsChanged(this));
    }
}
