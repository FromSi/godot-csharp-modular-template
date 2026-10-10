using Game.Game.Common.Domain;
using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Common.Service.FileHandler;
using Game.Game.Common.Service.JsonConverter;
using Game.Game.Example.Domain;
using Game.Game.Example.Service;
using Game.Game.Example.UI.Factory;
using Game.Game.Level.Adapter;
using Game.Game.Level.Observer;
using Game.Game.MainMenu.UI.Factory;
using Game.Game.Settings.Domain;
using Game.Game.Settings.Service;
using Game.Game.Settings.UI;
using Game.Game.Settings.UI.Factory;
using System.Collections.Generic;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// Root node of the main scene and the composition root of the project.
/// Everything is wired here by hand: shared infrastructure from <c>Common</c>,
/// per-module services/factories, and the level screens that observe level changes.
///
/// The game starts on the main menu. The game screen is built when the player continues a save
/// or starts a new game (so its UIs subscribe to the loaded / fresh states) and dropped when they
/// return to the menu. Every persistent state is an <see cref="IStateSlot"/>: listed once, it is
/// saved, loaded, reset on a new game and registered for JSON automatically. A failed save is
/// logged and shown to the player (<see cref="IsLastSaveFailed"/>). Settings live in a file of
/// their own, so New Game never resets them.
/// </summary>
public partial class GameLevel : Node, IGameLevel
{
    public Enum.Level CurrentLevel { get; private set; } = Enum.Level.MainMenu;
    public Enum.Level PreviousLevel { get; private set; } = Enum.Level.MainMenu;

    private readonly List<ILevelObserver> _observers = [];

    // --- Shared infrastructure (Common). Reusable across any project. ---
    private readonly IOsService _osService;
    private readonly IRandomGeneratorService _randomGeneratorService;
    private readonly IClockService _clockService;
    private readonly IJsonConverterStateService _jsonConverterStateService;
    private readonly IJsonStateFileHandlerService _fileHandlerService;

    private readonly ISingleRepository<IdState> _idRepository;
    private readonly IIdService _idService;

    // Central persistence & reset over every state slot.
    private const string SavePath = "user://saves/game.save";
    private readonly SaveService _saveService;
    private readonly NewGameService _newGameService;
    private readonly MainMenuUIFactory _mainMenuUiFactory;

    // --- Settings module: player settings in their own file (not reset by New Game). ---
    private const string SettingsPath = "user://settings.json";
    private readonly ISingleRepository<SettingsState> _settingsRepository;
    private readonly ISettingsService _settingsService;
    private readonly SettingsUIFactory _settingsUiFactory;

    private CanvasLayer _canvasLayer = null!;
    private ExampleLevel? _gameScreen;

    // --- Example module. A self-contained feature; delete or copy as a starting point. ---
    private readonly ISingleRepository<NoteState> _noteRepository;
    private readonly INoteService _noteService;
    private readonly ExampleUIFactory _exampleUiFactory;

    public GameLevel()
    {
        ProcessMode = ProcessModeEnum.Always;

        _osService = new OsService();
        var fileSystemService = new GodotFileSystemService();

        _jsonConverterStateService = new JsonConverterStateService();

        _fileHandlerService = new JsonStateFileHandlerService(
            _osService,
            fileSystemService,
            _jsonConverterStateService
        );

        _randomGeneratorService = new RandomGeneratorService();
        _clockService = new ClockService();

        // Repositories start empty; NewGameService fills them (see the end of the constructor).
        _idRepository = new SingleRepository<IdState>();
        _idService = new IdService(_idRepository);

        // Per-module wiring order: State → Repository → Service → Factory.
        _noteRepository = new SingleRepository<NoteState>();
        _noteService = new NoteService(_noteRepository, _randomGeneratorService, _clockService);

        // Everything that is saved: one slot per state (order = save file order).
        var slots = new List<IStateSlot>
        {
            new StateSlot<IdState>(_idRepository, () => new IdState()),
            new StateSlot<NoteState>(_noteRepository, () => new NoteState()),
        };

        foreach (var slot in slots)
        {
            _jsonConverterStateService.Register(slot.StateType);
        }

        _saveService = new SaveService(SavePath, _fileHandlerService, slots);
        _newGameService = new NewGameService(slots);
        _newGameService.Create();

        // Settings: one slot in a file of its own, loaded once (fresh when there is none yet).
        _settingsRepository = new SingleRepository<SettingsState>();
        var settingsSlot = new StateSlot<SettingsState>(_settingsRepository, () => new SettingsState());
        _jsonConverterStateService.Register(settingsSlot.StateType);
        var settingsSaveService = new SaveService(SettingsPath, _fileHandlerService, [settingsSlot]);

        if (!settingsSaveService.Load())
        {
            settingsSlot.Reset();
        }

        _settingsService = new SettingsService(_settingsRepository, new SettingsFileStore(settingsSaveService));
        _settingsUiFactory = new SettingsUIFactory(_settingsService);

        _mainMenuUiFactory = new MainMenuUIFactory(_saveService);
        _exampleUiFactory = new ExampleUIFactory(_noteService);
    }

