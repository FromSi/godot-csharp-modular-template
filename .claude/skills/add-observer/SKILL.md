---
name: add-observer
description: Wire state → UI reactivity using Common's ObservableState<TObserver> — make a state notify listeners and have the UI react instead of polling. Use when asked to add an observer, make the UI react to state changes, or update UI when a model changes.
---

# Add a state observer

Use the generic mechanism in `Common`
([Domain/Observer/ObservableState.cs](../../../src/Game/Common/Domain/Observer/ObservableState.cs)).
Reference implementation: [NoteState](../../../src/Game/Example/Domain/NoteState.cs),
[NoteService](../../../src/Game/Example/Service/NoteService.cs),
[ExampleUI](../../../src/Game/Example/UI/ExampleUI.cs). See the Observer section in the
[Common README](../../../src/Game/Common/README.md).

Steps for a module `<Module>` and state `<Name>State`:

1. **Observer interface** — `<Module>/Domain/Observer/I<Name>StateObserver.cs`, namespace
   `Game.Game.<Module>.Domain.Observer`. Put only the change events you need:
   ```csharp
   public interface I<Name>StateObserver { void OnSomethingChanged(<type> value); }
   ```

2. **Make the state observable** — extend the base and notify on change:
   ```csharp
   public class <Name>State : ObservableState<I<Name>StateObserver>
   {
       public <type> Value { get; set; }             // public setter only for JSON
       public void Change(<type> value)
       {
           Value = value;
           Notify(observer => observer.OnSomethingChanged(value));
       }
   }
   ```
   Mutate **only** through such methods so observers fire.

3. **Expose subscription from the service** (UI depends on Service, not the repository):
   ```csharp
   public void Subscribe(I<Name>StateObserver observer) => _repository.GetOne().AddObserver(observer);
   ```
   Route every change through the service so it calls `state.Change(...)`.

4. **The UI observes** — implement the interface, subscribe in `_Ready`, react in the callback:
   ```csharp
   public partial class <Name>UI : Control, I<Name>StateObserver
   {
       public override void _Ready() { ...; _service.Subscribe(this); }
       public void OnSomethingChanged(<type> value) { /* update nodes */ }
   }
   ```

## Critical rules
- **Re-subscribe after a load.** The central `SaveService.Load` replaces state objects
  (`Delete()`+`Update()`), so any observer subscribed to the old object is now stale.
  After a successful load, re-subscribe the UI to the fresh state and re-render — see
  `SubscribeAndRender` in [ExampleUI](../../../src/Game/Example/UI/ExampleUI.cs).
- **Serialization still works** — the observer list is a private field, not serialized.
- **No `virtual`** — the base uses a non-virtual `Notify(Action<TObserver>)` helper.
- `Level`/`Node`-derived classes can't extend `ObservableState` (single inheritance);
  they keep a manual `List<IObserver>` like `GameLevel` does with `ILevelObserver`.

## UI → Level (the other direction)
When the UI must report an intent (navigation, "quit", "back") rather than react to
state, the UI is the publisher and the level the listener:
1. `<Module>/UI/Observer/I<Name>UIObserver.cs` with the intent methods (e.g. `OnQuitRequested`).
2. The UI keeps a manual `List<I<Name>UIObserver>` + `AddObserver` and notifies on the
   action (it's a `Node`, so it can't extend `ObservableState`).
3. The hosting level implements the interface, `ui.AddObserver(this)` in `_Ready`
   **before** `AddChild`, and translates the intent (e.g. `_gameLevel.OpenLevel(...)`).
Keeps navigation logic in the level, not the UI. Reference:
[IExampleUIObserver](../../../src/Game/Example/UI/Observer/IExampleUIObserver.cs) +
[ExampleLevel](../../../src/Game/Level/ExampleLevel.cs).

## Tests
Test that a change notifies subscribers (mock the observer interface, `Verify` the call)
and that the generic base behaves (add/remove/idempotent) — see
[ObservableStateTests](../../../tests/Game.Tests/Common/Domain/ObservableStateTests.cs)
and the `Subscribe_*` test in
[NoteServiceTests](../../../tests/Game.Tests/Example/Service/NoteServiceTests.cs).
Then run the `verify` skill.
