namespace Game.Game.Settings.UI.Observer;

/// <summary>
/// UI → Level: the settings screen was closed ("Back").
/// </summary>
public interface ISettingsUIObserver
{
    void OnSettingsClosed();
}
