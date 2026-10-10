---
name: arch-check
description: Review changed C# against this template's architecture rules — layer boundaries, Game.Game namespaces, no Godot in Domain/Service/Repository, no virtual, UI built in code. Use when asked to check architecture, conventions, or review a module.
---

# Architecture check

Check the changed/target code against [CLAUDE.md](../../../CLAUDE.md) and the layer
READMEs. Report violations as a concrete list (file:line → rule → fix), not vague advice.

Checklist:

1. **Namespaces** — `Game.Game.<Module>.<Layer>`, mirroring the folder path. File name == class name.

2. **Layer boundaries** (who may know whom):
   - `Domain` — no dependencies, no Godot.
   - `Repository` — knows State only.
   - `Service` — knows Repository (interfaces from `Common`); **no Godot**.
   - `UI` — knows Service + State + `Common/UI`; Godot allowed here.
   - `Common/UI` — shared Godot widgets only, no game logic, no module types.
   - `Level` — knows Factories and module Services; the only place wiring happens.
   - Flag any `using Godot;` in Domain/Service/Repository and in `Level/Adapter/`.
   - **No module references another module** (`using Game.Game.<OtherModule>` inside a module).
     Cross-module data → an interface in the consumer's `Service` + an adapter in
     `Level/Adapter/`.

3. **UI in code only** — no `.tscn` for UI; the only scene is `level/game_level.tscn`. UI nodes built in C#.

4. **Dependency injection** — dependencies passed via constructors (usually through factories). Modules don't `new` each other; only `Level` wires them.

5. **No `virtual` methods** — extension via `abstract` or interface.

6. **Persistable state** — every saved POCO has a `StateSlot<T>` in `GameLevel`'s `slots` list and is
   in the `JsonConverterStateServiceTests` round trip; states are plain POCOs; no manual
   `Register(...)`, no module types in `Common` (incl. `SaveService`/`NewGameService`); repositories
   are not seeded in `GameLevel` (the slot factory does that).
   **Ids**: every entity (item inside a state) has an `int Id`, slot states themselves don't; references are an id + display snapshot, not nested
   objects of another state; checks look up by id and compare the snapshot; `Id` isn't reused as
   a player-visible number.

   **Saving**: game saves only via `GameLevel.SaveGame()` (its `bool` result reaches the player);
   no `SaveService.Save()` in module code; data that survives New Game has its own `SaveService`.

7. **Session lifetime** — no Load / New Game while a game UI exists; game UIs live in the
   screen built by `GameLevel.EnterGame()`, subscribe in `_Ready`, unsubscribe in `_ExitTree`.
   No `DateTime.Now` / `new Random()` in Service (use `IClockService` / `IRandomGeneratorService`).

8. **Tests** — engine-free `Service`/`Domain`/`Level` classes have tests; `Common` deps are mocked;
   mutable test fields are assigned in `[SetUp]`, not inline.

9. **Style** — 4 spaces (no tabs), ≤120 chars, no trailing whitespace, braces on all blocks
   (except short `?:`), ≤3 nesting levels, `using` order project (`Game.*`) → BCL (`System.*`) →
   third-party (`Godot`, `Moq`, `NUnit`). The `verify` skill has the line-length/whitespace commands.

If everything passes, say so briefly. Otherwise list each violation with the fix.
