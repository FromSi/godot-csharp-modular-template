using Game.Game.Settings.Domain;
using Game.Game.Settings.Domain.Observer;
using Game.Game.Settings.Enum;
using Game.Game.Settings.Service;
using Game.Game.Settings.UI.Observer;
using System.Collections.Generic;
using Godot;

namespace Game.Game.Settings.UI;

/// <summary>
/// Settings screen: a dimmed overlay with a centred panel — the display mode and the window
/// resolution (inactive in fullscreen), each applied and saved as soon as it is picked, and
/// "Back" (raised to the level via <see cref="ISettingsUIObserver"/>; Esc does the same).
/// Watches the settings, so F11 / Alt+Enter pressed while it is open shows up at once.
/// </summary>
public partial class SettingsUI : Control, ISettingsStateObserver
{
    private readonly ISettingsService _settingsService;
    private readonly List<ISettingsUIObserver> _observers = [];

    private OptionButton _displayMode = null!;
    private OptionButton _resolution = null!;
    private IReadOnlyList<Resolution> _resolutions = [];

    public SettingsUI(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public void AddObserver(ISettingsUIObserver observer)
    {
        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;

        var dim = new ColorRect { Color = new Color(0, 0, 0, 0.7f) };
        dim.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(dim);

        var center = new CenterContainer();
        center.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(center);

        var panel = new PanelContainer { CustomMinimumSize = new Vector2(480, 0) };
        var style = new StyleBoxFlat { BgColor = new Color(0.12f, 0.12f, 0.12f) };
        style.SetCornerRadiusAll(6);
        style.SetContentMarginAll(24);
        panel.AddThemeStyleboxOverride("panel", style);
        center.AddChild(panel);

        var box = new VBoxContainer();
        box.AddThemeConstantOverride("separation", 16);
        panel.AddChild(box);

        var title = new Label { Text = "Settings" };
        title.AddThemeFontSizeOverride("font_size", 28);
        box.AddChild(title);

        _displayMode = AddRow(box, "Display mode");
        _displayMode.ItemSelected += OnDisplayModeSelected;
        _resolution = AddRow(box, "Resolution");
        _resolution.ItemSelected += OnResolutionSelected;

        var back = new Button { Text = "Back", CustomMinimumSize = new Vector2(0, 40) };
        back.Pressed += Close;
        box.AddChild(back);

        Refresh();
        _settingsService.Subscribe(this);
    }

    public override void _ExitTree()
    {
        _settingsService.Unsubscribe(this);
    }

    // ISettingsStateObserver
    public void OnSettingsChanged(SettingsState settings)
    {
        Refresh();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
    }

    // Shows the saved choices (call when opening).
    public void Refresh()
    {
        var screen = GameWindow.Screen(GetWindow());

        _resolutions = _settingsService.GetResolutions(screen);
        Fill(_displayMode, _settingsService.GetDisplayModes(), _settingsService.GetDisplayMode());
        Fill(_resolution, _resolutions, _settingsService.GetResolution(screen));
        _resolution.Disabled = _settingsService.GetDisplayMode() == DisplayMode.Fullscreen;
    }

    private static OptionButton AddRow(VBoxContainer box, string text)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 16);
        box.AddChild(row);
        row.AddChild(new Label { Text = text, SizeFlagsHorizontal = SizeFlags.ExpandFill });

        var options = new OptionButton { CustomMinimumSize = new Vector2(180, 0) };
        row.AddChild(options);

        return options;
    }

    private static void Fill<T>(OptionButton options, IReadOnlyList<T> items, T current)
    {
        options.Clear();

        for (var i = 0; i < items.Count; i++)
        {
            options.AddItem(items[i]!.ToString(), i);

            if (Equals(items[i], current))
            {
                options.Selected = i;
            }
        }
    }

    private void OnDisplayModeSelected(long index)
    {
        if (_settingsService.SetDisplayMode(_settingsService.GetDisplayModes()[(int)index]))
        {
            GameWindow.Apply(GetWindow(), _settingsService);
        }
    }

    private void OnResolutionSelected(long index)
    {
        if (_settingsService.SetResolution(_resolutions[(int)index]))
        {
            GameWindow.Apply(GetWindow(), _settingsService);
        }
    }

    private void Close()
    {
        foreach (var observer in _observers)
        {
            observer.OnSettingsClosed();
        }
    }
}
