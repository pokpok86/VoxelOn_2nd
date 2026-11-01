---
id: TASK-20261101-MOBILITY-ARCHETYPES-017
mode: check
status: ready
priority: 4
title: Plan bounded playable comparison of humanoid, tank and quadruped locomotion
---

## Latest producer direction (authoritative)
- **Humanoid is NOT synonymous with all biped legs.** Humanoid means fast, running, agile, mouse-camera-facing aligned mechs. Other biped legs may support independent upper/lower body rotation.
- Core/leg pairing can restrict humanoid archetype for aesthetic and engineering reasons (e.g. low allowable supported mass); specific compatibility and mass values remain **proposals**, not final approvals.
- Humanoid has a **fast ground leap/dash with NO boost gauge cost**, physically feeling like pushing off/jumping, distinct from Space hold/reboost that consumes boost.
- Tank/tread chassis supports separate upper-body aiming, and any fast traversal is restricted to the chassis' current forward heading (not instant side/back dash).
- Quadruped has independent upper aiming and **no high-speed movement/dash**.
- Producer explicitly wants **one or two of the current playable robots to use alternate movement modes** to validate the handling in-game, without broadly rewriting all eight.
- Lock-on weapon eligibility is NEVER restricted by leg type. Lock-on performance belongs to head and weapon parts (TASK-20261101-LOCKON-EQUIPMENT-016).

## Required prototype plan
1. Inspect latest verified game candidate codex/implementation/2026-11-01-sd-ground, identify all playable robots, their actual core/leg components, locomotion and input handling, camera/aim and dash implementation. Record full source SHA and exact files.
2. Recommend which 1-2 EXISTING robots are best candidates for alternate movement types (prefer one tank-style and one quadruped if geometry and code support; if not, choose one alternative and document limitations). Preserve remaining humanoid test control. Do not claim visuals support chassis types they do not.
3. Design bounded, reversible prototype: humanoid free zero-boost-cost leap/dash (with appropriate cooldown/grounding constraints, no infinite chain or free vertical flight); tank forward-only fast move; quadruped no high-speed dash. Clarify Space jump/hold boost and midair reboost remain separate and are not accidentally disabled.
4. Define facing: humanoid mouse/camera/body aligned; non-humanoid leg type may have independent torso aiming, including special biped cases. Keep all lock-on weapons equip-eligible, with head/weapon determining tracking.
5. Draft tunable supported-mass/core-leg compatibility constraints as OPTIONAL balance proposal only. Avoid new hard-coded equipment restrictions without producer signoff.
6. Provide a minimal prototype implementation plan with per-robot config flags and minimal shared movement changes, not a full locomotion architecture rewrite. Separate tasks for any code changes; use a pinned latest candidate source and unique branch. Coordinate with pending 2x jump/Space boost tasks and target-facing changes.
7. Acceptance tests: each archetype movement, input and facing, no boost consumption for humanoid ground dash, boost consumption for Space ascent, tank no lateral/reverse dash, quadruped dash disabled, landing/collision/roof support, mouse aim, lock-on/head-weapon behavior, build and built-player smoke; plus producer hands-on A/B checklist.
8. Design/check only; do not edit gameplay source or main, and do not report new executable until independently built and validated. Publish to codex/task-reports.

## Prioritization
Producer specifically wants an actual one- or two-robot playable comparison; deliver an implementation-ready bounded plan promptly, with clear safety blockers and no conflicting assumptions from older tasks.
