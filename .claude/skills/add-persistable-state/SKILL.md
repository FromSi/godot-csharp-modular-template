---
name: add-persistable-state
description: Add a POCO state that can be saved and loaded (and reset on a new game). Use when adding save data, persistent settings/progress, or any serializable state.
---

# Add a persistable state

Persistence goes through **state slots**: one `StateSlot<T>` per saved state, listed once
in `GameLevel` ([src/Game/Level/GameLevel.cs](../../../src/Game/Level/GameLevel.cs)). That
single line makes the state saved, loaded, reset on "New Game" and registered for JSON —
nothing to touch in `Common` (`SaveService` / `NewGameService` work on slots only). Background:
the SaveService section in [src/Game/Common/README.md](../../../src/Game/Common/README.md).

Steps:

1. **Define the state** as a plain POCO in `<Module>/Domain/<Name>State.cs`
   (public get/set properties, sane defaults, no logic, no Godot):
   ```csharp
   public class InventoryState
   {
       public List<InventoryItem> Items { get; set; } = [];
   }

   public class InventoryItem
   {
       public int Id { get; set; }              // from IIdService
       public int ProductId { get; set; }       // reference by id …
       public string ProductName { get; set; } = "";  // … plus a display snapshot
       public decimal ProductPrice { get; set; }
   }
   ```
   - **The state itself has no `Id`** — it's a singleton identified by its type.
   - **Every entity inside it gets an `int Id`** (`IIdService.Next()`); never reuse it as a
     display number — add a separate `Number` field if players see one.
   - **References are ids + snapshots**, never nested objects of another state: store
     `ProductId` (nullable `int?` if optional) and the name/price shown at that moment.
     Checks look the entity up by id and compare the snapshot (see "Entities and ids" in
     [src/Game/README.md](../../../src/Game/README.md)).
   - Observable? Extend `ObservableState<I<Name>StateObserver>` (see `add-observer`) — the
     observer list is private and not serialized.

2. **Create its repository** in the `GameLevel` constructor — empty, don't seed it there:
   ```csharp
   _inventoryRepository = new SingleRepository<InventoryState>();
   ```
   `StateSlot<T>` works over `ISingleRepository<T>`, so keep a collection as a list inside one
   state (as above) rather than in a `CollectionRepository`.

3. **Add a slot** to the `slots` list in `GameLevel`:
   ```csharp
   new StateSlot<InventoryState>(_inventoryRepository, () => new InventoryState()),
   // or with starting content: InventorySeed.Create  (Service/InventorySeed.cs, a static factory)
   ```
   The factory builds the fresh state for a new game. `NewGameService.Create()` runs once at
   the end of the constructor, so the repository is filled before anything reads it.
   Slot order = save file order; a save whose slots don't match (count or types) is rejected
   as a whole, so an old save can't half-load — Continue simply does nothing. Once players
   have saves, append new slots and consider versioning `SavePath`.

   **Must survive New Game** (player settings, unlocks across runs)? Don't add it to the game
   `slots`; give it its own `SaveService` over its own slot and path, loaded once in the
   constructor (`Load()` or `slot.Reset()`), and let the module save through a store interface
   implemented in `Level/Adapter/` — exactly like `SettingsState` / `SettingsFileStore`.

4. **Custom field types** (not handled by System.Text.Json, e.g. `Vector2`) need a converter
   like `Vector2JsonConverter` added in `JsonConverterStateService`. Dictionaries with enum or
   int keys, nullables and `DateTime` work out of the box — the round-trip test proves it.

5. **Observable state?** Load / New Game replace state objects. The game screen is built only
   after Continue / New Game, so UIs that subscribe in `_Ready` get the right objects;
   don't add load/reset buttons to the game screen (see `add-observer`).

6. **Test**: add the type to the `Register` list and a filled instance to the round trip in
   [JsonConverterStateServiceTests](../../../tests/Game.Tests/Common/Service/JsonConverterStateServiceTests.cs)
   (real JSON — catches types that silently don't (de)serialize), and assert the fields
   that matter after `Deserialize`. Then run `verify`.

Saving can fail (`SaveService.Save()` returns `false`; the old file stays intact). Game saves
go only through `GameLevel.SaveGame()`, which reports the failure to the player — don't call
`SaveService.Save()` from module UI.

Don't: register types by hand, add repositories to `SaveService`/`NewGameService`, or write
file I/O in a module service.
