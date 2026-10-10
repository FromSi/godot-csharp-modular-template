namespace Game.Game.Settings.Enum;

/// <summary>
/// How the game window is shown. Fullscreen is borderless at the screen's own size, so the
/// chosen resolution applies only to a window. Saved as a number — append new values at the end.
/// </summary>
public enum DisplayMode
{
    Windowed,
    Fullscreen,
}
