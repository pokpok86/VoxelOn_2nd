# Iteration 22 — Per-design multiline description (2026-11-07)

## Scope

Implemented one slice of Max request #2: each of the eight player design slots now has an editable multiline description, saved as part of that design. The field is limited to 1,000 characters. Older design documents without the optional field load with an empty description. Copy/paste includes the description, and resetting a slot restores its default description. Three sample designs include short Korean descriptions.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237323822 bytes` in `Logs/Iteration22-Build-Final-20261107-195950.log`.
- Built-player balance editor study passed 96 checks, including multiline round-trip persistence, clipboard independence, reset, legacy normalization, and editor capture: `Logs/Iteration22-Editor-Study-Final.log`.
- Built-player smoke test passed 373 checks: `Logs/Iteration22-Smoke-Final.log`.
- Captured the visible loadout editor with the two-line description field at `Logs/Balance-Editor-Test-20261107-110103/Screenshots/03-independent-loadout.png`.
- `git diff --check` passed. `Assets/Scenes/Arena.unity` was restored to its pre-build SHA-256 `0D8A557C49A0477753CC5DE6C55E3B57D72CD65A11686D79DEB474A105BF9B14`; the build folder was backed up under `Logs/Builds-Windows-before-iteration22-20261107-195650`.

## Remaining in request #2

The other requested work remains: usable empty design slots, part/color editing, delete flow, and encrypted design-code import/export with compatibility rules.

All work remains local on `work`; no commit, push, merge, or `main` change.
