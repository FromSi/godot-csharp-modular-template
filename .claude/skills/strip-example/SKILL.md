---
name: strip-example
description: Cleanly remove the Example demo module and its wiring, tests and docs, turning the template into a bare project. Use when the template is becoming a real game and the demo is no longer needed.
---

# Strip the Example module

The `Example` module is a demo; removing it must leave the project building and running
(menu → New Game → game screen → Esc → menu). Confirm with the user before deleting.
`Common`, `MainMenu`, `QuitLevel`, `MainMenuLevel` and the slot-based save system stay.

Steps:

1. **Delete the module and its tests** — `src/Game/Example/` (with `.uid` files and its README)
   and `tests/Game.Tests/Example/`.

2. **Replace the game screen.** `src/Game/Level/ExampleLevel.cs` hosts the Example UI and is
   what `GameLevel.EnterGame()` builds. Ask the user which they want:
   - the screen of their first real module (use `add-module` + `add-screen`), or
   - a minimal placeholder `src/Game/Level/GameScreenLevel.cs`: a `Control, ILevelObserver`
     with a centered `Label`, `Visible = newLevel == Enum.Level.Main`, and Esc →
     `_gameLevel.ReturnToMenu()` in `_Input` (keep `GetViewport().SetInputAsHandled()`).
   Delete `ExampleLevel.cs` and change the `_gameScreen` field type / `EnterGame()` accordingly.

3. **Unwire from [GameLevel](../../../src/Game/Level/GameLevel.cs)**:
   - remove the `using Game.Game.Example.*;` lines;
   - remove the `NoteState` repository, `NoteService`, `ExampleUIFactory` fields and their construction;
   - remove `new StateSlot<NoteState>(...)` from the `slots` list (nothing else in `Common` refers
     to `NoteState` — `SaveService`/`NewGameService` work on slots only).

4. **Fix the JSON round-trip test**
   [JsonConverterStateServiceTests](../../../tests/Game.Tests/Common/Service/JsonConverterStateServiceTests.cs):
   drop `NoteState` from the `Register` list and from the round trip (keep `IdState`; use a
   type that is still registered in `Serialize_SkipsUnregisteredTypes`, e.g. a private test POCO).

5. **Docs** — remove/replace mentions of `Example`, `NoteState`, `NoteService`, `ExampleUI`,
   `ExampleLevel` in root [README.md](../../../README.md) (incl. the screenshot caption),
   [CLAUDE.md](../../../CLAUDE.md) (the namespace example), [src/Game/README.md](../../../src/Game/README.md),
   [Common/README.md](../../../src/Game/Common/README.md) (Observer live example) and
   [Level/README.md](../../../src/Game/Level/README.md) (game screen, control flow).

6. **Update links in the skills.** The skills describe patterns with inline snippets, but some
   still name Example files as optional references. Find them:
   ```bash
   grep -rnE 'Example|NoteState|NoteService|ExampleUI|ExampleLevel' .claude/skills CLAUDE.md README.md src tests
   ```
   Point each hit at the replacement module/screen, or drop the reference — no link may lead
   to a deleted file. (This skill itself may keep its own mentions.)

7. Run the `verify` skill (0 warnings, green tests, clean style checks), then `run-godot`
   to confirm menu → New Game → game screen → Esc → menu → Continue.
