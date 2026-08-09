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
   - `UI` — knows Service + State; Godot allowed here.
   - `Level` — knows Factories; the only place wiring happens.
   - Flag any `using Godot;` in Domain/Service/Repository.

3. **UI in code only** — no `.tscn` for UI; the only scene is `level/game_level.tscn`. UI nodes built in C#.

4. **Dependency injection** — dependencies passed via constructors (usually through factories). Modules don't `new` each other; only `Level` wires them.

5. **No `virtual` methods** — extension via `abstract` or interface.

6. **Persistable state** — every saved POCO is registered in `JsonConverterStateService`; states are plain POCOs.

7. **Tests** — engine-free `Service`/`Domain` have tests; `Common` deps are mocked.

8. **Style** — 4 spaces (no tabs), ≤120 chars, braces on all blocks (except short `?:`), ≤3 nesting levels, `using` order project→BCL→Godot.

If everything passes, say so briefly. Otherwise list each violation with the fix.
