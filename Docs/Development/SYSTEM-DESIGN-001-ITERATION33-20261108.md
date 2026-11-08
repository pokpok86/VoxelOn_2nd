# Iteration 33 — Quadruped walk-to-trot gait (2026-11-08)

## Scope

Changed the four-leg visual gait from one fixed diagonal pattern to a speed-dependent transition. At low speed it uses a staggered four-beat walk ordered by each hip's actual front/rear and left/right position. As speed increases, the phase offsets blend into an alternating diagonal trot. The planted-foot IK and world-space foot placement remain in place. Movement speed, hitboxes, and jump rules are unchanged.

Added an isolated capture destination (`--gait-review`) so this iteration's motion screenshots do not overwrite the earlier `Before` or `After` evidence.

## Validation

- Unity 6000.6.7f1 Windows build passed: `VOXSKY_BUILD_PASS 237369110 bytes` in `Logs/Quadruped-Gait-Build-20261108.log`.
- Built-player smoke test passed 377 checks, including staggered low-speed footfalls and diagonal high-speed trot: `Logs/Quadruped-Gait-Smoke-20261108.log`.
- BASTION and TARANTULA motion study passed all 6 checks in `Logs/Quadruped-Gait-Study-20261108.log`. TARANTULA recorded 51 lower-skin samples with 0.0000 minimum sole gap and 0.0591 maximum gap during the sampled movement.
- Eight actual images saved under `Docs/Screenshots/PD14-Chassis-Motion/Iteration33/`; TARANTULA moving frames are `3-TARANTULA-moving-0.png` through `3-TARANTULA-moving-2.png`.
- `git diff --check` passed. Existing dirty `Assets/Scenes/Arena.unity` was preserved byte-for-byte around the build; SHA-256 remains `F40E0AAF64482CA8A4DD1DE775E1585141B1FA240330E1A12C8912E2CF7915CB`.

## Remaining user requests

Continue adding distinct data-driven part models. Remaining gameplay work includes varying weapon stats and evaluating them in simulated duels; adding explosion or homing behavior to appropriate non-lock-on weapons; scaling maps to participant count; and making the active weapon obvious in the HUD or reticle. When shoulder-exclusive weapons are developed, add a dedicated shoulder-to-weapon connector distinct from hand mounts.

All work remains local on `work`; no commit, push, merge, or `main` change.
