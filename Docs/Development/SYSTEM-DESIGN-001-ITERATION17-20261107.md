# Iteration 17 — Four-slot portrait angle and drag rotation (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Changes

- Kept all four configured weapons visible, following PD's choice.
- Shifted the frame portrait camera closer to a front view so shoulder and hand equipment are easier to distinguish.
- Removed the ±45° step buttons. The preview now rotates by dragging left/right; redraw is throttled to 150 ms during movement and finalized on mouse release.
- Expanded the portrait viewport vertically to use the space freed by the buttons. No loadout, combat, or balance data changed.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237307709 bytes` in `Logs/HangarPreview-20261107-Build.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/HangarPreview-20261107-Smoke.log`.
- Visible balance editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 88 checks; 14 images` in `Logs/HangarPreview-20261107-EditorStudy.log`.
- Inspected the four-equipment portrait and drag instruction in `Logs/Balance-Editor-Test-20261107-095356/Screenshots/02-part-options.png`. Interactive mouse input was not manually exercised; the study confirms the updated screen render only. `git diff --check` passed. The generated Arena scene was restored; existing model references and balance-study data remain clean.
