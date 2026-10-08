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
public enum Level { MainMenu, Main, Quit }
```

### 2. The composition root — [GameLevel.cs](GameLevel.cs)
The root `Node` of the main scene ([../../../level/game_level.tscn](../../../level/game_level.tscn)):

- **in the constructor** it creates all of the `Common` infrastructure and the
  modules (repositories, services, factories) — the one and only place the game is
  "assembled";
- **in `_Ready`** it creates the start screens (main menu, quit), subscribes them to
  level changes and opens `MainMenu`;
- **`ContinueGame()` / `StartNewGame()`** load the save or reset every state slot, then
  build the game screen and open `Main` — so its UIs subscribe to the loaded / fresh
  states; **`ReturnToMenu()`** (Esc) saves, opens `MainMenu` and frees the game screen (it
  is rebuilt on the next entry); **`SaveGame()`** writes the save — also from
  `_Notification` when the window is closed during play.

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
The interface through which screens "talk" to the root: they read the current level,
ask to switch, and enter / leave the game (`ContinueGame`, `StartNewGame`, `ReturnToMenu`). Screens depend on the interface, not on the concrete `GameLevel` —
which makes them easier to test and reuse.

### 4. The observer — [Observer/ILevelObserver.cs](Observer/ILevelObserver.cs)
A single method, called on every level change. The screen decides what to do: show
itself, hide, free resources, etc.

```csharp
void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel);
```

### 5. Level screens
A screen is a `Node` (usually a `Control`) that implements `ILevelObserver`. Two
lifetimes:

- **Session screens** — created once in `_Ready`, live until exit:
  - **[QuitLevel.cs](QuitLevel.cs)** — no UI: when switching to `Quit` it closes the game.
    An example of a "logical" level.
  - **[MainMenuLevel.cs](MainMenuLevel.cs)** — hosts the [MainMenu](../MainMenu/README.md)
    UI (Continue — inactive without a save — / New Game / Quit) and turns its intents into
    `ContinueGame()` / `StartNewGame()` / `OpenLevel(Quit)`.
- **Game screen** — created by `GameLevel.EnterGame()` after the states were loaded or
  reset, freed by `ReturnToMenu()`:
  - **[ExampleLevel.cs](ExampleLevel.cs)** — shows itself on `Main`, hosts the
    [Example](../Example/README.md) module's UI via its factory, and **listens to that UI**
    (`IExampleUIObserver`) to handle navigation (Menu); Esc does the same. This is the
    UI → Level direction: the UI reports intent, the level decides where to go.

Why the game screen is rebuilt: `SaveService.Load` / `NewGameService.Create` replace the
state objects. A UI that subscribed in `_Ready` stays subscribed to the objects of *its*
session; building the screen after the load and freeing it on exit keeps that true without
any re-subscribe code. Never load or reset while the game screen exists.

### 6. Adapters — `Adapter/` (create on first use)
Modules don't know each other. When one needs another's data, it declares an interface
in its own `Service` layer, and `Level` implements it over the other module's service:

```csharp
// Shop/Service/IShopCatalog.cs — the consumer's view, in its own types
public interface IShopCatalog
{
    IReadOnlyList<ShopItem> GetItems();
}

// Level/Adapter/InventoryShopCatalog.cs — the only place that knows both modules
public class InventoryShopCatalog : IShopCatalog
{
    private readonly IInventoryService _inventoryService;

    public InventoryShopCatalog(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    public IReadOnlyList<ShopItem> GetItems()
    {
        return _inventoryService.GetAll()
            .Select(item => new ShopItem { Id = item.Id, Name = item.Name, Price = item.Price })
            .ToList();
    }
}

// GameLevel constructor
_shopService = new ShopService(_shopRepository, new InventoryShopCatalog(_inventoryService));
```

The consumer's tests mock `IShopCatalog`; the adapter itself is engine-free and can be
tested in `tests/Game.Tests/Level/Adapter/` when the mapping has logic. The same shape works
for callbacks the other way (`IShopPurchaseListener` implemented over another module's service).

## Initialization order

Wiring follows a strict order so dependencies always exist before they're used.

Per module, in the `GameLevel` **constructor**: `State → Repository → Service → Factory`
(create the state, put it in a repository, hand the repository to the service, hand the
service to the factory). Repositories start empty: the `slots` list (one `StateSlot<T>` per
persistent state) is built after them, then `SaveService` / `NewGameService` over it, and
`NewGameService.Create()` fills every repository. Factories that need `SaveService` come last.

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
GameLevel._Ready()       → creates the menu + quit screens, OpenLevel(MainMenu)
        │
        ▼
Continue / New Game      → Load() or NewGameService.Create(), build ExampleLevel, OpenLevel(Main)
        │
        ▼
Esc / Menu               → save, OpenLevel(MainMenu), free ExampleLevel · window close → save
Menu Quit                → OpenLevel(Quit)
        │
        ▼
GameLevel.Notify(...)    → OnLevelOppened on every observer
        │
        ├─ MainMenuLevel: Visible = (newLevel == MainMenu), refresh Continue
        ├─ ExampleLevel:  Visible = (newLevel == Main)
        └─ QuitLevel:     if (newLevel == Quit) GetTree().Quit()
```

## Adding your own screen

1. Add a value to [Enum/Level.cs](Enum/Level.cs) (e.g. `Menu`).
2. Create a screen class (a `Control` subclass implementing `ILevelObserver`); take
   `IGameLevel` in the constructor and, if it has UI, the module's factory. Reference —
   [ExampleLevel.cs](ExampleLevel.cs).
3. Session screen (menu, settings): in `GameLevel._Ready()` create it, add it to
   `_observers` and to the tree. Game screen content (HUD, inventory): host it inside the
   game screen built in `EnterGame()`, so it lives exactly one session.
4. Switch to it from anywhere: `_gameLevel.OpenLevel(Enum.Level.Menu)`.

> A screen is only about "when to show". The feature itself (data, logic, layout)
> lives in a separate module folder — see [Example](../Example/README.md) and
> [../README.md](../README.md).
