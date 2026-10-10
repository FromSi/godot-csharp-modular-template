# Example — a sample feature module

A ready-made example of a single module (feature). It does nothing important — it
shows an input field and three buttons. Its purpose is to be a **structure template**:
copy the folder for your own feature and rename the classes.

The module is self-contained: you can delete it entirely (removing the wiring in
[../Level/GameLevel.cs](../Level/GameLevel.cs)) and the rest of the template keeps working.

> The general module conventions (the `Domain → Service → UI/Factory` layers, the
> dependency rules, the "how to make your own module" steps) are in
> [../README.md](../README.md). This document covers what this example specifically does.

## What the demo does

The game screen opened by the main menu's Continue / New Game: an input field and
**Random / Save / Menu** buttons:

- **Random** — puts a random number into the field (`IRandomGeneratorService` from `Common`);
- every change of the note is stamped with the time from `IClockService` (`NoteState.ChangedAt`);
  the label shows it — "changed at 12:34:56" or "never changed";
- **Save** — commits the field's value to the note and asks the level to save
  (`GameLevel.SaveGame()`); the status line shows "Saved" or a red "Could not save the game";
- **Menu** (or Esc) — raises a UI intent to the level (see below), which saves and returns
  to the main menu; if the save fails, it asks "Exit without saving?".

Load and New Game are not here — they are the menu's Continue / New Game, done *before* this
screen is built (see [../Level/README.md](../Level/README.md)).

The field is kept numeric (non-digits are stripped on input — `LineEdit` has no built-in
numeric-only mode; `SpinBox` is the out-of-the-box alternative). All controls sit in a
`VBoxContainer` inside a `CenterContainer`, so the column stays centered at any resolution.

It shows the observer pattern in **both directions**:
- **state → UI**: `NoteState` extends `Common`'s `ObservableState<INoteStateObserver>`;
  `ExampleUI` subscribes and updates the field/label on change (see the Observer section
  in [../Common/README.md](../Common/README.md)).
- **UI → Level**: `ExampleUI` publishes `IExampleUIObserver`; the hosting
  [ExampleLevel](../Level/ExampleLevel.cs) listens and handles navigation (Menu), so the
  UI stays free of navigation logic.

So it shows three things from `Common` in action: the random number generator, the clock
(`IClockService` — mocked in tests, so the time stamp is asserted exactly) and the central
save system (`SaveService`). `NoteService` itself does no file I/O — it only
owns the in-memory note.

`ExampleUI` subscribes once in `_Ready` (and renders the current value), unsubscribes in
`_ExitTree`; the "Current value" label then changes only through `OnTextChanged`.

## Module files

| File | Layer | What it does |
|------|-------|--------------|
| [Domain/NoteState.cs](Domain/NoteState.cs) | Domain | observable state: a text + when it last changed |
| [Domain/Observer/INoteStateObserver.cs](Domain/Observer/INoteStateObserver.cs) | Domain | state → UI listener contract |
| [Service/INoteService.cs](Service/INoteService.cs) | Service | logic contract |
| [Service/NoteService.cs](Service/NoteService.cs) | Service | in-memory note: get / set (time-stamped) / random / (un)subscribe (no file I/O) |
| [UI/ExampleUI.cs](UI/ExampleUI.cs) | UI | screen: field + buttons; observes state, publishes UI intents |
| [UI/Observer/IExampleUIObserver.cs](UI/Observer/IExampleUIObserver.cs) | UI | UI → Level intent contract |
| [UI/Factory/ExampleUIFactory.cs](UI/Factory/ExampleUIFactory.cs) | UI | builds the UI with dependency injection |

Note how responsibility is split: `NoteService` knows nothing about Godot (and so is
tested without the engine), while `ExampleUI` holds no logic — it just calls the
services and displays the result. Persistence lives in `Common`'s `SaveService`, not
in the module.

## Data flow (Save example)

```
Save button (ExampleUI)
        │  _noteService.SetText(text)      → note state changes (observers fire)
        │  OnSaveRequested()               → ExampleLevel → GameLevel.SaveGame()
        ▼
SaveService.Save()  → bool, reported back via ExampleUI.ShowSaveResult
        │  gathers every state slot (IdState, NoteState, …)
        ▼
IJsonStateFileHandlerService.Store([...states], "user://saves/game.save")
        ▼
game.save.tmp → read back → renamed over game.save
```

Load goes through the menu's Continue → `SaveService.Load()`, which restores every slot
from the file; only then is this screen built, so `ExampleUI` subscribes to the loaded note.

For how `Store`/`Load` work, type registration, `user://`, and the central `SaveService`
— see the FileHandler and SaveService sections in [../Common/README.md](../Common/README.md).

## How it connects to the rest of the project

- The wiring (repository, service, factory) and the `StateSlot<NoteState>` that makes the
  note persistent are in [../Level/GameLevel.cs](../Level/GameLevel.cs).
- The screen is the game screen [../Level/ExampleLevel.cs](../Level/ExampleLevel.cs), built
  by `GameLevel.EnterGame()` (see [../Level/README.md](../Level/README.md)).

## Tests

Engine-free logic is covered without a real disk:
[NoteServiceTests](../../../tests/Game.Tests/Example/Service/NoteServiceTests.cs) (in-memory
note + observer) and [JsonConverterStateServiceTests](../../../tests/Game.Tests/Common/Service/JsonConverterStateServiceTests.cs)
(`NoteState` survives a real JSON round trip). We test the `Service` precisely because it isn't tied to Godot.
