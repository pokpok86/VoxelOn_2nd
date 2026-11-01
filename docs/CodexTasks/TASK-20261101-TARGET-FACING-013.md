---
id: TASK-20261101-TARGET-FACING-013
mode: check
status: ready
priority: 9
title: Design automatic target-facing while moving and attacking
---

## Producer feedback
Producer finds it unpleasant that while moving and attacking, the mech's body does not naturally face the opponent, forcing additional directional input just to reorient. VoxelOn is a lock-on mech combat game. Design a correction so that **movement input never has to be used solely to face the locked target**. Preserve player-selected movement direction, including strafe and retreat.

## Intended feel
- With a valid lock-on target, continuously orient combat facing toward target, including while walking, strafing, backing up, jumping, boosting, and firing.
- Prefer upper-body/weapon aim first, with body/legs turning smoothly if existing rig supports it; otherwise smoothly rotate the whole mech without changing movement velocity. No instant snapping or oscillation.
- On attack, projectile/weapon aim should match valid target direction and camera/crosshair rules; do not require an extra direction-key press. Avoid unfair instant 180-degree attacks if current weapon arcs/turn rates forbid it.
- Distinguish target lock lost, no target, multiple targets, close pass-through, airborne and grounded states; no sudden movement direction reversals or camera jumps.
- Preserve keyboard movement, collision, boost, jump, aim assist, practice AI, network determinism and combat balance.

## Investigation and deliverables
Inspect latest game candidate codex/implementation/2026-11-01-sd-ground (verify SHA), current target lock, movement, aiming, rig orientation, camera and attack input paths. Reproduce/characterize mismatch if possible and propose minimal implementation with turn rate and dead-zone parameters plus tests for strafe-fire, backward-fire, jump-fire, boost-fire, target crosses behind, target lost, and no-lock movement. Explain any upper-body split feasibility vs whole-body fallback. Produce precise implementation task only after confirming pinned candidate base and no conflicts with jump/leg tasks. No gameplay source changes in this design check, no main edits, no merges. Report to codex/task-reports.
