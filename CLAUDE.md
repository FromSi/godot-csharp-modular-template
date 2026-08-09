# CLAUDE.md

Guidance for AI agents working in this repo. Read the layer READMEs before changing
architecture: [src/Game/README.md](src/Game/README.md) (start here),
[Common](src/Game/Common/README.md), [Level](src/Game/Level/README.md),
[Example](src/Game/Example/README.md).

## What this is
A **Godot 4.6 + C# (.NET 8)** modular game template. Game code is `Game.csproj`
(assembly `Game`); tests are a separate project `tests/Game.Tests`.

## Commands
```bash
dotnet build Game.csproj      # build game code
dotnet test Game.sln          # run all tests
```
- Android builds target **.NET 9.0** (conditional `TargetFramework` in Game.csproj); everything else is .NET 8.0.
- `<Nullable>enable</Nullable>` is on — keep it building with **0 warnings**.
- `tests/**` is excluded from `Game.csproj` compilation (`DefaultItemExcludes`); the tests are a separate project.

## Architecture in one line
`Level` (composition root) → feature `Modules` → `Common` (infrastructure).
Dependencies point only toward `Common`; `Common` knows nothing about modules or `Level`.
Details and the "add a feature" steps are in [src/Game/README.md](src/Game/README.md) — don't duplicate them elsewhere.

## Layout
- `src/Game/Common/` — reusable infrastructure (no game logic here).
- `src/Game/<Feature>/` — one folder per feature, layers `Domain / Service / UI / UI/Factory` (+ `Enum/`, `Observer/` when needed).
- `src/Game/Level/` — composition root (`GameLevel`) and level screens; the only place wiring happens.
- `level/game_level.tscn` — the single Godot scene (main scene).
- `tests/Game.Tests/` — NUnit + Moq, mirrors the `src/Game` folder structure.

## Naming
- **Namespaces mirror the folder path with a `Game.Game` prefix**: `src/Game/Example/Service` → `namespace Game.Game.Example.Service`. (The double `Game` = assembly `Game` + `src/Game`.) This is the most common gotcha — match it exactly.
- Classes/files/methods/public props: `PascalCase`. File name == class name.
- Private fields: `_camelCase`. Locals/params: `camelCase`. Names full and descriptive.

## Layer rules
| Layer                    | Knows about    | Tested |
|--------------------------|----------------|--------|
| `<Module>/Enum`          | —              | —      |
| `<Module>/Domain`        | —              | +      |
| `<Module>/Repository`    | State          | +      |
| `<Module>/Service`       | Repository     | +      |
| `<Module>/UI`            | Service, State | —      |
| `Level`                  | Factory        | —      |

## Hard constraints
- **C# only.** All logic, UI and models in C#. No GDScript.
- **No `.tscn` for UI.** Build every UI node in code. The only scene is `level/game_level.tscn`.
- **Godot API only in `UI` and `Level`.** `Domain`/`Service`/`Repository` must stay engine-free (that's why they're testable).
- **Dependency injection via constructors**, usually through factories. `Level` creates and wires everything; modules never create each other.
- **State = POCO.** To persist a new state type: register it in `JsonConverterStateService` in [GameLevel](src/Game/Level/GameLevel.cs) (else it won't (de)serialize) **and** add its repository to both [SaveService](src/Game/Common/Service/SaveService.cs) and [NewGameService](src/Game/Common/Service/NewGameService.cs). Saving/reset is centralized there — module services do no file I/O. These two are the only spots in `Common` allowed to reference concrete module states.
- **Wiring order.** In the `GameLevel` constructor: per module `State → Repository → Service → Factory`; central services after their repositories. In `_Ready`/screens: `create → subscribe observers → AddChild` (subscribe before adding to the tree).
- **Observer pattern (both directions).** state → UI: a `Domain` state may extend `Common`'s `ObservableState<TObserver>`, mutate via methods that call `Notify(...)`, UI subscribes. UI → Level: a UI node publishes an `I<X>UIObserver` (manual list, it's a `Node`) and the level listens to handle navigation. `SaveService.Load`/`NewGameService.Create` replace state objects, so the UI re-subscribes afterwards (see `SubscribeAndRender` in [ExampleUI](src/Game/Example/UI/ExampleUI.cs)). See the `add-observer` skill.
- **No `virtual` methods** — use `abstract` or an interface for extension points.
- **Tests are mandatory** for new/changed logic; keep them current, don't write redundant ones. Test `Service`/`Domain` (engine-free); mock `Common` interfaces (see [NoteServiceTests](tests/Game.Tests/Example/Service/NoteServiceTests.cs)).

## Code style
- 4 spaces, no tabs. Max 120 chars/line. No trailing whitespace. One trailing newline.
- Always use braces for blocks (except short `?:`).
- One blank line between methods and between distinct logical blocks (if/for/while, assignments vs `return`); no extra blanks between consecutive assignments.
- Max 3 levels of nesting — extract methods beyond that.
- `using` order: project types first, then C# BCL, then Godot; alias the secondary ones if names clash.
- Prefer simple solutions; add abstraction only when it pays off immediately. Follow SOLID without over-engineering.

## Working agreement
- Make a short plan before non-trivial changes; if a change is unclear or maybe unneeded, ask first with evidence.
- Be terse and concrete. When editing existing code, show only the changed lines with a little context, not the whole file.
