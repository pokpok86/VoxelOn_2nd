# Iteration 13 — Ten developer balance presets (2026-11-07)

Work remains local on `work`; no commit, push, merge, or `main` change.

## Changes

- Added ten stable `DEV_PRESET_01`…`DEV_PRESET_10` slots in a separate `DeveloperPresets.json` file with an independent `.bak` backup on overwrite.
- The new editor tab compares each occupied snapshot's selected chassis, left-hand weapon, damage and rate of fire. Empty slots can be filled from the current balance draft.
- Loading a snapshot copies it into the editable draft only. Saving the regular balance file and applying values to combat still require their existing explicit actions.
- Preset snapshots are normalized through the existing balance schema validator. Invalid IDs, unsupported versions, invalid values and an eleventh slot are rejected. A populated flag distinguishes empty slots because Unity's `JsonUtility` emits default-shaped objects for null nested records.
- The player build slots remain eight; developer presets are ten separate bank entries. No weapon or part icon editing, new combat behavior, or balance rule was added in this slice.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237298805 bytes` in `Logs/DeveloperPresets-20261107-Build5.log`.
- Built-player smoke test exited 0: `VOXSKY_SMOKE_PASS 369 checks` in `Logs/DeveloperPresets-20261107-Smoke5.log`.
- Actual balance editor study exited 0: `BALANCE_EDITOR_STUDY_PASS 82 checks; 14 images` in `Logs/DeveloperPresets-20261107-EditorStudy5.log`. This covered all ten IDs, save/reload, distinct snapshots, safe overwrite backup, keeping the regular balance file untouched during preset save, and rejecting slot 11.
- Inspected the rendered preset tab at `Logs/Balance-Editor-Test-20261107-080528/DeveloperPresets-UI.png`. `git diff --check` passed. Build-generated `Assets/Scenes/Arena.unity` IDs were restored; no prior scene or screenshot changes remain.

## Remaining

The preset comparison is a compact snapshot summary, not a side-by-side numeric diff for every weapon/part. Loading brings a snapshot into the editor draft; it does not directly create an isolated live experiment runtime. Attribute-icon editing, broader experiment controls, and a separate promotion flow remain unimplemented. The Max request checklist now reflects this partial status.
