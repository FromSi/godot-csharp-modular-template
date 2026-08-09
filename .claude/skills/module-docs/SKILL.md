---
name: module-docs
description: Create or update a module's README in the project's documentation style, without duplicating the src/Game hub. Use after adding or changing a module, or when asked to document a module.
---

# Module docs

Keep docs deduplicated: shared concepts (three roles, dependency direction, module
anatomy, namespaces, "how to add a feature") live only in
[src/Game/README.md](../../../src/Game/README.md). A module README covers only what is
specific to that module and links to the hub/Common for shared mechanics.
Model it on [src/Game/Example/README.md](../../../src/Game/Example/README.md).

Write `src/Game/<Module>/README.md` in English with these sections:

1. **Title + one-line purpose** — what the module does and why.
2. **A short "> see ../README.md for the general conventions"** callout — don't re-explain layers or DI.
3. **What it does** — concrete behavior/features.
4. **Module files** — a table: file (linked) | layer | what it does.
5. **Data flow** — a small ASCII diagram if the module has a non-obvious flow (e.g. save/load).
6. **How it connects** — where it's wired in [GameLevel](../../../src/Game/Level/GameLevel.cs) and which screen shows it.
7. **Tests** — link to the module's tests.

Style: terse, for an experienced C# dev. No filler ("this module", "as we can see"),
no explaining the obvious from a method name. Relative, clickable links only.

If a README already exists, edit only the changed parts — don't rewrite from scratch.
After creating/renaming, keep cross-links consistent with the neighbouring READMEs.
