using Game.Game.Settings.Domain;
using Game.Game.Settings.Enum;
using Game.Game.Settings.Service;
using Godot;

namespace Game.Game.Settings.UI;

/// <summary>
/// Shows the window as the settings say: fullscreen (borderless, the screen's own size) or a
/// window at the chosen resolution, centred on its screen. The project stretches the 1280 × 720
/// layout (<c>canvas_items</c>, <c>keep</c>) to the window, so a bigger window shows everything bigger.
/// Goes through the <see cref="Window"/> node, not <c>DisplayServer</c>: only then does the
/// root viewport learn its new size and stretch the layout instead of leaving a black margin.
/// A window never goes past the usable area (screen minus taskbar), so its title bar stays visible.
/// </summary>
public static class GameWindow
{
    // The size of the screen the window is on — the resolutions offered must fit it.
    public static Resolution Screen(Window window)
    {
        var size = DisplayServer.ScreenGetSize(window.CurrentScreen);

        return new Resolution(size.X, size.Y);
    }

    public static void Apply(Window window, ISettingsService settingsService)
    {
        if (settingsService.GetDisplayMode() == DisplayMode.Fullscreen)
        {
            window.Mode = Window.ModeEnum.Fullscreen;

            return;
        }

        window.Mode = Window.ModeEnum.Windowed;
        ApplySize(window, settingsService.GetResolution(Screen(window)));
    }

    private static void ApplySize(Window window, Resolution resolution)
    {
        var usable = DisplayServer.ScreenGetUsableRect(window.CurrentScreen);
        var size = new Vector2I(resolution.Width, resolution.Height).Min(usable.Size);

        window.Size = size;
        window.Position = usable.Position + (usable.Size - size) / 2;
    }
}
