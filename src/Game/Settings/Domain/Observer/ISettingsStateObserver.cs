namespace Game.Game.Settings.Domain.Observer;

/// <summary>
/// Listener for <see cref="SettingsState"/> changes. Implemented by the UI.
/// </summary>
public interface ISettingsStateObserver
{
    void OnSettingsChanged(SettingsState settings);
}
