# Iteration 21 — Subtle soft-aim target cue (2026-11-07)

Work remains local on `work`; no commit, push, merge or `main` change.

## Changes

- When head-radar weak assist can correct an on-screen target without RMB lock, show a small pulsing cyan corner cue and `ASSIST` label around the target.
- The cue is smaller and dimmer than the full lock reticle, fades out while full lock is active, and does not affect camera or aim direction.
- Added a visible HUD capture to the control review and smoke checks for unlocked/on-screen gating.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237322110 bytes` in `Logs/Iteration21-Build-20261107-194938.log`.
- Built-player control review passed 21 checks; the unlocked target cue was captured at `Docs/Screenshots/Controls-20261105/After/soft-aim-assist.png` (`Logs/Iteration21-Control-Review.log`).
- Built-player smoke test passed 373 checks in `Logs/Iteration21-Smoke.log`, including unlocked target visibility and full-lock separation.
- `git diff --check` passed. The build-generated `Arena.unity` IDs were restored to the pre-build version.
- Existing tracked control-review images overwritten by the capture run were restored. New `soft-aim-assist.png` remains as a review artifact.
