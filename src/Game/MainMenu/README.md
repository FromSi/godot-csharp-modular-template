# MainMenu — the start menu

The first screen: **Continue** (inactive without a save that loads) / **New Game** /
**Settings** / **Quit**, and "Could not save the game" under them after the player left the
game without saving. A UI-only module — it reads `SaveService.CanLoad()` and raises the
player's choice; loading, resetting and navigation are done by the level.

> General module conventions — [../README.md](../README.md). How the menu fits the game
> screen's lifetime — [../Level/README.md](../Level/README.md).

## Module files

| File | Layer | What it does |
|------|-------|--------------|
| [UI/MainMenuUI.cs](UI/MainMenuUI.cs) | UI | centered column: title + four buttons; `RefreshContinue()`, `ShowSaveFailed()` |
| [UI/Observer/IMainMenuUIObserver.cs](UI/Observer/IMainMenuUIObserver.cs) | UI | UI → Level intents |
| [UI/Factory/MainMenuUIFactory.cs](UI/Factory/MainMenuUIFactory.cs) | UI | builds the UI with `SaveService` |

## How it connects

[MainMenuLevel](../Level/MainMenuLevel.cs) hosts it for the whole session, listens to
`IMainMenuUIObserver` and calls `GameLevel.ContinueGame()` / `StartNewGame()` /
`OpenLevel(Quit)`; Settings opens the [Settings](../Settings/README.md) screen over the menu.
Each time `MainMenu` opens it calls `RefreshContinue()` (Continue becomes active after the
first save) and `ShowSaveFailed(IsLastSaveFailed)`.

No tests: there is no engine-free logic here (`CanLoad` is covered in
[SaveServiceTests](../../../tests/Game.Tests/Common/Service/SaveServiceTests.cs)).
