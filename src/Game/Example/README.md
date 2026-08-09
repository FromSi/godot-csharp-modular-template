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

A screen with an input field and **Random / Save / Load** buttons:

- **Random** — puts a random number into the field (`IRandomGeneratorService` from `Common`);
- **Save** — commits the field's value to the note, then persists via the central `SaveService`;
- **Load** — restores the game through `SaveService`; the field and label show the loaded value;
- **New** — resets the game via `NewGameService`;
- **Quit** — raises a UI intent to the level (see below), which quits.

The field is kept numeric (non-digits are stripped on input — `LineEdit` has no built-in
numeric-only mode; `SpinBox` is the out-of-the-box alternative). All controls sit in a
`VBoxContainer` inside a `CenterContainer`, so the column stays centered at any resolution.

It shows the observer pattern in **both directions**:
- **state → UI**: `NoteState` extends `Common`'s `ObservableState<INoteStateObserver>`;
  `ExampleUI` subscribes and updates the field/label on change (see the Observer section
  in [../Common/README.md](../Common/README.md)).
- **UI → Level**: `ExampleUI` publishes `IExampleUIObserver`; the hosting
  [ExampleLevel](../Level/ExampleLevel.cs) listens and handles navigation (Quit), so the
  UI stays free of navigation logic.

So it shows two things from `Common` in action: the random number generator and the
central save system (`SaveService`). `NoteService` itself does no file I/O — it only
owns the in-memory note.

It also demonstrates the **observer pattern**: `NoteState` extends `Common`'s
`ObservableState<INoteStateObserver>`, and `ExampleUI` subscribes to it. The
"Current value" label is updated only through `OnTextChanged` — the UI reacts to the
state on Save/Load instead of re-reading the service. See the Observer section in
[../Common/README.md](../Common/README.md).

## Module files

| File | Layer | What it does |
|------|-------|--------------|
| [Domain/NoteState.cs](Domain/NoteState.cs) | Domain | observable state: a single text |
| [Domain/Observer/INoteStateObserver.cs](Domain/Observer/INoteStateObserver.cs) | Domain | state → UI listener contract |
| [Service/INoteService.cs](Service/INoteService.cs) | Service | logic contract |
| [Service/NoteService.cs](Service/NoteService.cs) | Service | in-memory note: get / set / random (no file I/O) |
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
        │  _saveService.Save()             → central save
        ▼
SaveService.Save()
        │  gathers every repository (IdState, NoteState, …)
        ▼
IJsonStateFileHandlerService.Store([...states], "user://saves/game.save")
        ▼
JSON file on disk
```

Load goes through `SaveService.Load()`, which restores every repository from the file.
Because it replaces the state objects, `ExampleUI` re-subscribes to the fresh note
state afterwards (`SubscribeAndRender`).

For how `Store`/`Load` work, type registration, `user://`, and the central `SaveService`
— see the FileHandler and SaveService sections in [../Common/README.md](../Common/README.md).

## How it connects to the rest of the project

- The wiring (repository, service, factory) and registering `NoteState` for saving
  are in [../Level/GameLevel.cs](../Level/GameLevel.cs); `NoteState`'s repository is
  handed to `SaveService` there.
- The screen is shown through the level-observer
  [../Level/ExampleLevel.cs](../Level/ExampleLevel.cs) (see [../Level/README.md](../Level/README.md)).

## Tests

Engine-free logic is covered without a real disk:
[NoteServiceTests](../../../tests/Game.Tests/Example/Service/NoteServiceTests.cs) (in-memory
note + observer) and [SaveServiceTests](../../../tests/Game.Tests/Common/Service/SaveServiceTests.cs)
(save/load with a mocked file handler). We test the `Service` precisely because it isn't tied to Godot.
