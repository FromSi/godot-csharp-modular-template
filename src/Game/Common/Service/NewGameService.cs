using System.Collections.Generic;

namespace Game.Game.Common.Service;

/// <summary>
/// The "new game" counterpart of <see cref="SaveService"/>: resets every <see cref="IStateSlot"/>
/// to its fresh state (empty, or seeded with a new game's starting content).
/// </summary>
public class NewGameService
{
    private readonly IReadOnlyList<IStateSlot> _slots;

    public NewGameService(IReadOnlyList<IStateSlot> slots)
    {
        _slots = slots;
    }

    public void Create()
    {
        foreach (var slot in _slots)
        {
            slot.Reset();
        }
    }
}
