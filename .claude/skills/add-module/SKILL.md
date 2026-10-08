---
name: add-module
description: Scaffold a new feature module under src/Game following this template's layered structure (Domain/Service/UI/Factory), wire it into GameLevel, and add tests. Use when asked to "add a module", "create a feature", or introduce a new gameplay/UI feature.
---

# Add a feature module

Read [src/Game/README.md](../../../src/Game/README.md) first (layers, "Entities and ids",
"Between modules"). If `src/Game/Example/` still exists, it's a ready shape to copy; otherwise
the steps below are enough. Follow [CLAUDE.md](../../../CLAUDE.md) rules.

Ask for the feature name (PascalCase, e.g. `Inventory`) if not given. Then:

1. **Create the folders** `src/Game/<Feature>/` with layers as needed:
   - `Domain/` — POCO state class(es), no logic, no Godot. The state itself has no id; every entity inside it has an `int Id`
     (`IIdService.Next()`); references to other entities are an id + a display snapshot
     (`ProductId` + `ProductName`/`ProductPrice`), never the object. A player-visible number
     ("Order #12") is a separate field, not the id.
   - `Service/` — `I<Feature>Service.cs` (contract) + `<Feature>Service.cs` (logic, engine-free), depends on repository interfaces from `Common`.
     Time via `IClockService`, randomness via `IRandomGeneratorService`.
   - `UI/<Feature>UI.cs` — Godot `Control`/node, builds UI in code, only calls the service.
   - `UI/Factory/<Feature>UIFactory.cs` — creates the UI with injected dependencies.
   - Add `Enum/` or `Observer/` only if actually needed.

2. **Namespaces** mirror the path with the `Game.Game` prefix: `namespace Game.Game.<Feature>.<Layer>`.

3. **Wire it in** [GameLevel](../../../src/Game/Level/GameLevel.cs) constructor: create repository (empty) → service → factory (in that order), assign to private fields.

4. **If the state must persist**, add a `StateSlot<T>` for it to the `slots` list in `GameLevel` — use the `add-persistable-state` skill. Nothing to change in `SaveService`/`NewGameService`.

5. **Needs another module's data?** Don't reference that module. Declare the interface in this module's `Service` in its own types:
   ```csharp
   public interface IShopCatalog { IReadOnlyList<ShopItem> GetItems(); }
   ```
   implement it in `src/Game/Level/Adapter/<Source><Consumer>.cs` over the other module's service interface (namespace `Game.Game.Level.Adapter`), and pass `new InventoryShopCatalog(_inventoryService)` to this service in `GameLevel`. See "Adapters" in [Level/README.md](../../../src/Game/Level/README.md).

6. **Show it** — game content lives in the game screen built by `GameLevel.EnterGame()` (so it subscribes after Load / New Game); a standalone screen — use the `add-screen` skill.

7. **Tests** — add `tests/Game.Tests/<Feature>/Service/<Feature>ServiceTests.cs`, mirroring the folder path, mocking `Common` interfaces and the consumer-side interfaces (see `add-tests`); add a persistable state to `JsonConverterStateServiceTests`.

8. **Docs** — add `src/Game/<Feature>/README.md` (use `module-docs`).

9. **Verify** — run the `verify` skill (0 warnings, green tests).

Keep layer boundaries: UI → Service → Domain/Common. No Godot in Domain/Service/Repository. No `virtual` (use `abstract`/interface).
