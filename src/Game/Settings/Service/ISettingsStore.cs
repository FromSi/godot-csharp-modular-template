namespace Game.Game.Settings.Service;

/// <summary>
/// Writes the settings to their file. Implemented in <c>Level</c> (modules do no file I/O).
/// </summary>
public interface ISettingsStore
{
    void Save();
}
