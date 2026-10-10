using Game.Game.Settings.Domain;
using Game.Game.Settings.Domain.Observer;
using Game.Game.Settings.Enum;
using System.Collections.Generic;

namespace Game.Game.Settings.Service;

public interface ISettingsService
{
    IReadOnlyList<DisplayMode> GetDisplayModes();
    DisplayMode GetDisplayMode();
    bool SetDisplayMode(DisplayMode displayMode);
    DisplayMode ToggleDisplayMode();
    IReadOnlyList<Resolution> GetResolutions(Resolution screen);
    Resolution GetResolution(Resolution screen);
    bool SetResolution(Resolution resolution);
    void Subscribe(ISettingsStateObserver observer);
    void Unsubscribe(ISettingsStateObserver observer);
}
