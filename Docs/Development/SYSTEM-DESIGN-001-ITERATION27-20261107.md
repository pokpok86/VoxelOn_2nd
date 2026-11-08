# Iteration 27 — Six assembly groups and separate arm selection (2026-11-07)

## Scope

Reorganized the part editor into the six requested groups: head, torso, arms, legs, backpack, and skill. The arm group retains separate left-arm and right-arm detail controls, backed by the existing independent `LEFT_ARM` and `RIGHT_ARM` design parts. The skill card opens the existing skill selector. Core-part variants remain inside the torso group instead of appearing as an extra assembly category.

Each physical category resolves to the selected design's data-backed part. The selection tiles use schematic silhouettes from Iteration 26; they are not renders of interchangeable 3D part models.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237343266 bytes` in `Logs/Iteration27-Build-20261107-iteration27-verified.log`.
- Built-player balance editor study passed 109 checks, including exactly six selector groups and distinct left/right arm part IDs: `Logs/Iteration27-Design-Study-Final-20261107.log`.
- Built-player smoke test passed 373 checks: `Logs/Iteration27-Smoke-Final-20261107.log`.
- Visually reviewed the arm selector and six-group menu: `Logs/Balance-Editor-Test-20261107-132506/Screenshots/02e-arm-selector.png`.
- `git diff --check` passed. Dirty `Assets/Scenes/Arena.unity` was copied before the scene-generation build and restored afterward. Pre/post SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

All work remains local on `work`; no commit, push, merge, or `main` change.
