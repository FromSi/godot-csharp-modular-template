using Game.Game.Level.Observer;
using Game.Game.MainMenu.UI;
using Game.Game.MainMenu.UI.Factory;
using Game.Game.MainMenu.UI.Observer;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// The start screen: hosts the main menu and turns its choices into game actions —
/// Continue loads the save, New Game resets everything, Quit exits. Shown only on the
/// MainMenu level; lives for the whole session.
/// </summary>
public partial class MainMenuLevel : Control, ILevelObserver, IMainMenuUIObserver
{
    private readonly IGameLevel _gameLevel;
    private readonly MainMenuUIFactory _mainMenuUiFactory;

    private MainMenuUI? _menu;

    public MainMenuLevel(IGameLevel gameLevel, MainMenuUIFactory mainMenuUiFactory)
    {
        _gameLevel = gameLevel;
        _mainMenuUiFactory = mainMenuUiFactory;

        ProcessMode = ProcessModeEnum.Always;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Ready()
    {
        _menu = _mainMenuUiFactory.Create();
        _menu.AddObserver(this);   // subscribe before adding to the tree
        AddChild(_menu);
    }

    // IMainMenuUIObserver
    public void OnContinuePressed()
    {
        _gameLevel.ContinueGame();
    }

    public void OnNewGamePressed()
    {
        _gameLevel.StartNewGame();
    }

    public void OnQuitPressed()
    {
        _gameLevel.OpenLevel(Enum.Level.Quit);
    }

    // ILevelObserver — show this screen only on the MainMenu level.
    public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
    {
        Visible = newLevel == Enum.Level.MainMenu;

        if (Visible && IsNodeReady())
        {
            _menu?.RefreshContinue();
        }
    }
}
