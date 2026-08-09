---
name: add-persistable-state
description: Add a POCO state that can be saved and loaded, including the mandatory JsonConverter registration. Use when adding save data, persistent settings/progress, or any serializable state.
---

# Add a persistable state

Two common bugs: a state that silently won't (de)serialize because its type was never
registered, and a state that never gets saved because it wasn't added to the central
`SaveService`. This skill prevents both. Read the FileHandler and SaveService sections
in [src/Game/Common/README.md](../../../src/Game/Common/README.md) and use
[SaveService.cs](../../../src/Game/Common/Service/SaveService.cs) as the reference.

Steps:

1. **Define the state** as a plain POCO in `<Module>/Domain/<Name>State.cs`
   (public get/set properties, sane defaults, no logic, no Godot).
   If it lives in a `CollectionRepository`, give it an `int Id` (indexes/deserialization rely on it).

2. **Put it in a repository** (`SingleRepository<T>` or `CollectionRepository<TKey,TValue>`),
   created and seeded in [GameLevel](../../../src/Game/Level/GameLevel.cs).

3. **Register the type** in `GameLevel` next to the others:
   ```csharp
   _jsonConverterStateService.Register(typeof(<Name>State));
   ```
   Without this line the serializer skips it. This is the step people forget — do not skip it.

4. **Add the repository to [SaveService](../../../src/Game/Common/Service/SaveService.cs).**
   Saving is centralized — do NOT write file I/O in the module service. Take the new
   repository in the `SaveService` constructor and add it to both methods, keeping the
   list order identical:
   ```csharp
   // Save():  single state → GetOne(), collection → GetAll()
   var data = new List<object?> { _idRepository.GetOne(), ..., _<name>Repository.GetOne() };
   // Load():  single → Delete()+Update((T)data[i]!);  collection → DeleteAll()+loop Update(key,(T)value!)
   ```
   Bump the `data is not { Count: N }` guard in `Load` to the new slot count.
   Pass the repository into `SaveService` from [GameLevel](../../../src/Game/Level/GameLevel.cs).
   Also reset it in [NewGameService](../../../src/Game/Common/Service/NewGameService.cs)
   (`Delete()`+`Update(new ...)` for single, `DeleteAll()` for collections) so "new game" clears it too.

5. **Custom field types** (not handled by System.Text.Json) need a converter like
   `Vector2JsonConverter` registered in `JsonConverterStateService`.

6. **Observable state?** If the state is observable, remember `SaveService.Load` replaces
   the object — subscribers must re-subscribe after a load (see `add-observer`).

7. **Test** with a mocked `IJsonStateFileHandlerService` (see `SaveServiceTests` and `add-tests`), then run `verify`.
