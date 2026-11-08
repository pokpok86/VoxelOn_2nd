# Iteration 16 — Use configured gear in frame portraits (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Changes

- Removed the fixed showcase weapon sets from frame portraits. Preview render now keeps the gear created from the configured loadout instead of replacing it with a different per-chassis example set.
- This changes portrait presentation only. Saved loadouts, available mounts, and combat equipment rules were not changed.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237307730 bytes` in `Logs/PortraitLoadoutPreview-20261107-Build.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/PortraitLoadoutPreview-20261107-Smoke.log`.
- Visible balance editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 88 checks; 14 images` in `Logs/PortraitLoadoutPreview-20261107-EditorStudy.log`.
- Inspected `Logs/Balance-Editor-Test-20261107-090955/Screenshots/02-part-options.png`. The previous artificial gear set is gone, but the four configured mount items still made the portrait visually busy. PD chose to keep all four equipped items visible; the next iteration adjusts their viewing angle and removes the step-rotation buttons.
