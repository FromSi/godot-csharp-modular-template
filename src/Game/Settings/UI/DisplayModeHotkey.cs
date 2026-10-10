using Game.Game.Settings.Service;
using Godot;

namespace Game.Game.Settings.UI;

/// <summary>
/// F11 or Alt+Enter switches windowed ↔ fullscreen anywhere in the game (menu, play, settings)
/// and saves it. Handled in <c>_Input</c>, before a focused button can take Enter as a press.
/// </summary>
public partial class DisplayModeHotkey : Node
{
    private readonly ISettingsService _settingsService;

    public DisplayModeHotkey(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key || !IsToggle(key))
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        _settingsService.ToggleDisplayMode();
        GameWindow.Apply(GetWindow(), _settingsService);
    }

    private static bool IsToggle(InputEventKey key)
    {
        var isEnter = key.Keycode is Key.Enter or Key.KpEnter;

        return key.Keycode == Key.F11 || (isEnter && key.AltPressed);
    }
}
