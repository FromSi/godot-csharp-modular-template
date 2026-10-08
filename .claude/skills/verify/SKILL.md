---
name: verify
description: Build and test the project as a quality gate — zero build warnings and green tests. Use after making code changes, or when asked to verify, check, or validate the build.
---

# Verify (quality gate)

Run all of these from the repo root:

```bash
dotnet build Game.csproj --no-incremental   # game code
dotnet test Game.sln                        # all tests

# style: lines over 120 chars, trailing whitespace / missing final newline
git ls-files -co --exclude-standard '*.cs' | xargs awk 'length > 120 { print FILENAME ":" FNR ": " length }'
git diff --check HEAD                       # changed tracked files
git ls-files -o --exclude-standard '*.cs' | xargs -r grep -n '[[:space:]]$'   # new, untracked files
```

(`xargs` keeps it working in zsh, which doesn't word-split `$files`.)

Pass criteria (this template's policy):
- **0 errors and 0 warnings.** `<Nullable>enable</Nullable>` is on — warnings are not acceptable. If new warnings appear, fix them, don't suppress.
- **All tests green.**
- **The style commands print nothing.** Wrap long lines (break a parameter list / `where`
  clause / chained call onto the next line); strip trailing whitespace.

If it fails:
- Report the actual compiler/test output (don't paraphrase).
- Fix the root cause, then re-run until clean.

Common gotchas to check when it breaks:
- Namespace must be `Game.Game.<Module>.<Layer>` (double `Game`).
- No Godot API references in `Domain`/`Service`/`Repository`.
- A new persistable state has no `StateSlot<T>` in `GameLevel`'s `slots` list, or isn't in
  `JsonConverterStateServiceTests` (see `add-persistable-state`).
- Order-dependent test failures: a mutable test field initialized inline instead of in `[SetUp]`
  (NUnit reuses one fixture instance; see `add-tests`).

Report the final result plainly: what built, how many tests passed, any remaining warnings.
