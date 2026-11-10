# Settings — player settings

Settings of the player, not of a playthrough: kept in a file of their own
(`user://settings.json`), so **New Game never resets them** and a missing or broken game save
doesn't touch them. Out of the box: windowed or fullscreen, the window size and the language.

> General module conventions — [../README.md](../README.md). This document covers only what
> this module does.

## What it does

**Display mode** — Windowed (default) or Fullscreen ([DisplayMode](Enum/DisplayMode.cs)).
Fullscreen is borderless at the screen's own size (`Window.ModeEnum.Fullscreen`, not exclusive),
so the resolution drop-down is inactive there; the saved resolution comes back in a window.
**F11** or **Alt+Enter** switches it anywhere in the game ([DisplayModeHotkey](UI/DisplayModeHotkey.cs),
a node `GameLevel` adds at start) and saves it.

**Resolution** — the common monitor sizes of every aspect (4:3, 5:4, 16:10, 16:9, 21:9, 32:9),
from `1024 × 768` to `5120 × 2160`, by width then height, shown as "1920 × 1080 (16:9)";
`1280 × 720` is the default. Only the sizes that fit the player's screen are offered (the UI
passes the screen size in; the smallest is always offered). The game is laid out for
1280 × 720 (`project.godot`: viewport 1280 × 720, `window/stretch/mode="canvas_items"`) and
stretched to the window: at 1920 × 1080 everything is 1.5× bigger; a non-16:9 size gets black
bars. Picking a size applies it at once ([GameWindow](UI/GameWindow.cs) — resize, centre on the
screen, never past its usable area so the title bar stays visible) and saves it; at start
`GameLevel` applies the saved mode and size. A saved size that is not offered (unknown, or too
big for this screen) falls back to the default (or the smallest), but stays in the file — on a
bigger screen it comes back.

**Language** — English (default) or Русский ([Language](Domain/Language.cs): the locale code and
the name in that language, never translated). The list is `SettingsService.Languages`, English
first — the language the texts are written in. Picking one switches at once
([GameLanguage](UI/GameLanguage.cs) → `TranslationServer.SetLocale`; Godot re-translates every
control's text) and saves it; at start `GameLevel` applies the saved one before building any
screen, so a fresh install starts in English whatever the OS language. An unknown saved code falls
back to English. How texts are translated — [Texts and translation](../README.md#texts-and-translation);
another language — the [add-language](../../../.claude/skills/add-language/SKILL.md) skill.

**Settings screen** ([SettingsUI](UI/SettingsUI.cs)) — opened from the main menu (**Settings**):
a dimmed overlay with a panel, the display mode, resolution and language drop-downs and **Back**
(Esc too). It watches `SettingsState` (`ISettingsStateObserver`), so F11 pressed while it is open
shows up at once. Its texts are plain control texts (the display modes' names too), so they turn
into the picked language at once.

**Storage** — the same machinery as the game save: one `StateSlot<SettingsState>` in a separate
`SaveService` with its own path, loaded once in the `GameLevel` constructor (fresh when there
is no file). The module does no file I/O: `SettingsService` calls `ISettingsStore.Save()`,
implemented by the adapter [SettingsFileStore](../Level/Adapter/SettingsFileStore.cs). A failed
settings write is not reported — losing a setting is harmless. Adding a setting = a property on
`SettingsState` + a method on the service + a row in the UI.

## Module files

| File | Layer | What it does |
|------|-------|--------------|
| [Enum/DisplayMode.cs](Enum/DisplayMode.cs) | Enum | windowed / fullscreen (saved as a number) |
| [Domain/Resolution.cs](Domain/Resolution.cs) | Domain | width × height, aspect name ("16:9") |
| [Domain/Language.cs](Domain/Language.cs) | Domain | locale code + the name in that language |
| [Domain/SettingsState.cs](Domain/SettingsState.cs) | Domain | display mode, window width / height, language code; setters notify |
| [Domain/Observer/ISettingsStateObserver.cs](Domain/Observer/ISettingsStateObserver.cs) | Domain | state → UI: settings changed |
| [Service/ISettingsStore.cs](Service/ISettingsStore.cs) | Service | save the settings file (implemented in `Level`) |
| [Service/ISettingsService.cs](Service/ISettingsService.cs) | Service | contract |
| [Service/SettingsService.cs](Service/SettingsService.cs) | Service | offered modes / resolutions (fitting the screen) / languages, current ones (with fallback), change + save, toggle the mode |
| [UI/SettingsUI.cs](UI/SettingsUI.cs) | UI | settings screen |
| [UI/GameWindow.cs](UI/GameWindow.cs) | UI | screen size; fullscreen, or resize and centre the window inside the usable area |
| [UI/DisplayModeHotkey.cs](UI/DisplayModeHotkey.cs) | UI | F11 / Alt+Enter → windowed ↔ fullscreen |
| [UI/GameLanguage.cs](UI/GameLanguage.cs) | UI | switch the locale |
| [UI/Observer/ISettingsUIObserver.cs](UI/Observer/ISettingsUIObserver.cs) | UI | UI → Level: closed |
| [UI/Factory/SettingsUIFactory.cs](UI/Factory/SettingsUIFactory.cs) | UI | builds the screen and the hotkey node |

## How it connects

- Wired in [../Level/GameLevel.cs](../Level/GameLevel.cs): `SettingsState` repository → its slot
  → `SaveService("user://settings.json")` → `Load()` or `Reset()` → `SettingsService(repository,
  SettingsFileStore)` → `SettingsUIFactory`. `_Ready` applies the saved mode, size and language
  and adds the `DisplayModeHotkey`.
- [../Level/MainMenuLevel.cs](../Level/MainMenuLevel.cs) hosts the screen over the menu.

## Tests

[SettingsServiceTests](../../../tests/Game.Tests/Settings/Service/SettingsServiceTests.cs) —
display mode (toggle, fallback), offered sizes (fitting the screen) and languages, a change saves
and notifies, same / unoffered refused, fallbacks;
[ResolutionTests](../../../tests/Game.Tests/Settings/Domain/ResolutionTests.cs) — aspect names;
[SettingsAdapterTests](../../../tests/Game.Tests/Level/Adapter/SettingsAdapterTests.cs); JSON round
trip (and an older file without a language) in `JsonConverterStateServiceTests`;
[TranslationTests](../../../tests/Game.Tests/Locale/TranslationTests.cs) — every offered language
but English has a registered, well-formed translation (the display modes' names included).
