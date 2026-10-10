using Game.Game.Example.UI;
using Game.Game.Example.UI.Factory;
using Game.Game.Example.UI.Observer;
using Game.Game.Level.Observer;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// The game screen: an <see cref="ILevelObserver"/> that shows itself on the Main level and
/// hosts a module's UI built via a factory. <c>GameLevel</c> creates it only after Continue /
/// New Game (so the UI subscribes to the loaded / fresh states) and frees it on the way back to
/// the menu. It also listens to the UI as an <see cref="IExampleUIObserver"/> — saving and
/// navigation live here, not in the UI. When a save fails, leaving asks before the unsaved
/// progress is lost. Copy this shape for your game screens.
/// </summary>
public partial class ExampleLevel : Control, ILevelObserver, IExampleUIObserver
{
    private readonly IGameLevel _gameLevel;
    private readonly ExampleUIFactory _exampleUiFactory;

    private ExampleUI _ui = null!;
    private ConfirmationDialog _exitWithoutSaving = null!;

    public ExampleLevel(IGameLevel gameLevel, ExampleUIFactory exampleUiFactory)
    {
        _gameLevel = gameLevel;
        _exampleUiFactory = exampleUiFactory;

        ProcessMode = ProcessModeEnum.Always;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Ready()
    {
        _ui = _exampleUiFactory.Create();
        _ui.AddObserver(this);   // subscribe before adding to the tree
        AddChild(_ui);

        if (_gameLevel.IsLastSaveFailed)
        {
            _ui.ShowSaveResult(false);
        }

        _exitWithoutSaving = new ConfirmationDialog
        {
            Title = "Could not save the game",
            DialogText = "The progress since the last save will be lost. Exit without saving?",
            OkButtonText = "Exit without saving",
            CancelButtonText = "Stay",
        };
        _exitWithoutSaving.Confirmed += _gameLevel.LeaveToMenu;
        AddChild(_exitWithoutSaving);
    }

    public override void _Input(InputEvent @event)
    {
        // The exit dialog takes its own keys (Esc = stay, Enter = exit).
        if (_gameLevel.CurrentLevel != Enum.Level.Main || _exitWithoutSaving.Visible)
        {
            return;
        }

        if (@event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            ReturnToMenu();
        }
    }

    // Called by GameLevel after every save.
    public void ShowSaveResult(bool isSaved)
    {
        _ui.ShowSaveResult(isSaved);
    }

    // IExampleUIObserver — saving and navigation are decided here.
    public void OnSaveRequested()
    {
        _gameLevel.SaveGame();
    }

    public void OnMenuRequested()
    {
        ReturnToMenu();
    }

    // Saves and leaves; if the save failed, stays and asks before losing progress.
    private void ReturnToMenu()
    {
        if (!_gameLevel.ReturnToMenu())
        {
            _exitWithoutSaving.PopupCentered();
        }
    }

    // ILevelObserver — show this screen only on the Main level.
    public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
    {
        Visible = newLevel == Enum.Level.Main;
    }
}
