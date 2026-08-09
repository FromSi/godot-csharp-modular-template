using Godot;

namespace Game.Game.Common.Service;

/// <summary>
/// Environment adapter. This is the one <c>Common/Service</c> class that touches the
/// Godot API on purpose — it hides the "am I a release build?" check behind
/// <see cref="IOsService"/> so the rest of the code (and tests) stays engine-free.
/// </summary>
public class OsService : IOsService
{
    // "debug" is present when running from the editor (F5) and in debug exports;
    // it's absent only in a release export. So dev runs get plain, readable saves
    // and only shipped release builds encrypt them.
    public bool IsProduction()
    {
        return !OS.HasFeature("debug");
    }
}
