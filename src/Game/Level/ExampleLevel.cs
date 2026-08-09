using Game.Game.Example.UI.Factory;
using Game.Game.Example.UI.Observer;
using Game.Game.Level.Observer;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// A level screen: it is an <see cref="ILevelObserver"/>, so it shows or hides itself in
/// reaction to level changes, and it hosts a module's UI built via a factory. It also
/// listens to that UI as an <see cref="IExampleUIObserver"/> — navigation lives here, not
/// in the UI. Copy this shape for every screen in your game.
/// </summary>
public partial class ExampleLevel : Control, ILevelObserver, IExampleUIObserver
{
    private readonly IGameLevel _gameLevel;
    private readonly ExampleUIFactory _exampleUiFactory;

    public ExampleLevel(IGameLevel gameLevel, ExampleUIFactory exampleUiFactory)
    {
        _gameLevel = gameLevel;
        _exampleUiFactory = exampleUiFactory;

        ProcessMode = ProcessModeEnum.Always;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Ready()
    {
        var ui = _exampleUiFactory.Create();
        ui.AddObserver(this);   // subscribe before adding to the tree
        AddChild(ui);
    }

    public override void _Input(InputEvent @event)
    {
        if (_gameLevel.CurrentLevel == Enum.Level.Main && @event.IsActionPressed("ui_cancel"))
        {
            _gameLevel.OpenLevel(Enum.Level.Quit);
        }
    }

    // IExampleUIObserver — the UI asked to quit; navigation is decided here.
    public void OnQuitRequested()
    {
        _gameLevel.OpenLevel(Enum.Level.Quit);
    }

    // ILevelObserver — show this screen only on the Main level.
    public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
    {
        Visible = newLevel == Enum.Level.Main;
    }
}
