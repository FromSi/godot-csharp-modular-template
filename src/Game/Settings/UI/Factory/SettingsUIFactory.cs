using Game.Game.Settings.Service;

namespace Game.Game.Settings.UI.Factory;

/// <summary>
/// Builds <see cref="SettingsUI"/> and <see cref="DisplayModeHotkey"/> with their dependencies injected.
/// </summary>
public class SettingsUIFactory
{
    private readonly ISettingsService _settingsService;

    public SettingsUIFactory(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public SettingsUI Create()
    {
        return new SettingsUI(_settingsService);
    }

    public DisplayModeHotkey CreateDisplayModeHotkey()
    {
        return new DisplayModeHotkey(_settingsService);
    }
}
