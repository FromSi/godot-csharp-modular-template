# Common — reusable infrastructure

The foundation of the project: generic, game-agnostic building blocks that feature
modules are built on — state storage, id generation, random numbers, file handling
and serialization.

> How `Common` relates to modules and `Level`, and why dependencies point at it —
> see [../README.md](../README.md). This document covers only the contents of `Common`.

Almost everything is described through interfaces (`ISingleRepository`, `IIdService`,
`IJsonStateFileHandlerService`, …), so tests can swap implementations for mocks
(example — [SaveServiceTests](../../../tests/Game.Tests/Common/Service/SaveServiceTests.cs)).
One rule: **there is no game-specific logic in `Common`** — only generic mechanisms
that could be dropped into another project unchanged.

## Structure

```
Common/
  Domain/                 # shared value objects and states
    IdState.cs            #   counter for generating unique ids
    Color.cs              #   a color that doesn't depend on Godot
    Observer/             #   generic observer base for states
      IObservableState.cs / ObservableState.cs
  Enum/
    FileError.cs          #   result codes for file operations
  Repository/             # in-memory state storage
    ISingleRepository.cs / SingleRepository.cs         # a single state
    ICollectionRepository.cs / CollectionRepository.cs # a keyed collection
    IRepositoryIndex.cs / RepositoryIndex.cs           # secondary indexes
  Service/                # reusable services
    IIdService.cs / IdService.cs                # hands out the next id
    IRandomGeneratorService.cs / RandomGeneratorService.cs  # random numbers
    IOsService.cs / OsService.cs               # production/debug flag
    IClockService.cs / ClockService.cs         # current time
    IStateSlot.cs / StateSlot.cs  # one persistent state: save, restore, reset
    SaveService.cs        #   central save/load of all slots
    NewGameService.cs     #   reset all slots to a fresh game
    FileHandler/          #   reading/writing files (low-level)
    JsonConverter/        #   serializing states to JSON
  UI/                     # shared Godot widgets (UI layer), created on first need
```

## UI — shared widgets (`UI/`)

Godot widgets reused by several module UIs live here (the folder is empty in the template). It is a UI layer: Godot API is
allowed, game logic is not, and only `UI` code and `Level` screens may use it.

Move a widget here once a second module needs it; until then keep it in the module's `UI`.

## Repository — state storage

A repository is an in-memory store of state objects. Modules don't keep data in
their own fields — they put it in a repository, so state is easy to fetch, update
and save to disk.

### SingleRepository — a single state
For entities there is exactly one of: settings, progress, the active camera.

```csharp
var repo = new SingleRepository<IdState>();
repo.Update(new IdState { Counter = 1 });
var state = repo.GetOne();   // throws if not initialized
```

### CollectionRepository — a keyed collection
For many entities of one type (units, items, planets) — a `key → state` dictionary:

```csharp
var repo = new CollectionRepository<int, ItemState>();
repo.Update(item.Id, item);
var all = repo.GetAll();
```

### RepositoryIndex — secondary indexes
When you need to look up not by the primary key but by another field (e.g. all items
of a given player). An index supports that lookup and **updates itself** on every
`Update`/`DeleteAll`:

```csharp
var byOwner = repo.AddIndex(item => item.OwnerId); // index "items by owner"
repo.Update(item.Id, item);                        // the index updates automatically
var playerItems = byOwner.GetAll(ownerId);
```

## Service — reusable services

### IdService — [Service/IdService.cs](Service/IdService.cs)
Hands out the next unique `int` id (via `IdState` in a repository). Needed when you
create entities and assign them identifiers.

### RandomGeneratorService — [Service/RandomGeneratorService.cs](Service/RandomGeneratorService.cs)
Random numbers: `RandiRange(from, to)` and `SetSeed(seed)`. Behind an interface, so
it's easy to mock or fix the seed for reproducibility in tests.

### OsService — [Service/OsService.cs](Service/OsService.cs)
Answers a single question — `IsProduction()` — and is the one `Common/Service` class
that intentionally touches the Godot API (hidden behind `IOsService` so the rest stays
engine-free). It reports production via `!OS.HasFeature("debug")`: the `debug` feature
is present when running from the editor (F5) and in debug exports, and absent only in a
**release export**. The file handler uses this to keep saves as plain, readable JSON
during development and encrypt them only in shipped release builds.

