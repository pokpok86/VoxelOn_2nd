# Iteration 25 — Restore base finish when paint is cleared (2026-11-07)

## Scope

Updated design-paint application so selecting the default finish restores each eligible chassis renderer's own material color in its material property block. This also clears a previous design tint when the applied design has no paint value. Weapon, shield, saber, and temporary effect renderers remain excluded. Existing property-block data is read first and retained while the color is updated.

Added a runtime editor-study check that applies an orange design tint, clears the design tint, reapplies the paint helper, and compares the chassis property-block color to its source material color.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237336046 bytes` in `Logs/Iteration25-Build-20261107-iteration25-final.log`.
- Built-player balance editor study passed 106 checks, including paint apply and restore: `Logs/Iteration25-Design-Study-Final-20261107.log`.
- Built-player smoke test passed 373 checks: `Logs/Iteration25-Smoke-Final-20261107.log`.
- `git diff --check` passed. Dirty `Assets/Scenes/Arena.unity` was copied before the scene-generation build and restored after Unity exited. Pre/post SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

All work remains local on `work`; no commit, push, merge, or `main` change.
