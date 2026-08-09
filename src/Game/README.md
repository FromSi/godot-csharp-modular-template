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

`Common` knows nothing about modules or `Level`. Modules only know about `Common`.
`Level` knows about everyone — because its whole job is to connect them. This
direction keeps the architecture from "bleeding" and makes testing easy.

## Structure

```
src/Game/
  Common/     # foundation: infrastructure not tied to any game
  Level/      # composition root + level screens
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
   (repositories, services, factories); in `_Ready` it creates the screens and
   subscribes them to level changes.
3. From there the game lives in switching levels via observers.

The full control flow is in [Level/README.md](Level/README.md).

## Adding a new feature (in short)

1. Copy the [Example/](Example) folder to `src/Game/<YourFeature>/` and rename the classes.
2. Describe the state (`Domain`), the logic (`Service` via an interface), the screen
   (`UI` + `UI/Factory`).
3. Wire it up in [Level/GameLevel.cs](Level/GameLevel.cs); if the state must be
   saved, register its type in `JsonConverter`.
4. Add a level-observer screen (following `ExampleLevel`) to show the screen.
5. Cover the `Service` with tests (it doesn't depend on Godot) — see
   [tests/Game.Tests](../../tests/Game.Tests).

## Principles worth keeping

- **One feature — one module folder.** Don't smear logic across the project.
- **Logic in `Service`, data in `Domain`/repository, presentation in `UI`.**
- **Depend on `Common` interfaces, not implementations** — that keeps code testable.
- **All wiring lives only in `Level`.** Modules don't create each other.
