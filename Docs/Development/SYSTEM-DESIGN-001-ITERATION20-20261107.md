# Iteration 20 — Head radar gates soft aim assistance (2026-11-07)

Work remains local on `work`; no commit, push, merge or `main` change.

## Changes

- The existing weak aim assist now requires the target to be within both the equipped weapon's range and the selected head's `radarRange`.
- This affects firing correction only. It does not acquire RMB lock-on, rotate the camera, or extend weapon range.
- The default radar value leaves the primary weapon's prior assist reach unchanged. Reducing head radar range shortens assist awareness; increasing it can help long-range weapons when their own range permits it.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237320105 bytes` in `Logs/Iteration20-Build-20261107-193443.log`.
- Built-player smoke test passed: 371 checks in `Logs/Iteration20-Smoke.log`; new checks verify the head radar threshold and that it does not acquire camera lock.
- `git diff --check` passed. The build-generated scene IDs were restored; `Arena.unity` still matches its original pre-build hash.
