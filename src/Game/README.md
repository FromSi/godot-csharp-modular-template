# src/Game — architecture overview

This is the root of all C# code in the project. It hosts a **modular architecture**:
the game is assembled from independent feature modules that sit on a shared foundation
and are wired together in one place. The goal is to let you add a new feature by
following a ready-made pattern — without touching the others or reinventing the
structure each time.

Start here, then read the README of a specific layer:
- [Common/README.md](Common/README.md) — reusable infrastructure (the foundation).
- [Level/README.md](Level/README.md) — game assembly and screen switching.
- [Example/README.md](Example/README.md) — a sample feature module.
- [MainMenu/README.md](MainMenu/README.md) — the start menu (Continue / New Game / Settings / Quit).
- [Settings/README.md](Settings/README.md) — player settings: display mode and resolution.

## Three roles

Everything splits into three parts with distinct responsibilities:

```
Common   — the tools        : storage, ids, randomness, files, serialization
Modules  — the parts        : Example and your future features (one folder each)
Level    — the assembly shop : creates everything and decides which screen is active
```

Dependencies point strictly one way — toward the foundation:

```
Level ─────► Modules ─────► Common
(assembles)  (use it)       (knows nobody)
```

`Common` knows nothing about modules or `Level`. Modules only know about `Common`;
if one needs another's data, it declares an interface and `Level` adapts (see
[Between modules](#between-modules)).
`Level` knows about everyone — because its whole job is to connect them. This
direction keeps the architecture from "bleeding" and makes testing easy.

## Structure

```
src/Game/
  Common/     # foundation: infrastructure not tied to any game
  Level/      # composition root + level screens + cross-module adapters
  MainMenu/   # start menu UI (Continue / New Game / Settings / Quit)
  Settings/   # player settings: windowed / fullscreen, resolution (own file)
  Example/    # a sample feature module (template to copy)
  <YourFeature>/   # your modules go here
```

### Anatomy of a module

Every feature module is split into the same layers — top to bottom by dependency:

```
<Feature>/
  Domain/         # state data (no logic, no Godot)
  Service/        # feature logic via interfaces (no Godot → testable)
  UI/             # Godot nodes: draw and call the service
    Factory/      # builds the UI with dependency injection
```

Layer rule: **UI → Service → Domain/Common**. UI never touches the repository
directly, and logic never knows about Godot nodes. A detailed walkthrough is in
[Example/README.md](Example/README.md).

## Namespaces

The namespace mirrors the folder path with a `Game.Game` prefix:

```
src/Game/Example/Service/NoteService.cs   →   namespace Game.Game.Example.Service
src/Game/Common/Repository/...            →   namespace Game.Game.Common.Repository
```

(the double `Game` is `<assembly>.<src/Game folder>`; the assembly is named `Game`,
see `project.godot` → `[dotnet] project/assembly_name`).

## How it boots

1. Godot opens the main scene [../../level/game_level.tscn](../../level/game_level.tscn),
   whose root node is `GameLevel`.
2. In its constructor `GameLevel` creates the `Common` infrastructure and all modules
   (repositories, services, factories), lists the persistent states as slots and fills
   them via `NewGameService.Create()`; in `_Ready` it creates the menu and quit screens.
3. Continue / New Game loads or resets the states, *then* builds the game screen; Esc
   saves and returns to the menu, dropping the game screen.

The full control flow is in [Level/README.md](Level/README.md).

## Entities and ids

Everything the player can tell apart (an item, a unit, an order, a catalog entry, an
option of that entry) is an **entity with an `int Id`**, handed out by `IIdService`.

A **state** is not an entity. `NoteState`, `InventoryState` — one per game, kept in a
`SingleRepository`, identified by its type (the save file tags it with `_cls`); nobody
refers to it, so it has no `Id`. Entities are the items inside states
(`InventoryState.Items[i]`). If a singleton later becomes a collection, its elements get
ids then.

- **References go by id.** An entity that points at another stores the id
  (`ProductId`, `PlanetId`, an option's `Id`), never the object — states stay flat POCOs
  that serialize and load cleanly.
- **What it shows is a snapshot.** Next to the id, keep the values shown or charged at
  that moment (`ProductName`, `ProductPrice`). The source can change later; the snapshot
  records what was true then.
- **Checks look up by id, then compare the snapshot.** "Not found by id" and "found, but
  the name/price differs" are different problems — report them separately:

  ```csharp
  var product = catalog.FirstOrDefault(p => p.Id == item.ProductId);
  if (product == null)                       { /* referenced entity is gone */ }
  else if (product.Name != item.ProductName) { /* snapshot is stale or forged */ }
  ```

  An optional reference is a nullable id (`int? PromoCodeId`).
- **An id is not a display number.** If the player sees "Order #12" or "Planet 3", that is
  its own field (`Number`), with its own rules; don't reuse `Id` for it.

## Between modules

Modules never reference each other. When one module (the consumer) needs another's data:

1. The consumer declares an interface **in its own `Service`** in its own terms, e.g.
   `Shop/Service/IShopCatalog.cs` returning the consumer's DTOs.
2. An **adapter** in `Level/Adapter/` implements it over the other module's service
   (`IInventoryService`) and maps the types. Only `Level` knows both modules.
3. `GameLevel` passes the adapter to the consumer's service constructor.

Details — [Level/README.md](Level/README.md).

## Adding a new feature (in short)

1. Copy the [Example/](Example) folder to `src/Game/<YourFeature>/` and rename the classes.
2. Describe the state (`Domain`), the logic (`Service` via an interface), the screen
   (`UI` + `UI/Factory`).
3. Wire it up in [Level/GameLevel.cs](Level/GameLevel.cs); if the state must be
   saved, add a `StateSlot<T>` for it to the `slots` list (that's all persistence needs).
4. Host its UI in the game screen (following `ExampleLevel`).
5. Need another module's data? Interface in your `Service` + adapter in `Level/Adapter/`.
6. Cover the `Service` with tests (it doesn't depend on Godot) — see
   [tests/Game.Tests](../../tests/Game.Tests).

## Principles worth keeping

- **One feature — one module folder.** Don't smear logic across the project.
- **Logic in `Service`, data in `Domain`/repository, presentation in `UI`.**
- **Depend on `Common` interfaces, not implementations** — that keeps code testable.
- **All wiring lives only in `Level`.** Modules don't create or reference each other.
- **Entities have ids; references are ids + snapshots.**
