---
name: verify
description: Build and test the project as a quality gate — zero build warnings and green tests. Use after making code changes, or when asked to verify, check, or validate the build.
---

# Verify (quality gate)

Run both, from the repo root:

```bash
dotnet build Game.csproj --no-incremental   # game code
dotnet test Game.sln                        # all tests
```

Pass criteria (this template's policy):
- **0 errors and 0 warnings.** `<Nullable>enable</Nullable>` is on — warnings are not acceptable. If new warnings appear, fix them, don't suppress.
- **All tests green.**

If it fails:
- Report the actual compiler/test output (don't paraphrase).
- Fix the root cause, then re-run until clean.

Common gotchas to check when it breaks:
- Namespace must be `Game.Game.<Module>.<Layer>` (double `Game`).
- No Godot API references in `Domain`/`Service`/`Repository`.
- A new persistable state was not registered in `JsonConverterStateService` (see `add-persistable-state`).

Report the final result plainly: what built, how many tests passed, any remaining warnings.
