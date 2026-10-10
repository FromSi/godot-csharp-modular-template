using Game.Game.Common.Service;
using Game.Game.Settings.Service;

namespace Game.Game.Level.Adapter;

/// <summary>
/// The settings (Settings module) go to their own file through a <see cref="SaveService"/> over
/// the settings slot alone — apart from the game save, so New Game never resets them.
/// </summary>
public class SettingsFileStore : ISettingsStore
{
    private readonly SaveService _saveService;

    public SettingsFileStore(SaveService saveService)
    {
        _saveService = saveService;
    }

    public void Save()
    {
        _saveService.Save();
    }
}
