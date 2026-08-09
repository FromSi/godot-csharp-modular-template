# Level — levels and the composition root

`Level` does two things: it **assembles the game** (creates services, repositories
and module factories in one place) and it **manages screens** (holds the current
level and notifies screens when it changes). Other modules don't start themselves —
this component brings them to life.

> The overall role of `Level` in the architecture and the dependency direction are in
> [../README.md](../README.md). This document covers how the component itself works.

## What it consists of

### 1. The list of levels — [Enum/Level.cs](Enum/Level.cs)
A simple enum of all game states. Add a screen — add a value.

```csharp
public enum Level { Main, Quit }
```

### 2. The composition root — [GameLevel.cs](GameLevel.cs)
The root `Node` of the main scene ([../../../level/game_level.tscn](../../../level/game_level.tscn)):

- **in the constructor** it creates all of the `Common` infrastructure and the
  modules (repositories, services, factories) — the one and only place the game is
  "assembled";
- **in `_Ready`** it creates the screens, subscribes them to level changes and adds
  them to the tree.

Switching happens via `OpenLevel(...)` / `OpenPreviousLevel()`: they change
`CurrentLevel` and notify observers.

```csharp
public void OpenLevel(Enum.Level level)
{
    var oldLevel = CurrentLevel;
    CurrentLevel = level;
    PreviousLevel = oldLevel;
    Notify(level, oldLevel); // pokes every ILevelObserver
}
```

### 3. The root contract — [IGameLevel.cs](IGameLevel.cs)
The interface through which screens "talk" to the root: they read the current level
and ask to switch. Screens depend on the interface, not on the concrete `GameLevel` —
which makes them easier to test and reuse.

### 4. The observer — [Observer/ILevelObserver.cs](Observer/ILevelObserver.cs)
A single method, called on every level change. The screen decides what to do: show
itself, hide, free resources, etc.

```csharp
void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel);
```

### 5. Level screens
A screen is a `Node` (usually a `Control`) that implements `ILevelObserver`. The
template has two:

- **[QuitLevel.cs](QuitLevel.cs)** — a screen with no UI: when switching to `Quit` it
  closes the game. An example of a "logical" level.
- **[ExampleLevel.cs](ExampleLevel.cs)** — a screen with UI: shows/hides itself based
  on the current level, hosts the [Example](../Example/README.md) module's screen via
  its factory, and **listens to that UI** (`IExampleUIObserver`) to handle navigation
  intents like Quit. This is the UI → Level direction: the UI reports intent, the level
  decides where to go. An example of a "visual" level.

## Initialization order

Wiring follows a strict order so dependencies always exist before they're used.

Per module, in the `GameLevel` **constructor**: `State → Repository → Service → Factory`
(create the state, put it in a repository, hand the repository to the service, hand the
service to the factory). Central services (`SaveService`, `NewGameService`) are built
after the repositories they own.

Per screen, in `GameLevel._Ready` (and inside a screen's own `_Ready`):
`create → subscribe observers → AddChild`. Subscribe **before** adding to the tree so no
early event is missed. See `ExampleLevel._Ready`, which builds the UI via its factory,
subscribes to it, then adds it.

## Control flow

```
Godot launches game_level.tscn
        │
        ▼
GameLevel (constructor)  → assembles services/modules/factories
        │
        ▼
GameLevel._Ready()       → creates screens, subscribes them as ILevelObserver
        │                  and calls OpenLevel(Main)
        ▼
screen presses button/Esc → _gameLevel.OpenLevel(Quit)
        │
        ▼
GameLevel.Notify(...)    → OnLevelOppened on every observer
        │
        ├─ ExampleLevel: Visible = (newLevel == Main)
        └─ QuitLevel:    if (newLevel == Quit) GetTree().Quit()
```

## Adding your own screen

1. Add a value to [Enum/Level.cs](Enum/Level.cs) (e.g. `Menu`).
2. Create a screen class (a `Control` subclass implementing `ILevelObserver`); take
   `IGameLevel` in the constructor and, if it has UI, the module's factory. Reference —
   [ExampleLevel.cs](ExampleLevel.cs).
3. In `GameLevel._Ready()` create the screen, add it to `_observers` and to the tree.
4. Switch to it from anywhere: `_gameLevel.OpenLevel(Enum.Level.Menu)`.

> A screen is only about "when to show". The feature itself (data, logic, layout)
> lives in a separate module folder — see [Example](../Example/README.md) and
> [../README.md](../README.md).
