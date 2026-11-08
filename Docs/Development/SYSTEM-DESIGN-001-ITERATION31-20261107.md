# Iteration 31 — Selectable head sensor models (2026-11-07)

## Scope

Added three data-backed head model choices to the existing `HEAD` part: standard sensor, rangefinder antenna, and omni optical array. The model ID is stored on each saved design's head part, so one design can use a different sensor than another. Files without a model ID keep the standard sensor; unsupported IDs are rejected. The customization editor shows distinct head silhouettes and refreshes the live portrait when a model is changed. Applying a design attaches its selected sensor geometry to the fighter head.

These choices are visual only. Existing radar and other head performance values continue to be edited through the head part numbers; the model choice does not introduce new targeting rules or hidden stat changes.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237360314 bytes` in `Logs/Head-Visual-Build-20261107.log`.
- Built-player balance editor study passed 119 checks and captured 23 images in `Logs/Head-Visual-Study-20261107.log`, including compatibility fallback, invalid ID rejection, saved-profile round trip, and live rangefinder/omni model assembly checks.
- Built-player smoke test passed 375 checks in `Logs/Head-Visual-Smoke-20261107.log`.
- 3D model previews: `Logs/Balance-Editor-Test-20261107-143738/Screenshots/02h-rangefinder-head-preview.png` and `02i-omni-head-preview.png`.
- `git diff --check` passed. Existing dirty `Assets/Scenes/Arena.unity` was preserved byte-for-byte around the build; pre/post SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

## Remaining user requests

Continue adding distinct models to other part slots. Other queued items: improve quadruped gait; vary weapon statistics and evaluate them with simulated duels; add explosive or homing behavior for suitable non-lock-on weapons; scale map size to participant count; and make the selected weapon obvious in the HUD or reticle. UI polish remains lower priority than completing functionality.

All work remains local on `work`; no commit, push, merge, or `main` change.
