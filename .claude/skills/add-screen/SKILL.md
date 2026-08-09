---
name: add-screen
description: Add a level screen (an ILevelObserver shown/hidden on level change) to the Level component. Use when asked to add a screen, menu, HUD, pause screen, or any full-screen game state.
---

# Add a level screen

Read [src/Game/Level/README.md](../../../src/Game/Level/README.md); model it on
[ExampleLevel.cs](../../../src/Game/Level/ExampleLevel.cs) (screen with UI) or
[QuitLevel.cs](../../../src/Game/Level/QuitLevel.cs) (logical, no UI).

Ask for the level name if not given. Then:

1. **Add the enum value** in [Enum/Level.cs](../../../src/Game/Level/Enum/Level.cs) (e.g. `Menu`).

2. **Create the screen class** in `src/Game/Level/<Name>Level.cs`:
   - namespace `Game.Game.Level`;
   - a `Control` (or other `Node`) implementing `ILevelObserver`;
   - constructor takes `IGameLevel` and, if it has UI, the owning module's factory;
   - set `ProcessMode = ProcessModeEnum.Always` if it must react while the tree is paused;
   - build UI in `_Ready` (or host a module UI via its factory);
   - implement `OnLevelOppened(newLevel, oldLevel)` — typically `Visible = newLevel == Enum.Level.<Name>`.

3. **Register it** in [GameLevel._Ready](../../../src/Game/Level/GameLevel.cs): create it, `_observers.Add(...)`, `AddChild(...)` (usually under the `CanvasLayer`).

4. **Switch to it** from anywhere with `_gameLevel.OpenLevel(Enum.Level.<Name>)`.

5. **Verify** with the `verify` skill.

A screen only decides *when to show*. Feature data/logic/layout belong in a module (`add-module`), not in the screen.
