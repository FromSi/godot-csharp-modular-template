---
name: add-screen
description: Add a level screen (an ILevelObserver shown/hidden on level change) to the Level component. Use when asked to add a screen, menu, HUD, pause screen, or any full-screen game state.
---

# Add a level screen

Read [src/Game/Level/README.md](../../../src/Game/Level/README.md). Core screens that always
exist: [QuitLevel.cs](../../../src/Game/Level/QuitLevel.cs) (logical, no UI) and
[MainMenuLevel.cs](../../../src/Game/Level/MainMenuLevel.cs) (hosts a module UI).

Ask for the level name if not given. First decide the **lifetime**:

- **Session screen** (menu, settings, credits) — doesn't show game state; created once in
  `GameLevel._Ready`, lives until exit.
- **Game screen content** (HUD, inventory, map) — shows game state; must live exactly one
  game session, because Load / New Game replace state objects. Host it inside the game
  screen that `GameLevel.EnterGame()` builds (or make it *the* game screen), never in `_Ready`.

Then:

1. **Add the enum value** in [Enum/Level.cs](../../../src/Game/Level/Enum/Level.cs) (e.g. `Settings`)
   — only if it's a separate level; a panel inside the game screen needs no value.

2. **Create the screen class** `src/Game/Level/<Name>Level.cs`, namespace `Game.Game.Level`:
   ```csharp
   public partial class SettingsLevel : Control, ILevelObserver, ISettingsUIObserver
   {
       private readonly IGameLevel _gameLevel;
       private readonly SettingsUIFactory _settingsUiFactory;

       public SettingsLevel(IGameLevel gameLevel, SettingsUIFactory settingsUiFactory)
       {
           _gameLevel = gameLevel;
           _settingsUiFactory = settingsUiFactory;

           ProcessMode = ProcessModeEnum.Always;   // if it must react while the tree is paused
           SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
       }

       public override void _Ready()
       {
           var ui = _settingsUiFactory.Create();
           ui.AddObserver(this);   // subscribe before adding to the tree
           AddChild(ui);
       }

       public void OnBackRequested() => _gameLevel.OpenPreviousLevel();   // UI → Level intent

       public void OnLevelOppened(Enum.Level newLevel, Enum.Level oldLevel)
       {
           Visible = newLevel == Enum.Level.Settings;
       }
   }
   ```
   Layout and data come from the module's UI (via its factory) — the screen only decides
   *when to show* and *where to navigate*.

3. **Register it** in [GameLevel](../../../src/Game/Level/GameLevel.cs):
   - session screen — in `_Ready`, next to the menu:
     ```csharp
     var settingsLevel = new SettingsLevel(this, _settingsUiFactory);
     _observers.Add(settingsLevel);
     _canvasLayer.AddChild(settingsLevel);
     ```
   - game screen — in `EnterGame()` the same three lines, keep the reference in a field and
     undo them in `ReturnToMenu()` (`_observers.Remove(...)`, `QueueFree()`, field = `null`).

4. **Switch to it** with `_gameLevel.OpenLevel(Enum.Level.<Name>)`; to leave the game use
   `_gameLevel.ReturnToMenu()` (it saves), not `OpenLevel(MainMenu)`.

5. **Verify** with the `verify` skill, then `run-godot` to see it.

A screen only decides *when to show*. Feature data/logic/layout belong in a module (`add-module`).
