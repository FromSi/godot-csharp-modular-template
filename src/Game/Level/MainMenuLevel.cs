using Game.Game.Level.Observer;
using Game.Game.MainMenu.UI;
using Game.Game.MainMenu.UI.Factory;
using Game.Game.MainMenu.UI.Observer;
using Game.Game.Settings.UI;
using Game.Game.Settings.UI.Factory;
using Game.Game.Settings.UI.Observer;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// The start screen: hosts the main menu and turns its choices into game actions —
/// Continue loads the save, New Game resets everything, Settings opens the settings over the
/// menu, Quit exits. Shown only on the MainMenu level; lives for the whole session.
/// </summary>
public partial class MainMenuLevel : Control, ILevelObserver, IMainMenuUIObserver, ISettingsUIObserver
{
    private readonly IGameLevel _gameLevel;
    private readonly MainMenuUIFactory _mainMenuUiFactory;
    private readonly SettingsUIFactory _settingsUiFactory;

    private MainMenuUI? _menu;
    private SettingsUI? _settings;

    public MainMenuLevel(
        IGameLevel gameLevel,
        MainMenuUIFactory mainMenuUiFactory,
        SettingsUIFactory settingsUiFactory
    )
    {
        _gameLevel = gameLevel;
        _mainMenuUiFactory = mainMenuUiFactory;
        _settingsUiFactory = settingsUiFactory;

        ProcessMode = ProcessModeEnum.Always;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
    }

    public override void _Ready()
    {
        _menu = _mainMenuUiFactory.Create();
        _menu.AddObserver(this);   // subscribe before adding to the tree
        AddChild(_menu);

        _settings = _settingsUiFactory.Create();
        _settings.AddObserver(this);
        _settings.Visible = false;
        AddChild(_settings);
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

    public void OnSettingsPressed()
    {
        _settings!.Refresh();
        _settings.Visible = true;
    }

    public void OnQuitPressed()
    {
        _gameLevel.OpenLevel(Enum.Level.Quit);
    }

    // ISettingsUIObserver
    public void OnSettingsClosed()
    {
        _settings!.Visible = false;
    }

    // ILevelObserver — show this screen only on the MainMenu level.
    public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
    {
        Visible = newLevel == Enum.Level.MainMenu;

        if (Visible && IsNodeReady())
        {
            _menu?.RefreshContinue();
            _menu?.ShowSaveFailed(_gameLevel.IsLastSaveFailed);
        }
    }
}
