# Iteration 30 — Selectable booster models (2026-11-07)

## Scope

Added two model choices to the existing data-driven booster part: a finned vector pack with gimballed nozzles, and a rear-facing cyclone turbine with an eight-vane rotor. The default twin-canister pack remains available. The selected model ID is stored in each design's existing part record, loaded with old files as the twin-canister default, and applied to the hangar portrait and equipped chassis. The booster editor now previews the three choices and the schematic tile changes with the selected model.

These are visual modules attached to the existing backpack socket. Their model choice does not change booster performance or add colliders; the booster part's implemented performance numbers remain editable separately.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237354074 bytes` in `Logs/Iteration30-Build-Final-20261107.log`.
- Built-player balance editor study passed 114 checks in `Logs/Iteration30-Editor-Study-Final-20261107.log`, including legacy-file fallback, rejection of unknown visual IDs, JSON persistence, and distinct vector/cyclone model assemblies.
- Built-player smoke test passed 375 checks in `Logs/Iteration30-Smoke-20261107.log`.
- Captured 3D previews: `Logs/Balance-Editor-Test-20261107-141637/Screenshots/02f-vector-booster-preview.png` and `02g-cyclone-booster-preview.png`.
- An AI-proxy baseline run also completed 12 scenarios and wrote `Docs/Validation/PD14-Balance-Study.csv`; this records a baseline, not a weapon-balance approval.
- `git diff --check` passed. Dirty `Assets/Scenes/Arena.unity` was preserved byte-for-byte around the build; pre/post SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

## Remaining user requests

Continue adding distinct part models. Other queued items: improve quadruped gait; vary weapon statistics and evaluate them with simulated duels; add explosive or homing behavior for suitable non-lock-on weapons; scale map size to participant count; and make the selected weapon obvious in the HUD or reticle. UI polish remains lower priority than completing functionality.

All work remains local on `work`; no commit, push, merge, or `main` change.
