# Iteration 18 — Booster, skill selection and visual loadout tiles (2026-11-07)

Work remains local on `work`; no commit, push, merge or `main` change.

## Changes

- Added a per-design skill selector using the eight existing implemented special moves. Each saved design retains its own skill ID; older design files receive the matching chassis default.
- Kept the existing data-driven `BOOSTER` part and its implemented boost speed, energy drain and vertical thrust controls, and surfaced it as an icon tile in the part selector.
- Replaced weapon and part option names in the choice tiles with rendered equipment art or trait glyphs. The selected-item detail area keeps its name, explanation and performance readout.
- Added a cyan location outline on the central rig for the selected equipment mount or body part. Equipment changes refresh the draft portrait immediately.
- Filtered the part tiles to the active chassis. Draft portrait generation restores the previously applied balance after drawing, so an unapplied preview does not become the battle configuration.

## Limits

- The current catalog has one tunable booster definition per chassis, not multiple swappable booster models. The part selector exposes and highlights the existing booster controls without inventing new art or combat rules.
- Part options are currently data/control entries, so their glyphs represent function; the chassis model geometry does not change when a part entry is selected.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237317710 bytes` in `Logs/Iteration18-Verified-Build-20261107-191736.log`.
- Built-player balance editor study passed: 92 checks and 16 captured images in `Logs/Iteration18-Verified-Editor-Study.log`.
- Built-player smoke test passed: 369 checks in `Logs/Iteration18-Verified-Smoke.log`.
- Inspected the booster, skill and loadout screenshots. Mouse drag itself was not manually exercised. The generated `Assets/Scenes/Arena.unity` IDs were restored to the clean pre-build version.
- `git diff --check` passed. Existing `Builds/Windows` content was copied to dated `Logs/Builds-Windows-before-*` folders before builds.
