# Iteration 32 — Selectable core models (2026-11-08)

## Scope

Added three data-backed body/core appearances to each design's existing `CORE` part: standard core, reactor module, and bulwark chest armor. The customization editor previews the silhouettes and updates the 3D design portrait when the selected appearance changes. Applying the loadout creates the selected visual module on the fighter. Existing profiles without a model ID retain the standard body; unsupported core model IDs are rejected.

The appearances are visual only. They do not alter health, armor, booster capacity, collision, or other gameplay rules. The shoulder-only weapon connector remains queued for a later shoulder-weapon pass, separate from hand-weapon mounts.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237367094 bytes` in `Logs/Core-Visual-Build-20261108.log`.
- Built-player balance editor study passed 124 checks and captured 25 images in `Logs/Core-Visual-Study-20261108.log`, including old-profile defaults, unknown ID rejection, JSON round trip, and distinct reactor/bulwark assemblies.
- Built-player smoke test passed 375 checks in `Logs/Core-Visual-Smoke-20261108.log`.
- 3D previews: `Logs/Balance-Editor-Test-20261107-151143/Screenshots/02j-reactor-core-preview.png` and `02k-bulwark-core-preview.png`.
- `git diff --check` passed. Existing dirty `Assets/Scenes/Arena.unity` remained byte-identical around builds; SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

## Remaining user requests

Continue adding part models, then improve quadruped gait; vary weapon stats and evaluate them in simulated duels; add explosion or homing behavior to suitable non-lock-on weapons; scale maps to participant count; and make the active weapon obvious in the HUD or reticle. When shoulder-exclusive weapons are developed, add a dedicated shoulder-to-weapon connector distinct from hand mounts.

All work remains local on `work`; no commit, push, merge, or `main` change.
