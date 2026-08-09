using System.Collections.Generic;
using Game.Game.Common.Domain;
using Game.Game.Common.Repository;
using Game.Game.Common.Service;
using Game.Game.Common.Service.FileHandler;
using Game.Game.Common.Service.JsonConverter;
using Game.Game.Example.Domain;
using Game.Game.Example.Service;
using Game.Game.Example.UI.Factory;
using Game.Game.Level.Observer;
using Godot;

namespace Game.Game.Level;

/// <summary>
/// Root node of the main scene and the composition root of the project.
/// Everything is wired here by hand: shared infrastructure from <c>Common</c>,
/// per-module services/factories, and the level screens that observe level changes.
///
/// This is the file you edit first when starting a new project on this template:
/// register your services in the constructor, add your level screens in <see cref="_Ready"/>.
/// </summary>
public partial class GameLevel : Node, IGameLevel
{
    public Enum.Level CurrentLevel { get; private set; } = Enum.Level.Main;
    public Enum.Level PreviousLevel { get; private set; } = Enum.Level.Main;

    private readonly List<ILevelObserver> _observers = [];

    // --- Shared infrastructure (Common). Reusable across any project. ---
    private readonly IOsService _osService;
    private readonly IRandomGeneratorService _randomGeneratorService;
    private readonly IJsonConverterStateService _jsonConverterStateService;
    private readonly IJsonStateFileHandlerService _fileHandlerService;

    private readonly ISingleRepository<IdState> _idRepository;
    private readonly IIdService _idService;

    // Central persistence & reset: own every repository that takes part in a save / new game.
    private const string SavePath = "user://saves/game.save";
    private readonly SaveService _saveService;
    private readonly NewGameService _newGameService;

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
        _jsonConverterStateService.Register(typeof(IdState));
        _jsonConverterStateService.Register(typeof(NoteState));

        _fileHandlerService = new JsonStateFileHandlerService(
            _osService,
            fileSystemService,
            _jsonConverterStateService
        );

        _randomGeneratorService = new RandomGeneratorService();

        _idRepository = new SingleRepository<IdState>();
        _idRepository.Update(new IdState());
        _idService = new IdService(_idRepository);

        // Per-module wiring order: State → Repository → Service → Factory.
        _noteRepository = new SingleRepository<NoteState>();
        _noteRepository.Update(new NoteState());
        _noteService = new NoteService(_noteRepository, _randomGeneratorService);

        _saveService = new SaveService(SavePath, _fileHandlerService, _idRepository, _noteRepository);
        _newGameService = new NewGameService(_idRepository, _noteRepository);

        _exampleUiFactory = new ExampleUIFactory(_noteService, _saveService, _newGameService);
    }

    public override void _Ready()
    {
        var canvasLayer = new CanvasLayer { Layer = 99 };
        AddChild(canvasLayer);

        var quitLevel = new QuitLevel(this);
        _observers.Add(quitLevel);
        canvasLayer.AddChild(quitLevel);

        var exampleLevel = new ExampleLevel(this, _exampleUiFactory);
        _observers.Add(exampleLevel);
        canvasLayer.AddChild(exampleLevel);

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
