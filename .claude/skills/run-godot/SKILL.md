---
name: run-godot
description: Launch the Godot project to verify a change in the running app (all UI is built in code, so behavior must be seen live). Use when asked to run/start the game or confirm a UI change actually works.
---

# Run the Godot project

Because UI is built entirely in C# (no `.tscn` for UI), a build passing is not proof
the screen looks/behaves right — run it.

1. **Find the editor binary.** `godot` may not be on `PATH`. Check `which godot`, then
   common names (`godot4`, `Godot_v4.6*`). It must be the **.NET / Mono** build.
   If none is found, tell the user and ask for the path — do not guess.

2. **Build C# first** (headless-safe): `dotnet build Game.csproj`. Fix errors before launching.

3. **Run the main scene** from the repo root:
   ```bash
   <godot> --path . 
   ```
   Add `--headless --quit-after 2` for a smoke run in CI-like contexts (won't show UI but confirms it boots).

4. **Verify the change** — the app starts on [level/game_level.tscn](../../../level/game_level.tscn).
   Interact with the relevant screen; check `GD.Print` output in the console and any on-screen result.

5. **Report** what you observed (started cleanly / errors in console / expected UI shown). If you can't launch a GUI in this environment, say so and fall back to `verify` (build + tests) plus a note that a human should run it.

Prefer the built-in `run` skill if it already covers this project better; this skill is the Godot-specific fallback.
