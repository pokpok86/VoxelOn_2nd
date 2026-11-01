---
id: TASK-20261101-LOCKON-EQUIPMENT-016
mode: check
status: ready
priority: 5
title: Correct lock-on model: weapon/head performance, no leg-based equip restrictions
---

## Latest producer correction (authoritative)
- **Lock-on performance is determined by weapon and head parts**, e.g. acquisition speed, tracking capability, lock range, tracking quality and target retention, subject to later balance.
- **No leg-part-based lock-on weapon equip restrictions or compatibility gates.** Any appropriate mech may equip a lock-on weapon; practical usefulness naturally depends on handling/aiming style.
- Default ordinary biped control remains Overwatch-like: mouse direction aligns camera and robot facing, so lock-on can be awkward/less useful but must not be disabled or forbidden.
- Quadruped/tank-like legs allow independent upper/lower body rotation and may exploit lock-on more naturally. Leg parts determine mechanical rotation capability and mobility, **not lock-on eligibility or lock-on performance stats**.
- Supersedes any suggestion in TASK-20261101-LEG-BASED-AIM-MODES-015 or earlier tasks that a lock-on weapon should require specific leg parts.

## Design-only work
Inspect current game candidate codex/implementation/2026-11-01-sd-ground and relevant design reports. Update proposed part responsibility matrix and minimal prototype/test plan: head/weapon control lock acquisition and tracking; legs control chassis/torso rotation, jump/landing and mobility; mouse drives default camera-facing. Test biped with lock-on weapon equipped (usable, potentially less convenient), quadruped/tank with same weapon, free mouse aim, target loss and transitions. Identify potential existing code equip restrictions to avoid introducing new ones. Reconcile earlier conflicting task specifications before any implementation. No source edits, no main changes, no assumptions of completed implementation. Publish findings to codex/task-reports.
