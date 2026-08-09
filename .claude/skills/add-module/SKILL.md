---
name: add-module
description: Scaffold a new feature module under src/Game following this template's layered structure (Domain/Service/UI/Factory), wire it into GameLevel, and add tests. Use when asked to "add a module", "create a feature", or introduce a new gameplay/UI feature.
---

# Add a feature module

Read [src/Game/README.md](../../../src/Game/README.md) and [src/Game/Example/README.md](../../../src/Game/Example/README.md)
first; copy the `Example` module as the shape. Follow [CLAUDE.md](../../../CLAUDE.md) rules.

Ask for the feature name (PascalCase, e.g. `Inventory`) if not given. Then:

1. **Create the folders** `src/Game/<Feature>/` with layers as needed:
   - `Domain/` — POCO state class(es), no logic, no Godot.
   - `Service/` — `I<Feature>Service.cs` (contract) + `<Feature>Service.cs` (logic, engine-free), depends on repository interfaces from `Common`.
   - `UI/<Feature>UI.cs` — Godot `Control`/node, builds UI in code, only calls the service.
   - `UI/Factory/<Feature>UIFactory.cs` — creates the UI with injected dependencies.
   - Add `Enum/` or `Observer/` only if actually needed.

2. **Namespaces** mirror the path with the `Game.Game` prefix: `namespace Game.Game.<Feature>.<Layer>`.

3. **Wire it in** [GameLevel](../../../src/Game/Level/GameLevel.cs) constructor: create repository → service → factory (in that order), assign to private fields.

4. **If the state must persist**, register its type in `JsonConverterStateService` inside `GameLevel` — use the `add-persistable-state` skill's rules.

5. **Show it** via a level screen — use the `add-screen` skill (create an `ILevelObserver` that hosts the module's UI).

6. **Tests** — add `tests/Game.Tests/<Feature>/Service/<Feature>ServiceTests.cs`, mirroring the folder path, mocking `Common` interfaces (see `add-tests`).

7. **Docs** — add `src/Game/<Feature>/README.md` (use `module-docs`).

8. **Verify** — run the `verify` skill (0 warnings, green tests).

Keep layer boundaries: UI → Service → Domain/Common. No Godot in Domain/Service/Repository. No `virtual` (use `abstract`/interface).
