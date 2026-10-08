---
name: run-godot
description: Launch the Godot project to verify a change in the running app (all UI is built in code, so behavior must be seen live). Use when asked to run/start the game or confirm a UI change actually works.
---

# Run the Godot project

Because UI is built entirely in C# (no `.tscn` for UI), a build passing is not proof
the screen looks/behaves right — run it.

1. **Find the editor binary.** It must be the **.NET / Mono** build: check
   `which godot-mono godot godot4`, then names like `Godot_v4.*_mono*`. If none is found,
   tell the user and ask for the path — do not guess. Its version must match the
   `Godot.NET.Sdk` version in `Game.csproj` (`<godot> --version`).

2. **Build C# first** (headless-safe): `dotnet build Game.csproj`. Fix errors before launching.

3. **Run the main scene** from the repo root:
   ```bash
   <godot> --path .
   ```
   Add `--headless --quit-after 2` for a smoke run (no UI, but confirms it boots).

4. **See it without a human — record frames.** Movie Maker mode renders a fixed number of
   frames to PNGs (needs a display/GPU, not `--headless`), then quits:
   ```bash
   SP=<scratchpad>/frames && rm -rf "$SP" && mkdir -p "$SP"
   timeout 120 <godot> --path . --write-movie "$SP/f.png" --fixed-fps 10 --quit-after 35
   ```
   This writes `f00000000.png … f00000034.png` (frame N = N/10 s). Look at the frames that
   matter with the Read tool (it shows images), e.g. one per step of the scenario; several
   can be pasted into one grid with PIL to save reads.

   The game only shows its start screen unless something drives it. To check a flow, add a
   **temporary** demo script — back the file up first and restore it with `command cp -f`
   (plain `cp` is often an interactive `cp -i` alias and would hang waiting for "y"):
   ```bash
   command cp -f src/Game/Level/GameLevel.cs "$SP/../GameLevel.cs.bak"
   # at the end of GameLevel._Ready():
   #   void At(double sec, System.Action act) { GetTree().CreateTimer(sec).Timeout += act; }
   #   At(0.5, StartNewGame);
   #   At(1.5, () => Input.ParseInputEvent(new InputEventAction { Action = "ui_cancel", Pressed = true }));
   #   At(2.5, () => GD.Print("continue: ", ContinueGame()));
   dotnet build Game.csproj && timeout 120 <godot> --path . --write-movie "$SP/f.png" --fixed-fps 10 --quit-after 35
   command cp -f "$SP/../GameLevel.cs.bak" src/Game/Level/GameLevel.cs
   git diff src/Game/Level/GameLevel.cs   # must show no demo lines
   ```
   Always restore, even if the run fails, and rebuild afterwards. `GD.Print` output appears
   in the console next to the Movie Maker summary.

5. **New `.cs` files need `.uid` files** (they are committed). A normal run creates only some;
   `timeout 120 <godot> --headless --path . --import` generates the rest.

6. **Report** what you observed (started cleanly / errors in console / expected UI on the
   frames). If no display is available, say so and fall back to `verify` (build + tests) plus a
   note that a human should run it.

Prefer the built-in `run` skill if it already covers this project better; this skill is the Godot-specific fallback.
