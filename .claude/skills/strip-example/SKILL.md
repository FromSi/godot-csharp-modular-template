---
name: strip-example
description: Cleanly remove the Example demo module and its wiring, tests and docs, turning the template into a bare project. Use when the template is becoming a real game and the demo is no longer needed.
---

# Strip the Example module

The `Example` module is a demo; removing it must leave the project building and running.
Confirm with the user before deleting.

Steps:

1. **Delete the module** `src/Game/Example/` (and any `.uid` files).

2. **Delete its tests** `tests/Game.Tests/Example/`.

3. **Delete its docs** `src/Game/Example/README.md` (already covered by folder deletion).

4. **Unwire from [GameLevel](../../../src/Game/Level/GameLevel.cs)**:
   - remove the `using Game.Game.Example.*;` lines;
   - remove the `NoteState`/`NoteService`/`ExampleUIFactory` fields and their construction in the constructor;
   - remove `_jsonConverterStateService.Register(typeof(NoteState));`;
   - remove the `ExampleLevel` creation/registration in `_Ready`.

5. **Fix [SaveService](../../../src/Game/Common/Service/SaveService.cs) and
   [NewGameService](../../../src/Game/Common/Service/NewGameService.cs)** — both reference
   `NoteState`. Remove the `NoteState` repository from each (constructor param, field, the
   `Save` list slot + `Load` cast, the reset in `Create`), lower the `data is not { Count: N }`
   guard, and update the `SaveService(...)`/`NewGameService(...)` calls in `GameLevel` and
   the tests ([SaveServiceTests](../../../tests/Game.Tests/Common/Service/SaveServiceTests.cs),
   [NewGameServiceTests](../../../tests/Game.Tests/Common/Service/NewGameServiceTests.cs)).
   If nothing else is persisted yet, both may keep only `IdState`.

6. **Remove the screen** `src/Game/Level/ExampleLevel.cs` (it hosts the Example UI). Keep `QuitLevel` and the `Level` scaffolding.

7. **Fallback content** — with no screen, the main scene shows nothing. Either leave `GameLevel` empty (blank window) or add a minimal placeholder screen with `add-screen`. Ask which the user wants.

8. **Docs** — update the folder tree/mentions of `Example` in root [README.md](../../../README.md), [CLAUDE.md](../../../CLAUDE.md) and [src/Game/README.md](../../../src/Game/README.md) so they don't reference a deleted module.

9. Run the `verify` skill; the project must build with 0 warnings and tests must pass.
