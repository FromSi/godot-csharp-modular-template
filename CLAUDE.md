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
- `src/Game/Common/` — reusable infrastructure (no game logic here); `Common/UI/` holds shared Godot widgets.
- `src/Game/<Feature>/` — one folder per feature, layers `Domain / Service / UI / UI/Factory` (+ `Enum/`, `Observer/` when needed).
- `src/Game/MainMenu/` — start menu UI (Continue / New Game / Quit).
- `src/Game/Level/` — composition root (`GameLevel`), level screens, cross-module adapters (`Level/Adapter/`); the only place wiring happens.
- `level/game_level.tscn` — the single Godot scene (main scene).
- `tests/Game.Tests/` — NUnit + Moq, mirrors the `src/Game` folder structure.

## Naming
- **Namespaces mirror the folder path with a `Game.Game` prefix**: `src/Game/Example/Service` → `namespace Game.Game.Example.Service`. (The double `Game` = assembly `Game` + `src/Game`.) This is the most common gotcha — match it exactly.
- Classes/files/methods/public props: `PascalCase`. File name == class name.
- Private fields: `_camelCase`. Locals/params: `camelCase`. Names full and descriptive.

## Layer rules
| Layer                          | Knows about                     | Tested |
|--------------------------------|---------------------------------|--------|
| `<Module>/Enum`                | —                               | —      |
| `<Module>/Domain`              | —                               | +      |
| `<Module>/Repository`          | State                           | +      |
| `<Module>/Service`             | Repository                      | +      |
| `<Module>/UI`                  | Service, State, `Common/UI`     | —      |
| `Common/UI`                    | Godot (shared widgets)          | —      |
| `Level` screens                | Factory                         | —      |
| `Level/Adapter`                | module Services (no Godot)      | +      |

## Hard constraints
- **C# only.** All logic, UI and models in C#. No GDScript.
- **No `.tscn` for UI.** Build every UI node in code. The only scene is `level/game_level.tscn`.
- **Godot API only in `UI` (module or `Common/UI`) and `Level` screens.** `Domain`/`Service`/`Repository` must stay engine-free (that's why they're testable).
- **Dependency injection via constructors**, usually through factories. `Level` creates and wires everything; modules never create each other.
- **Modules never reference each other.** Cross-module data: the consumer declares an interface in its own `Service` (e.g. `I<Consumer>Catalog`), an adapter in `src/Game/Level/Adapter/` implements it over the other module's service. See [Level/README.md](src/Game/Level/README.md).
- **State = POCO.** To persist a state: add one `StateSlot<T>(repository, createFresh)` to the `slots` list in [GameLevel](src/Game/Level/GameLevel.cs) — that saves, loads, resets it on New Game and registers its type for JSON. [SaveService](src/Game/Common/Service/SaveService.cs) / [NewGameService](src/Game/Common/Service/NewGameService.cs) work on slots only and never reference module types; module services do no file I/O. Repositories start empty and are filled by `NewGameService.Create()`. Add the state to `JsonConverterStateServiceTests` (real JSON round trip). See the `add-persistable-state` skill.
- **Entities have ids; references go by id.** Every entity has an `int Id` (from `IIdService`). A slot state (`<Name>State` in a `SingleRepository`) is a singleton identified by its type — it gets no `Id`; entities are the items *inside* states. An entity that refers to another stores its id (`ProductId`) plus a snapshot of what it shows (`ProductName`, `ProductPrice`) — never the object itself. Checks look the referenced entity up by id and compare the snapshot. An id is not a display number: if players see "Order #12" or "Planet 3", that's a separate field.
- **Wiring order.** In the `GameLevel` constructor: per module `State → Repository → Service → Factory`; then the `slots` list, `SaveService`/`NewGameService` over it, `NewGameService.Create()`. In `_Ready`/screens: `create → subscribe observers → AddChild` (subscribe before adding to the tree).
- **Observer pattern (both directions).** state → UI: a `Domain` state may extend `Common`'s `ObservableState<TObserver>`, mutate via methods that call `Notify(...)`, UI subscribes. UI → Level: a UI node publishes an `I<X>UIObserver` (manual list, it's a `Node`) and the level listens to handle navigation. See the `add-observer` skill.
- **Game screen lifetime = one session.** `SaveService.Load`/`NewGameService.Create` replace state objects, so a UI subscribed before them watches stale objects. Hence: the main menu does Continue / New Game, *then* `GameLevel` creates the game screen (its UIs subscribe once in `_Ready`); returning to the menu saves and frees it (UIs unsubscribe in `_ExitTree`). Closing the window during play saves (`_Notification`). Never load or reset while a game screen is alive.
- **No `virtual` methods** — use `abstract` or an interface for extension points.
- **Tests are mandatory** for new/changed logic; keep them current, don't write redundant ones. Test `Service`/`Domain` (engine-free); mock `Common` interfaces (see the `add-tests` skill). Adapters in `Level/Adapter/` are engine-free and tested too.
- **Time and randomness behind interfaces** — `IClockService.Now`, `IRandomGeneratorService`; never `DateTime.Now`/`Random` in a Service.

## Code style
- 4 spaces, no tabs. Max 120 chars/line. No trailing whitespace. One trailing newline.
- Always use braces for blocks (except short `?:`).
- One blank line between methods and between distinct logical blocks (if/for/while, assignments vs `return`); no extra blanks between consecutive assignments.
- Max 3 levels of nesting — extract methods beyond that.
- `using` order, one group after another (no blank lines): project (`Game.*`) → BCL (`System.*`) → third-party (`Godot`, `Moq`, `NUnit`), aliases last; alias the secondary ones if names clash. IDE "sort usings" puts `System` first — don't apply it.
- Prefer simple solutions; add abstraction only when it pays off immediately. Follow SOLID without over-engineering.

## Working agreement
- Make a short plan before non-trivial changes; if a change is unclear or maybe unneeded, ask first with evidence.
- Be terse and concrete. When editing existing code, show only the changed lines with a little context, not the whole file.
