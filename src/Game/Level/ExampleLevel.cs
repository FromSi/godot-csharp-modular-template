using Game.Game.Example.UI.Factory;
using Game.Game.Example.UI.Observer;
using Game.Game.Level.Observer;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// The game screen: an <see cref="ILevelObserver"/> that shows itself on the Main level and
/// hosts a module's UI built via a factory. <c>GameLevel</c> creates it only after Continue /
/// New Game (so the UI subscribes to the loaded / fresh states) and frees it on the way back to
/// the menu. It also listens to the UI as an <see cref="IExampleUIObserver"/> — navigation lives
/// here, not in the UI. Copy this shape for your game screens.
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
            GetViewport().SetInputAsHandled();
            _gameLevel.ReturnToMenu();
        }
    }

    // IExampleUIObserver — the UI asked for the menu; navigation is decided here.
    public void OnMenuRequested()
    {
        _gameLevel.ReturnToMenu();
    }

    // ILevelObserver — show this screen only on the Main level.
    public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
    {
        Visible = newLevel == Enum.Level.Main;
    }
}
