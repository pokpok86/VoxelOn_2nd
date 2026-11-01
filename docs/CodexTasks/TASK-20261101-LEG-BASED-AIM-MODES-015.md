---
id: TASK-20261101-LEG-BASED-AIM-MODES-015
mode: check
status: ready
priority: 6
title: Supersede lock-on control modes with leg-part-based facing and independent turret aiming
---

## Producer's latest explicit design decision (supersedes conflicting earlier proposals)
The producer rejects the earlier proposal to switch movement/facing schemes simply by lock-on ON/OFF. **Default control for ordinary bipedal mechs is Overwatch-like third-person mouse-camera-robot facing alignment**: mouse turns camera and robot facing together; WASD moves relative to camera. No automatic target-facing body rotation merely because a target is locked. Ordinary bipedal mechs retain intuitive mouse-controlled facing.

**Leg-part structure, not lock-on toggle, determines whether upper and lower body can rotate independently.** Quadruped and tank/tread leg parts (and other physically appropriate leg types) can keep lower body oriented along travel while upper body/turret separately tracks aim/locked target. Lock-on weapons can exploit these specialized leg-part capabilities; do not assume every weapon and leg type gets independent rotation.

## Design analysis
1. Inspect latest verified game candidate codex/implementation/2026-11-01-sd-ground, verify SHA and actual camera, input, lock-on, target-facing, weapon aim, robot rig/part hierarchy. Check reports/tasks TARGET-FACING-013 and COMBAT-CONTROLS-PROTOTYPE-014, mark their conflicting assumptions superseded.
2. Define two distinct leg-based facing capabilities: coupled biped (camera/mouse/body heading together) vs independent-turret quadruped/tank (lower travel heading separate from upper aiming heading). Explain how aim and camera relate in both modes, including with/without target lock.
3. Specify lock-on as weapon targeting aid, **not** universal change to movement or camera controls. Avoid claiming leg types categorically prohibit lock-on weapons unless producer specifies equip restrictions; suggest compatibility attributes for weapon/leg combinations.
4. Design safe transitions when switching leg parts in garage, target acquired/lost, 180-degree mouse turns, strafing, backward movement, escape dash, boost jumps, airborne reboost, high-fall landing stagger, target crossing behind, and torso yaw limits. Clarify animation/model feasibility and fallback for current voxel rig.
5. Propose minimum prototype: ordinary biped mouse-aligned camera/robot with normal WASD and dash; one quadruped or tank leg type with separate upper-body yaw. Compare playability and test mouse motion, attack alignment, dash, aim, target lock, collision, and camera stability. No broad weapon behavior rewrite.
6. Produce a short developer-ready implementation sequence and regression plan using pinned latest candidate source; account for pending jump work and avoid overlapping edits.
7. This is a design-only check. Do not modify source, main, or runner settings, and do not assert the behavior is implemented. Report to codex/task-reports.

## Critical interpretation
The producer's latest correction is authoritative: do **not** implement universal lock-on auto-facing or the previous lock-on-dependent free/combat control split. Any existing implementation tasks depending on those assumptions should be reconciled before execution.
