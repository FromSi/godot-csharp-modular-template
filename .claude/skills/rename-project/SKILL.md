---
name: rename-project
description: Rename this template for a real game — assembly name, application name, the Game.Game namespace prefix, and the solution/project files. Use once when starting a new project from the template.
---

# Rename the project

Ask for the new name in two forms if not given:
- **AssemblyName / project name** (PascalCase, valid C# identifier, e.g. `SpaceGame`).
- **Display name** (human-readable, for the window title; may contain spaces).

This is a wide, mechanical change — do it carefully and verify at the end.

Steps (current name is `Game`):

1. **project.godot**
   - `[application] config/name="<Display Name>"`
   - `[dotnet] project/assembly_name="<NewName>"`

2. **Csproj / solution**
   - Rename `Game.csproj` → `<NewName>.csproj` and `Game.sln` → `<NewName>.sln`.
   - Update the `ProjectReference` in `tests/Game.Tests/Game.Tests.csproj` to the new csproj path.
   - Optionally rename the test project too (`<NewName>.Tests`); if so, update the `.sln` and folder.

3. **Namespaces** — the code uses the `Game.Game.*` prefix (assembly `Game` + `src/Game` folder). Decide the new prefix:
   - Simplest: keep the `src/Game/` folder → new prefix becomes `<NewName>.Game.*`.
   - Or rename the folder `src/Game` → `src/<NewName>` for a `<NewName>.<NewName>.*` prefix (more churn).
   - Do a global replace of the namespace prefix across `src/**` and `tests/**` accordingly, plus `using` statements.

4. **Scene/script refs** — `level/game_level.tscn` references the root script by `uid`, not by name, so it survives. Just confirm it still loads.

5. **Docs** — update root [README.md](../../../README.md) and [CLAUDE.md](../../../CLAUDE.md) references to the old name where relevant.

6. **Delete build artifacts** (`bin/`, `obj/`, `.godot/mono/`) to avoid stale references, then run the `verify` skill.

Prefer the least-churn option (keep `src/Game/` folder, change only the assembly prefix) unless the user explicitly wants the folder renamed. Confirm the plan before doing the global replace.
