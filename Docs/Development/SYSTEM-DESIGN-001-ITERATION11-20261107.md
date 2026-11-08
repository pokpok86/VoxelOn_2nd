# Iteration 11 — Combat HUD health bar polish (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Visual change

- Added a dark inset, thin highlight and four damage-reading segments to the player and rival hull/shield bars.
- Added cyan and orange vertical identity marks beside the corresponding HUD blocks. Combat values and damage rules are unchanged.
- Captured and inspected a real gameplay HUD frame. The capture overwrote three tracked Canyon preview images temporarily; they were restored to their pre-run `HEAD` versions after review. `Assets/Scenes/Arena.unity` was also restored after the build script regenerated its scene object IDs; it was clean before the build.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237289286 bytes` in `Logs/HUD-HealthBar-20261107-Build.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/HUD-HealthBar-20261107-Smoke.log`.
- A visible gameplay UI capture passed in `Logs/HUD-HealthBar-20261107-Capture.log` and was inspected. An earlier hidden-window capture returned a blank frame and was not used as evidence.
- `git diff --check` passed. No screenshot or scene changes remain from validation.

## Follow-up

Continue bounded visible graphics improvements alongside the unfinished match lifecycle work, preserving the existing gameplay rules and approved art. The user-facing roster/mode entry point remains pending participant-count and scoring decisions.
