---
name: add-language
description: Add a game language — its locale/<code>.po translation, the registration in project.godot and the entry in the settings' language list. Use when asked to add or support another language, or to translate the game into one.
---

# Add a language

Texts are English in code; every other language is a Godot translation file `locale/<code>.po`
(see "Texts and translation" in [src/Game/README.md](../../../src/Game/README.md)). English and
Russian are built in. Ask for the language if not given, then — e.g. German, `de`:

1. **Offer it** — append `new("de", "Deutsch")` to `SettingsService.Languages` in
   [SettingsService.cs](../../../src/Game/Settings/Service/SettingsService.cs): the locale code (as
   Godot names locales: `de`, `pt_BR`) and the name in that language, which is shown untranslated.
   English stays first — the default and the language of the code.

2. **Translate** — copy [locale/ru.po](../../../locale/ru.po) to `locale/de.po`, set its header line
   to `"Language: de\n"` (Godot takes the translation's locale from it, not from the file name), the
   first comment to `# German translation. … msgstr = German.`, and translate every `msgstr` from
   the English `msgid`. Keep every `{0}` / `{1:HH:mm:ss}`, one line per msgid / msgstr, the module
   comments; no plural forms (counts are written `Items: {0}`). Keep it about as short as the
   English — it must fit the same place.

3. **Register** it in `project.godot`:
   `locale/translations=PackedStringArray("res://locale/ru.po", "res://locale/de.po")`.

4. **Tests** — [TranslationTests](../../../tests/Game.Tests/Locale/TranslationTests.cs) take the
   languages from `SettingsService.Languages`: the file, its header and its registration, every text
   translated once, the placeholders, the data texts, the same texts as the other translations.
   Update the expected list in `SettingsServiceTests.Languages_AreEnglishByDefaultAndRussian`
   (and its name); its unoffered / unknown language is `xx`, which no real one uses.

5. **Docs** — the language list in [Settings/README.md](../../../src/Game/Settings/README.md),
   [src/Game/README.md](../../../src/Game/README.md) ("Texts and translation"), the root
   [README.md](../../../README.md) and the `Texts` rule in [CLAUDE.md](../../../CLAUDE.md).

6. Run `verify`, then `run-godot` with `TranslationServer.SetLocale("de");` as the first demo line
   to see every screen in the new language.

From then on every new text goes to every `locale/*.po` — the tests fail on a translation that
lacks one.
