---
name: add-screen
description: Add a level screen (an ILevelObserver shown/hidden on level change) to the Level component. Use when asked to add a screen, menu, HUD, pause screen, or any full-screen game state.
---

# Add a level screen

Read [src/Game/Level/README.md](../../../src/Game/Level/README.md). Core screens that always
exist: [QuitLevel.cs](../../../src/Game/Level/QuitLevel.cs) (logical, no UI) and
[MainMenuLevel.cs](../../../src/Game/Level/MainMenuLevel.cs) (hosts a module UI).

Ask for the level name if not given. First decide the **lifetime**:

- **Session screen** (menu, credits) — doesn't show game state; created once in
  `GameLevel._Ready`, lives until exit.
- **Game screen content** (HUD, inventory, map) — shows game state; must live exactly one
  game session, because Load / New Game replace state objects. Host it inside the game
  screen that `GameLevel.EnterGame()` builds (or make it *the* game screen), never in `_Ready`.

Then:

1. **Add the enum value** in [Enum/Level.cs](../../../src/Game/Level/Enum/Level.cs) (e.g. `Credits`)
   — only if it's a separate level; a panel inside the game screen needs no value.

2. **Create the screen class** `src/Game/Level/<Name>Level.cs`, namespace `Game.Game.Level`:
   ```csharp
   public partial class CreditsLevel : Control, ILevelObserver, ICreditsUIObserver
   {
       private readonly IGameLevel _gameLevel;
       private readonly CreditsUIFactory _creditsUiFactory;

       public CreditsLevel(IGameLevel gameLevel, CreditsUIFactory creditsUiFactory)
       {
           _gameLevel = gameLevel;
           _creditsUiFactory = creditsUiFactory;

           ProcessMode = ProcessModeEnum.Always;   // if it must react while the tree is paused
           SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
       }

       public override void _Ready()
       {
           var ui = _creditsUiFactory.Create();
           ui.AddObserver(this);   // subscribe before adding to the tree
           AddChild(ui);
       }

       public void OnBackRequested() => _gameLevel.OpenPreviousLevel();   // UI → Level intent

       public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
       {
           Visible = newLevel == Enum.Level.Credits;
       }
   }
   ```
   Layout and data come from the module's UI (via its factory) — the screen only decides
   *when to show* and *where to navigate*.

3. **Register it** in [GameLevel](../../../src/Game/Level/GameLevel.cs):
   - session screen — in `_Ready`, next to the menu:
     ```csharp
     var creditsLevel = new CreditsLevel(this, _creditsUiFactory);
     _observers.Add(creditsLevel);
     _canvasLayer.AddChild(creditsLevel);
     ```
   - game screen — in `EnterGame()` the same three lines, keep the reference in a field and
     undo them in `LeaveToMenu()` (`_observers.Remove(...)`, `QueueFree()`, field = `null`).
     It also gets `ShowSaveResult(bool)` (called by `GameLevel.SaveGame()`) and, on a failed
     `ReturnToMenu()`, asks "Exit without saving?" → `LeaveToMenu()` (see `ExampleLevel`).
   - an overlay over a session screen (like Settings over the menu) — create it in that
     screen's `_Ready`, hidden, and toggle `Visible` from the intents.

4. **Switch to it** with `_gameLevel.OpenLevel(Enum.Level.<Name>)`; to leave the game use
   `_gameLevel.ReturnToMenu()` (it saves; `false` = the save failed, ask the player), not
   `OpenLevel(MainMenu)`. Save from a game screen only via `_gameLevel.SaveGame()`.

5. **Verify** with the `verify` skill, then `run-godot` to see it.

A screen only decides *when to show*. Feature data/logic/layout belong in a module (`add-module`).
