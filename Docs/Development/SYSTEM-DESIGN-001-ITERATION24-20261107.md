# Iteration 24 — Design paint palette (2026-11-07)

## Scope

Added a per-design exterior paint color to the hangar editor. The palette offers the default finish plus ivory, orange, cyan, olive, steel, and purple. The selected color is stored in `BalanceDesign.paintColorHex`, copied with design slots, validated as a six-digit RGB value, and applied to chassis renderers through material property blocks. Weapon and effect renderers are excluded. The editor's robot preview updates after a color selection.

The WISP imported head bone has no separate child renderer, so a paint color applied to that bone cannot selectively recolor the head. This iteration therefore applies a uniform chassis tint per design. Independent per-part paint requires renderable part surfaces/model separation. Encrypted design-code import/export remains unimplemented.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237334769 bytes` in `Logs/Iteration24-Build-Final-20261107-211050.log`.
- Built-player balance editor study passed 105 checks, including paint save/normalization, copy/paste/reset, and live chassis material-property-block application: `Logs/Iteration24-Design-Study-20261107-211210.log`.
- Built-player smoke test passed 373 checks: `Logs/Iteration24-Smoke-20261107-211256.log`.
- Preview screenshot: `Logs/Balance-Editor-Test-20261107-121214/Screenshots/03c-design-paint-preview.png`.
- `git diff --check` passed. The pre-build dirty `Assets/Scenes/Arena.unity` was backed up before Unity's scene-generation build step and restored after the Unity process exited; its pre/post SHA-256 is `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

All work remains local on `work`; no commit, push, merge, or `main` change.
