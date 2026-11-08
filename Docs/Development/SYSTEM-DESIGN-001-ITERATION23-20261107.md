# Iteration 23 — Clear and reuse player design slots (2026-11-07)

## Scope

Added a reusable empty state to the eight stable player design slots. “슬롯 비우기” clears one slot while preserving its ID. The Hangar labels it `EMPTY`, disables battle apply, and lets the user pick WISP, BASTION, or TARANTULA to create a fresh design using that chassis's default loadout and registered parts. Copy/paste, save/load, and reset work with empty slots. Existing files without the new optional `isEmpty` field continue to load as populated designs.

The slot is cleared rather than permanently removed, so the eight stable slot IDs remain available. Part/color editing and encrypted design-code import/export remain unimplemented.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237328866 bytes` in `Logs/Iteration23-Build-Final.log`.
- Built-player balance editor study passed 102 checks, including clear, stable ID, empty-slot copy/paste and save/load, chassis-based recreation, and occupied-slot rejection: `Logs/Iteration23-Design-Study-Final.log`.
- Built-player smoke test passed 373 checks: `Logs/Iteration23-Smoke-Final.log`.
- Visible empty-slot editor capture: `Logs/Balance-Editor-Test-20261107-113245/Screenshots/03-empty-design-slot.png`.
- `git diff --check` passed. `Assets/Scenes/Arena.unity` was restored to pre-build SHA-256 `0D8A557C49A0477753CC5DE6C55E3B57D72CD65A11686D79DEB474A105BF9B14`; a Windows build backup is under `Logs/Builds-Windows-before-iteration23-20261107-202807`.

All work remains local on `work`; no commit, push, merge, or `main` change.
