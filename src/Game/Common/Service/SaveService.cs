using Game.Game.Common.Enum;
using Game.Game.Common.Service.FileHandler;
using System.Collections.Generic;
using System.Linq;

namespace Game.Game.Common.Service;

/// <summary>
/// Central persistence: writes every <see cref="IStateSlot"/> to a single file and restores them.
/// Module services do NOT do file I/O themselves — they own in-memory state, this service owns
/// saving. The slot list (built in <c>GameLevel</c>) defines what is saved and in which order;
/// a load is applied only if the file has exactly one state of the right type per slot, so a
/// broken or outdated save never leaves the game half-loaded.
/// </summary>
public class SaveService
{
    private readonly string _savePath;
    private readonly IJsonStateFileHandlerService _fileHandlerService;
    private readonly IReadOnlyList<IStateSlot> _slots;

    public SaveService(
        string savePath,
        IJsonStateFileHandlerService fileHandlerService,
        IReadOnlyList<IStateSlot> slots
    )
    {
        _savePath = savePath;
        _fileHandlerService = fileHandlerService;
        _slots = slots;
    }

    // True when Load() would succeed: the file exists, parses and matches the slots. A broken,
    // outdated or foreign (debug/release) save counts as no save.
    public bool CanLoad()
    {
        return ReadMatching() != null;
    }

    // False when the file could not be written (no access, full disk); the old save stays intact.
    public bool Save()
    {
        var data = _slots.Select(slot => (object?)slot.Current()).ToList();

        return _fileHandlerService.Store(data, _savePath) == FileError.Ok;
    }

    public bool Load()
    {
        var data = ReadMatching();

        if (data == null)
        {
            return false;
        }

        for (var i = 0; i < _slots.Count; i++)
        {
            _slots[i].Restore(data[i]!);
        }

        return true;
    }

    // The saved states, or null unless there is exactly one state of the right type per slot.
    private List<object?>? ReadMatching()
    {
        var data = _fileHandlerService.Load(_savePath);
        var matches = data.Count == _slots.Count
            && data.Zip(_slots).All(pair => pair.Second.StateType.IsInstanceOfType(pair.First));

        return matches ? data : null;
    }
}