    public override void _Ready()
    {
        GameWindow.Apply(GetWindow(), _settingsService);
        AddChild(_settingsUiFactory.CreateDisplayModeHotkey());

        _canvasLayer = new CanvasLayer { Layer = 99 };
        AddChild(_canvasLayer);

        var quitLevel = new QuitLevel(this);
        _observers.Add(quitLevel);
        _canvasLayer.AddChild(quitLevel);

        var mainMenuLevel = new MainMenuLevel(this, _mainMenuUiFactory, _settingsUiFactory);
        _observers.Add(mainMenuLevel);
        _canvasLayer.AddChild(mainMenuLevel);

        OpenLevel(Enum.Level.MainMenu);
    }

    public bool ContinueGame()
    {
        if (!_saveService.Load())
        {
            return false;
        }

        EnterGame();

        return true;
    }

    public void StartNewGame()
    {
        _newGameService.Create();
        EnterGame();
    }

    public bool IsLastSaveFailed { get; private set; }

    public bool SaveGame()
    {
        var isSaved = _saveService.Save();

        IsLastSaveFailed = !isSaved;
        _gameScreen?.ShowSaveResult(isSaved);

        if (!isSaved)
        {
            GD.PushError($"Could not write the save file {SavePath}");
        }

        return isSaved;
    }

    public bool ReturnToMenu()
    {
        if (!SaveGame())
        {
            return false;
        }

        LeaveToMenu();

        return true;
    }

    public void LeaveToMenu()
    {
        OpenLevel(Enum.Level.MainMenu);

        if (_gameScreen != null)
        {
            _observers.Remove(_gameScreen);
            _gameScreen.QueueFree();
            _gameScreen = null;
        }
    }

    // Closing the window while playing keeps the progress (a failure is only logged — the game is closing).
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest && CurrentLevel == Enum.Level.Main)
        {
            SaveGame();
        }
    }

    // Built on every entry, after the states were loaded / reset, so its UIs subscribe to the
    // current state objects (Load / New Game replace them).
    private void EnterGame()
    {
        _gameScreen = new ExampleLevel(this, _exampleUiFactory);
        _observers.Add(_gameScreen);
        _canvasLayer.AddChild(_gameScreen);

        OpenLevel(Enum.Level.Main);
    }

    public void OpenLevel(Enum.Level level)
    {
        var oldLevel = CurrentLevel;

        CurrentLevel = level;
        PreviousLevel = oldLevel;

        Notify(level, oldLevel);
    }

    public void OpenPreviousLevel()
    {
        var newLevel = PreviousLevel;
        var oldLevel = CurrentLevel;

        CurrentLevel = newLevel;
        PreviousLevel = oldLevel;

        Notify(newLevel, oldLevel);
    }

    private void Notify(Enum.Level newLevel, Enum.Level oldLevel)
    {
        foreach (var observer in _observers)
        {
            observer.OnLevelOppened(newLevel, oldLevel);
        }
    }
}