### ClockService — [Service/ClockService.cs](Service/ClockService.cs)
Current time: `Now`. Behind `IClockService`, so time-stamping logic can be tested with a
fixed or advancing clock (`clock.Setup(c => c.Now).Returns(...)`). Services never call
`DateTime.Now` directly. Live usage — [NoteService](../Example/Service/NoteService.cs) stamps
each change of the note.

### FileHandler — the save system ([Service/FileHandler/](Service/FileHandler))
Reads and writes files. The key class is `JsonStateFileHandlerService`: it takes a
list of states, turns them into JSON and writes them to a file (and back).

```csharp
_fileHandler.Store([state], "user://saves/game.save");        // save
var s = _fileHandler.Load(path).OfType<IdState>().First();    // load
```

What matters when saving:
- `user://` is the OS-specific writable app folder.
- Every saved type must be **registered** in `JsonConverter`
  (`Register(typeof(...))`), otherwise the serializer silently skips it.
  `GameLevel` registers every state slot's type automatically — don't register by hand.
- In production the file is encrypted; in debug it's readable JSON (decided by `OsService`).
- The details (creating directories, encryption, `Godot.FileAccess`) are hidden
  behind the `IFileSystemService` / `IFileAccess` interfaces — which can be mocked too.

`Exists(path)` answers "is there a save?" without reading it. Live usage —
[SaveService](Service/SaveService.cs).

### SaveService / NewGameService — state slots ([Service/SaveService.cs](Service/SaveService.cs), [Service/NewGameService.cs](Service/NewGameService.cs))
Every persistent state is an [IStateSlot](Service/IStateSlot.cs) — usually a
[StateSlot&lt;T&gt;](Service/StateSlot.cs) over a single-state repository plus a factory for the
fresh state. `GameLevel` lists the slots once; from that list:

- `SaveService.Save()` writes every slot's state to one file (slot order = file order);
- `SaveService.Load()` restores them — only if the file has exactly one state of the right
  type per slot, otherwise nothing changes (no half-loaded game); `HasSave()` drives the
  menu's Continue;
- `NewGameService.Create()` resets every slot to its fresh state (empty, or seeded);
- each slot's `StateType` is registered in the JSON converter.

```csharp
var slots = new List<IStateSlot>
{
    new StateSlot<IdState>(_idRepository, () => new IdState()),
    new StateSlot<InventoryState>(_inventoryRepository, InventorySeed.Create),
};
```

Repositories start empty; `GameLevel` calls `NewGameService.Create()` once after building
the slots, so every repository has a state before anything reads it. `Common` never
references concrete module states, and module services do no file I/O.

A save written before a slot was added or removed no longer matches and is rejected as a
whole (Continue does nothing). Keep the list append-only once players have saves, or
version the save path.

### JsonConverter — serialization ([Service/JsonConverter/](Service/JsonConverter))
`JsonConverterStateService` turns state objects into JSON and back (see the `Register`
rule above). The custom `Vector2JsonConverter` is an example of teaching the
serializer a non-standard type.

## Domain — shared data

- **[Domain/IdState.cs](Domain/IdState.cs)** — the id counter state (used by `IdService`).
- **[Domain/Color.cs](Domain/Color.cs)** — a color as a pure C# struct, not tied to
  `Godot.Color`. An example value object that can be serialized and tested outside
  the engine.

### Observer — state → UI reactivity ([Domain/Observer/](Domain/Observer))
`ObservableState<TObserver>` is the publisher side of the observer pattern: a state
extends it, exposes a specific observer interface, and calls `Notify(...)` when it
changes. Listeners (usually the UI) subscribe via `AddObserver` and react instead of
polling.

```csharp
public class ScoreState : ObservableState<IScoreStateObserver>
{
    public int Score { get; set; }
    public void Add(int points) { Score += points; Notify(o => o.OnScoreChanged(Score)); }
}
```

The observer list is a private field, so an observable state is still a plain saved
POCO (it serializes fine). Mutate it through a method that calls `Notify(...)`. Note
that `SaveService.Load` / `NewGameService.Create` **replace** the state object, so a
subscriber is bound to one game session: the game screen is built after them and freed on
the way back to the menu (see [../Level/README.md](../Level/README.md)). Live example:
[NoteState](../Example/Domain/NoteState.cs).
