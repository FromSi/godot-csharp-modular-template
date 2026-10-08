---
name: add-observer
description: Wire state → UI reactivity using Common's ObservableState<TObserver> — make a state notify listeners and have the UI react instead of polling. Use when asked to add an observer, make the UI react to state changes, or update UI when a model changes.
---

# Add a state observer

Use the generic mechanism in `Common`
([Domain/Observer/ObservableState.cs](../../../src/Game/Common/Domain/Observer/ObservableState.cs));
background in the Observer section of the [Common README](../../../src/Game/Common/README.md).
The snippets below are the whole pattern — no need to open a reference module.

Steps for a module `<Module>` and state `<Name>State`:

1. **Observer interface** — `<Module>/Domain/Observer/I<Name>StateObserver.cs`, namespace
   `Game.Game.<Module>.Domain.Observer`. Put only the change events you need:
   ```csharp
   public interface IScoreStateObserver
   {
       void OnScoreChanged(int score);
   }
   ```

2. **Make the state observable** — extend the base and notify on change:
   ```csharp
   public class ScoreState : ObservableState<IScoreStateObserver>
   {
       public int Score { get; set; }   // public setter only for JSON

       public void Add(int points)
       {
           Score += points;
           Notify(observer => observer.OnScoreChanged(Score));
       }
   }
   ```
   Mutate **only** through such methods so observers fire.

3. **Expose (un)subscription from the service** (UI depends on Service, not the repository):
   ```csharp
   public void Subscribe(IScoreStateObserver observer) => _repository.GetOne().AddObserver(observer);
   public void Unsubscribe(IScoreStateObserver observer) => _repository.GetOne().RemoveObserver(observer);
   ```
   Route every change through the service so it calls `state.Add(...)`.

4. **The UI observes** — subscribe and render once in `_Ready`, unsubscribe in `_ExitTree`:
   ```csharp
   public partial class ScoreUI : Control, IScoreStateObserver
   {
       public override void _Ready()
       {
           // build nodes …
           _scoreService.Subscribe(this);
           OnScoreChanged(_scoreService.Get());   // initial render
       }

       public override void _ExitTree()
       {
           _scoreService.Unsubscribe(this);
       }

       public void OnScoreChanged(int score) { _label.Text = $"Score: {score}"; }
   }
   ```

## The flow that keeps subscriptions valid
`SaveService.Load` and `NewGameService.Create` **replace** state objects (`Delete()` +
`Update()`). A UI subscribed before them keeps watching the old objects. So the template
never loads or resets while a game UI exists:

```
MainMenu ──Continue──► SaveService.Load()      ──┐
         ──New Game──► NewGameService.Create() ──┴► GameLevel.EnterGame(): build game screen
                                                    (UIs subscribe in _Ready) → OpenLevel(Main)
Game ──Esc/Menu──► GameLevel.ReturnToMenu(): Save → OpenLevel(MainMenu) → free game screen
                                             (UIs unsubscribe in _ExitTree)
```

- Host game UIs inside the game screen that `GameLevel.EnterGame()` creates — not in a
  session screen built in `_Ready`.
- Don't put Load / New Game buttons on the game screen; they belong to the main menu.
- If something long-lived must observe a state across loads (rare), re-subscribe it right
  after `Load()`/`Create()` in `GameLevel` — never rely on the old object.

## Critical rules
- **Serialization still works** — the observer list is a private field, not serialized.
- **No `virtual`** — the base uses a non-virtual `Notify(Action<TObserver>)` helper.
- `Level`/`Node`-derived classes can't extend `ObservableState` (single inheritance);
  they keep a manual `List<IObserver>` like `GameLevel` does with `ILevelObserver`.

## UI → Level (the other direction)
When the UI must report an intent (navigation, "back to menu") rather than react to state,
the UI is the publisher and the level the listener:

```csharp
// <Module>/UI/Observer/IScoreUIObserver.cs
public interface IScoreUIObserver { void OnMenuRequested(); }

// in ScoreUI (a Node, so a manual list instead of ObservableState)
private readonly List<IScoreUIObserver> _observers = [];

public void AddObserver(IScoreUIObserver observer)
{
    if (!_observers.Contains(observer))
    {
        _observers.Add(observer);
    }
}

private void OnMenuPressed()
{
    foreach (var observer in _observers)
    {
        observer.OnMenuRequested();
    }
}

// in the hosting level screen, implementing IScoreUIObserver
public override void _Ready()
{
    var ui = _scoreUiFactory.Create();
    ui.AddObserver(this);   // before AddChild
    AddChild(ui);
}

public void OnMenuRequested() => _gameLevel.ReturnToMenu();
```

Keeps navigation logic in the level, not the UI.

## Tests
Test that a change notifies subscribers and that unsubscribing stops it (mock the observer
interface, `Verify` the call):
```csharp
var observer = new Mock<IScoreStateObserver>();
_service.Subscribe(observer.Object);

_service.AddPoints(5);

observer.Verify(o => o.OnScoreChanged(5), Times.Once);
```
The generic base (add/remove/idempotent) is already covered by
[ObservableStateTests](../../../tests/Game.Tests/Common/Domain/ObservableStateTests.cs).
Then run the `verify` skill.
