# Iteration 34 — Independent arm models (2026-11-08)

## Scope

Added selectable visual models to the existing left and right arm parts: standard, reinforced forearm, and precision servo. Each side is saved independently per design, and the arm picker updates the schematic tile and live 3D preview. Applying the design adds armor details to the existing arm joints. Hand weapons and the right guard remain attached to their existing sockets; the arm model does not create or equip a shield. Legacy files default to the standard arms, and unknown model IDs are rejected.

The changes are cosmetic. They do not change handling, damage, hitboxes, or weapon mounting rules.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237377429 bytes` in `Logs/Arm-Visual-Build-20261108.log`.
- Built-player balance editor study passed 129 checks and captured 26 images in `Logs/Arm-Visual-Study-20261108.log`, including independent left/right save/load, legacy defaults, unknown-ID rejection, actual arm assembly, and preservation of hand/guard mounts.
- Built-player smoke test passed 377 checks in `Logs/Arm-Visual-Smoke-20261108.log`.
- Arm preview: `Logs/Balance-Editor-Test-20261107-160708/Screenshots/02l-independent-arm-models-preview.png`.
- `git diff --check` passed. Existing dirty `Assets/Scenes/Arena.unity` remained byte-identical around the build; SHA-256: `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

## Remaining user requests

Continue adding data-driven part models. Remaining gameplay work includes varying weapon stats and evaluating them in simulated duels; adding explosion or homing behavior to appropriate non-lock-on weapons; scaling maps to participant count; and making the active weapon obvious in the HUD or reticle. When shoulder-exclusive weapons are developed, add a dedicated shoulder-to-weapon connector distinct from hand mounts.

All work remains local on `work`; no commit, push, merge, or `main` change.
