# TASK-20261102-MOBILITY-RUNNER-029

- Type: implementation
- Status: implemented on work; Unity build and gameplay validation pending
- Source requests: TASK-20261101-ARCHETYPE-FIRST-018, TASK-20261101-MOBILITY-ARCHETYPES-017
- Target branch: work (never main without user approval)

## Requirement
Preserve the eight existing catalog entries while introducing explicit mobility archetypes. Prototype runner ground dodge without spending boost fuel; airborne dodge still uses fuel. Keep tank and quadruped opt-in until corresponding movement and visual implementations exist.

## Changes made
- `Assets/Scripts/CraftCatalog.cs`: MobilityKind enum (StandardBiped, Runner, Tank, Quadruped), per-spec mobility property, WISP Runner prototype.
- `Assets/Scripts/VoxArena.cs`: runner ground dodge has zero fuel cost while retaining cooldown, distance, and other dash constraints.
- Code commits: `04b3e66b121cb41e32eaa069f7559eb3f5955a51`, `f42f4753495de59d27643d8603186c6b46c31374`.

## Verification pending on home PC
- Unity compile and smoke suite; WISP ground double-tap dash spends zero fuel.
- WISP airborne dash still spends fuel; other frames unchanged.
- Test ground-edge timing and blocked/knockdown behavior.
- Tank and quadruped geometry, upper/lower independent facing and movement remain future tasks.
