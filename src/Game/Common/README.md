# Common — reusable infrastructure

The foundation of the project: generic, game-agnostic building blocks that feature
modules are built on — state storage, id generation, random numbers, file handling
and serialization.

> How `Common` relates to modules and `Level`, and why dependencies point at it —
> see [../README.md](../README.md). This document covers only the contents of `Common`.

Almost everything is described through interfaces (`ISingleRepository`, `IIdService`,
`IJsonStateFileHandlerService`, …), so tests can swap implementations for mocks
(example — [NoteServiceTests](../../../tests/Game.Tests/Example/Service/NoteServiceTests.cs)).
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
    SaveService.cs        #   central save/load of all repositories
    NewGameService.cs     #   reset all repositories to a fresh game
    FileHandler/          #   reading/writing files (low-level)
    JsonConverter/        #   serializing states to JSON
```

## Repository — state storage

A repository is an in-memory store of state objects. Modules don't keep data in
their own fields — they put it in a repository, so state is easy to fetch, update
and save to disk.

### SingleRepository — a single state
For entities there is exactly one of: settings, progress, the active camera.

```csharp
var repo = new SingleRepository<NoteState>();
repo.Update(new NoteState { Text = "hi" });
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

### FileHandler — the save system ([Service/FileHandler/](Service/FileHandler))
Reads and writes files. The key class is `JsonStateFileHandlerService`: it takes a
list of states, turns them into JSON and writes them to a file (and back).

```csharp
_fileHandler.Store([state], "user://saves/game.save");        // save
var s = _fileHandler.Load(path).OfType<NoteState>().First();  // load
```

What matters when saving:
- `user://` is the OS-specific writable app folder.
- Every saved type must be **registered** in `JsonConverter`
  (`Register(typeof(...))`), otherwise the serializer won't recognize it and skips it.
  Registration happens once in [../Level/GameLevel.cs](../Level/GameLevel.cs).
- In production the file is encrypted; in debug it's readable JSON (decided by `OsService`).
- The details (creating directories, encryption, `Godot.FileAccess`) are hidden
  behind the `IFileSystemService` / `IFileAccess` interfaces — which can be mocked too.

A live usage example — [NoteService](../Example/Service/NoteService.cs).

### SaveService — central save/load ([Service/SaveService.cs](Service/SaveService.cs))
The single place that persists the game. It holds every repository that takes part in
a save and writes them to one file (and restores them). Module services do **not** do
file I/O — they own in-memory state; `SaveService` owns saving.

```csharp
public void Save()
{
    var data = new List<object?> { _idRepository.GetOne(), _noteRepository.GetOne() };
    _fileHandlerService.Store(data, _savePath);
}
```

`Save` builds a `List<object?>` (single state → `GetOne()`, collection → `GetAll()`);
`Load` reads it back and, per slot, `Delete()`+`Update()` (single) or `DeleteAll()`+loop
(collection). The list order in `Save` must match the positional casts in `Load`.

When you add a persistable module, add its repository here and register its type in
`JsonConverter` (in [../Level/GameLevel.cs](../Level/GameLevel.cs)). This is the one
place in `Common` that references concrete module states — a deliberate trade-off for a
single, explicit save file.

### NewGameService — reset to a fresh game ([Service/NewGameService.cs](Service/NewGameService.cs))
The counterpart of `SaveService`: `Create()` resets every repository to its initial
state and seeds a new game's starting content (single → `Delete()`+`Update(new ...)`,
collection → `DeleteAll()`+`UpdateAll(...)`). Like `SaveService`, it references concrete
module states on purpose — it must know what a fresh game looks like. Add each new
persistable module here too.

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
public class NoteState : ObservableState<INoteStateObserver>
{
    public string Text { get; set; } = "";
    public void ChangeText(string text) { Text = text; Notify(o => o.OnTextChanged(text)); }
}
```

The observer list is a private field, so an observable state is still a plain saved
POCO (it serializes fine). Mutate it through a method that calls `Notify(...)`. Note
that `SaveService.Load` **replaces** the state object, so subscribers must re-subscribe
to the fresh state after a load — see `SubscribeAndRender` in
[ExampleUI](../Example/UI/ExampleUI.cs). Live example:
[NoteState](../Example/Domain/NoteState.cs).
